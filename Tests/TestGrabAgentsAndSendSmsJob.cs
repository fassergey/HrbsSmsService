using Models.SMS;
using NUnit.Framework;
using SmsService;
using System;
using System.Reflection;

namespace Tests
{
    [TestFixture]
    public class TestGrabAgentsAndSendSmsJob
    {
        private GrabAgentsAndSendSmsJob InitJobWithSmsSettings(SmsNoticeSettingsDTO smsSettings)
        {
            GrabAgentsAndSendSmsJob job = new GrabAgentsAndSendSmsJob();
            Type t = typeof(GrabAgentsAndSendSmsJob);
            FieldInfo settings = t.GetField("smsSettings", BindingFlags.NonPublic | BindingFlags.Instance);
            settings.SetValue(job, smsSettings);

            FieldInfo firstSmsLim = t.GetField("firstSmsMinLimit", BindingFlags.NonPublic | BindingFlags.Instance);
            firstSmsLim.SetValue(job, (short)(smsSettings.MinutesInactiveForFirstSms - smsSettings.FirstSmsTimeDeviation));

            FieldInfo secondSmsLim = t.GetField("secondSmsMinLimit", BindingFlags.NonPublic | BindingFlags.Instance);
            secondSmsLim.SetValue(job, (short)(smsSettings.MinutesInactiveForSecondSms - smsSettings.SecondSmsTimeDeviation));

            return job;
        }

        [Test]
        public void IsServiceAllowedToUse_True()
        {
            // Arrange
            SmsNoticeSettingsDTO smsSettings = new SmsNoticeSettingsDTO {
                Active = true,
                EndDailyTime = new TimeSpan(19, 0, 0),
                MinutesInactiveForFirstSms = 1,
                MinutesInactiveForSecondSms = 5,
                StartDailyTime = new TimeSpan(6, 10, 0)
            };

            GrabAgentsAndSendSmsJob job = InitJobWithSmsSettings(smsSettings);
            Type t = typeof(GrabAgentsAndSendSmsJob);

            // Act
            var allow = t.InvokeMember("IsServiceAllowedToUse",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic |
                BindingFlags.Public | BindingFlags.Instance,
                null, job, null);

            // Assert
            Assert.IsNotNull(allow);
            Assert.IsTrue((bool)allow);
        }

        [Test]
        public void IsServiceAllowedToUse_False()
        {
            // Arrange
            SmsNoticeSettingsDTO smsSettings = new SmsNoticeSettingsDTO
            {
                Active = true,
                EndDailyTime = new TimeSpan(19, 0, 0),
                MinutesInactiveForFirstSms = 1,
                MinutesInactiveForSecondSms = 5,
                StartDailyTime = new TimeSpan(10, 10, 0)
            };

            GrabAgentsAndSendSmsJob job = InitJobWithSmsSettings(smsSettings);
            Type t = typeof(GrabAgentsAndSendSmsJob);

            // Act
            var allow = t.InvokeMember("IsServiceAllowedToUse",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic |
                BindingFlags.Public | BindingFlags.Instance,
                null, job, null);

            // Assert
            Assert.IsNotNull(allow);
            Assert.IsFalse((bool)allow);
        }
    }
}