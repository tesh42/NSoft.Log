using System;
using System.ServiceModel;
using NSoft.Log.Sample.Service;

namespace NSoft.Log.Sample.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Starting service...    ");
            using (var host = new ServiceHost(typeof(LogManager)))
            {
                host.Open();
                Console.WriteLine("Success!");
                Console.WriteLine("Press <Enter> for exit...");
                Console.ReadLine();
                host.Close();
            }
        }
    }
}
