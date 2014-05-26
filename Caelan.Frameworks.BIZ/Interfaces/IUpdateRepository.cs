namespace Caelan.Frameworks.BIZ.Interfaces
{
	interface IUpdateRepository<in TDTO>
	{
		void Update(TDTO dto);
	}
}
