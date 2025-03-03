﻿using System.Linq.Expressions;

namespace MeetingApp.Core.IRepositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);

        void Remove(TEntity entity);

        void RemoveRange(List<TEntity> list);

        TEntity Update(TEntity entity);
    }
}