using System;
using System.IO;
using System.Timers;
using DasMulli.Win32.ServiceUtils;

namespace WindowsServiceCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var myService = new MyService();
            var serviceHost = new Win32ServiceHost(myService);
            File.AppendAllText(MyService.logPath, $"{Environment.NewLine}{DateTime.Now} state: {myService.IsRunning}");
            serviceHost.Run();
        }
    }

    class MyService : IWin32Service
    {
        private Timer timer = null;
        private void DoWork(object sender, ElapsedEventArgs e)
        {
            File.AppendAllText(MyService.logPath, $"{Environment.NewLine}{DateTime.Now} running: {IsRunning}");
        }

        public const string logPath = @"C:\code\WindowsServiceCore\WindowsServiceCore\bin\Debug\netcoreapp2.0\log.txt";

        public string ServiceName => "Test Service";

        public bool IsRunning { get; set; }

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            System.IO.File.AppendAllText(logPath, $"{Environment.NewLine}{DateTime.Now} start");
            IsRunning = true;
            timer = new Timer();
            timer.Interval = 5000;
            timer.Elapsed += new ElapsedEventHandler(DoWork);
            timer.Enabled = true;
        }

        public void Stop()
        {
            IsRunning = false;
            System.IO.File.AppendAllText(logPath, $"{Environment.NewLine}{DateTime.Now} stop");
        }
    }
}
