namespace SharePointDeployOTron.Watcher
{
    using System;
    using System.IO;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var sourceFolder = System.Configuration.ConfigurationManager.AppSettings["SourceFolder"];
            var processor = new Logic.Processor(Logic.ProcessorProvider.GetDefault());

            var fileMonitor = new System.Collections.Generic.Dictionary<string, DateTime>();

            while (true)
            {
                var outFiles = Directory.GetFiles(sourceFolder);

                foreach (var filePath in outFiles)
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileName = fileInfo.Name;
                    var fileUpdated = fileInfo.LastWriteTime;

                    if (!fileMonitor.ContainsKey(fileName))
                    {
                        fileMonitor[fileName] = DateTime.MinValue;
                    }

                    if (fileMonitor[fileName] < fileUpdated)
                    {
                        try
                        {
                            var sourceStream = System.IO.File.OpenRead(filePath);
                            sourceStream.Dispose();
                            sourceStream = null;
                        }
                        catch
                        {
                            continue;
                        }

                        processor.Process(filePath);
                        fileMonitor[fileName] = fileUpdated;
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
