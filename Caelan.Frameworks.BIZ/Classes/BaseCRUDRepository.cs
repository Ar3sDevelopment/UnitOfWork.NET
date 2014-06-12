using System;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public abstract class BaseCRUDRepository<TEntity, TDTO, TKey> : BaseRepository<TEntity, TDTO, TKey>, IInsertRepository<TDTO>, IUpdateRepository<TDTO>, IDeleteRepository<TDTO>
        where TEntity : class, IEntity<TKey>, new()
        where TDTO : class, IDTO<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected BaseCRUDRepository(BaseUnitOfWork manager)
            : base(manager)
        {
        }

        public abstract void Insert(TDTO dto);
        public abstract void Update(TDTO dto);
        public abstract void Delete(TDTO dto);

        public virtual void Delete(TKey id)
        {
            Delete(Single(id));
        }
    }
}