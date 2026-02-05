using System.Linq.Expressions;
using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T? Get(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            // Include Category and Restaurant for Product entities
            if (typeof(T) == typeof(Product))
            {
                var productDbSet = _db.Set<Product>();
                var products = productDbSet
                    .Include(p => p.Category)
                    .Include(p => p.Restaurant)
                    .ToList();
                return products.Cast<T>();
            }
            
            return dbSet.ToList();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter)
        {
            // Include Category and Restaurant for Product entities
            if (typeof(T) == typeof(Product))
            {
                var productDbSet = _db.Set<Product>();
                // For Product, we need to apply filter after loading
                // This is less efficient but works correctly
                var allProducts = productDbSet
                    .Include(p => p.Category)
                    .Include(p => p.Restaurant)
                    .ToList();
                var compiledFilter = filter.Compile();
                return allProducts.Where(p => compiledFilter((T)(object)p)).Cast<T>();
            }
            
            return dbSet.Where(filter).ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}

