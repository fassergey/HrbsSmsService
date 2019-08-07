using Domain;
using DomainService.Concrete;
using Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class TestBotStorageRepository
    {
        //private FakeBotStorageEntities _fakeDbContext = new FakeBotStorageEntities();
        private Mock<FakeBotStorageEntities> _fakeDbContext = new Mock<FakeBotStorageEntities>();
        private IEnumerable<AgentActivityDTO> agents;
        private IEnumerable<SMSUserDayStatistic> stat;

        [SetUp]
        public void Setup()
        {
            agents = new List<AgentActivityDTO> {
                new AgentActivityDTO {
                    UserId = default(Guid),
                    UserName = "Sergey",
                    CountTodaySmsSent = 2,
                    UpdatedMinutesAgo = 15
                }
            };

            stat = new SMSUserDayStatistic[] {
                new SMSUserDayStatistic
                {
                    UserId = new Guid("fb33939c-179d-4049-8a3c-30bbd8538304"),
                    Date = DateTime.Now.Date,
                    MessageCount = 2
                },
                new SMSUserDayStatistic
                {
                    UserId = new Guid("fb33939c-179d-4049-8a3c-30bbd8538304"),
                    Date = DateTime.Now.AddDays(-1).Date,
                    MessageCount = 3
                }
            };
        }

        [Test]
        public void AddAgentsInSmsQueue_NewUser_FirstSms()
        {
            // arrange
            List<SMSSendingQueue> data = new List<SMSSendingQueue>();
            _fakeDbContext.Setup(x => x.SMSSendingQueue).Returns(_fakeDbContext.Object.SmsSendingQueueMock.Object);
            _fakeDbContext.Object.SmsSendingQueueMock.SetMockQueryable(data.AsQueryable());
            
            _fakeDbContext.Object.SmsSendingQueueMock.Setup(m => m.Add(It.IsAny<SMSSendingQueue>()))
                                              .Callback(() =>
                                              {
                                                  data.AddRange(agents.Select(x => new SMSSendingQueue
                                                  {
                                                      UserId = x.UserId,
                                                      TimeSecondSmsSent = DateTime.Now.TimeOfDay
                                                  }));
                                              });

            BotStorageRepo repo = new BotStorageRepo(_fakeDbContext.Object);

            // act
            TimeSpan time = repo.AddAgentsInSmsQueue(agents);

            // assert
            _fakeDbContext.Object.SmsSendingQueueMock.Verify(x => x.Add(It.IsAny<SMSSendingQueue>()), Times.AtLeastOnce());
            Assert.AreEqual(default(Guid), _fakeDbContext.Object.SMSSendingQueue.First().UserId);
            Assert.AreNotEqual(default(TimeSpan), _fakeDbContext.Object.SMSSendingQueue.First().TimeSecondSmsSent);
        }

        [Test]
        public void AddAgentsInSmsQueue_ExistedUser_FirstSms()
        {
            // arrange
            List<SMSSendingQueue> data = new List<SMSSendingQueue> {
                new SMSSendingQueue
                {
                    Id = 1,
                    UserId = default(Guid),
                    TimeFirstSmsSent = new TimeSpan(1, 10, 0)
                }
            };
            _fakeDbContext.Setup(x => x.SMSSendingQueue)
                          .Returns(_fakeDbContext.Object.SmsSendingQueueMock.Object);
            _fakeDbContext.Object.SmsSendingQueueMock.SetMockQueryable(data.AsQueryable());
            _fakeDbContext.Setup(x => x.SaveChanges()).Callback(() => {
                data.First().TimeFirstSmsSent = DateTime.Now.TimeOfDay;
            });            

            Mock<BotStorageRepo> repoMock = new Mock<BotStorageRepo>(new object[] { _fakeDbContext.Object });
            repoMock.Setup(x => x.SetEntityStateModified<SMSSendingQueue>(It.IsAny<SMSSendingQueue>()));

            // act
            TimeSpan time = repoMock.Object.AddAgentsInSmsQueue(agents);

            // assert
            Assert.AreNotEqual(new TimeSpan(1, 10, 0), _fakeDbContext.Object.SmsSendingQueueMock.Object.First().TimeFirstSmsSent);
        }

        [Test]
        public void AddAgentsInSmsQueue_NewUser_SecondSms()
        {
            // arrange
            List<SMSSendingQueue> data = new List<SMSSendingQueue>();
            _fakeDbContext.Setup(x => x.SMSSendingQueue).Returns(_fakeDbContext.Object.SmsSendingQueueMock.Object);
            _fakeDbContext.Object.SmsSendingQueueMock.SetMockQueryable(data.AsQueryable());

            _fakeDbContext.Object.SmsSendingQueueMock.Setup(m => m.Add(It.IsAny<SMSSendingQueue>()))
                                              .Callback(() =>
                                              {
                                                  data.AddRange(agents.Select(x => new SMSSendingQueue
                                                  {
                                                      UserId = x.UserId,
                                                      TimeSecondSmsSent = DateTime.Now.TimeOfDay
                                                  }));
                                              });

            BotStorageRepo repo = new BotStorageRepo(_fakeDbContext.Object);

            // act
            TimeSpan time = repo.AddAgentsInSmsQueue(agents);

            // assert
            _fakeDbContext.Object.SmsSendingQueueMock.Verify(x => x.Add(It.IsAny<SMSSendingQueue>()), Times.AtLeastOnce());
            Assert.AreEqual(default(Guid), _fakeDbContext.Object.SMSSendingQueue.First().UserId);
            Assert.AreNotEqual(default(TimeSpan), _fakeDbContext.Object.SmsSendingQueueMock.Object.First().TimeSecondSmsSent);
        }

        [Test]
        public void AddAgentsInSmsQueue_ExistedUser_SecondSms()
        {
            // arrange
            List<SMSSendingQueue> data = new List<SMSSendingQueue> {
                new SMSSendingQueue
                {
                    Id = 1,
                    UserId = default(Guid),
                    TimeSecondSmsSent = new TimeSpan(1, 10, 0)
                }
            };
            _fakeDbContext.Setup(x => x.SMSSendingQueue)
                          .Returns(_fakeDbContext.Object.SmsSendingQueueMock.Object);
            _fakeDbContext.Object.SmsSendingQueueMock.SetMockQueryable(data.AsQueryable());
            _fakeDbContext.Setup(x => x.SaveChanges()).Callback(() => {
                data.First().TimeSecondSmsSent = DateTime.Now.TimeOfDay;
            });

            Mock<BotStorageRepo> repoMock = new Mock<BotStorageRepo>(new object[] { _fakeDbContext.Object });
            repoMock.Setup(x => x.SetEntityStateModified<SMSSendingQueue>(It.IsAny<SMSSendingQueue>()));

            // act
            TimeSpan time = repoMock.Object.AddAgentsInSmsQueue(agents);

            // assert
            Assert.AreNotEqual(new TimeSpan(1, 10, 0), _fakeDbContext.Object.SmsSendingQueueMock.Object.First().TimeSecondSmsSent);
        }

        [Test]
        public void GetUserDailyStat_Test()
        {
            // arrange
            _fakeDbContext.Setup(x => x.SMSUserDayStatistic).Returns(_fakeDbContext.Object.SmsUserDayStatisticMock.Object);
            _fakeDbContext.Object.SmsUserDayStatisticMock.SetMockQueryable(stat.AsQueryable());
            BotStorageRepo repo = new BotStorageRepo(_fakeDbContext.Object);
            byte expectedCount = stat.Where(x => x.Date == DateTime.UtcNow.Date).First().MessageCount;

            // act
            Dictionary<Guid, byte> res = repo.GetUsersDailyStatistic(new Guid[] { new Guid("fb33939c-179d-4049-8a3c-30bbd8538304") });

            // assert
            Assert.AreEqual(expectedCount, _fakeDbContext.Object.SMSUserDayStatistic.First().MessageCount);
        }
    }
}
