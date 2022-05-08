namespace WizardInstaller.Template.Models
{
    /// <summary>
    /// Maps an entity class to a database table
    /// </summary>
    internal class EntityDBMap
    {
        /// <summary>
        /// The entity class name
        /// </summary>
        public string EntityClassName { get; set; } = string.Empty;

        /// <summary>
        /// The type of database server that hosts the database and table
        /// </summary>
        public string DBServerType { get; set; } = string.Empty;    

        /// <summary>
        /// The schema name for the table
        /// </summary>
        public string EntitySchema { get; set; } = string.Empty;

        /// <summary>
        /// The database table
        /// </summary>
        public string EntityTable { get; set; } = string.Empty;

        /// <summary>
        /// The connection string used to access the database
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
