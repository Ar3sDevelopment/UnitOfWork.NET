using System;
using ClassBuilder.Classes;
using UnitOfWork.NET.NUnit.Classes;
namespace UnitOfWork.NET.NUnit.Mappers
{
	public class FloatMapper : DefaultMapper<DoubleValue, FloatValue>
	{
		public override FloatValue CustomMap(DoubleValue source, FloatValue destination)
		{
			var res = base.CustomMap(source, destination);

			res.Value = (float)source.Value;

			return res;
		}
	}
}

