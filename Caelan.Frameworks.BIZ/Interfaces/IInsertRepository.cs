namespace Caelan.Frameworks.BIZ.Interfaces
{
	public interface IInsertRepository<in TDTO>
	{
		void Insert(TDTO dto);
	}
}
