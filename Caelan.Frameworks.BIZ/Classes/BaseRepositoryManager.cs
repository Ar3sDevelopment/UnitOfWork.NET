using System;
using System.Data.Entity;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public abstract class BaseRepositoryManager : IDisposable
    {
        protected abstract DbContext Context();

        internal DbSet<TEntity> GetDbSet<TEntity, TDTO, TKey>(BaseRepository<TEntity, TDTO, TKey> repository)
            where TEntity : class, IEntity<TKey>, new()
            where TDTO : class, IDTO<TKey>, new()
            where TKey : IEquatable<TKey>
        {
            return repository.DbSetFuncGetter().Invoke(Context());
        }

        public int SaveChanges()
        {
            return Context().SaveChanges();
        }

        public void Dispose()
        {
            //Context.Dispose();
        }
    }
}
