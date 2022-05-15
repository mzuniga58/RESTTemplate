using System.Data.SqlClient;
using System.Reflection;
using Tense;$if$ ($userql$ == True)
using Tense.Rql;$endif$$if$ ($userqldatabase$ == SQLServer)
using Tense.Rql.SqlServer;$endif$

namespace $safeprojectname$.Repositories
{
    ///	<summary>
    ///	The repository
    ///	</summary>
    public class Repository : IRepository
    {
        private readonly ILogger<Repository> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly TimeSpan _timeout;
        private readonly int _batchLimit;$if$ ($userql$ == True)
        private readonly RqlSqlGenerator _sqlGenerator;$endif$

        ///	<summary>
        ///	Instantiates the Repository
        ///	</summary>
        ///	<param name="logger">A generic interface for logging where the category name is derrived from the <see cref="Repository"/> name.
        ///	Generally used to enable activation of a named <see cref="ILogger"/> from dependency injection.</param>
        ///	<param name="configuration">The <see cref="IConfiguration"/> interface</param>
        public Repository(ILogger<Repository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            _timeout = _configuration.GetSection("ServiceSettings").GetValue<TimeSpan>("Timeout");
            _batchLimit = _configuration.GetSection("ServiceSettings").GetValue<int>("BatchLimit");$if$ ($userql$ == True)
            _sqlGenerator = new RqlSqlGenerator(_batchLimit);$endif$

            logger.LogTrace("Repository instantiated");
        }$if$ ($userql$ == True)

        #region Generic Functions
        /// <summary>
        /// Returns a single entity of type T
        /// </summary>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        public async Task<T?> GetSingleEntityAsync<T>(RqlNode node) where T : class
        {
            return (T?)await GetSingleEntityAsync(typeof(T), node);
        }

        /// <summary>
        /// Returns a single resource of type entityType
        /// </summary>
        /// <param name="entityType">The type of entity to retrieve</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        public async Task<object?> GetSingleEntityAsync(Type entityType, RqlNode node)
        {
            using var ctc = new CancellationTokenSource();

            var task = Task.Run(async () =>
            {
                _logger.LogTrace("[REPOSITORY] GetSingleResourceAsync");

                //  Generate the SQL Statement to get a single object filtered by the Rql Node
                var sqlStatement = _sqlGenerator.GenerateSelectSingle(entityType, node, out List<SqlParameter> parameters);

                _logger.BeginScope(sqlStatement.ToString());

                //  Open a connection to the database
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    //	We now have a SQL query that needs to be executed in order to get our object.
                    using var command = new SqlCommand(sqlStatement, connection);

                    //  Add any parameters that we may have
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    //  Create our reader
                    using var reader = await command.ExecuteReaderAsync(ctc.Token).ConfigureAwait(false);

                    //  Go get the data
                    if (await reader.ReadAsync(ctc.Token).ConfigureAwait(false))
                    {
                        //  Read the EApiResource model from the database
                        return await reader.GetObjectAsync(entityType, node, ctc.Token).ConfigureAwait(false);
                    }
                }

                return null;
            });

            if (await Task.WhenAny(task, Task.Delay(_timeout)).ConfigureAwait(false) != task)
            {
                ctc.Cancel();
                throw new InvalidOperationException("Task exceeded time limit.");
            }

            return task.Result;
        }

        /// <summary>
        /// Returns a collection of resources of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The collection of resources matching the query filters.</returns>
        public async Task<PagedSet<T>> GetEntityCollectionAsync<T>(RqlNode node) where T : class
        {
            return (PagedSet<T>)await GetEntityCollectionAsync(typeof(T), node);
        }

        /// <summary>
        /// Returns a collection of entities of type entityType
        /// </summary>
        /// <param name="entityType">The type of entities to retrieve.</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The collection of resources matching the query filters.</returns>
        public async Task<object> GetEntityCollectionAsync(Type entityType, RqlNode node)
        {
            using var ctc = new CancellationTokenSource();

            var task = Task.Run(async () =>
            {
                _logger.LogTrace("[REPOSITORY] GetResourceCollectionAsync");

                //  Construct a paged set
                var rxgeneric = typeof(PagedSet<>);
                var rx = rxgeneric.MakeGenericType(entityType);
                var results = Activator.CreateInstance(rx);

                if (results is null)
                    throw new Exception("Internal Server Error");

                var countProperty = results.GetType().GetRuntimeField("Count");
                var itemsProperty = results.GetType().GetRuntimeField("Items");
                var startProperty = results.GetType().GetRuntimeField("Start");
                var pageSizeProperty = results.GetType().GetRuntimeField("PageSize");

                if (countProperty is null || itemsProperty is null || startProperty is null || pageSizeProperty is null)
                    throw new Exception("Intenal Server Error");

                //  First, get the total number of recorreds in the set
                var sqlStatement = _sqlGenerator.GenerateCollectionCountStatement(entityType, node, out List<SqlParameter> countParameters);

                _logger.BeginScope(sqlStatement.ToString());

                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                int totalRecords = 0;

                using (var countcommand = new SqlCommand(sqlStatement, connection))
                {
                    foreach (var parameter in countParameters)
                    {
                        countcommand.Parameters.Add(parameter);
                    }

                    using var reader = await countcommand.ExecuteReaderAsync(ctc.Token).ConfigureAwait(false);

                    if (await reader.ReadAsync(ctc.Token).ConfigureAwait(false))
                    {
                        totalRecords = reader.GetInt32(0);
                        countProperty.SetValue(results, totalRecords);
                    }
                }

                _logger.LogTrace("[REPOSITORY] GetResourceCollectionAsync : total records in the set = {totalRecordsInSet}", totalRecords);

                sqlStatement = _sqlGenerator.GenerateResourceCollectionStatement(entityType, node, out List<SqlParameter> queryParameters);
                _logger.BeginScope(sqlStatement.ToString());

                using (var command = new SqlCommand(sqlStatement, connection))
                {
                    foreach (var parameter in countParameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    using var reader = await command.ExecuteReaderAsync(ctc.Token).ConfigureAwait(false);

                    var rlgeneric = typeof(List<>);
                    var rl = rlgeneric.MakeGenericType(entityType);
                    var collection = Activator.CreateInstance(rl);

                    if (collection is null)
                        throw new Exception("Internal server error");

                    var addMethod = collection.GetType().GetMethod("Add");
                    var toArrayMethod = collection.GetType().GetMethod("ToArray");

                    if (addMethod is null || toArrayMethod is null)
                        throw new Exception("Internal server error");

                    while (await reader.ReadAsync(ctc.Token).ConfigureAwait(false))
                    {
                        var obj = await reader.GetObjectAsync(entityType, node, ctc.Token).ConfigureAwait(false);

                        if (obj is not null)
                        {
                            var entity = Convert.ChangeType(obj, entityType);
                            addMethod.Invoke(collection, new object[] { entity });
                        }
                    }

                    var itemArray = toArrayMethod.Invoke(collection, null);

                    if (itemArray is null)
                        throw new Exception("Internal Server Error");

                    var itemLengthProperty = itemArray.GetType().GetProperty("Length");

                    if (itemLengthProperty is null)
                        throw new Exception("Internal Server Error");

                    itemsProperty.SetValue(results, itemArray);

                    RqlNode? limitClause = node.ExtractLimitClause();

                    if (limitClause == null)
                    {
                        startProperty.SetValue(results, 1);
                        pageSizeProperty.SetValue(results, itemLengthProperty.GetValue(itemArray));
                    }
                    else
                    {
                        if (limitClause.Count > 0)
                            startProperty.SetValue(results, limitClause.NonNullValue<int>(0));
                        else
                            startProperty.SetValue(results, 1);

                        if (limitClause.Count > 1)
                            pageSizeProperty.SetValue(results, limitClause.NonNullValue<int>(1));
                        else
                            pageSizeProperty.SetValue(results, _batchLimit);
                    }
                }

                return results;
            });

            if (await Task.WhenAny(task, Task.Delay(_timeout)).ConfigureAwait(false) != task)
            {
                ctc.Cancel();
                throw new InvalidOperationException("Task exceeded time limit.");
            }

            var collection = task.Result;

            if (collection is null)
                throw new Exception("Internal server error");

            return collection;
        }

        /// <summary>
        /// Adds a new resource of type T
        /// </summary>
        /// <param name="entityType">The type of entity to add</param>
        /// <param name="entity">The resource to add.</param>
        /// <returns>The newly created resource</returns>
        public async Task<object> AddEntityAsync(Type entityType, object entity)
        {
            using var ctc = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                var sqlStatement = _sqlGenerator.GenerateInsertStatement(entityType, entity, out List<SqlParameter> parameters, out PropertyInfo? identity);

                _logger.BeginScope(sqlStatement.ToString());
                _logger.LogTrace("[REPOSITORY] AddResourceAsync<{n}>", entityType.Name);

                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(sqlStatement, connection);
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                if (identity != null)
                {
                    using var reader = await command.ExecuteReaderAsync(ctc.Token).ConfigureAwait(false);
                    if (await reader.ReadAsync(ctc.Token).ConfigureAwait(false))
                    {
                        identity.SetValue(entity, await reader.GetFieldValueAsync<object>(0, ctc.Token).ConfigureAwait(false));
                        return entity;
                    }
                }
                else
                {
                    await command.ExecuteNonQueryAsync(ctc.Token).ConfigureAwait(false);
                    return entity;
                }

                throw new Exception("insert failed");
            });

            if (await Task.WhenAny(task, Task.Delay(_timeout)).ConfigureAwait(false) == task)
                return task.Result;

            ctc.Cancel();
            throw new InvalidOperationException("Task exceeded time limit.");

        }

        /// <summary>
        /// Adds a new resource of type T
        /// </summary>
        /// <param name="entity">The resource to add.</param>
        /// <returns>The newly created resource</returns>
        public async Task<T> AddEntityAsync<T>(T entity) where T : class
        {
            return (T)await AddEntityAsync(typeof(T), entity);
        }

        /// <summary>
        /// Updates a resource of type T
        /// </summary>
        /// <param name="entity">The resource to update.</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        public async Task UpdateEntityAsync<T>(T entity, RqlNode node) where T : class
        {
            await UpdateEntityAsync(typeof(T), entity, node);
        }

        /// <summary>
        /// Updates a resource of type T
        /// </summary>
        /// <param name="entityType">The type of entity to udpate</param>
        /// <param name="entity">The resource to update.</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        public async Task UpdateEntityAsync(Type entityType, object entity, RqlNode node)
        {
            using var ctc = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                var sqlStatement = _sqlGenerator.GenerateUpdateStatement(entityType, entity, node, out List<SqlParameter> parameters);

                _logger.BeginScope(sqlStatement.ToString());
                _logger.LogTrace("[REPOSITORY] UpdateResourceAsync<{n}>", entityType.Name);

                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(sqlStatement, connection);
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                await command.ExecuteNonQueryAsync(ctc.Token).ConfigureAwait(false);
                return;
            });

            if (await Task.WhenAny(task, Task.Delay(_timeout)).ConfigureAwait(false) == task)
                return;

            ctc.Cancel();
            throw new InvalidOperationException("Task exceeded time limit.");
        }

        /// <summary>
        /// Deletes an entity from the datastore
        /// </summary>
        /// <typeparam name="T">The type of resource to delete</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        public async Task DeleteEntityAsync<T>(RqlNode node)
        {
            await DeleteEntityAsync(typeof(T), node);
        }

        /// <summary>
        /// Deletes an entity from the datastore
        /// </summary>
        /// <param name="entityType">The type of resource to delete</param>
        /// <param name="node">The <see cref="RqlNode"/> that contains the filters for the query.</param>
        /// <returns>The single entity matching the query filters, or null if not found.</returns>
        public async Task DeleteEntityAsync(Type entityType, RqlNode node)
        {
            using var ctc = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                var sqlStatement = _sqlGenerator.GenerateDeleteStatement(entityType, node, out List<SqlParameter> parameters);

                _logger.BeginScope(sqlStatement.ToString());
                _logger.LogTrace("[REPOSITORY] DeleteResourceAsync<{n}>", entityType.Name);

                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(sqlStatement, connection);
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                await command.ExecuteNonQueryAsync(ctc.Token).ConfigureAwait(false);
                return;
            });

            if (await Task.WhenAny(task, Task.Delay(_timeout)).ConfigureAwait(false) == task)
                return;

            ctc.Cancel();
            throw new InvalidOperationException("Task exceeded time limit.");
        }

        #endregion$endif$

        #region Dispose Pattern
        private bool disposedValue;

        /// <summary>
        /// Called when the service is being disposed.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> when the service needs to dispose managed resources; <see langword="false"/> otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.LogTrace("Repository is being disposed.");
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Called when the service is being disposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
