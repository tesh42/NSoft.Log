using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using NSoft.Log.Core;

namespace NSoft.Log.ConsoleClient
{
    class Program
    {
        /// <summary>
        /// Date and time format parameter name.
        /// </summary>
        const string DateTimeFormatParameterName = "DateTimeFormat";

        /// <summary>
        /// Messages logger.
        /// </summary>
        static SimpleLogger log;

        /// <summary>
        /// List of the threads that are used for test.
        /// </summary>
        static readonly List<Thread> testThreads = new List<Thread>();

        /// <summary>
        /// Event that will be switched to the signaled state when termination of the test threads be requested.
        /// </summary>
        static readonly ManualResetEvent stopThreadsEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var configuration = LoadLogManagerConfiguration();
            var logManager = new LogManagerFactory().Create(configuration);
            log = new SimpleLogger(logManager);
            var dateTimeFormat = ConfigurationManager.AppSettings[DateTimeFormatParameterName];
            if (!string.IsNullOrEmpty(dateTimeFormat))
                log.DateTimeFormat = dateTimeFormat;
            var normalCount = RequestNumber("Normal threads count: ");
            var errorCount = RequestNumber("Error threads count: ");
            CreateAndStartThreads(normalCount, NormalThreadCallback, c => string.Format("Hello, Thread{0}N!", c));
            CreateAndStartThreads(errorCount, ErrorThreadCallback, c => new Exception(string.Format("Error in Thread{0}E!", c)));
            Console.WriteLine("Press <Enter> to stop...");
            Console.ReadLine();
            stopThreadsEvent.Set();
            foreach (var testThread in testThreads)
                testThread.Join();
        }

        /// <summary>
        /// Requests and reads number from the standard input stream.
        /// </summary>
        /// <param name="prompt">Prompt that should be shown to user.</param>
        static int RequestNumber(string prompt)
        {
            int normalCount;
            do
            {
                Console.Write(prompt);
            } while (!int.TryParse(Console.ReadLine(), out normalCount));
            return normalCount;
        }

        /// <summary>
        /// Creates the and start threads.
        /// </summary>
        /// <param name="count">Number of the threads.</param>
        /// <param name="callback">Callback that will be used by threads.</param>
        /// <param name="getParameter">Function that takes a thread number and returns parameter for that thread.</param>
        static void CreateAndStartThreads(int count, ParameterizedThreadStart callback, Func<int, object> getParameter)
        {
            for (var i = 0; i < count; ++i)
            {
                var thread = new Thread(callback) {IsBackground = true};
                thread.Start(getParameter(i));
                testThreads.Add(thread);
            }
        }

        /// <summary>
        /// Loads configuration of the log manager.
        /// </summary>
        static LogManagerConfiguration LoadLogManagerConfiguration()
        {
            try
            {
                return (LogManagerConfiguration)ConfigurationManager.GetSection("logManager");
            }
            catch (Exception)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "> Failed to load database writer settings from the 'logManager' section.");
                throw;
            }
        }

        /// <summary>
        /// Writes normal messages to the log.
        /// </summary>
        /// <param name="parameter">Message that should be written to the log.</param>
        static void NormalThreadCallback(object parameter)
        {
            while (!stopThreadsEvent.WaitOne(1, false))
                log.LogMain(parameter.ToString());
        }

        /// <summary>
        /// Writes errors to the log.
        /// </summary>
        /// <param name="parameter">Exception that should be written.</param>
        static void ErrorThreadCallback(object parameter)
        {
            while (!stopThreadsEvent.WaitOne(1, false))
                log.LogError(((Exception)parameter).Message, (Exception)parameter);
        }
    }
}
