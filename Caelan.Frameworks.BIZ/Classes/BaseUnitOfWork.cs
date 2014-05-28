using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public abstract class BaseUnitOfWork : DynamicObject, IDisposable
    {
        private readonly Dictionary<string, BaseRepository> _repositories;

        protected BaseUnitOfWork()
        {
            _repositories = new Dictionary<string, BaseRepository>();
            //_repositories = Assembly.GetCallingAssembly().GetReferencedAssemblies().OrderBy(t => t.Name).Select(Assembly.Load).SelectMany(assembly => assembly.GetTypes().Where(t => t.BaseType == typeof(BaseRepository))).Select(t => new KeyValuePair<string, BaseRepository>(t.Name.Replace("Repository", string.Empty), Activator.CreateInstance(t.MakeGenericType(t.GetGenericArguments()), this) as BaseRepository)).ToDictionary(t => t.Key, t => t.Value);
        }

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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_repositories.ContainsKey(binder.Name))
            {
                var repoType = Type.GetType(binder.Name + "Repository");

                if (repoType == null)
                {
                    result = false;
                    return false;
                }

                var repo = Activator.CreateInstance(repoType, this) as BaseRepository;

                if (repo == null)
                {
                    result = null;
                    return false;
                }

                var property = Context().GetType().GetProperty(binder.Name);

                if (property == null)
                {
                    result = null;
                    return false;
                }

                if (!_repositories.ContainsKey(binder.Name))
                    _repositories.Add(binder.Name, repo);
                else
                    _repositories[binder.Name] = repo;
            }

            result = _repositories[binder.Name];

            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _repositories.Keys;
        }
    }
}
