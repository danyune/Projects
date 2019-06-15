using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Team7GUI.Information;
using Team7GUI.QueryBuilder;
using static Team7GUI.QueryBuilder.BuildQuery;

namespace Team7GUI
{
    internal static class Query
    {
        /// <summary>
        /// Connection string for connecting to database that queries use
        /// </summary>
        /// <returns>connection string</returns>
        private static string BuildConnectionString()
        {
            return "Host=localhost; Username=postgres; Password=team7; Database=milestone2db";
        }

        /// <summary>
        /// If all attributes for a 'select * from table_name' are wanted with no 'where' clauses
        /// </summary>
        /// <param name="table">string of what class</param>
        /// <returns>List of classtype</returns>
        public static List<BaseInfo> BaseQuery(string table)
        {
            return BaseQuery(table, new Dictionary<string, string>());
        }

        /// <summary>
        /// Generic SQL statement builder, takes in what table for a 'select * from table_name' and stores all the results into a List of that class type
        /// </summary>
        /// <param name="table">String of what table</param>
        /// <param name="filters">Dictionary for how to filter results, such as item1=businessid, item2='id_string'</param>
        /// <returns>List of class instances</returns>
        public static List<BaseInfo> BaseQuery(string table, Dictionary<string, string> filters)
        {
            // Check if the type exists that needs to be queried
            Type thisType = ClassTypeExist(table);
            if (thisType != null)
            {
                table = table.ToLower();
                Select data = new Select(table);
                DictionaryBuild(data);
                return CreateList(thisType, RunQuery(data));
            }
            else
            {
                throw new TypeLoadException("This class is not yet implemented");
            }

            // Parse the dictionary and convert it to a Select statement
            void DictionaryBuild(Select data)
            {
                filters = new Dictionary<string, string>(filters, StringComparer.InvariantCultureIgnoreCase);

                if (filters.TryGetValue("select", out string selectvalue))
                {
                    data.AddColumns(selectvalue);
                    filters.Remove("Select");
                }

                if (filters.TryGetValue("groupby", out string groupbyvalue))
                {
                    data.SetGroupBy(groupbyvalue);
                    filters.Remove("GroupBy");
                }

                if (filters.TryGetValue("orderby", out string orderbyvalue))
                {
                    data.SetOrderBy(orderbyvalue);
                    filters.Remove("OrderBy");
                }

                if (filters.Count > 0)
                {
                    foreach (string key in filters.Keys)
                    {
                        data.AddFilter($"{key} like '{filters[key].Replace("'", "''")}'");
                    }
                }
            }
        }

        /// <summary>
        /// Updates the business grid checking all filters in the GUI
        /// </summary>
        /// <param name="business">Business to get state, city, and zip</param>
        /// <param name="categories">Selected categories</param>
        /// <param name="price">Price checkmarked</param>
        /// <param name="meals">Meals checkmarked</param>
        /// <param name="attributes">Selected attributes</param>
        /// <returns>List of businesses remaining</returns>
        public static List<BaseInfo> UpdateBusinessGrid(Business business, List<BusinessCategory> categories, string price, List<string> meals, List<BusinessAttribute> attributes, Users users, string orderby, List<string> additionalFilters = null)
        {
            Select innerData = new Select("businesscategories");
            innerData.AddColumns($"name, address, city, state, zipcode, reviewcount, reviewrating, numcheckins, stars, business.businessid, categoryname, latitude, longitude, distance('{users.Latitude}', '{users.Longitude}', business.latitude, business.longitude)");
            innerData.AddInnerJoin("business", "businessid");
            innerData.AddFilter($"state = '{business.State}'");
            innerData.AddFilter($"city = '{business.City}'");
            innerData.AddFilter($"zipcode = '{business.Zipcode}'");

            foreach (BusinessCategory category in categories)
            {
                innerData.AddFilter("exists (select * from businesscategories as b1 where b1.businessid = businesscategories.businessid and b1.categoryname = '" + category.Categoryname.Replace("'", "''") + "')");
            }

            foreach (BusinessAttribute attribute in attributes)
            {
                if (attribute.Attributename.Equals("WiFi"))
                {
                    innerData.AddFilter("exists (select * from businessattributes as b2 where b2.businessid = businessattributes.businessid and b2.attributename = '" + attribute.Attributename.Replace("'", "''") + "' and b2.value <> 'False' and b2.value <> 'no')");
                }
                else
                {
                    innerData.AddFilter("exists (select * from businessattributes as b2 where b2.businessid = businessattributes.businessid and b2.attributename = '" + attribute.Attributename.Replace("'", "''") + "')");
                }
            }

            if (!string.IsNullOrEmpty(price) || meals.Count > 0 || attributes.Count > 0)
            {
                innerData.AddInnerJoin("businessattributes", "businessid");

                if (!string.IsNullOrEmpty(price))
                {
                    innerData.AddFilter($"(businessattributes.attributename = 'RestaurantsPriceRange2' and businessattributes.value = '{price}')");
                }

                if (meals.Count > 0)
                {
                    foreach (string meal in meals)
                    {
                        innerData.AddFilter($"exists (select attributename, value from businessattributes as ba1 where ba1.businessid = businesscategories.businessid and (ba1.attributename = '{meal.ToLower()}' and ba1.value = 'True'))");
                    }
                }
            }

            Select outerData = new Select($"({Build(innerData)}) as temp");
            outerData.AddColumns("distinct name, address, city, state, zipcode, reviewcount, reviewrating, numcheckins, stars, businessid, latitude, longitude, distance");
            if (additionalFilters != null)
            {
                foreach (string filter in additionalFilters)
                {
                    outerData.AddFilter(filter);
                }
            }
            outerData.SetOrderBy(string.IsNullOrEmpty(orderby) ? "name" : orderby);

            return CreateList(typeof(Business), RunQuery(outerData));
        }

        /// <summary>
        /// Gets the reviews for a selected business that were left by friends of the user
        /// </summary>
        /// <param name="business">Selected business</param>
        /// <param name="users">Current user</param>
        /// <returns>List of reviews</returns>
        public static List<BaseInfo> BusinessFriendsReviews(Business business, Users users)
        {
            Select data = new Select("review");
            data.AddColumns("reviewid, review.userid, businessid, reviewtext, stars, reviewdate, review.funny, review.useful, review.cool, name as username");
            data.AddFilter($"review.businessid = '{business.Businessid}'");
            data.AddFilter($"friends.userid = '{users.Userid}'");
            data.AddInnerJoin("users", "userid");
            data.AddInnerJoin("friends", "userid", "isfriend");
            data.SetOrderBy("reviewdate");

            return CreateList(typeof(Review), RunQuery(data));
        }

        /// <summary>
        /// Query which price ranges exist among the current list of businesses
        /// </summary>
        /// <param name="businesses">List of businesses on the businessGrid</param>
        /// <returns>bool of prices and meals that exist</returns>
        public static (bool[] priceArray, Dictionary<string, bool> mealsList) BusinessPriceMealsList(List<BaseInfo> businesses)
        {
            return (Prices(), Meals());

            // Find what priceranges exist
            bool[] Prices()
            {
                bool[] prices = new bool[4];
                Select priceQuery = new Select("businessattributes");
                priceQuery.AddColumns("distinct value");
                priceQuery.AddFilter($"lower(attributename) = 'restaurantspricerange2'");
                priceQuery.AddOrFilter(AddBusinessOrFilter());

                DataTable priceResult = RunQuery(priceQuery);
                foreach (DataRow row in priceResult.Rows)
                {
                    object[] element = row.ItemArray;
                    if (int.TryParse(element[0].ToString(), out int index))
                    {
                        prices[index - 1] = true;
                    }
                }

                return prices;
            }

            // Find which meals are offered
            Dictionary<string, bool> Meals()
            {
                Dictionary<string, bool> mealFilters = new Dictionary<string, bool>
                {
                    { "breakfast", false },
                    { "brunch", false },
                    { "dessert", false },
                    { "dinner", false },
                    { "latenight", false },
                    { "lunch", false }
                };
                Select mealsQuery = new Select("businessattributes");
                mealsQuery.AddColumns("distinct attributename, value");
                mealsQuery.AddFilter($"lower(value) = 'true'");
                OrFilter orfilter = new OrFilter();
                foreach (string meal in mealFilters.Keys)
                {
                    orfilter.Add($"lower(attributename) = '{meal}'");
                }
                mealsQuery.AddOrFilter(AddBusinessOrFilter());
                mealsQuery.AddOrFilter(orfilter);
                DataTable mealsResult = RunQuery(mealsQuery);
                foreach (DataRow row in mealsResult.Rows)
                {
                    object[] element = row.ItemArray;
                    if (element.Length > 0)
                    {
                        if (element[1].ToString() == "True")
                        {
                            mealFilters[element[0].ToString()] = true;
                        }
                    }
                }

                return mealFilters;
            }

            // Creates a big OrFilter for this query
            OrFilter AddBusinessOrFilter()
            {
                OrFilter returnorfilter = new OrFilter();
                foreach (Business business in businesses)
                {
                    returnorfilter.Add($"businessid = '{business.Businessid}'");
                }
                return returnorfilter;
            }
        }

        /// <summary>
        /// Gets the Attributes, Categories, and Hours for the selected business
        /// </summary>
        /// <param name="business">Selected business</param>
        /// <returns>Dictionary of all the information</returns>
        public static Dictionary<string, string> BusinessInformation(Business business)
        {
            Dictionary<string, string> queryDictionary = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();

            Select attributes = new Select("businessattributes");
            attributes.AddColumns("attributename, value");
            attributes.AddFilter($"businessid = '{business.Businessid}'");
            attributes.AddFilter("value <> 'False'");
            attributes.SetOrderBy("attributename");
            DataTable queryResults = RunQuery(attributes);

            // Attributes
            foreach (DataRow row in queryResults.Rows)
            {
                object[] element = row.ItemArray;
                if (element[1].ToString().Trim().ToLower() == "true")
                {
                    sb.AppendLine(element[0].ToString() + ", ");
                }
                else
                {
                    sb.AppendLine(element[0].ToString() + "(" + element[1].ToString() + "), ");
                }
            }

            // Remove the final appendline and comma
            if (sb.Length > 4)
            {
                sb.Remove(sb.Length - 4, 4);
            }

            queryDictionary.Add("attributes", sb.ToString());
            queryResults.Clear();
            sb.Clear();

            Select categories = new Select("businesscategories");
            categories.AddColumns("categoryname");
            categories.AddFilter($"businessid = '{business.Businessid}'");
            categories.SetOrderBy("categoryname");
            queryResults = RunQuery(categories);

            // Categories
            foreach (DataRow row in queryResults.Rows)
            {
                object[] element = row.ItemArray;
                sb.AppendLine(element[0].ToString());
            }
            queryDictionary.Add("categories", sb.ToString());

            queryResults.Clear();
            sb.Clear();

            Select innerHours = new Select("business");
            innerHours.AddColumns("to_char(current_timestamp, 'FMDay'::text) as today, business.businessid");

            Select hours = new Select();
            hours.AddColumns("today, opentime, closetime");
            hours.AddFrom($"({Build(innerHours)}) as tempquery left outer join businesshours on tempquery.businessid = businesshours.businessid and tempquery.today = businesshours.day ");
            hours.AddFilter($"tempquery.businessid = '{business.Businessid}'");

            queryResults = RunQuery(hours);

            // Hours
            foreach (DataRow row in queryResults.Rows)
            {
                object[] element = row.ItemArray;
                sb.AppendLine("Today (" + element[0].ToString() + ")");
                sb.AppendLine($"  Opens: {ConvertTimeSpan(element[1])}");
                sb.AppendLine($"  Closes: {ConvertTimeSpan(element[2])}");
            }
            queryDictionary.Add("hours", sb.ToString());

            return queryDictionary;
        }

        /// <summary>
        /// Converts a TimeSpan object to proper string format or N/A if nonexistant from query
        /// </summary>
        /// <param name="input">Presumably TimeSpan object</param>
        /// <returns>string format</returns>
        private static string ConvertTimeSpan(object input)
        {
            if (string.IsNullOrEmpty(input.ToString()))
            {
                return "N/A";
            }
            else if (input is TimeSpan timespan)
            {
                return new DateTime().Add(timespan).ToString("HH:mm");
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the business categories that exist within the businesses at the said state, city, and zipcode
        /// </summary>
        /// <param name="business">Selected business</param>
        /// <returns>List of categories</returns>
        public static List<BaseInfo> BusinessCategories(List<BaseInfo> businesses)
        {
            if (businesses.Count > 0)
            {
                if (businesses[0] is Business business)
                {
                    Select data = new Select("businesscategories");
                    data.AddColumns("categoryname, Count(categoryname) as total");
                    data.AddInnerJoin("business", "businessid");
                    data.SetGroupBy("categoryname");
                    data.AddFilter($"state = '{business.State}'");
                    data.AddFilter($"city = '{business.City}'");
                    data.AddFilter($"zipcode = '{business.Zipcode}'");

                    OrFilter orfilter = new OrFilter();
                    foreach (Business b in businesses)
                    {
                        orfilter.Add($"business.businessid = '{b.Businessid}'");
                    }
                    data.AddOrFilter(orfilter);

                    return CreateList(typeof(BusinessCategory), RunQuery(data));
                }
            }
            return new List<BaseInfo>();
        }

        /// <summary>
        /// Reinitialize the Attributes list with the given businesses remaining
        /// </summary>
        /// <param name="businesses">List of businesses remaining</param>
        /// <returns>List of attributes found</returns>
        public static List<BaseInfo> BusinessAttributes(List<BaseInfo> businesses)
        {
            List<string> qualifiedAttributes = new List<string>(MainData.GetQualifiedAttributes().Keys);

            OrFilter orfilter = new OrFilter();
            foreach (string atr in qualifiedAttributes)
            {
                orfilter.Add($"attributename like '{atr}'");
            }
            if (businesses.Count > 0)
            {
                if (businesses[0] is Business business)
                {
                    Select data = new Select("businessattributes");
                    data.AddColumns("attributename, Count(attributename) as total");
                    data.AddInnerJoin("business", "businessid");
                    data.SetGroupBy("attributename");
                    data.AddFilter($"state = '{business.State}'");
                    data.AddFilter($"city = '{business.City}'");
                    data.AddFilter($"zipcode = '{business.Zipcode}'");
                    data.AddFilter($"value <> 'False'");
                    data.AddFilter($"value <> 'no'");
                    data.AddOrFilter(orfilter);

                    OrFilter orfilter2 = new OrFilter();
                    foreach (Business b in businesses)
                    {
                        orfilter2.Add($"business.businessid = '{b.Businessid}'");
                    }
                    data.AddOrFilter(orfilter2);

                    return CreateList(typeof(BusinessAttribute), RunQuery(data));
                }
            }
            return new List<BaseInfo>();
        }

        /// <summary>
        /// Parses the array of query results and stores them into a list of Users instances
        /// </summary>
        /// <param name="searchName">Name to search for</param>
        /// <returns>List of Users</returns>
        public static List<BaseInfo> SearchUsers(string searchName)
        {
            Select data = new Select("users");
            data.AddFilter($"lower(name) like '%{searchName.Replace(" ", string.Empty).ToLower()}%'");

            return CreateList(typeof(Users), RunQuery(data));
        }

        /// <summary>
        /// Parses the array of query results and stores them into a list of Business instances
        /// </summary>
        /// <param name="currentUser">Selected user</param>
        /// <returns>List of Businesses</returns>
        public static List<BaseInfo> FavoriteBusiness(Users currentUser)
        {
            Select data = new Select("favorite");
            data.AddColumns("business.businessid, name, address, city, state, zipcode, reviewrating, reviewcount, numcheckins, latitude, longitude");
            data.AddInnerJoin("business", "businessid");
            data.AddFilter($"userid = '{currentUser.Userid}'");

            return CreateList(typeof(Business), RunQuery(data));
        }

        /// <summary>
        /// Parses the array of query results and stores them into a list of Review instances
        /// </summary>
        /// <param name="currentUser">Selected user</param>
        /// <returns>List of Reviews</returns>
        public static List<BaseInfo> RecentReviews(Users currentUser)
        {
            Select data = new Select("review");
            data.AddColumns("reviewdate, business.name as businessname, review.stars, review.reviewtext, review.funny, review.useful, review.cool");
            data.AddInnerJoin("business", "businessid");
            data.AddFilter($"userid = '{currentUser.Userid}'");
            data.SetOrderBy("reviewdate desc");
            return CreateList(typeof(Review), RunQuery(data));
        }

        /// <summary>
        /// Gets most recent reviews of a business
        /// </summary>
        /// <param name="business"></param>
        /// <returns></returns>
        public static List<BaseInfo> RecentBusinessReviews(Business business)
        {
            Select data = new Select("review");
            data.AddColumns("name as username, stars, reviewdate, reviewtext");
            data.AddInnerJoin("users", "userid");
            data.AddFilter($"businessid = '{business.Businessid}'");
            data.SetOrderBy("reviewdate desc");
            return CreateList(typeof(Review), RunQuery(data));
        }

        /// <summary>
        /// Get the hours for a business
        /// </summary>
        /// <param name="business"></param>
        /// <returns></returns>
        public static List<BaseInfo> BusinessHours(Business business)
        {
            Select data = new Select();
            data.AddColumns("day, business.businessid, opentime, closetime");
            data.AddFrom($"business left outer join businesshours on business.businessid = businesshours.businessid ");
            data.AddFilter($"business.businessid = '{business.Businessid}'");

            return CreateList(typeof(BusinessHours), RunQuery(data));
        }

        /// <summary>
        /// Parses the array of query results and stores them into a list of Users instances
        /// </summary>
        /// <param name="currentUser">Selected user</param>
        /// <returns>List of Users</returns>
        public static List<BaseInfo> FriendsInformation(Users currentUser)
        {
            Select data = new Select("friends");
            data.AddColumns("users.userid, name, yelpingsince, reviewcount, fans, averagestars, funny, useful cool, latitude, longitude");
            data.AddInnerJoin("users", "isfriend", "userid");
            data.AddFilter($"friends.userid = '{currentUser.Userid}'");
            data.SetOrderBy("name");
            return CreateList(typeof(Users), RunQuery(data));
        }

        /// <summary>
        /// Parses the array of query results and stores them into a list of Review instances
        /// </summary>
        /// <param name="currentUser">Selected User</param>
        /// <returns>List of Reviews</returns>
        public static List<BaseInfo> FriendsReviews(Users currentUser)
        {
            // Friend Reviews
            Select data = new Select("review");
            data.AddColumns("review.userid, users.name as uname, business.name as bname, review.stars, review.reviewdate, review.reviewtext, (row_number() over(partition by review.userid, users.name order by reviewdate desc)) as rn");
            data.AddFilter($"friends.userid = '{currentUser.Userid}'");
            data.AddInnerJoin("friends", "userid", "isfriend");
            data.AddInnerJoin("business", "businessid");
            data.AddInnerJoin("users", "userid");
            Select outerData = new Select($"({Build(data)}) as p3");
            outerData.AddColumns("p3.uname as username, p3.bname as businessname, p3.stars, p3.reviewtext, p3.reviewdate");
            outerData.AddFilter("rn = 1");
            outerData.SetOrderBy("p3.reviewdate desc");
            return CreateList(typeof(Review), RunQuery(outerData));
        }

        /// <summary>
        /// Inserts a checkin for a business
        /// </summary>
        /// <param name="businessid">Business id to check into</param>
        public static void CheckIn(Business business)
        {
            Insert data = new Insert("businesscheckins");
            data.AddParameter("businessid", business.Businessid);
            RunNonQuery(data);
        }

        /// <summary>
        /// Insert or update hours for a business
        /// </summary>
        /// <param name="business">business</param>
        /// <param name="day">which day</param>
        /// <param name="opentime">opening time</param>
        /// <param name="closetime">closing time</param>
        public static void UpdateHours(Business business, string day, TimeSpan opentime, TimeSpan closetime)
        {
            Insert data = new Insert("businesshours");
            data.AddParameter("day", day);
            data.AddParameter("businessid", business.Businessid);
            data.AddParameter("opentime", opentime);
            data.AddParameter("closetime", closetime);
            RunNonQuery(data);
        }

        /// <summary>
        /// Update business name address city state and/or zip
        /// </summary>
        /// <param name="business">business</param>
        public static void UpdateBusinessInformationOwnerPage(Business business)
        {
            Insert data = new Insert("business");
            data.AddParameter("businessid", business.Businessid);
            data.AddParameter("name", business.Name);
            data.AddParameter("address", business.Address);
            data.AddParameter("city", business.City);
            data.AddParameter("state", business.State);
            data.AddParameter("zipcode", business.Zipcode);
            RunNonQuery(data);
        }

        /// <summary>
        /// Update user coordinates and name if wanted
        /// </summary>
        /// <param name="users">User</param>
        public static void UpdateUserInformation(Users users)
        {
            Insert data = new Insert("users");
            data.AddParameter("userid", users.Userid);
            data.AddParameter("name", users.Name);
            data.AddParameter("latitude", users.Latitude);
            data.AddParameter("longitude", users.Longitude);
            RunNonQuery(data);
        }

        /// <summary>
        /// Get checkins for a business
        /// </summary>
        /// <param name="business">business</param>
        /// <returns>Dictionary with day and count</returns>
        public static Dictionary<string, int> GetCheckIns(Business business)
        {
            Select data = new Select("businesscheckins");
            data.AddColumns("day, sum(count)");
            data.AddFilter($"businessid = '{business.Businessid}'");
            data.SetGroupBy("day");

            DataTable dt = RunQuery(data);
            Dictionary<string, int> checkins = new Dictionary<string, int>();

            // Parse the DataTable and store them in a dictionary
            foreach (DataRow row in dt.Rows)
            {
                object[] element = row.ItemArray;
                checkins.Add(element[0].ToString(), Convert.ToInt32(element[1]));
            }

            return checkins;
        }

        /// <summary>
        /// Adds all selected businesses to favorites
        /// </summary>
        /// <param name="users">User</param>
        /// <param name="businesses">Selected businesses</param>
        public static void AddToFavorites(Users users, List<Business> businesses)
        {
            foreach (Business business in businesses)
            {
                Insert insertData = new Insert("favorite");
                insertData.AddParameter("userid", users.Userid);
                insertData.AddParameter("businessid", business.Businessid);
                RunNonQuery(insertData);
            }
        }

        /// <summary>
        /// Delete all selected businesses from favorites on user page
        /// </summary>
        /// <param name="users">User</param>
        /// <param name="businesses">Selected businesses</param>
        public static void RemoveFromFavorites(Users users, Business business)
        {
            Delete delete = new Delete("favorite");
            delete.AddFilter($"userid = '{users.Userid}'");
            delete.AddFilter($"businessid = '{business.Businessid}'");
            RunNonQuery(delete);
        }

        /// <summary>
        /// Inserts a written review into the database
        /// </summary>
        /// <param name="review">Review written</param>
        public static void SubmitReview(Review review)
        {
            string reviewid = GenerateId();
            Select data = new Select("review");
            data.AddColumns("reviewid");
            data.AddFilter($"reviewid = '{reviewid}'");
            bool duplicate = true;

            // Check to see (however unlikely) if our reviewid generated already exists
            while (duplicate)
            {
                if (RunQuery(data).Rows.Count == 0)
                {
                    duplicate = false;
                }
                else
                {
                    Reinitialize();
                }
            }

            review.Reviewid = reviewid;

            // Reviewid is unique and not a duplicate, start the insert
            Insert insertData = new Insert("review");
            insertData.AddParameter("reviewid", review.Reviewid);
            insertData.AddParameter("userid", review.Userid);
            insertData.AddParameter("businessid", review.Businessid);
            insertData.AddParameter("stars", review.Stars);
            insertData.AddParameter("reviewtext", review.Reviewtext);
            RunNonQuery(insertData);

            // Generate a new ID and clear the old one
            void Reinitialize()
            {
                reviewid = GenerateId();
                data.ClearFilter();
                data.AddFilter($"reviewid = '{reviewid}'");
            }
        }

        /// <summary>
        /// Generate a random 22-digit ID that userid, reviewid, and businessid all seem to use
        /// </summary>
        /// <returns>randomly generated 22-digit ID</returns>
        private static string GenerateId()
        {
            Random random = new Random();

            // Add chars to make IDs more complex if desired
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Enumerable.Repeat(chars, 22)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Run the created insert command
        /// </summary>
        /// <param name="insert">Information about the insert</param>
        private static void RunNonQuery(QueryBase command)
        {
            using (var conn = new NpgsqlConnection(BuildConnectionString()))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(Build(command), conn))
                {
                    if (command is Insert insert)
                    {
                        foreach (string key in insert.Parameters.Keys)
                        {
                            cmd.Parameters.AddWithValue(key, insert.Parameters[key]);
                        }
                    }
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Runs a SQL query with the given string
        /// </summary>
        /// <param name="query">query to run</param>
        /// <returns>List of arrays which are the rows from the query</returns>
        private static DataTable RunQuery(QueryBase query)
        {
            DataSet ds = new DataSet();

            using (var conn = new NpgsqlConnection(BuildConnectionString()))
            {
                conn.Open();
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(Build(query), conn);
                ds.Reset();

                da.Fill(ds);
                conn.Close();
            }
            return ds.Tables[0];
        }

        /// <summary>
        /// Takes the data table and what class type, and generates a List based on that information
        /// </summary>
        /// <param name="thisType">Type of class object</param>
        /// <param name="dt">Data from SQL query</param>
        /// <returns>List of class instances</returns>
        private static List<BaseInfo> CreateList(Type thisType, DataTable dt)
        {
            PropertyInfo[] properties = thisType.GetProperties();
            List<BaseInfo> queryList = new List<BaseInfo>();

            foreach (DataRow row in dt.Rows)
            {
                object[] element = row.ItemArray;
                object instance = Activator.CreateInstance(thisType);

                foreach (DataColumn column in dt.Columns)
                {
                    PropertyInfo thisProperty = properties.FirstOrDefault(x => x.Name.ToLower().Equals(column.ColumnName));
                    object value = element[dt.Columns.IndexOf(column.ColumnName)];

                    // Truncate the time
                    if (value is DateTime date)
                    {
                        value = date.ToString("yyyy-MM-dd");
                    }

                    // Convert the time to just hours and minutes
                    else if (value is TimeSpan time)
                    {
                        value = ConvertTimeSpan(time);
                    }

                    // Making sure the property and the database return are both not null
                    if (thisProperty != null && !(value is DBNull))
                    {
                        thisProperty.SetValue(instance, value);
                    }
                }

                // Add tuple as the interface since it can be users, review, or business
                queryList.Add(instance as BaseInfo);
            }
            return queryList;
        }

        /// <summary>
        /// Determines if the class exists
        /// </summary>
        /// <param name="thisType">class type</param>
        /// <returns>Type that exists</returns>
        private static Type ClassTypeExist(string thisType) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(t => t.Name.Equals(thisType, StringComparison.OrdinalIgnoreCase));
    }
}