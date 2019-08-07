using Models;
using System.Collections.Generic;

namespace DomainService.Abstract
{
    public interface IHorseRacingDbRepo
    {
        IEnumerable<AgentActivityDTO> GetAgentsStatuses();
    }
}
