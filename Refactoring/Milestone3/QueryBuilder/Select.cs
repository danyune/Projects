using System;
using System.Collections.Generic;
using System.Linq;

namespace Team7GUI.QueryBuilder
{
    internal class Select : QueryBase
    {
        // Structures to store information when building query
        internal List<string> Columns { get; private set; } = new List<string>();
        internal string From { get; private set; } = string.Empty;
        internal List<string> Filter { get; private set; } = new List<string>();
        internal List<OrFilter> Orfilter { get; private set; } = new List<OrFilter>();
        internal Dictionary<string, (string, string)> Innerjoin { get; private set; } = new Dictionary<string, (string, string)>();
        internal string Orderby { get; private set; } = string.Empty;
        internal string Groupby { get; private set; } = string.Empty;

        /// <summary>
        /// Initialize a Select statement
        /// </summary>
        public Select()
        {
        }

        /// <summary>
        /// Initialize a Select statement with a base from table
        /// </summary>
        /// <param name="from">Table</param>
        public Select(string from)
        {
            From = from;
        }

        /// <summary>
        /// Add select columns for projection
        /// </summary>
        /// <param name="columns">Column names separated by comma</param>
        public void AddColumns(string columns)
        {
            string[] splitcolumns = columns.Split(',').Select(x => x.Trim()).ToArray();
            Array.ForEach(splitcolumns, x => Columns.Add(x));
        }

        /// <summary>
        /// Add 'where' filters, automatically changes from where to and after the first
        /// </summary>
        /// <param name="filter">Filter to add</param>
        public void AddFilter(string filter)
        {
            Filter.Add(filter);
        }

        /// <summary>
        /// Add an OrFilter class object and appends onto query in the form of 'and (filtertext or filtertext...)'
        /// </summary>
        /// <param name="filter">Generated OrFilter</param>
        public void AddOrFilter(OrFilter filter)
        {
            Orfilter.Add(filter);
        }

        /// <summary>
        /// Overloaded innerjoin if both columnNames are the same
        /// </summary>
        /// <param name="table">Table to join</param>
        /// <param name="attributeName">On what columns to join with</param>
        public void AddInnerJoin(string table, string attributeName)
        {
            AddInnerJoin(table, attributeName, attributeName);
        }

        /// <summary>
        /// Overloaded innerjoin if columnNames differ
        /// </summary>
        /// <param name="table">Table to join</param>
        /// <param name="attributeName">First table columnName</param>
        /// <param name="attributeSecond">Second table columnName</param>
        public void AddInnerJoin(string table, string attributeName, string attributeSecond)
        {
            Innerjoin.Add(table, (attributeName, attributeSecond));
        }

        /// <summary>
        /// If crossjoin is used, shown as 'from table1, table2'
        /// </summary>
        /// <param name="from">Table</param>
        public void AddFrom(string from)
        {
            if (string.IsNullOrEmpty(From))
            {
                From = from;
            }
            else
            {
                From += $",{from}";
            }
        }

        /// <summary>
        /// Sets what to put in 'order by'
        /// </summary>
        /// <param name="orderby">which column to order by</param>
        public void SetOrderBy(string orderby)
        {
            Orderby = orderby;
        }

        /// <summary>
        /// Sets what to put in 'group by'
        /// </summary>
        /// <param name="groupby">which column(s) to group by</param>
        public void SetGroupBy(string groupby)
        {
            Groupby = groupby;
        }

        /// <summary>
        /// Clears the filter List to re-use a query with different filters
        /// </summary>
        public void ClearFilter()
        {
            Filter.Clear();
        }
    }
}