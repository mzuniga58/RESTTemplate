using $safeprojectname$.Services;
using System.Threading.Tasks;
using Tense;
$if$ ($userql$ == True)using Tense.Rql;
$endif$
namespace $safeprojectname$.Orchestration
{
	///	<summary>
	///	The IOrchestrator interface. The orchestrator provides the means for the developer to read, add, update
    ///	and delete resources.
	///	</summary>
    ///	<remarks>The developer is free to add new functions to the orchestrator, in order to accomplish orchestrations
    ///	combining the base functions, or to invent entirely new functions, as needs arise.</remarks>
	public interface IOrchestrator : IDisposable
    {$if$ ($userql$ == True)
        #region Generic Operations
        /// <summary>
        /// Retrieves a single resource from the datastore according to the <see cref="RqlNode"/> filter
        /// </summary>
        /// <typeparam name="T">The type of resource to retrieve</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that restricts the query.</param>
        /// <returns>A single resource</returns>
        Task<T?> GetSingleResourceAsync<T>(RqlNode node) where T : class;

        /// <summary>
        /// Retrieves a collection of resources from the datastore according to the <see cref="RqlNode"/> filter
        /// </summary>
        /// <typeparam name="T">The type of resources to retrieve</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that filters the query.</param>
        /// <returns>A collection of resources of type T</returns>
        Task<PagedSet<T>> GetResourceCollectionAsync<T>(RqlNode node) where T : class;

        /// <summary>
        /// Adds a new resource to the collection of scopes
        /// </summary>
        /// <typeparam name="T">The type of resources to retrieve</typeparam>
        /// <param name="resource">The resource to add.</param>
        /// <returns>The newly created resource</returns>
        Task<T> AddResourceAsync<T>(T resource) where T : class;

        /// <summary>
        /// Updates one or many resources
        /// </summary>
        /// <typeparam name="T">The type of resources to retrieve.</typeparam>
        /// <param name="resource">The resource to update.</param>
        /// <param name="node">The <see cref="RqlNode"/> that restricts the update</param>
        Task UpdateResourceAsync<T>(T resource, RqlNode node) where T : class;

        /// <summary>
        /// Deletes one or many resources
        /// </summary>
        /// <typeparam name="T">The type of resources to retrieve</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that filters the query.</param>
        Task DeleteResourceAsync<T>(RqlNode node) where T : class;
        #endregion
    $endif$}
}