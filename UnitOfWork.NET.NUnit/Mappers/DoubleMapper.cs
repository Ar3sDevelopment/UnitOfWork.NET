using System;
using ClassBuilder.Classes;
using UnitOfWork.NET.NUnit.Classes;
namespace UnitOfWork.NET.NUnit.Mappers
{
	public class DoubleMapper : DefaultMapper<FloatValue, DoubleValue>
	{
		public override DoubleValue CustomMap(FloatValue source, DoubleValue destination)
		{
			var res = base.CustomMap(source, destination);

			res.Value = (double)source.Value;

			return res;
		}
	}
}

