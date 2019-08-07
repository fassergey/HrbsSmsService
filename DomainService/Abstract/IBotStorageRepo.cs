using Models;
using Models.SMS;
using System;
using System.Collections.Generic;

namespace DomainService.Abstract
{
    public interface IBotStorageRepo
    {
        SmsNoticeSettingsDTO GetSmsSettings();
        void ChangePermissionToEraseDailyData(bool permission);
        void ChangeAllowanceToSendErrorSms(bool allowance);


        IEnumerable<SmsSendingQueueDTO> GetUsersInQueue();
        TimeSpan AddAgentsInSmsQueue(IEnumerable<AgentActivityDTO> agents);
        TimeSpan UpdateAgentsInSmsQueue(IEnumerable<AgentActivityDTO> agents);
        void RemoveAgentsFromQueue(IEnumerable<Guid> userIds);
        void SetEntityStateModified<T>(T entry) where T : class;


        Dictionary<Guid, byte> GetUsersDailyStatistic(IEnumerable<Guid> users);

        IEnumerable<string> GetPhoneNumbers(IEnumerable<Guid> userIds);

        void TruncateTable(string tableName);
    }
}
