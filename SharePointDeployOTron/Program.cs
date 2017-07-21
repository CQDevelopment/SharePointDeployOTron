namespace SharePointDeployOTron
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sourceFolder = System.Configuration.ConfigurationManager.AppSettings["SourceFolder"];
            var processor = new Logic.Processor(Logic.ProcessorProvider.GetDefault());

            var sourceFiles = System.IO.Directory.GetFiles(sourceFolder);
            
            foreach (var filePath in sourceFiles)
            {
                processor.Process(filePath);
            }
        }
    }
}
