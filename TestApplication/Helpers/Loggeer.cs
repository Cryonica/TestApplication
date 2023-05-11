using System.IO;

namespace TestApplication.Helpers
{
    internal class Loggeer
    {
        private readonly string _filePath;
        internal Loggeer(string filePath)
        {
            _filePath = filePath;
        }
        public void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter(_filePath, true))
            {
                writer.WriteLine(message);
            }
        }
    }
}
