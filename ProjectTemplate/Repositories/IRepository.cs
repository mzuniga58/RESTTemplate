using System.Threading.Tasks;
using $safeprojectname$.Services;
using Tense;$if$ ($userql$ == True)
using Tense.Rql;$endif$

namespace $safeprojectname$.Repositories
{
	///	<summary>
	///	The interface to the repository layer
	///	</summary>
    public interface IRepository : IDisposable
	{$if$ ($userql$ == True)
        #region Generic functions
        /// <summary>
        /// Returns a single entity of type T
        /// </summary>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        Task<T?> GetSingleEntityAsync<T>(RqlNode node) where T : class;

        /// <summary>
        /// Returns a single entity of type entityType
        /// </summary>
        /// <param name="entityType">The type of entity to retrieve</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        Task<object?> GetSingleEntityAsync(Type entityType, RqlNode node);

        /// <summary>
        /// Returns a collection of entities of type T
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The collection of resources matching the query filters.</returns>
        Task<PagedSet<T>> GetEntityCollectionAsync<T>(RqlNode node) where T : class;

        /// <summary>
        /// Returns a collection of entities of type T
        /// </summary>
        /// <param name="entityType">The type of entity to retrieve.</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The collection of resources matching the query filters.</returns>
        Task<object> GetEntityCollectionAsync(Type entityType, RqlNode node);

        /// <summary>
        /// Adds a new resource of type T
        /// </summary>
        /// <param name="entityType">The type of entity to add</param>
        /// <param name="entity">The resource to add.</param>
        /// <returns>The newly created resource</returns>
        Task<object> AddEntityAsync(Type entityType, object entity);

        /// <summary>
        /// Adds a new resource of type To
        /// </summary>
        /// <param name="entity">The resource to add.</param>
        /// <returns>The newly created resource</returns>
        Task<T> AddEntityAsync<T>(T entity) where T : class;

        /// <summary>
        /// Updates a resource of type T
        /// </summary>
        /// <param name="entity">The resource to update.</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        Task UpdateEntityAsync<T>(T entity, RqlNode node) where T : class;

        /// <summary>
        /// Updates a resource of type T
        /// </summary>
        /// <param name="entityType">The type of entity to update</param>
        /// <param name="entity">The resource to update.</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        Task UpdateEntityAsync(Type entityType, object entity, RqlNode node);

        /// <summary>
        /// Deletes an entity from the datastore
        /// </summary>
        /// <param name="entityType">The type of resource to delete</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        Task DeleteEntityAsync(Type entityType, RqlNode node);

        /// <summary>
        /// Deletes an entity from the datastore
        /// </summary>
        /// <typeparam name="T">The type of resource to delete</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        Task DeleteEntityAsync<T>(RqlNode node);
        #endregion
	$endif$}
}
