using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using RESTInstaller.Models;

namespace RESTInstaller.Services
{
    internal interface ICodeService
    {
        #region Properties
        string ConnectionString { get; set; }
        DBServerType DefaultServerType { get; }
        List<string> Policies { get; }
        ProjectFolder InstallationFolder { get; }
        List<EntityClass> EntityClassList { get; }
        List<ResourceClass> ResourceClassList { get; }
        bool GetUseRql();
        bool GetUseHal();
        #endregion

        ProjectMapping LoadProjectMapping();
        string GetConnectionStringForEntity(string entityClassName);

        void SaveProjectMapping();

        void AddEntityMap(EntityDBMap entityDBMap);

        void OnProjectItemRemoved(ProjectItem ProjectItem);
        void OnProjectItemAdded(ProjectItem ProjectItem);

        void OnSolutionOpened();

        EntityClass GetEntityClassBySchema(string schema, string tableName);

        ResourceClass GetResourceClassBySchema(string schema, string tableName);

        EntityClass GetEntityClass(string name);

        ResourceClass GetResourceClass(string name);

        void AddEntity(ProjectItem projectItem);

        void AddResource(ProjectItem projectItem);

        void LoadEntityClassList(string folder = "");

        void LoadResourceClassList(string folder = "");


        string NormalizeClassName(string className);

        string CorrectForReservedNames(string columnName);
        DBColumn[] LoadEntityColumns(CodeClass2 codeClass);
        DBColumn[] LoadResourceColumns(ResourceClass resource);
        DBColumn[] LoadResourceEnumColumns(ResourceClass resource);
        DBColumn[] LoadEntityEnumColumns(CodeEnum enumElement);

        bool IsChildOf(string parentPath, string candidateChildPath);

        string GetRelativeFolder(ProjectFolder folder);
        string FindOrchestrationNamespace();
        ProfileMap GenerateProfileMap(ResourceClass resourceModel);


        Project GetProject(string projectName);
        object GetProjectFromFolder(string folder);

        void RegisterComposite(string className, string entityNamespace, ElementType elementType, string tableName);

        ProfileMap OpenProfileMap(ResourceClass resourceModel, out bool isAllDefined);

        CodeClass2 FindCollectionExampleCode(ResourceClass parentModel, string folder = "");
        string GetExampleModel(int skipRecords, ResourceClass resourceModel, DBServerType serverType, string connectionString);
        string ResolveMapFunction(JObject entityJson, string columnName, DBColumn[] entityColumns, ResourceClass model, string mapFunction);
        CodeClass2 FindExampleCode(ResourceClass parentModel, string folder = "");

        string GetProjectItemNamespace(ProjectItem item);
        string GetProjectItemPath(ProjectItem item);


    }
}
