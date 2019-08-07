using System.Collections.Generic;
using DomainService.Abstract;
using Models;
using Domain;
using System.Linq;
using System;

namespace DomainService.Concrete
{
    public class HorseRacingDbRepo : IHorseRacingDbRepo
    {
        HorseRacingDBEntities horseRacingDbContext;
        BotStorageEntities botStorageContext;

        public HorseRacingDbRepo(HorseRacingDBEntities horseRacingDbContext = null, BotStorageEntities botStorageContext = null)
        {
            this.botStorageContext = botStorageContext ?? new BotStorageEntities();
            this.horseRacingDbContext = horseRacingDbContext ?? new HorseRacingDBEntities();
        }

        public IEnumerable<AgentActivityDTO> GetAgentsStatuses()
        {
            AgentActivityDTO[] result = horseRacingDbContext.UserActivityGet().Select(x => new AgentActivityDTO
            {
                UserId = x.UserId ?? default(Guid),
                UserName = x.UserName,
                UpdatedMinutesAgo = GetMinutes(x.LocalTime, x.UtcTime, x.UserName),
                CountTodaySmsSent = 0
            }).ToArray();

            IEnumerable<SMSUserDayStatistic> statistic = botStorageContext.SMSUserDayStatistic.Select(x => x).AsEnumerable();

            foreach (var entry in statistic)
            {
                if (result.Any(x => x.UserId == entry.UserId))
                {
                    result.First(x => x.UserId == entry.UserId).CountTodaySmsSent = entry.MessageCount;
                }
            }

            return result;
        }

        private short GetMinutes(DateTimeOffset? time1, DateTimeOffset? time2, string name)
        {
            short result = 0;

            if (name == "TestVasil")
            { }

            if (time1.HasValue && time2.HasValue)
            {
                try
                {
                    result = (short)(time2.Value - time1.Value).TotalMinutes;
                    if (result < 0) result = short.MaxValue;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    result = short.MaxValue;
                }
            }

            return result;
        }
    }
}