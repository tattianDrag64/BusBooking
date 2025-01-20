using BusBooking.Data;
using BusBooking.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext Context;
        internal DbSet<T> Items;

        public Repository(DbContext context)
        {
            Context = new BusBookingDbContext();
            Items = Context.Set<T>();
        }

        //public Repository(UnitOfWork uow)
        //{
        //    Context = (DbContext)uow.GetContext();
        //    Items = Context.Set<T>();
        //}

        public void Add(T entity)
        {
            Items.Add(entity);
        }

        public T Get(System.Linq.Expressions.Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? Items : Items.AsNoTracking();
            query = query.Where(filter);
            if(!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var includeProperty in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)) 
                    query = query.Include(includeProperty);
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public void Remove(T entity)
        {
            Items.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            Items.RemoveRange(entities);
        }
    }
}
