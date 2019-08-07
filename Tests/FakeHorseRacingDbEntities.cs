using Domain;
using Moq;
using System.Data.Entity.Core.Objects;

namespace Tests
{
    public class FakeHorseRacingDBEntities : HorseRacingDBEntities
    {
        public Mock<ObjectResult<UserActivityGet_Result>> UserActivityMock = new Mock<ObjectResult<UserActivityGet_Result>>();

        public override ObjectResult<UserActivityGet_Result> UserActivityGet()
        {
            return UserActivityMock.Object;
        }
    }
}
