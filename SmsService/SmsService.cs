using NLog;
using Quartz;
using Quartz.Impl;
using System.ServiceProcess;

namespace SmsService
{
    public partial class SmsService : ServiceBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        //// get a scheduler, start the schedular before triggers or anything else
        private static IScheduler sched;

        public SmsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _logger.Info("Service is started");
            //// construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            sched.Start();

            // create job
            IJobDetail job = JobBuilder.Create<GrabAgentsAndSendSmsJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

            // create trigger
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
                .Build();

            // Schedule the job using the job and trigger 
            sched.ScheduleJob(job, trigger);
        }

        protected override void OnStop()
        {
            _logger.Info("Service is stopped");
            sched.Shutdown();
        }
    }
}
