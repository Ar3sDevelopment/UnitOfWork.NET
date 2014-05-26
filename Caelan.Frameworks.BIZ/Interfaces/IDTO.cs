using System;

namespace Caelan.Frameworks.BIZ.Interfaces
{
	public interface IDTO
	{
	}

	public interface IDTO<out TKey> : IDTO where TKey : IEquatable<TKey>
	{
		TKey ID { get; }
	}
}
