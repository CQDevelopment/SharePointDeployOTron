namespace SharePointDeployOTron.Logic
{
    using Microsoft.SharePoint.Client;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    public class Processor
    {
        public static void Process(string source)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Console.WriteLine("[{0}] Deployment starting.", DateTime.Now);

            var url = ConfigurationManager.AppSettings["Url"];
            var user = ConfigurationManager.AppSettings["User"];
            var password = ConfigurationManager.AppSettings["Password"];

            var securePassword = new System.Security.SecureString();
            Array.ForEach(password.ToCharArray(), (character) => { securePassword.AppendChar(character); });

            var credentials = new SharePointOnlineCredentials(user, securePassword);


            var cthClient = new ClientContext(url + "/sites/contenttypehub");
            cthClient.Credentials = credentials;



            var cthLists = cthClient.Web.Lists;
            cthClient.Load(cthLists);
            cthClient.ExecuteQuery();

            var assetsList = cthLists.SingleOrDefault(
                (list) =>
                    list.Title.Equals("assets", StringComparison.InvariantCultureIgnoreCase));

            if (assetsList == default(List))
            {
                assetsList = cthClient.Web.Lists.Add(
                    new ListCreationInformation
                    {
                        Title = "Assets",
                        TemplateType = (int)ListTemplateType.DocumentLibrary,
                        Url = "assets"
                    });

                cthClient.ExecuteQuery();
            }

            var assetsListFolder = assetsList.RootFolder;
            var outFolder = Directory.GetFiles(source + @"\out");

            foreach (var sourceFilePath in outFolder)
            {
                Console.WriteLine(sourceFilePath);

                var sourceStream = System.IO.File.OpenRead(sourceFilePath);

                var file = assetsListFolder.Files.Add(
                    new FileCreationInformation
                    {
                        ContentStream = sourceStream,
                        Url = new FileInfo(sourceFilePath).Name,
                        Overwrite = true
                    });

                cthClient.ExecuteQuery();

                sourceStream.Dispose();

                try
                {
                    file.CheckIn(string.Empty, CheckinType.MajorCheckIn);
                    cthClient.ExecuteQuery();
                }
                catch { }

                try
                {
                    file.Publish(string.Empty);
                    cthClient.ExecuteQuery();
                }
                catch { }

                try
                {
                    file.Approve(string.Empty);
                    cthClient.ExecuteQuery();
                }
                catch { }
            }

            cthClient.Dispose();
            cthClient = null;

            var rootClient = new ClientContext(url);
            rootClient.Credentials = credentials;

            var masterPageList = rootClient.Web.Lists.GetByTitle("Master Page Gallery");
            rootClient.Load(masterPageList);
            rootClient.ExecuteQuery();

            var masterPageFolder = masterPageList.RootFolder;
            var srcFolder = Directory.GetFiles(source + @"\src");

            foreach (var sourceFilePath in srcFolder)
            {
                if (!sourceFilePath.EndsWith(".master", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                Console.WriteLine(sourceFilePath);

                var file = masterPageFolder.Files.Add(
                    new FileCreationInformation
                    {
                        Content = System.IO.File.ReadAllBytes(sourceFilePath),
                        Url = new FileInfo(sourceFilePath).Name,
                        Overwrite = true
                    });

                rootClient.ExecuteQuery();

                try
                {
                    file.CheckIn(string.Empty, CheckinType.MajorCheckIn);
                    rootClient.ExecuteQuery();
                }
                catch { }

                try
                {
                    file.Publish(string.Empty);
                    rootClient.ExecuteQuery();
                }
                catch { }

                try
                {
                    file.Approve(string.Empty);
                    rootClient.ExecuteQuery();
                }
                catch { }
            }

            rootClient.Dispose();
            rootClient = null;

            stopwatch.Stop();

            Console.WriteLine("[{0}] Deployment complete.", DateTime.Now);
            Console.WriteLine(stopwatch.Elapsed);
        }
    }
}
