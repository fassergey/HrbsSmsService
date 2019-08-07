using System;

namespace Models.SMS
{
    public class SmsUserDailyStatisticDTO
    {
        public Guid UserId { get; set; }
        public byte SmsCount { get; set; }
    }
}
