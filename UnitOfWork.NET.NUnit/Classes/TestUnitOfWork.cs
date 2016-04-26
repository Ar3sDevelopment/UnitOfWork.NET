using System;
using UoW = UnitOfWork.NET.Classes.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
namespace UnitOfWork.NET.NUnit.Classes
{
	public class TestUnitOfWork : UoW
	{
		//public IntRepository Ints { get; set; }
		//public DoubleRepository Doubles{ get; set; }

		public override IEnumerable<T> Data<T>()
		{
			var dataType = typeof(T);
			var rand = new Random((int)DateTime.Now.Ticks);
			var count = rand.Next() % 10 + 1;
			if (dataType == typeof(IntValue))
				return Enumerable.Range(0, count).Select(t => new IntValue { Value = rand.Next() }).Cast<T>();
			else if (dataType == typeof(DoubleValue))
				return Enumerable.Range(0, count).Select(t => new DoubleValue { Value = rand.NextDouble() }).Cast<T>();
			else if (dataType == typeof(FloatValue))
				return Enumerable.Range(0, count).Select(t => new FloatValue { Value = rand.Next() }).Cast<T>();
			else return base.Data<T>();
		}
	}
}

