using System.Threading.Tasks;
using $safeprojectname$.Services;$if$ ($userql$ == True)
using Rql;
using Rql.Services;
using Rql.Models;$endif$

namespace $safeprojectname$.Repositories
{
	///	<summary>
	///	The interface to the repository layer
	///	</summary>
    public interface IRepository$if$ ($userql$ == True) : IRqlRepository$endif$
	{
	}
}
