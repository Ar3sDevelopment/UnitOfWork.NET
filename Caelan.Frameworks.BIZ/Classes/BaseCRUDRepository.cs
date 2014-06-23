using System;
using System.Linq;
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

		public virtual void Insert(TDTO dto)
		{
			All().Add(EntityBuilder().Build(dto));
		}

		public virtual void Update(TDTO dto)
		{
			var entity = All().FirstOrDefault(t => t.ID.Equals(dto.ID));

			EntityBuilder().Build(dto, ref entity);
		}

		public virtual void Delete(TDTO dto)
		{
			var entity = All().FirstOrDefault(t => t.ID.Equals(dto.ID));

			All().Remove(entity);
		}

		public virtual void Delete(TKey id)
		{
			Delete(Single(id));
		}
	}
}