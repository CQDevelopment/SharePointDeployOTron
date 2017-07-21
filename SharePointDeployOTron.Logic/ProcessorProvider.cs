namespace SharePointDeployOTron.Logic
{
    public class ProcessorProvider : IProcessor
    {
        public string TargetWeb { get; set; }

        public string TargetLibrary { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public static ProcessorProvider GetDefault()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;

            return new ProcessorProvider
            {
                TargetWeb = appSettings["TargetWeb"],
                TargetLibrary = appSettings["TargetLibrary"],
                User = appSettings["User"],
                Password = appSettings["Password"]
            };
        }
    }
}
