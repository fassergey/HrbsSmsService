using System.ServiceProcess;

namespace SmsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] servicesToRun = new ServiceBase[]
            {
                new SmsService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
