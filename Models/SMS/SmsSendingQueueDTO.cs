using System;

namespace Models.SMS
{
    public class SmsSendingQueueDTO
    {
        public Guid UserId { get; set; }
        public bool TimeFirstSmsSent { get; set; }
        public bool TimeSecondSmsSent { get; set; }
    }
}
