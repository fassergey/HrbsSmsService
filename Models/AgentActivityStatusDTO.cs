using System;

namespace Models
{
    public class AgentActivityDTO
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public short UpdatedMinutesAgo { get; set; }

        public byte CountTodaySmsSent { get; set; }
    }
}
