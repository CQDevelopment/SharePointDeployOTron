namespace SharePointDeployOTron.Watcher
{
    using System;
    using System.IO;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var source = args.Length != 1 ? @"C:\Users\AJ\Google Drive\Source\SharePointCiTest" : args[0];
            var outFolder = source + @"\out";

            var lastRun = DateTime.MinValue;

            while (true)
            {
                var outFiles = Directory.GetFiles(outFolder);
                var fileInfo = new FileInfo(outFiles[0]);

                if (fileInfo.LastWriteTimeUtc > lastRun)
                {
                    lastRun = fileInfo.LastWriteTimeUtc;

                    Logic.Processor.Process(source);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
