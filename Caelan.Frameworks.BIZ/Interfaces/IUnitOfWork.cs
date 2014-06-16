using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caelan.Frameworks.BIZ.Classes;

namespace Caelan.Frameworks.BIZ.Interfaces
{
    public interface IUnitOfWork
    {
        int SaveChanges();

        T GetRepository<T>() where T : BaseRepository;
    }
}
