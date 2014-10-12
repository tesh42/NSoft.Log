using System;
using System.Threading;
using Moq;
using NSoft.Log.Core;
using NUnit.Framework;

namespace NSoft.Log.Tests
{
    /// <summary>
    /// Summary description for LogManagerTests
    /// </summary>
    [TestFixture]
    public class LogManagerTests
    {

        const string TestChannel1 = "Test1";
        const string TestChannel2 = "Test2";
        const string TestData = "bla";

        LogManager logManager;
        ILogManagerConfigurator logManagerConfigurator;
        Mock<LogWriterBase> logWriter1;
        Mock<LogWriterBase> logWriter2;

        [SetUp]
        public void MyTestInitialize()
        {
            logManager = new LogManager();
            logManagerConfigurator = logManager;

            logWriter1 = new Mock<LogWriterBase>(1);
            logWriter2 = new Mock<LogWriterBase>(2);

        }

        [Test]
        public void OneCategoryNormalWorkTest()
        {
            logManagerConfigurator.CreateCategory(1, 30000);

            logManagerConfigurator.BindWriter(1, logWriter2.Object, 9);
            logManagerConfigurator.BindWriter(1, logWriter1.Object, 12);
            

            logManagerConfigurator.BindChannel(1, TestChannel1);

            logManager.Write(TestChannel1, TestData);

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }


        [Test]
        public void TwoCategoriesSeparatedChannelsTest()
        {
            logManagerConfigurator.CreateCategory(1, 30000);
            logManagerConfigurator.CreateCategory(2, 30000);

            logManagerConfigurator.BindWriter(1, logWriter1.Object, 9);
            logManagerConfigurator.BindWriter(2, logWriter2.Object, 12);

            logManagerConfigurator.BindChannel(1, TestChannel1);
            logManagerConfigurator.BindChannel(2, TestChannel2);

            logManager.Write(TestChannel1, TestData);

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            logManager.Write(TestChannel2, TestData);

            logWriter1.Verify(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(TestChannel2, TestData), Times.Exactly(1));
        }

        [Test]
        public void TwoCategoriesSameChannelTest()
        {
            logManagerConfigurator.CreateCategory(1, 30000);
            logManagerConfigurator.CreateCategory(2, 30000);

            logManagerConfigurator.BindWriter(1, logWriter1.Object, 9);
            logManagerConfigurator.BindWriter(2, logWriter2.Object, 12);

            logManagerConfigurator.BindChannel(1, TestChannel1);
            logManagerConfigurator.BindChannel(2, TestChannel1);

            logManager.Write(TestChannel1, TestData);

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
        }

        [Test]
        public void DisposeTest()
        {
            logManagerConfigurator.CreateCategory(1, 30000);

            logWriter1.As<IDisposable>();

            logManagerConfigurator.BindWriter(1, logWriter1.Object, 12);

            logManagerConfigurator.BindChannel(1, TestChannel1);

            logManager.Dispose();

            logWriter1.As<IDisposable>().Verify(obj => obj.Dispose(), Times.Exactly(1));
        }

        [Test]
        public void UnknownChannelWriteTest()
        {
            logManagerConfigurator.CreateCategory(1, 30000);

            logManagerConfigurator.BindWriter(1, logWriter2.Object, 9);
            logManagerConfigurator.BindWriter(1, logWriter1.Object, 12);


            logManagerConfigurator.BindChannel(1, TestChannel1);

            logManager.Write(TestChannel2, TestData);

            logWriter1.Verify(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            logWriter2.Verify(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void FirstWriterFailedSwitchTest()
        {
            logManagerConfigurator.CreateCategory(1, 1000);

            logWriter1.Setup(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Failed"));

            logManagerConfigurator.BindWriter(1, logWriter1.Object, 12);
            logManagerConfigurator.BindWriter(1, logWriter2.Object, 9);

            logManagerConfigurator.BindChannel(1, TestChannel1);

            logManager.Write(TestChannel1, TestData);

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            
            logManager.Write(TestChannel1, TestData);

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(2));

            Thread.Sleep(1100);

            logManager.Write(TestChannel1, TestData);

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(2));
            logWriter2.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(3));

        }

        [Test]
        public void AllWritersFailedTest()
        {
            logManagerConfigurator.CreateCategory(1, 1000);

            logWriter1.Setup(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
            logWriter2.Setup(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            logManagerConfigurator.BindWriter(1, logWriter1.Object, 12);
            logManagerConfigurator.BindWriter(1, logWriter2.Object, 9);

            logManagerConfigurator.BindChannel(1, TestChannel1);

            Assert.Throws<Exception>(() => logManager.Write(TestChannel1, TestData));

            logWriter1.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
            logWriter2.Verify(obj => obj.Write(TestChannel1, TestData), Times.Exactly(1));
        }

        [Test]
        public void WriterFailedEventTest()
        {
            logManagerConfigurator.CreateCategory(1, 30000);

            logWriter1.Setup(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
            logWriter2.Setup(obj => obj.Write(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            logManagerConfigurator.BindWriter(1, logWriter1.Object, 12);
            logManagerConfigurator.BindWriter(1, logWriter2.Object, 9);

            logManagerConfigurator.BindChannel(1, TestChannel1);

            var errors = 0;
            var fatalErrors = 0;
            logManager.WriteFailed += (sender, e) => { errors++; fatalErrors += e.IsFatalError ? 1 : 0; };

            Assert.Throws<Exception>(() => logManager.Write(TestChannel1, TestData));

            Assert.AreEqual(2, errors);
            Assert.AreEqual(1, fatalErrors);
        }
    }
}
