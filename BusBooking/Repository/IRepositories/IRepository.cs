using System.Linq.Expressions;
using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        IEnumerable<T> GetPage(int skip, int take, Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        int Count(Expression<Func<T, bool>>? filter = null);
        T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void AddRange(IEnumerable<T> entities);
        T? GetById(Guid id);
    }
}