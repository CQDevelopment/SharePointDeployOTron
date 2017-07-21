namespace SharePointDeployOTron.Logic
{
    using Microsoft.SharePoint.Client;
    using System;
    using System.IO;
    using System.Linq;

    public class Processor
    {
        public IProcessor Provider { get; }
        public ClientContext Context { get; }
        public Folder RootFolder { get; }

        public Processor(IProcessor provider)
        {
            this.Provider = provider;

            var securePassword = new System.Security.SecureString();
            Array.ForEach(this.Provider.Password.ToCharArray(), (character) => { securePassword.AppendChar(character); });

            var credentials = new SharePointOnlineCredentials(this.Provider.User, securePassword);

            this.Context = new ClientContext(this.Provider.TargetWeb)
            {
                Credentials = credentials
            };


            var allLists = this.Context.Web.Lists;
            this.Context.Load(allLists);
            this.Context.ExecuteQuery();

            var assetsList = allLists.SingleOrDefault(
                (list) =>
                    list.Title.Equals(this.Provider.TargetLibrary, StringComparison.InvariantCultureIgnoreCase));

            if (assetsList == default(List))
            {
                assetsList = this.Context.Web.Lists.Add(
                    new ListCreationInformation
                    {
                        Title = this.Provider.TargetLibrary,
                        TemplateType = (int)ListTemplateType.DocumentLibrary,
                        Url = this.Provider.TargetLibrary
                    });

                this.Context.ExecuteQuery();
            }

            this.RootFolder = assetsList.RootFolder;
        }

        public void Process(string filePath)
        {
            this.Process(filePath, default(FileInfo));
        }

        public void Process(string filePath, FileInfo fileInfo)
        {
            if (fileInfo == default(FileInfo))
            {
                fileInfo = new FileInfo(filePath);
            }

            Console.Write("[{0}] {1}", DateTime.Now, fileInfo.Name);

            var sourceStream = default(FileStream);

            while (sourceStream == default(FileStream))
            {
                try
                {
                    sourceStream = System.IO.File.OpenRead(filePath);
                }
                catch
                {
                    Console.Write(".");
                    System.Threading.Thread.Sleep(100);
                }
            }

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var file = this.RootFolder.Files.Add(
                new FileCreationInformation
                {
                    ContentStream = sourceStream,
                    Url = fileInfo.Name,
                    Overwrite = true
                });

            this.Context.ExecuteQuery();

            sourceStream.Dispose();
            sourceStream = null;

            try
            {
                file.CheckIn(string.Empty, CheckinType.MajorCheckIn);
                this.Context.ExecuteQuery();
            }
            catch { }

            try
            {
                file.Publish(string.Empty);
                this.Context.ExecuteQuery();
            }
            catch { }

            try
            {
                file.Approve(string.Empty);
                this.Context.ExecuteQuery();
            }
            catch { }

            stopwatch.Stop();

            Console.WriteLine(" {0:0.00}s", stopwatch.Elapsed.TotalSeconds);
        }
    }
}
