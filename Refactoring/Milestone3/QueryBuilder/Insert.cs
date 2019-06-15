using System.Collections.Generic;

namespace Team7GUI.QueryBuilder
{
    internal class Insert : QueryBase
    {
        // Structures to store information about inserting
        internal string Table { get; private set; } = string.Empty;
        internal string Conflict { get; private set; } = string.Empty;
        internal Dictionary<string, object> Parameters { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes an insert statement with table provided
        /// </summary>
        /// <param name="table">table to insert into</param>
        public Insert(string table)
        {
            Table = table;
            Conflict = SetConflict();
        }

        /// <summary>
        /// Overloaded to add a value of default to a column
        /// </summary>
        /// <param name="column">Columnname</param>
        public void AddParameter(string column)
        {
            AddParameter(column, "default");
        }

        /// <summary>
        /// Overloaded to add a value provided to a column to insert
        /// </summary>
        /// <param name="column">Columnname</param>
        /// <param name="value">Value to enter in cell</param>
        public void AddParameter(string column, object value)
        {
            Parameters.Add(column, value);
        }

        /// <summary>
        /// If inserting into businesscheckins or review, append the necessary 'on conflict' clause
        /// </summary>
        /// <returns>proper 'on conflict' string</returns>
        private string SetConflict()
        {
            switch (Table)
            {
                case "businesscheckins":
                    return "on conflict (day, checkintime, businessid) do update set count = businesscheckins.count + 1";

                case "review":
                    return "on conflict (reviewid) do nothing";

                case "businesshours":
                    return $"on conflict (day, businessid) do update set opentime = excluded.opentime, closetime = excluded.closetime";

                case "business":
                    return $"on conflict (businessid) do update set name = excluded.name, address = excluded.address, city = excluded.city, state = excluded.state, zipcode = excluded.zipcode";

                case "users":
                    return $"on conflict (userid) do update set name = excluded.name, latitude = excluded.latitude, longitude = excluded.longitude";
                // Compiler wouldn't allow default for literal
                case null:
                    return string.Empty;
            }
            return string.Empty;
        }
    }
}