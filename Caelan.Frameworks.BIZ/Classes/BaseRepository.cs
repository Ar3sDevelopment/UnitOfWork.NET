using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Caelan.DynamicLinq.Classes;
using Caelan.DynamicLinq.Extensions;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public abstract class BaseRepository
    {
        protected readonly BaseUnitOfWork Manager;

        protected BaseRepository(BaseUnitOfWork manager)
        {
            Manager = manager;
        }

        protected dynamic GetDynamicUnitOfWork()
        {
            return Manager;
        }

        protected BaseUnitOfWork GetUnitOfWork()
        {
            return Manager;
        }

        protected T GetUnitOfWork<T>() where T : BaseUnitOfWork
        {
            return Manager as T;
        }
    }

    public abstract class BaseRepository<TEntity, TDTO, TKey> : BaseRepository
        where TEntity : class, IEntity<TKey>, new()
        where TDTO : class, IDTO<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected BaseRepository(BaseUnitOfWork manager)
            : base(manager)
        {
        }

        protected abstract Func<DbContext, DbSet<TEntity>> DbSetFunc();

        internal Func<DbContext, DbSet<TEntity>> DbSetFuncGetter()
        {
            return DbSetFunc();
        }

        protected virtual DbSet<TEntity> All()
        {
            return Manager.GetDbSet(this);
        }

        protected virtual IQueryable<TEntity> AllQueryable()
        {
            return All();
        }

        protected virtual IQueryable<TEntity> All(Expression<Func<TEntity, bool>> whereFunc)
        {
            return whereFunc != null ? All().Where(whereFunc) : All();
        }

        protected virtual BaseDTOBuilder<TEntity, TDTO> DTOBuilder()
        {
            return GenericBusinessBuilder.GenericDTOBuilder<TEntity, TDTO>();
        }

        protected virtual BaseEntityBuilder<TDTO, TEntity> EntityBuilder()
        {
            return GenericBusinessBuilder.GenericEntityBuilder<TDTO, TEntity>();
        }

        public virtual DataSourceResult<TDTO> All(int take, int skip, IEnumerable<Sort> sort, Filter filter, Expression<Func<TEntity, bool>> where = null)
        {
            var queryResult = All(where).OrderBy(t => t.ID).ToDataSourceResult(take, skip, sort, filter);

            var result = new DataSourceResult<TDTO>
            {
                Data = DTOBuilder().BuildList(queryResult.Data),
                Total = queryResult.Total
            };

            return result;
        }

        public virtual DataSourceResult<TDTO> AllFull(int take, int skip, IEnumerable<Sort> sort, Filter filter, Expression<Func<TEntity, bool>> where = null)
        {
            var queryResult = All(where).OrderBy(t => t.ID).ToDataSourceResult(take, skip, sort, filter);

            var result = new DataSourceResult<TDTO>
            {
                Data = DTOBuilder().BuildFullList(queryResult.Data),
                Total = queryResult.Total
            };

            return result;
        }

        public virtual TDTO Single(TKey id)
        {
            return DTOBuilder().BuildFull(All().FirstOrDefault(t => t.ID.Equals(id)));
        }
    }
}
