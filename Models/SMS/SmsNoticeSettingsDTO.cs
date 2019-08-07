using System;

namespace Models.SMS
{
    public class SmsNoticeSettingsDTO
    {
        public bool Active { get; set; }
        public byte SmsCount { get; set; }
        public short MinutesInactiveForFirstSms { get; set; }
        public short MinutesInactiveForSecondSms { get; set; }
        public TimeSpan StartDailyTime { get; set; }
        public TimeSpan EndDailyTime { get; set; }
        public bool AllowToEraseDailyData { get; set; }
        public short FirstSmsTimeDeviation { get; set; }
        public short SecondSmsTimeDeviation { get; set; }
        public string SmsText { get; set; }
        public string DutyAdminPhone { get; set; }
        public string DeveloperPhone { get; set; }
        public bool FlagServiceErrorSmsSent { get; set; }

        public SmsNoticeSettingsDTO()
        {
            Active = false;
            SmsCount = 0;
            MinutesInactiveForFirstSms = 0;
            MinutesInactiveForSecondSms = 0;
            StartDailyTime = new TimeSpan(0);
            EndDailyTime = new TimeSpan(0);
            SmsText = "";
        }
    }
}
