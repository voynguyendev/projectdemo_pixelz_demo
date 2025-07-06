using DemoProject.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Domain.Interfaces
{
    public interface IRepository<TEntity, TEntityKey> where TEntity : class
    {
        void Insert(List<TEntity> entities, bool isSave = true);
        TEntity Insert(TEntity entity, bool isSave = true);

        TEntity Update(TEntity entity, bool isSave = true);

        void Delete(TEntity entity, bool isSave = true);

        void Delete(List<TEntity> entities, bool isSave = true);


        IQueryable<TEntity> Query();

        Task SaveChangesAsync(CancellationToken ct);

        Task<bool> CheckByPredicateAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);



        Task<TEntity> GetOneByPredicateAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> customQuery = null,
          CancellationToken ct = default);

          Task<List<TEntity>> GetManyByPredicateAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> customQuery = null,
           CancellationToken ct = default);



        Task<PageListDTO<TEntity>> GetManyByPredicatePagingListAsync(
          Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IQueryable<TEntity>> customQuery = null,
            int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
      

        Task<int> GetCountPredicateAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    }
}
