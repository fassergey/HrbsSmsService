using DomainService.Abstract;
using DomainService.Extensions;
using Microsoft.Practices.Unity;
using Models;
using Models.SMS;
using NLog;
using Quartz;
using SmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SmsService
{
    public class GrabAgentsAndSendSmsJob : IJob
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static IUnityContainer container = UnityConfig.GetConfiguredContainer();
        private static IHorseRacingDbRepo horseRacingDbRepo = container.Resolve<IHorseRacingDbRepo>();
        private static IBotStorageRepo botStorageRepo = container.Resolve<IBotStorageRepo>();

        #region Variables
        // 15-minutes sms will be sent when updatedTime is in range {14-16} minutes
        private short firstSmsMinLimit;
        private short firstSmsMaxLimit;

        // 1-hours sms will be sent when updatedTime is in range {55-65} minutes
        private short secondSmsMinLimit;
        private short secondSmsMaxLimit;

        private SmsNoticeSettingsDTO smsSettings;
        #endregion Variables

        public void Execute(IJobExecutionContext context)
        {
            _logger.Info("Job Execute method was called");

            try
            {
                InitializeJob();

                bool isAllowed = IsServiceAllowedToUse();
                _logger.Info("Job is allowed : " + isAllowed);

                if (isAllowed)
                {
                    if (!smsSettings.AllowToEraseDailyData) botStorageRepo.ChangePermissionToEraseDailyData(true);
                    //if (smsSettings.FlagServiceErrorSmsSent) botStorageRepo.ChangeAllowanceToSendErrorSms(false);

                    IEnumerable<AgentActivityDTO> allAgents = horseRacingDbRepo.GetAgentsStatuses();
                    _logger.Info("Available agents : " + allAgents?.Count());

                    if (!allAgents.IsNullOrEmpty())
                    {
                        // get all active agents and remove them from tbl SmsSendingQueue
                        IEnumerable<Guid> allActiveAgentIds = allAgents.Where(x => x.UpdatedMinutesAgo < firstSmsMinLimit).Select(x => x.UserId).ToArray();
                        botStorageRepo.RemoveAgentsFromQueue(allActiveAgentIds);

                        // get inactive agents for first sms
                        IEnumerable<AgentActivityDTO> inactiveAgentsFirst = GetInactiveAgents(allAgents, firstSmsMinLimit, firstSmsMaxLimit, true).ToArray();
                        string message = string.Format($"Got {inactiveAgentsFirst.Count()} inactive agents for the first sms: {string.Join(", ", inactiveAgentsFirst.Select(x => x.UserName))}");
                        _logger.Info(message);
                        ProcessInactiveAgents(inactiveAgentsFirst, true);

                        // get inactive agents for second sms
                        IEnumerable<AgentActivityDTO> inactiveAgentsSecond = GetInactiveAgents(allAgents, secondSmsMinLimit, secondSmsMaxLimit, false).ToArray();
                        message = string.Format($"Got {inactiveAgentsSecond.Count()} inactive agents for the second sms: {string.Join(", ", inactiveAgentsSecond.Select(x => x.UserName))}");
                        _logger.Info(message);
                        ProcessInactiveAgents(inactiveAgentsSecond, false);
                    }
                }
                else
                {
                    _logger.Info("Service is not allowed to use. AllowToEraseDailyData = " + smsSettings.AllowToEraseDailyData);

                    // from 01:00 till 02:00 erase daily data. Called only once
                    if (DateTime.Now.TimeOfDay > new TimeSpan(1, 0, 0) && DateTime.Now.TimeOfDay < new TimeSpan(2, 0, 0) && smsSettings.AllowToEraseDailyData)
                    {
                        botStorageRepo.TruncateTable("SMSUserDayStatistic");
                        _logger.Info("Table SMSUserDayStatistic was truncated");

                        botStorageRepo.TruncateTable("SMSSendingQueue");
                        _logger.Info("Table SMSSendingQueue was truncated");

                        botStorageRepo.ChangePermissionToEraseDailyData(false);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!smsSettings.FlagServiceErrorSmsSent)
                {
                    if (!string.IsNullOrEmpty(smsSettings.DeveloperPhone)) SendSms(new[] { smsSettings.DeveloperPhone }, "Error occured in the SmsService");

                    //botStorageRepo.ChangeAllowanceToSendErrorSms(true);
                    _logger.Error(ex);
                }
            }
        }

        public void InitializeJob()
        {
            smsSettings = botStorageRepo.GetSmsSettings();

            firstSmsMinLimit = (short)(smsSettings.MinutesInactiveForFirstSms - smsSettings.FirstSmsTimeDeviation);
            firstSmsMaxLimit = (short)(smsSettings.MinutesInactiveForFirstSms + smsSettings.FirstSmsTimeDeviation);
            secondSmsMinLimit = (short)(smsSettings.MinutesInactiveForSecondSms - smsSettings.SecondSmsTimeDeviation);
            secondSmsMaxLimit = (short)(smsSettings.MinutesInactiveForSecondSms + smsSettings.SecondSmsTimeDeviation);
        }

        private bool IsServiceAllowedToUse()
        {
            bool allowed = true;
            TimeSpan presentTime = DateTime.Now.TimeOfDay;

            if (!smsSettings.Active) allowed = false;
            if (firstSmsMinLimit <= 0) allowed = false;
            if (secondSmsMinLimit <= 0) allowed = false;
            if (smsSettings.StartDailyTime > presentTime) allowed = false;
            if (smsSettings.EndDailyTime < presentTime) allowed = false;

            return allowed;
        }

        private IEnumerable<AgentActivityDTO> GetInactiveAgents(IEnumerable<AgentActivityDTO> agents, short minMinutesInactive, short maxMinutesInactive, bool first)
        {
            IEnumerable<AgentActivityDTO> inactiveAgents = new AgentActivityDTO[] { };
            HashSet<Guid> userIdsWhoAlreadyGotSms = new HashSet<Guid>();
            SmsSendingQueueDTO[] usersInQueue = botStorageRepo.GetUsersInQueue().ToArray();

            // check if users who had already got sms exist
            if (usersInQueue.Any())
            {
                // select usersIds who had already got the first sms
                if (first && usersInQueue.Any(x => x.TimeFirstSmsSent))
                {
                    userIdsWhoAlreadyGotSms = new HashSet<Guid>(usersInQueue.Where(x => x.TimeFirstSmsSent).Select(x => x.UserId));
                }
                // select usersIds who had already got the second sms
                else if (!first && usersInQueue.Any(x => x.TimeSecondSmsSent))
                {
                    userIdsWhoAlreadyGotSms = new HashSet<Guid>(usersInQueue.Where(x => x.TimeSecondSmsSent).Select(x => x.UserId));
                }
            }


            if (!agents.IsNullOrEmpty() && agents.Any(x => x.UpdatedMinutesAgo > minMinutesInactive &&
                                                           x.UpdatedMinutesAgo < maxMinutesInactive &&
                                                           x.CountTodaySmsSent < smsSettings.SmsCount &&            // limit of dayly sms-count doesn't overflow
                                                           !userIdsWhoAlreadyGotSms.Contains(x.UserId)))            // sms wasn't sent before
            {
                inactiveAgents = agents.Where(x => x.UpdatedMinutesAgo > minMinutesInactive &&
                                                   x.UpdatedMinutesAgo < maxMinutesInactive &&
                                                   x.CountTodaySmsSent < smsSettings.SmsCount &&
                                                   !userIdsWhoAlreadyGotSms.Contains(x.UserId));
            }

            return inactiveAgents;
        }

        private void ProcessInactiveAgents(IEnumerable<AgentActivityDTO> inactiveAgents, bool first)
        {
            if (!inactiveAgents.IsNullOrEmpty())
            {
                // add agents into queue for sending sms
                TimeSpan time = (first) ? botStorageRepo.AddAgentsInSmsQueue(inactiveAgents) : botStorageRepo.UpdateAgentsInSmsQueue(inactiveAgents);

                if (time > new TimeSpan(0))
                {
                    string agentnames = string.Join(", ", inactiveAgents.Select(x => x.UserName));
                    short minutes = inactiveAgents.First().UpdatedMinutesAgo;
                    string smsText = string.Format(smsSettings.SmsText, agentnames, minutes);
                    IEnumerable<string> phones = botStorageRepo.GetPhoneNumbers(inactiveAgents.Select(x => x.UserId));
                    SendSms(phones, smsText);
                }
            }
        }

        private void SendSms(IEnumerable<string> phones, string smsText)
        {
            var phonesList = phones.Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (!string.IsNullOrEmpty(smsSettings.DutyAdminPhone)) phonesList.Add(smsSettings.DutyAdminPhone);
            phonesList = phonesList.Distinct().ToList();
            //phones = new string[] { "+380672881987" };//, "+380982843005", "+380662951759", "+380938908488", "+380965798193", "+380637196453" };

            if (!string.IsNullOrEmpty(smsText) && !phonesList.IsNullOrEmpty())
            {
                SmsRequest request = new SmsRequest
                {
                    app = new App()
                    {
                        application_id = "4734",
                        user_id = "8c67c09d-72cd-499d-bc99-2f3ee3b9679f"
                    },
                    messages = new Messages[] {
                        new Messages {
                            platform_id = "32768",
                            pattern_id = "17413",
                            param = new Params[] {
                                new Params {
                                    Key = "message",
                                    Value = smsText
                                }
                            },
                            devices = phonesList.ToArray()
                        }
                    }
                };
                var requestString = Newtonsoft.Json.JsonConvert.SerializeObject(request);

                var httpRequest = (HttpWebRequest)WebRequest.Create("http://backend.pushgate.mobi/SendNotification.svc/send");

                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json; charset=UTF-8";
                httpRequest.Accept = "json";
                byte[] bytedata = UTF8Encoding.UTF8.GetBytes(requestString);
                httpRequest.ContentLength = bytedata.Length;

                using (var requestStream = httpRequest.GetRequestStream())
                {
                    requestStream.Write(bytedata, 0, bytedata.Length);
                    requestStream.Close();
                }

                var response = (HttpWebResponse)httpRequest.GetResponse();
                WebHeaderCollection header = response.Headers;

                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                {
                    string responseText = reader.ReadToEnd();
                    _logger.Info("Sms request result: " + responseText);
                }
                /*
                 PNG server response fromat.
                    {
                          "gateway_message ":"",
                          "request_id":"34b552ef-5604-4b30-9952-2e30c49c25a5",
                           "request_time":"29.01.2013 16:27:51",
                           "status_code":"100"
                    }
                 */
            }
            else
            {
                if (!string.IsNullOrEmpty(smsText)) _logger.Info("List of phones is empty");
                else _logger.Info("Sms text is empty");
            }
        }
    }
}