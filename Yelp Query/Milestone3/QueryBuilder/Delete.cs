    using System.Collections.Generic;

namespace Team7GUI.QueryBuilder
{
    internal class Delete : QueryBase
    {
        // Structures to store information about inserting
        internal string Table { get; private set; } = string.Empty;
        internal List<string> Filters { get; private set; } = new List<string>();

        /// <summary>
        /// Initializes a delete statement from the following table
        /// </summary>
        /// <param name="table">table to delete from</param>
        public Delete(string table)
        {
            Table = table;
        }

        /// <summary>
        /// Specify condition to delete
        /// </summary>
        /// <param name="filter">delete clause</param>
        public void AddFilter(string filter)
        {
            Filters.Add(filter);
        }
    }
}