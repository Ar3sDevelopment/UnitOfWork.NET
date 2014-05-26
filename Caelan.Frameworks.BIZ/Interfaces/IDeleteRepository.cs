namespace Caelan.Frameworks.BIZ.Interfaces
{
	interface IDeleteRepository<in TDTO>
	{
		void Delete(TDTO dto);
	}
}
