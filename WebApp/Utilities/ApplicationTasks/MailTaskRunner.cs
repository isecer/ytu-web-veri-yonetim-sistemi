using System;
using System.Threading;

namespace WebApp.Utilities.ApplicationTasks
{
    public class MailTaskRunner
    {
        private static Timer _timer;

        public static void Start()
        { 
            _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private static void ExecuteTask(object state)
        {
            StartMezuniyetOtoMailsAsync();
        }

        private static async void StartMezuniyetOtoMailsAsync()
        {
           
 
        }

        public static void Stop()
        {
            // Timer'ı durdurun
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Restart()
        {
            // Timer'ı durdurun ve tekrar başlatın
            _timer?.Change(TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        public static void Dispose()
        {
            // Timer'ı temizleyin
            _timer?.Dispose();
        }
    }

}