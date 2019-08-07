using DomainService.Concrete;
using Quartz;
using Quartz.Impl;
using SmsService;

namespace ConsoleTestStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            //// construct a scheduler factory
            //ISchedulerFactory schedFact = new StdSchedulerFactory();

            //// get a scheduler, start the schedular before triggers or anything else
            //IScheduler sched = schedFact.GetScheduler();
            //sched.Start();

            //// create job
            //IJobDetail job = JobBuilder.Create<GrabAgentsAndSendSmsJob>()
            //        .WithIdentity("job1", "group1")
            //        .Build();

            //// create trigger
            //ITrigger trigger = TriggerBuilder.Create()
            //    .WithIdentity("trigger1", "group1")
            //    .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
            //    .Build();

            //// Schedule the job using the job and trigger 
            //sched.ScheduleJob(job, trigger);



            GrabAgentsAndSendSmsJob job = new GrabAgentsAndSendSmsJob();
            //job.InitializeJob();
            //bool b = job.IsServiceAllowedToUse();
            //job.SendSms(null, "testc");



            //BotStorageRepo repo = new BotStorageRepo();
            //repo.ChangeAllowanceToSendErrorSms(true);
            //repo.ChangePermissionToEraseDailyData(false);
        }
    }
}
