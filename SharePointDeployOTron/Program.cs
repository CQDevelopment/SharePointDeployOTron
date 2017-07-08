namespace SharePointDeployOTron
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var source = args.Length != 1 ? @"C:\Users\AJ\Google Drive\Source\SharePointCiTest" : args[0];

            Logic.Processor.Process(source);
        }
    }
}
