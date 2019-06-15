using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TestingParsing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Gets directory downloaded to, appends the 'TestingParsing/Debug/Bin/' from base directory location
            string directory = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 25);
            Console.WriteLine(directory);

            Console.WriteLine("\n-- Starting Users and friends --");
            //UserJson();

            Console.WriteLine("\n\n-- Starting Business --");
            //BusinessJson();

            Console.WriteLine("\n\n-- Starting Reviews --");
            ReviewsJson();

            Console.WriteLine("\n\n-- Starting Check Ins --");
            CheckInJson();

            Console.WriteLine("\n\n-- All JSONs successfully parsed --");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Store postgres connection info
        /// </summary>
        /// <returns>string of info</returns>
        private static string buildConnectionString()
        {
            return "Server=cpts451.postgres.database.azure.com; Port=5432; Database=milestone2db; Uid=postgres@cpts451; Pwd=Team7team7!";
            //return "Host=localhost; Username=postgres; Password=team7; Database=milestone2db";
        }

        /// <summary>
        /// Parse users JSON to database
        /// </summary>
        public static void UserJson()
        {
            string[] jsonList = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\yelp_user.JSON");
            Dictionary<string, List<string>> friendslist = new Dictionary<string, List<string>>();
            int number = 1;

            using (var conn = new NpgsqlConnection(buildConnectionString()))
            {
                conn.Open();
                foreach (string line in jsonList)
                {
                    dynamic parsed = JsonConvert.DeserializeObject<dynamic>(line);
                    List<dynamic> parseList = new List<dynamic>();
                    List<string> parsing = new List<string>();
                    List<string> friends = new List<string>();
                    foreach (var users in parsed)
                    {
                        parseList.Add(users);
                    }

                    foreach (var item in parseList)
                    {
                        string tester = item.Name.ToString();
                        bool write = true;
                        if (tester.Length > 10)
                        {
                            string substring = tester.Substring(0, 10);
                            if (substring == "compliment")
                            {
                                write = false;
                            }
                        }
                        if (tester == "elite")
                        {
                            write = false;
                        }

                        if (write && item.Name == "yelping_since")
                        {
                            parsing.Add(item.Value.ToString());
                        }
                        else if (write && item.Name == "friends")
                        {
                            foreach (var friend in item.Value)
                            {
                                friends.Add(friend.Value);
                            }
                        }
                        else if (write)
                        {
                            parsing.Add(item.Value.ToString());
                        }
                    }

                    friendslist.Add(parsing[7], friends);

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO users (userid, name, yelpingsince, reviewcount, fans, averagestars, funny, useful, cool) " +
                        "VALUES (@userid, @name, @yelpingsince, @reviewcount, @fans, @averagestars, @funny, @useful, @cool)" +
                        "ON CONFLICT (userid) DO NOTHING;";
                        cmd.Parameters.AddWithValue("userid", parsing[7]);
                        cmd.Parameters.AddWithValue("name", parsing[4]);
                        cmd.Parameters.AddWithValue("yelpingsince", Convert.ToDateTime(parsing[8]));
                        cmd.Parameters.AddWithValue("reviewcount", Convert.ToInt32(parsing[5]));
                        cmd.Parameters.AddWithValue("fans", Convert.ToInt32(parsing[2]));
                        cmd.Parameters.AddWithValue("averagestars", Convert.ToDouble(parsing[0]));
                        cmd.Parameters.AddWithValue("funny", Convert.ToInt32(parsing[3]));
                        cmd.Parameters.AddWithValue("useful", Convert.ToInt32(parsing[6]));
                        cmd.Parameters.AddWithValue("cool", Convert.ToInt32(parsing[1]));
                        cmd.ExecuteNonQuery();
                        Console.Write("\rUser entered: " + number++);
                    }
                }
                Console.WriteLine("");
                number = 1;

                using (var cmd = new NpgsqlCommand())
                {
                    foreach (string currentuser in friendslist.Keys)
                    {
                        foreach (string friend in friendslist[currentuser])
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = "INSERT INTO friends (userid, isfriend) " +
                            "VALUES (@userid, @isfriend) ON CONFLICT (isfriend, userid) DO NOTHING;";
                            cmd.Parameters.AddWithValue("userid", currentuser);
                            cmd.Parameters.AddWithValue("isfriend", friend);
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            Console.Write("\rFriend entered: " + number++);
                        }
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Parses review JSON to database
        /// </summary>
        public static void ReviewsJson()
        {
            string[] jsonList = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\yelp_review.JSON");
            int number = 1;

            using (var conn = new NpgsqlConnection(buildConnectionString()))
            {
                conn.Open();
                foreach (string line in jsonList)
                {
                    dynamic parsed = JsonConvert.DeserializeObject<dynamic>(line);
                    List<dynamic> parseList = new List<dynamic>();
                    List<string> parsing = new List<string>();

                    foreach (var reviews in parsed)
                    {
                        parseList.Add(reviews);
                    }

                    foreach (var item in parseList)
                    {
                        if (item.Name == "cool")
                        {
                            parsing.Add(item.Value.ToString());
                        }
                        else if (item.Name == "text")
                        {
                            string formattedText = item.Value.ToString();
                            parsing.Add(Regex.Replace(formattedText, "[\n\r]", " "));
                        }
                        else
                        {
                            parsing.Add(item.Value.ToString());
                        }
                    }

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO review (reviewid, userid, businessid, stars, reviewdate, reviewtext, useful, funny, cool) " +
                        "VALUES (@reviewid, @userid, @businessid, @stars, @reviewdate, @reviewtext, @useful, @funny, @cool)"
                        + "ON CONFLICT (reviewid) DO NOTHING;";
                        cmd.Parameters.AddWithValue("reviewid", parsing[0]);
                        cmd.Parameters.AddWithValue("userid", parsing[1]);
                        cmd.Parameters.AddWithValue("businessid", parsing[2]);
                        cmd.Parameters.AddWithValue("stars", Convert.ToInt32(parsing[3]));
                        cmd.Parameters.AddWithValue("reviewdate", Convert.ToDateTime(parsing[4]));
                        cmd.Parameters.AddWithValue("reviewtext", parsing[5]);
                        cmd.Parameters.AddWithValue("useful", Convert.ToInt32(parsing[6]));
                        cmd.Parameters.AddWithValue("funny", Convert.ToInt32(parsing[7]));
                        cmd.Parameters.AddWithValue("cool", Convert.ToInt32(parsing[8]));
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        Console.Write("\rReview entered: " + number++);
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Parses check in JSON to database
        /// </summary>
        public static void CheckInJson()
        {
            // 6:00-11:00 is Morning, 12:00-16:00 is afternoon, 17:00-22:00 is evening, 23:00-5:00 is night
            string[] jsonList = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\yelp_checkin.JSON");
            int number = 1;

            using (var conn = new NpgsqlConnection(buildConnectionString()))
            {
                conn.Open();
                foreach (string line in jsonList)
                {
                    dynamic parsed = JsonConvert.DeserializeObject<dynamic>(line);
                    List<dynamic> parseList = new List<dynamic>();
                    List<string> baseList = new List<string>();
                    Dictionary<string, Dictionary<string, string>> mainParsing = new Dictionary<string, Dictionary<string, string>>();

                    foreach (var checkin in parsed)
                    {
                        parseList.Add(checkin);
                    }

                    foreach (var item in parseList)
                    {
                        if (item.Name == "time")
                        {
                            foreach (var time in item.Value)
                            {
                                Dictionary<string, string> timeCheckin = new Dictionary<string, string>();
                                foreach (var dayTime in time.Value)
                                {
                                    timeCheckin.Add(dayTime.Name.ToString(), dayTime.Value.ToString());
                                }
                                mainParsing.Add(time.Name.ToString(), timeCheckin);
                            }
                        }
                        else
                        {
                            baseList.Add(item.Value.ToString());
                        }
                    }

                    using (var cmd = new NpgsqlCommand())
                    {
                        foreach (string key in mainParsing.Keys)
                        {
                            foreach (KeyValuePair<string, string> dictionary in mainParsing[key])
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = "INSERT INTO businesscheckins (day, checkintime, businessid, count) " +
                                "VALUES (@day, @checkintime, @businessid, @count)"
                                + "ON CONFLICT (day, checkintime, businessid) DO NOTHING;";
                                cmd.Parameters.AddWithValue("day", key);
                                cmd.Parameters.AddWithValue("checkintime", TimeSpan.Parse(dictionary.Key));
                                cmd.Parameters.AddWithValue("businessid", baseList[0]);
                                cmd.Parameters.AddWithValue("count", Convert.ToInt32(dictionary.Value));
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                                Console.Write("\rBusinesscheckins entered: " + number++);
                            }
                        }
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Parses business JSON to database
        /// </summary>
        public static void BusinessJson()
        {
            string[] jsonList = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\yelp_business.JSON");
            int number = 1;

            {
                using (var conn = new NpgsqlConnection(buildConnectionString()))
                {
                    conn.Open();
                    foreach (string line in jsonList)
                    {
                        dynamic parsed = JsonConvert.DeserializeObject<dynamic>(line);
                        List<dynamic> parseList = new List<dynamic>();
                        List<string> baseList = new List<string>();
                        List<string> categoriesList = new List<string>();
                        Dictionary<string, List<string>> hoursDict = new Dictionary<string, List<string>>();
                        Dictionary<string, string> attributeDict = new Dictionary<string, string>();

                        foreach (var business in parsed)
                        {
                            parseList.Add(business);
                        }
                        foreach (var item in parseList)
                        {
                            if (item.Name == "attributes")
                            {
                                foreach (var attribute in item.Value)
                                {
                                    if (attribute.Value.HasValues == null)
                                    {
                                        foreach (var attNest in attribute.Value)
                                        {
                                            attributeDict.Add(attNest.Name.ToString(), attNest.Value.ToString());
                                        }
                                    }
                                    else
                                    {
                                        attributeDict.Add(attribute.Name.ToString(), attribute.Value.ToString());
                                    }
                                }
                            }
                            else if (item.Name == "categories")
                            {
                                foreach (var categories in item.Value)
                                {
                                    categoriesList.Add(categories.Value.ToString());
                                }
                            }
                            else if (item.Name == "hours")
                            {
                                foreach (var hours in item.Value)
                                {
                                    string fixme = hours.Value.ToString();
                                    string[] hoursFix = fixme.Split('-');
                                    List<string> openClose = new List<string>() { hoursFix[0], hoursFix[1] };
                                    hoursDict.Add(hours.Name.ToString(), openClose);
                                }
                            }
                            else if (item.Name == "stars")
                            {
                                double rating = double.Parse(item.Value.ToString());
                                string output = string.Format("{0:0.0}", rating);
                                baseList.Add(output);
                            }
                            else if (item.Name == "is_open")
                            {
                                baseList.Add(item.Value.ToString());
                            }
                            else if (item.Name == "neighborhood")
                            {
                            }
                            else
                            {
                                baseList.Add(item.Value.ToString());
                            }
                        }

                        // Business
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = "INSERT INTO business (businessid, name, address, city, state, zipcode, latitude, longitude, stars, reviewcount, isopen) " +
                            "VALUES (@businessid, @name, @address, @city, @state, @zipcode, @latitude, @longitude, @stars, @reviewcount, @isopen)"
                            + "ON CONFLICT (businessid) DO NOTHING;";
                            cmd.Parameters.AddWithValue("businessid", baseList[0]);
                            cmd.Parameters.AddWithValue("name", baseList[1]);
                            cmd.Parameters.AddWithValue("address", baseList[2]);
                            cmd.Parameters.AddWithValue("city", baseList[3]);
                            cmd.Parameters.AddWithValue("state", baseList[4]);
                            cmd.Parameters.AddWithValue("zipcode", Convert.ToInt32(baseList[5]));
                            cmd.Parameters.AddWithValue("latitude", Convert.ToDouble(baseList[6]));
                            cmd.Parameters.AddWithValue("longitude", Convert.ToDouble(baseList[7]));
                            cmd.Parameters.AddWithValue("stars", Convert.ToDouble(baseList[8]));
                            cmd.Parameters.AddWithValue("reviewcount", Convert.ToInt32(baseList[9]));
                            cmd.Parameters.AddWithValue("isopen", Convert.ToBoolean(int.Parse(baseList[10])));
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }

                        // Categories
                        using (var cmd = new NpgsqlCommand())
                        {
                            foreach (string category in categoriesList)
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = "INSERT INTO businesscategories (categoryname, businessid) " +
                                "VALUES (@categoryname, @businessid)"
                                + "ON CONFLICT (categoryname, businessid) DO NOTHING;";
                                cmd.Parameters.AddWithValue("categoryname", category);
                                cmd.Parameters.AddWithValue("businessid", baseList[0]);
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                        }

                        // Hours
                        using (var cmd = new NpgsqlCommand())
                        {
                            foreach (string day in hoursDict.Keys)
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = "INSERT INTO businesshours (day, businessid, closetime, opentime) " +
                                "VALUES (@day, @businessid, @closetime, @opentime)"
                                + "ON CONFLICT (day, businessid) DO NOTHING;";
                                cmd.Parameters.AddWithValue("day", day);
                                cmd.Parameters.AddWithValue("businessid", baseList[0]);
                                cmd.Parameters.AddWithValue("closetime", TimeSpan.Parse(hoursDict[day][1]));
                                cmd.Parameters.AddWithValue("opentime", TimeSpan.Parse(hoursDict[day][0]));
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                        }

                        // Attributes
                        using (var cmd = new NpgsqlCommand())
                        {
                            foreach (string attribute in attributeDict.Keys)
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = "INSERT INTO businessattributes (attributename, businessid, value) " +
                                "VALUES (@attributename, @businessid, @value)"
                                + "ON CONFLICT (attributename, businessid) DO NOTHING;";
                                cmd.Parameters.AddWithValue("attributename", attribute);
                                cmd.Parameters.AddWithValue("businessid", baseList[0]);
                                cmd.Parameters.AddWithValue("value", attributeDict[attribute]);
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                        }
                        Console.Write("\rBusiness, categories, hours, and attributes entered: " + number++);
                    }
                    conn.Close();
                }
            }
        }
    }
}