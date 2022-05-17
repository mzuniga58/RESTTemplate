namespace RESTInstaller.Models
{
    public class EntityModel
    {
        public string ProjectName { get; set; }
        public string Namespace { get; set; }
        public string Folder { get; set; }
        public string ClassName { get; set; }
        public ElementType ElementType { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public DBServerType ServerType { get; set; }

        public override string ToString()
        {
            return ClassName;
        }
    }
}
