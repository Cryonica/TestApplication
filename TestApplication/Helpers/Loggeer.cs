using System;
using System.IO;
using System.Reflection;

namespace TestApplication.Helpers
{
    public sealed class Loggeer
    {
        private static volatile Loggeer instance;
        private static object syncRoot = new object();
        private const string logFileName = "errors.log";
        private readonly string logFilePath;
        private Loggeer() 
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(assemblyPath);
            logFilePath = Path.Combine(directoryPath, logFileName);

        }
        public static Loggeer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Loggeer();
                    }
                }

                return instance;
            }
        }
        public void Log(string message)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"{DateTime.Now}: {message}");
            }
        }




        //internal Loggeer(string filePath)
        //{
        //    _filePath = filePath;
        //}
        //public void Log(string message)
        //{
        //    using (StreamWriter writer = new StreamWriter(_filePath, true))
        //    {
        //        writer.WriteLine(message);
        //    }
        //}
    }
}
