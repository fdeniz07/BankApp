using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Core.Repositories;

public class EfRepositoryBase<TEntity, TContext> : IAsyncRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    protected readonly TContext Context;

    public EfRepositoryBase(TContext context)
    {
        Context = context;
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedDate = DateTimeOffset.UtcNow;
        await Context.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        foreach (TEntity entity in entities)
            entity.CreatedDate = DateTimeOffset.UtcNow;

        await Context.AddRangeAsync(entities, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query();

        if (!withDeleted)
            query = query.Where(e => !e.DeletedDate.HasValue);

        if (predicate is not null)
            query = query.Where(predicate);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(
        TEntity entity,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        await SetEntityAsDeleted(entity, permanent, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        await SetEntityAsDeleted(entities, permanent, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query();

        if (!enableTracking)
            query = query.AsNoTracking();

        if (include is not null)
            query = include(query);

        if (!withDeleted)
            query = query.Where(e => !e.DeletedDate.HasValue);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Query();

        if (!enableTracking)
            query = query.AsNoTracking();

        if (include is not null)
            query = include(query);

        if (!withDeleted)
            query = query.Where(e => !e.DeletedDate.HasValue);

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            return await orderBy(query).ToPaginateAsync(index, size, from: 0, cancellationToken);

        return await query.ToPaginateAsync(index, size, from: 0, cancellationToken);
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedDate = DateTimeOffset.UtcNow;
        Context.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        foreach (TEntity entity in entities)
            entity.UpdatedDate = DateTimeOffset.UtcNow;

        Context.UpdateRange(entities);
        await Context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    protected IQueryable<TEntity> Query() => Context.Set<TEntity>();

    protected async Task SetEntityAsDeleted(
        TEntity entity,
        bool permanent,
        CancellationToken cancellationToken = default)
    {
        if (!permanent)
        {
            entity.DeletedDate = DateTimeOffset.UtcNow;
            Context.Update(entity);
            return;
        }

        Context.Remove(entity);
        await Task.CompletedTask;
    }

    protected async Task SetEntityAsDeleted(
        ICollection<TEntity> entities,
        bool permanent,
        CancellationToken cancellationToken = default)
    {
        if (!permanent)
        {
            foreach (TEntity entity in entities)
                entity.DeletedDate = DateTimeOffset.UtcNow;

            Context.UpdateRange(entities);
            return;
        }

        Context.RemoveRange(entities);
        await Task.CompletedTask;
    }
}
