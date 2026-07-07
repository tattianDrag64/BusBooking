
using BusBookingDemo.Data;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BusBookingDemo.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext Context;
        internal DbSet<T> Items;

        public Repository(ApplicationDbContext context)
        {
            Context = context;
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

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
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
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
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
    }
}
