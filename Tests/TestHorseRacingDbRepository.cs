using Domain;
using DomainService.Concrete;
using Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class TestHorseRacingDbRepository
    {
        private readonly FakeHorseRacingDBEntities _fakeHorseRacingDBContext = new FakeHorseRacingDBEntities();
        private readonly FakeBotStorageEntities _fakeBotStorageContext = new FakeBotStorageEntities();

        //[SetUp]
        //public void Setup()
        //{
        //    IQueryable<UserActivityGet_Result> data = new List<UserActivityGet_Result>
        //    {
        //        new UserActivityGet_Result
        //        {
        //            UserId = new Guid("11111111-1111-1111-1111-111111111111"),
        //            ClientVersion = "",
        //            LastUpdateTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
        //            UserName = "",
        //            LocalTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
        //            UtcTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0))
        //        },
        //        new UserActivityGet_Result
        //        {
        //            UserId = new Guid("22222222-2222-2222-2222-222222222222"),
        //            ClientVersion = "",
        //            LastUpdateTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
        //            UserName = "",
        //            LocalTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
        //            UtcTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0))
        //        }
        //    }.AsQueryable();

        //    _fakeHorseRacingDBContext.UserActivityMock.SetMockQueryable(data);
        //}

        [Test]
        public void GetAgentsStatuses_WithEmptyUserId()
        {
            // arrange
            IQueryable<UserActivityGet_Result> data = new List<UserActivityGet_Result>
            {
                new UserActivityGet_Result
                {
                    UserId = null,
                    ClientVersion = "",
                    LastUpdateTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
                    UserName = "",
                    LocalTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
                    UtcTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0))
                }
            }.AsQueryable();
            _fakeHorseRacingDBContext.UserActivityMock.SetMockQueryable(data);

            IQueryable<SMSUserDayStatistic> data1 = new List<SMSUserDayStatistic>().AsQueryable();
            _fakeBotStorageContext.SmsUserDayStatisticMock.SetMockQueryable(data1);

            HorseRacingDbRepo repo = new HorseRacingDbRepo(_fakeHorseRacingDBContext, _fakeBotStorageContext);

            // act
            AgentActivityDTO[] result = repo.GetAgentsStatuses().ToArray();

            // assert
            Assert.AreEqual(default(Guid), result[0].UserId);
        }

        [Test]
        public void GetAgentsStatuses_UserIsInactiveFourMonthes()
        {
            // arrange
            IQueryable<UserActivityGet_Result> data = new List<UserActivityGet_Result>
            {
                new UserActivityGet_Result
                {
                    UserId = new Guid("11111111-1111-1111-1111-111111111111"),
                    ClientVersion = "",
                    LastUpdateTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
                    UserName = "",
                    LocalTime = new DateTimeOffset(2017, 02, 21, 0, 0, 0, new TimeSpan(0)),
                    UtcTime = new DateTimeOffset(2016, 10, 21, 0, 0, 0, new TimeSpan(0))
                }
            }.AsQueryable();
            _fakeHorseRacingDBContext.UserActivityMock.SetMockQueryable(data);

            IQueryable<SMSUserDayStatistic> data1 = new List<SMSUserDayStatistic> {
                new SMSUserDayStatistic {
                    UserId = new Guid("11111111-1111-1111-1111-111111111111"),
                    MessageCount = 1
                }
            }.AsQueryable();
            _fakeBotStorageContext.SmsUserDayStatisticMock.SetMockQueryable(data1);

            HorseRacingDbRepo repo = new HorseRacingDbRepo(_fakeHorseRacingDBContext, _fakeBotStorageContext);

            // act
            AgentActivityDTO[] result = repo.GetAgentsStatuses().ToArray();

            // assert
            Assert.AreEqual(short.MaxValue, result[0].UpdatedMinutesAgo);
            Assert.AreEqual(1, result[0].CountTodaySmsSent);
        }
    }
}
