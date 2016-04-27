using System;
using System.Collections.Generic;
using UnitOfWork.NET.Classes;
using UnitOfWork.NET.Interfaces;
using UnitOfWork.NET.NUnit.Classes;

namespace UnitOfWork.NET.NUnit.Repositories
{
    public class DoubleRepository : Repository<DoubleValue, FloatValue>
    {
        public DoubleRepository(IUnitOfWork manager) : base(manager)
        {
        }

        public IEnumerable<FloatValue> NewList()
        {
            Console.WriteLine("NewList");
            return AllBuilt();
        }
    }
}
