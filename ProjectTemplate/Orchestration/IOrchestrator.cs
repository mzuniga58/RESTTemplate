using $safeprojectname$.Services;
using System.Threading.Tasks;
$if$ ($userql$ == True)using Rql.Services;
$endif$
namespace $safeprojectname$.Orchestration
{
	///	<summary>
	///	The IOrchestrator interface, derrived from <see cref="IRqlOrchestrator"/>. The Rql orchestrator 
    ///	provides means for the developer to read, add, update and delete items from the datastore using RQL.
	///	</summary>
    ///	<remarks>The developer is free to add new functions to the orchestrator, in order to accomplish orchestrations
    ///	combining the base functions, or to invent entirely new functions, as needs arise.</remarks>
	public interface IOrchestrator$if$ ($userql$ == True) : IRqlOrchestrator$endif$
	{
        /// <summary>
        /// The <see cref="HttpRequest"/> associated with this instance
        /// </summary>
        HttpRequest? Request { get; set; }
}
}