using System;

namespace Models.SMS
{
    public class SmsSendingHistoryDTO
    {
        public Guid UserId { get; set; }
        public DateTimeOffset SendTime { get; set; }
        public string SendStatus { get; set; }
    }
}
