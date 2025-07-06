using DemoProject.Domain.DTOs;
using DemoProject.Domain.Entities;
using DemoProject.Domain.Helpers;
using DemoProject.Domain.Interfaces;
using DemoProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace DemoProject.Infrastructure.Repositories
{

    public class Repository<TEntity, TEntityKey>(DemoProjectContext db) : IRepository<TEntity, TEntityKey> where TEntity : BaseEntity<TEntityKey>
    {
        public IQueryable<TEntity> Query() => db.Set<TEntity>().AsQueryable();

        public void Insert(List<TEntity> entities, bool isSave = true)
        {
            db.Set<TEntity>().AddRange(entities);
            if (isSave)
                db.SaveChanges();

        }
        public TEntity Insert(TEntity entity, bool isSave = true)
        {
            var result = db.Set<TEntity>().Add(entity);
            if (isSave)
                db.SaveChanges();
            return result.Entity;
        }
        public TEntity Update(TEntity entity, bool isSave = true)
        {
            var result = db.Set<TEntity>().Update(entity);
            if (isSave)
                db.SaveChanges();
            return result.Entity;
        }
        public void Delete(TEntity entity, bool isSave = true)
        {
            db.Set<TEntity>().Remove(entity);
            if (isSave)
                db.SaveChanges();
        }
        public void Delete(List<TEntity> entities, bool isSave = true)
        {
            db.Set<TEntity>().RemoveRange(entities);
            if (isSave)
                db.SaveChanges();
        }
        public async Task<TEntity> GetOneByPredicateAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>  customQuery=null,
           CancellationToken ct = default)
        {
            var query = ApplyPredicateFilterAndPredicateOrder(predicate);
            if(customQuery!=null)
                customQuery(Query());
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<List<TEntity>> GetManyByPredicateAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>>  customQuery=null,
           CancellationToken ct = default)
        {
            var query = ApplyPredicateFilterAndPredicateOrder(predicate);
            if (customQuery != null)
                customQuery(Query());
            return await query.ToListAsync(ct);
        }


        public async Task<PageListDTO<TEntity>> GetManyByPredicatePagingListAsync(
         Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> customQuery=null,
           int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var query = ApplyPredicateFilterAndPredicateOrder(predicate);
            if (customQuery != null)
                customQuery(Query());
            return await query.ToPagingAsync(pageNumber, pageSize, ct);
        }

        public async Task<bool> CheckByPredicateAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return await db.Set<TEntity>().AnyAsync(predicate, ct);
        }
        public async Task<int> GetCountPredicateAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        {
            return await db.Set<TEntity>().Where(predicate).CountAsync(ct);
        }


        public void SaveChanges()
        {
            db.SaveChanges();
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await db.SaveChangesAsync(ct);
        }
        private IQueryable<TEntity> ApplyPredicateFilterAndPredicateOrder(Expression<Func<TEntity, bool>> predicateFilter)
        {
            var query = Query().Where(predicateFilter);
            return query;
        }

    }
}
