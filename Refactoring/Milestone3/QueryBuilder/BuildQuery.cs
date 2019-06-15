using System.Linq;

namespace Team7GUI.QueryBuilder
{
    internal static class BuildQuery
    {
        /// <summary>
        /// Main method to call to build the query statement, detects which type it is
        /// </summary>
        /// <param name="type">What kind of query</param>
        /// <returns>a query string</returns>
        public static string Build(QueryBase type)
        {
            if (type is Select select)
            {
                return BuildSelect(select);
            }
            else if (type is Insert insert)
            {
                return BuildInsert(insert);
            }
            else if (type is Delete delete)
            {
                return BuildDelete(delete);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Parses the Select instance and makes it into a valid SQL string
        /// </summary>
        /// <param name="data">Select instance</param>
        /// <returns>query string</returns>
        private static string BuildSelect(Select data)
        {
            // Sections of the SQL statement to append to
            string select = "select *";
            string from = $" from {data.From}";
            string alias = string.Empty;
            string innerjoin = string.Empty;
            string filter = string.Empty;
            string orderby = string.Empty;
            string groupby = string.Empty;

            // If Order by was added
            if (!string.IsNullOrEmpty(data.Orderby))
            {
                orderby = $" order by {data.Orderby}";
            }

            // If Group by was added
            if (!string.IsNullOrEmpty(data.Groupby))
            {
                groupby = $" group by {data.Groupby}";
            }

            // Figure out what a table alias is if duplicate tables used
            if (data.From.Contains(" as "))
            {
                int test = data.From.LastIndexOf(" as ");
                alias = data.From.Substring(data.From.IndexOf(" as ") + 4);
            }
            else
            {
                alias = data.From;
            }

            // Determines if specific select columns are needed, defaulted to *
            if (data.Columns.Count > 0)
            {
                select = "select ";
                data.Columns.ForEach(x => select += x + ",");

                // Remove last comma
                select = select.Substring(0, select.Length - 1);
            }

            // Inner join appends. If empty then nothing added
            foreach (string key in data.Innerjoin.Keys)
            {
                innerjoin += $" inner join {key} on {alias}.{data.Innerjoin[key].Item1} = {key}.{data.Innerjoin[key].Item2}";
            }

            // Adds 'where' filters, changing to 'and' after the first one
            if (data.Filter.Count > 0)
            {
                filter += " where ";
                foreach (string s in data.Filter)
                {
                    filter += s;

                    if (!s.Equals(data.Filter.Last()))
                    {
                        filter += " and ";
                    }
                }
            }

            // If OrFilter class was included, build that and append it
            if (data.Orfilter.Count > 0)
            {
                foreach (OrFilter or in data.Orfilter)
                {
                    if (string.IsNullOrEmpty(filter))
                    {
                        filter += " where ";
                    }
                    else
                    {
                        filter += " and (";
                    }

                    string current = string.Empty;
                    foreach (string s in or.Orfilter)
                    {
                        filter += s;

                        if (!s.Equals(or.Orfilter.Last()))
                        {
                            filter += " or ";
                        }
                        else
                        {
                            filter += ")";
                        }
                    }
                }
            }

            // Return each section put together as one giant sql query string
            return $"{select}{from}{innerjoin}{filter}{groupby}{orderby}";
        }

        /// <summary>
        /// Parses the Insert instance and makes it into a valid SQL string
        /// </summary>
        /// <param name="data">Insert instance</param>
        /// <returns>query string</returns>
        private static string BuildInsert(Insert data)
        {
            // Stuff to store insert
            string insert = $"insert into {data.Table} ";
            string parameters = "(";
            string values = "values (";
            string conflict = data.Conflict;

            // Add which columns and values to use
            foreach (string key in data.Parameters.Keys)
            {
                if (key.Equals(data.Parameters.Last().Key))
                {
                    parameters += $"{key}) ";
                    values += $"@{key}) ";
                }
                else
                {
                    parameters += $"{key}, ";
                    values += $"@{key}, ";
                }
            }

            // Return a giant SQL string
            return $"{insert}{parameters}{values}{conflict}";
        }

        /// <summary>
        /// Parses Delete instance and makes a valid SQL string
        /// </summary>
        /// <param name="data">Delete instance</param>
        /// <returns>SQL string</returns>
        private static string BuildDelete(Delete data)
        {
            // Stuff to store delete
            string delete = $"delete from {data.Table} ";
            string filter = "where ";

            if (data.Filters.Count > 0)
            {
                foreach (string s in data.Filters)
                {
                    filter += $"{s} ";
                    if (!s.Equals(data.Filters.Last()))
                    {
                        filter += " and ";
                    }
                }
                return $"{delete}{filter}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}