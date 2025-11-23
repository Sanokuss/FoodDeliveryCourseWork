using System.Linq.Expressions;

namespace CourseWork.Repositories
{
    public interface IRepository<T> where T : class
    {
        T? Get(Expression<Func<T, bool>> filter);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}

