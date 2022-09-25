using Continuations;

class Program
{
    private static Random random = new Random();

    static void ShowSplash()
    {
        Console.WriteLine("Splash...");
    }

    static void RequestLicense()
    {
        if (random.Next(100) >= 50)
            throw new LicenseException("Лицензия не действительна");
        Console.WriteLine("Request license...");
    }

    static void CheckUpdate()
    {
        if (random.Next(100) >= 50)
            throw new InternetException("Отсутствует подключение к интернету");
        Console.WriteLine("Check Update...");
    }

    static void DownloadUpdate()
    {
        if (random.Next(100) >= 50)
            throw new InternetException("Отсутствует подключение к интернету");
        Console.WriteLine("Download update...");
    }

    static void SetupMenus()
    {
        Console.WriteLine("Setup menus...");
    }

    static void DisplayWelcome()
    {
        Console.WriteLine("Display welcome...");
    }
    static void HideSplash()
    {
        Console.WriteLine("Hide splash...");
    }

    static void Main(string[] args)
    {
        TaskContinuationOptions taskContinuationOptions = TaskContinuationOptions.NotOnFaulted;

        Task splashTask = Task.Factory.StartNew(ShowSplash);

        Task checkLicenseTask = splashTask.ContinueWith((task) =>
        {
            RequestLicense();
        }, taskContinuationOptions);

        Task setupMenuesTask = checkLicenseTask.ContinueWith((task) =>
        {
            SetupMenus();
        }, taskContinuationOptions);

        Task checkUpdateTask = splashTask.ContinueWith((task) =>
        {
            CheckUpdate();
        }, taskContinuationOptions);

        Task downloadUpdateTask = checkUpdateTask.ContinueWith((task) =>
        {
            DownloadUpdate();
        }, taskContinuationOptions);

        Task.Run(() =>
        {
            try
            {
                Task.WaitAll(new Task[] { checkLicenseTask, setupMenuesTask, checkUpdateTask, downloadUpdateTask });

                DisplayWelcome();
                HideSplash();

                Console.WriteLine("Success");
            }
            catch (AggregateException e)
            {
                Console.Write(e.InnerException.Message + ", ");
                Console.WriteLine($"Ошибка в методе {e.InnerException.TargetSite}");
                Console.WriteLine("Fail");
            }
        }).Wait();
    }
}