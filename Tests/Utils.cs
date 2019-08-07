using Moq;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace Tests
{
    public static class Utils
    {
        public static void SetMockQueryable<T>(this Mock<DbSet<T>> mock, IQueryable<T> queryable) where T : class
        {
            mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());                        
        }

        public static void SetMockQueryable<T>(this Mock<ObjectResult<T>> mock, IQueryable<T> queryable) where T : class
        {            
            mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        }
    }
}
