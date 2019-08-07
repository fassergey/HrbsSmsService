using Domain;
using DomainService.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Models.SMS;
using DomainService.Extensions;

namespace DomainService.Concrete
{
    public sealed class BotStorageRepo : IBotStorageRepo
    {
        private BotStorageEntities context;

        public BotStorageRepo(BotStorageEntities context = null)
        {
            this.context = context ?? new BotStorageEntities();
        }

        #region SmsNoticeSettings
        public SmsNoticeSettingsDTO GetSmsSettings()
        {
            SmsNoticeSettingsDTO result = new SmsNoticeSettingsDTO();

            if (context.SMSNoticeSettings.Any(x => x.Active))
            {
                result = context.SMSNoticeSettings.Where(x => x.Active).Select(x => new SmsNoticeSettingsDTO
                {
                    Active = x.Active,
                    SmsCount = x.SMSCount,
                    MinutesInactiveForFirstSms = x.FirstSendTime,
                    MinutesInactiveForSecondSms = x.SecondSendTime,
                    StartDailyTime = x.StartTime,
                    EndDailyTime = x.EndTime,
                    AllowToEraseDailyData = x.AllowToEraseDailyData,
                    FirstSmsTimeDeviation = x.FirstSmsTimeDeviation,
                    SecondSmsTimeDeviation = x.SecondSmsTimeDeviation,
                    SmsText = x.SmsText,
                    DeveloperPhone = x.DeveloperPhone,
                    DutyAdminPhone = x.DutyAdminPhone,
                    FlagServiceErrorSmsSent = x.FlagServiceErrorSmsSent
                }).First();
            }

            return result;
        }

        public void ChangePermissionToEraseDailyData(bool permission)
        {
            if (context.SMSNoticeSettings.Any(x => x.Active))
            {
                SMSNoticeSettings dbEntry = context.SMSNoticeSettings.First(x => x.Active);
                dbEntry.AllowToEraseDailyData = permission;
                context.Entry(dbEntry).State = System.Data.Entity.EntityState.Modified;

                if (dbEntry.StartTime == new TimeSpan(0) || dbEntry.EndTime == new TimeSpan(0))
                    throw new Exception("ChangePermissionToEraseDailyData: " + Environment.StackTrace);

                context.SaveChanges();
            }
        }

        public void ChangeAllowanceToSendErrorSms(bool allowance)
        {
            if (context.SMSNoticeSettings.Any(x => x.Active))
            {
                SMSNoticeSettings dbEntry = context.SMSNoticeSettings.First(x => x.Active);
                dbEntry.FlagServiceErrorSmsSent = allowance;
                context.Entry(dbEntry).State = System.Data.Entity.EntityState.Modified;

                if (dbEntry.StartTime == new TimeSpan(0) || dbEntry.EndTime == new TimeSpan(0))
                    throw new Exception("ChangeAllowanceToSendErrorSms: " + Environment.StackTrace);

                context.SaveChanges();
            }
        }
        #endregion SmsNoticeSettings


        #region SmsSendingQueue
        public IEnumerable<SmsSendingQueueDTO> GetUsersInQueue()
        {
            IEnumerable<SmsSendingQueueDTO> result = new SmsSendingQueueDTO[] { };

            if (context.SMSSendingQueue.Any())
            {
                result = context.SMSSendingQueue.Select(x => new SmsSendingQueueDTO
                {
                    UserId = x.UserId,
                    TimeFirstSmsSent = x.TimeFirstSmsSent.HasValue,
                    TimeSecondSmsSent = x.TimeSecondSmsSent.HasValue
                });
            }

            return result;
        }

        /// <summary>
        /// add users whome first sms was sent
        /// </summary>
        /// <param name="agents"></param>
        /// <returns></returns>
        public TimeSpan AddAgentsInSmsQueue(IEnumerable<AgentActivityDTO> agents)
        {
            TimeSpan time = new TimeSpan(0);

            if (!agents.IsNullOrEmpty())
            {
                HashSet<Guid> userIds = new HashSet<Guid>(agents.Select(x => x.UserId));
                HashSet<Guid> allUserIdsInQueue = new HashSet<Guid>(context.SMSSendingQueue.Select(x => x.UserId).Distinct().ToArray());
                HashSet<Guid> userIdsNotInQueue = new HashSet<Guid>();
                if (userIds.Any(x => !allUserIdsInQueue.Contains(x))) userIdsNotInQueue = new HashSet<Guid>(userIds.Where(x => !allUserIdsInQueue.Contains(x)));
                               

                if (!userIdsNotInQueue.IsNullOrEmpty())
                {
                    time = DateTime.Now.TimeOfDay;

                    foreach (var userId in userIdsNotInQueue)
                    {
                        SMSSendingQueue dbEntry = new SMSSendingQueue()
                        {
                            UserId = userId
                        };
                        dbEntry.TimeFirstSmsSent = time;
                        SetEntityStateAdded<SMSSendingQueue>(dbEntry);
                    }
                }

                context.SaveChanges();
            }

            return time;
        }

        /// <summary>
        /// udate users whome second sms was sent
        /// </summary>
        /// <param name="agents"></param>
        /// <returns></returns>
        public TimeSpan UpdateAgentsInSmsQueue(IEnumerable<AgentActivityDTO> agents)
        {
            TimeSpan time = new TimeSpan(0);

            var agentActivityDtos = agents as AgentActivityDTO[] ?? agents.ToArray();
            if (!agentActivityDtos.IsNullOrEmpty())
            {
                HashSet<Guid> userIds = new HashSet<Guid>(agentActivityDtos.Select(x => x.UserId));
                HashSet<Guid> allUserIdsInQueue = new HashSet<Guid>(context.SMSSendingQueue.Select(x => x.UserId).Distinct());
                HashSet<Guid> userIdsInQueue = new HashSet<Guid>();
                if (userIds.Any(x => allUserIdsInQueue.Contains(x))) userIdsInQueue = new HashSet<Guid>(userIds.Where(x => allUserIdsInQueue.Contains(x)));

                time = DateTime.Now.TimeOfDay;

                if (!userIdsInQueue.IsNullOrEmpty())
                {
                    foreach (var userId in userIdsInQueue)
                    {
                        SMSSendingQueue dbEntry = context.SMSSendingQueue.FirstOrDefault(x => x.UserId == userId);
                        if (dbEntry != null)
                        {
                            dbEntry.TimeSecondSmsSent = time;
                            SetEntityStateModified<SMSSendingQueue>(dbEntry);
                        }
                    }
                }

                context.SaveChanges();
            }

            return time;
        }

        // for testing
        public void SetEntityStateModified<T>(T entry) where T : class
        {
            context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
        }

        // for testing
        public void SetEntityStateAdded<T>(T entry) where T : class
        {
            context.Entry(entry).State = System.Data.Entity.EntityState.Added;
        }

        public void RemoveAgentsFromQueue(IEnumerable<Guid> userIds)
        {
            var enumerable = userIds as Guid[] ?? userIds.ToArray();
            if (!enumerable.IsNullOrEmpty())
            {
                HashSet<Guid> hsUserIds = new HashSet<Guid>(enumerable);

                if (context.SMSSendingQueue.Any(x => hsUserIds.Contains(x.UserId)))
                {
                    var entriesToDelete = context.SMSSendingQueue.Where(x => hsUserIds.Contains(x.UserId)).ToArray();
                    foreach (var item in entriesToDelete)
                    {
                        context.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }

                    context.SaveChanges();
                }
            }
        }

        #endregion SmsSendingQueue


        public Dictionary<Guid, byte> GetUsersDailyStatistic(IEnumerable<Guid> users)
        {
            Dictionary<Guid, byte> result = new Dictionary<Guid, byte>();

            if (context.SMSUserDayStatistic.Any(x => x.Date == DateTime.UtcNow.Date))
            {
                result = context.SMSUserDayStatistic.Where(x => x.Date == DateTime.UtcNow.Date).ToDictionary(x => x.UserId, x => x.MessageCount);
            }

            return result;
        }

        public IEnumerable<string> GetPhoneNumbers(IEnumerable<Guid> userIds)
        {
            IEnumerable<string> phones = new string[] { };

            var enumerable = userIds as Guid[] ?? userIds.ToArray();
            if (!enumerable.IsNullOrEmpty())
            {
                HashSet<Guid> hsUserIds = new HashSet<Guid>(enumerable);

                if (context.aspnet_Users.Any(x => hsUserIds.Contains(x.UserId) && x.SendSms))
                {
                    phones = context.aspnet_Users.Where(x => hsUserIds.Contains(x.UserId) && x.SendSms).Select(x => x.Phone);
                }
            }

            return phones;
        }

        public void TruncateTable(string tableName)
        {
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.[" + tableName + "]");
        }
    }
}
