using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using $safeprojectname$.Repositories;
using $safeprojectname$.Services;
$if$ ($userql$ == True)using Rql.Services;
$endif$
namespace $safeprojectname$.Orchestration
{
	///	<summary>
	///	The ServiceOrchestrator
	///	</summary>
	public class Orchestrator : $if$ ($userql$ == True)RqlOrchestrator, $endif$IOrchestrator
{
	private readonly ILogger<Orchestrator> _logger;
	private readonly IRepository _repository;$if$ ($userql$ == False)
	private readonly IMapper _mapper;
	private readonly IServiceProvider _provider;$endif$

		///	<summary>
		///	Initiates the Service Orchestrator
		///	</summary>
		/// <param name="logger">A generic interface for logging where the category name is derrived from the <see cref="Orchestrator"/> name. 
		/// Generally used to enable activation of a named <see cref="ILogger"/> from dependency injection.</param>
        /// <param name="provider">The service provider.</param>
		/// <param name="repository">The named <see cref="IRepository"/> interface to the repository layer. Used to perform Create, Read, Update, 
        /// Delete (CRUD) and other funtions involing the underlying data repository.</param>
        /// <param name="mapper">The <see cref="IMapper"/> interface</param>
        /// <param name="options">The repository options</param>
		///	<remarks>Add new, customized functions to the <see cref="IOrchestrator"/> interface, and then add their
		///	implementations in this class, to extend the orchestrator with your custom functions.
		///	</remarks>
		public Orchestrator(ILogger<Orchestrator> logger, IServiceProvider provider, IRepository repository, IMapper mapper, IRepositoryOptions options)$if$ ($userql$ == True) 
		       : base(provider, options, repository, mapper)$endif$
		{
			_logger = logger;
			_repository = repository;$if$ ($userql$ == False)
			_mapper = mapper;
			_provider = provider;$endif$
		}
	}
}
