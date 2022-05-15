using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Reflection;
using System.Threading.Tasks;$if$ ($databasetechnology != None)
using $safeprojectname$.Repositories;$endif$
using $safeprojectname$.Services;
using Tense;
$if$ ($userql$ == True)using Tense.Rql;
$endif$
namespace $safeprojectname$.Orchestration
{
	///	<summary>
	///	The ServiceOrchestrator
	///	</summary>
	public class Orchestrator : IOrchestrator
	{
		private readonly ILogger<Orchestrator> _logger;$if$ ($databasetechnology$ != None)
		private readonly IRepository _repository;$endif$
		private readonly IMapper _mapper;$if$ ($userql$ == True)
		private readonly Translator _translator;$endif$

        ///	<summary>
        ///	Initiates the Service Orchestrator
        ///	</summary>
        /// <param name="logger">A generic interface for logging where the category name is derrived from the <see cref="Orchestrator"/> name. 
        /// Generally used to enable activation of a named <see cref="ILogger"/> from dependency injection.</param>
        /// <param name="provider">Defines a mechanism for retrieving a service object; that is, an object that provides custom support to other objects.</param>$if$ ($databasetechnology$ != None)
        /// <param name="repository">The named <see cref="IRepository"/> interface to the repository layer. Used to perform Create, Read, Update, 
        /// Delete (CRUD) and other funtions involing the underlying data repository.</param>$endif$
        /// <param name="mapper">The <see cref="IMapper"/> interface</param>
        ///	<remarks>Add new, customized functions to the <see cref="IOrchestrator"/> interface, and then add their
        ///	implementations in this class, to extend the orchestrator with your custom functions.
        ///	</remarks>
		public Orchestrator(ILogger<Orchestrator> logger, IServiceProvider provider, $if$ ($databasetechnology$ != None)IRepository repository, $endif$IMapper mapper)
		{
			_logger = logger;$if$ ($databasetechnology != None)
			_repository = repository;$endif$
			_mapper = mapper;$if$ ($userql$ == True)
			_translator = new Translator(provider, _mapper);$endif$

            _logger.LogTrace("Orchestrator is being instantiated.");
		}
	$if$ ($userql$ == True)
        #region Generic Operations
        /// <summary>
        /// Retrieves a single resource from the datastore according to the <see cref="RqlNode"/> filter
        /// </summary>
        /// <typeparam name="T">The type of resource to retrieve</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> filter used to specify the individual resource.</param>
        /// <returns>A single resource of type T, or null if none is found.</returns>
        public async Task<T?> GetSingleResourceAsync<T>(RqlNode node) where T : class
        {
            _logger.LogTrace("Orchestrator: GetSingleResourceAsync<>");

            var translatedNode = _translator.TranslateQueryR2E<T>(node);

            if (translatedNode.Find(RqlOperation.COUNT) != null)
            {
                throw new RqlFormatException("Invalid RQL Clause COUNT: COUNT can only apply to a collection.");
            }

            if (translatedNode.Find(RqlOperation.MAX) != null)
            {
                throw new RqlFormatException("Invalid RQL Clause MAX: MAX can only apply to a collection.");
            }

            if (translatedNode.Find(RqlOperation.MIN) != null)
            {
                throw new RqlFormatException("Invalid RQL Clause MIN: MIN can only apply to a collection.");
            }

            if (translatedNode.Find(RqlOperation.MEAN) != null)
            {
                throw new RqlFormatException("Invalid RQL Clause MEAN: MEAN can only apply to a collection.");
            }

            var entityAttribute = (Entity?)typeof(T).GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(Entity));

            if (entityAttribute is not null)
            {
                var entityType = entityAttribute.EntityType;
                var entity = await _repository.GetSingleEntityAsync(entityType, translatedNode);

                if (entity is not null)
                {
                    var resource = _mapper.Map(entity, entityType, typeof(T));
                    return (T?)resource;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves a collection of resources from the datastore according to the <see cref="RqlNode"/> filter
        /// </summary>
        /// <typeparam name="T">The type of resources to retrieve</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that filters the query.</param>
        /// <returns>A collection of resources of type T</returns>
        public async Task<PagedSet<T>> GetResourceCollectionAsync<T>(RqlNode node) where T : class
        {
            _logger.LogTrace("Orchestrator: GetResourceCollectionAsync");
            var entityAttribute = (Entity?)typeof(T).GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(Entity));

            if (entityAttribute is not null)
            {
                var entityType = entityAttribute.EntityType;
                var translatedNode = _translator.TranslateQueryR2E<T>(node);
                var collection = await _repository.GetEntityCollectionAsync(entityType, translatedNode);

                if (collection != null)
                {
                    var countProperty = collection.GetType().GetRuntimeField("Count");
                    var startProperty = collection.GetType().GetRuntimeField("Start");
                    var pageSizeProperty = collection.GetType().GetRuntimeField("PageSize");
                    var itemsProperty = collection.GetType().GetRuntimeField("Items");

                    if (countProperty is not null &&
                         startProperty is not null &&
                         pageSizeProperty is not null &&
                         itemsProperty is not null)
                    {
                        var rset = new PagedSet<T>()
                        {
                            Count = Convert.ToInt32(countProperty.GetValue(collection) ?? 0),
                            Start = Convert.ToInt32(startProperty.GetValue(collection) ?? 0),
                            PageSize = Convert.ToInt32(pageSizeProperty.GetValue(collection) ?? 0),
                            Items = (T[])_mapper.Map(itemsProperty.GetValue(collection), entityType.MakeArrayType(), typeof(T).MakeArrayType())
                        };

                        return rset;
                    }
                }
            }

            return new PagedSet<T>();
        }

        /// <summary>
        /// Adds a new resource to the collection of scopes
        /// </summary>
        /// <param name="resource">The resource to add.</param>
        /// <returns>The newly created resource</returns>
        public async Task<T> AddResourceAsync<T>(T resource) where T : class
        {
            var entityAttribute = (Entity?)typeof(T).GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(Entity));

            if (entityAttribute is null)
                throw new Exception($"{typeof(T).Name} is not a resource model");

            var entityType = entityAttribute.EntityType;

            var entity = _mapper.Map(resource, typeof(T), entityType);

            var newEntity = await _repository.AddEntityAsync(entityType, entity);

            return (T)_mapper.Map(newEntity, entityType, typeof(T));
        }

        /// <summary>
        /// Updates a resource of type T
        /// </summary>
        /// <param name="resource">The resource to update.</param>
        /// <param name="node">The <see cref="RqlNode"/> that restricts the update.</param>
        public async Task UpdateResourceAsync<T>(T resource, RqlNode node) where T : class
        {
            var entityAttribute = (Entity?)typeof(T).GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(Entity));

            if (entityAttribute is null)
                throw new Exception($"{typeof(T).Name} is not a resource model");

            var entityType = entityAttribute.EntityType;

            var entity = _mapper.Map(resource, typeof(T), entityType);
            var translatedNode = _translator.TranslateQueryR2E<T>(node);

            await _repository.UpdateEntityAsync(entityType, entity, translatedNode);
        }

        /// <summary>
        /// Deletes a resource of type T
        /// </summary>
        /// <typeparam name="T">The type of resources to retrieve</typeparam>
        /// <param name="node">The <see cref="RqlNode"/> that filters the query.</param>
        public async Task DeleteResourceAsync<T>(RqlNode node) where T : class
        {
            var entityAttribute = (Entity?)typeof(T).GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(Entity));

            if (entityAttribute is null)
                throw new Exception($"{typeof(T).Name} is not a resource model");

            var entityType = entityAttribute.EntityType;
            var translatedNode = _translator.TranslateQueryR2E<T>(node);
            await _repository.DeleteEntityAsync(entityType, translatedNode);
        }
        #endregion

	$endif$ #region Dispose Pattern
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
                    _logger.LogTrace("Orchestrator is being disposed.");
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
