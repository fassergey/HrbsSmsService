using Domain;
using Moq;
using System.Data.Entity;

namespace Tests
{
    public class FakeBotStorageEntities : BotStorageEntities
    {
        public Mock<DbSet<SMSNoticeSettings>> SmsNoticeSettingsMock = new Mock<DbSet<SMSNoticeSettings>>();
        public Mock<DbSet<SMSSendingQueue>> SmsSendingQueueMock = new Mock<DbSet<SMSSendingQueue>>();
        public Mock<DbSet<SMSUserDayStatistic>> SmsUserDayStatisticMock = new Mock<DbSet<SMSUserDayStatistic>>();

        public override DbSet<SMSNoticeSettings> SMSNoticeSettings
        {
            get { return SmsNoticeSettingsMock.Object; }
        }

        public override DbSet<SMSSendingQueue> SMSSendingQueue
        {
            get { return SmsSendingQueueMock.Object; }
        }

        public override DbSet<SMSUserDayStatistic> SMSUserDayStatistic
        {
            get { return SmsUserDayStatisticMock.Object; }
        }        
    }
}
