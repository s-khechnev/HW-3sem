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
        if (random.Next(100) >= 30)
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
        Task splashTask = Task.Factory.StartNew(ShowSplash);

        Task checkLicenseTask = splashTask.ContinueWith((task) =>
        {
            RequestLicense();
        }, TaskContinuationOptions.NotOnFaulted);

        Task setupMenuesTask = checkLicenseTask.ContinueWith((task) =>
        {
            SetupMenus();
        }, TaskContinuationOptions.NotOnFaulted);

        Task checkUpdateTask = splashTask.ContinueWith((task) =>
        {
            try
            {
                CheckUpdate();
            }
            catch (InternetException ex)
            {
                Console.WriteLine("CheckUpdate() error, " + ex.Message);
            }
        }, TaskContinuationOptions.NotOnFaulted);

        Task downloadUpdateTask = checkUpdateTask.ContinueWith((task) =>
        {
            try
            {
                DownloadUpdate();
            }
            catch (InternetException ex)
            {
                Console.WriteLine("DownloadUpdate() error, " + ex.Message);
            }

        }, TaskContinuationOptions.NotOnFaulted);

        Task.Factory.StartNew(() =>
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