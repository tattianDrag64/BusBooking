using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BusBooking.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext Context;
        internal DbSet<T> Items;

        public Repository(ApplicationDbContext context)
        {
            Context = context;
            Items = Context.Set<T>();
        }

        public void Add(T entity)
        {
            Items.Add(entity);
        }

        public T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? Items : Items.AsNoTracking();
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(includeProperty);
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = Items;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split([','], StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return [.. query];
        }

        public void Remove(T entity)
        {
            Items.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            Items.RemoveRange(entities);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            Items.AddRange(entities);
        }

        public void Update(T entity)
        {
            Items.Update(entity);
        }

        public T? GetById(Guid id)
        {
            return Items.FirstOrDefault(e => e.Id == id);
        }
    }
}