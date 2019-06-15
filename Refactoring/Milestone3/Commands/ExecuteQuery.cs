using Npgsql;
using System;
using System.Data;
using Team7GUI.Information;

namespace Team7GUI.Commands
{
    static class ExecuteQuery
    {
        /// <summary>
        /// Runs a SQL query with the given string
        /// </summary>
        /// <param name="query"></param>
        /// <returns>List of arrays which are the rows from the query</returns>
        public static DataTable RunSelectQuery(string query)
        {
            DataSet ds = new DataSet();

            using (var conn = new NpgsqlConnection(Query.BuildConnectionString()))
            {
                conn.Open();
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                ds.Reset();

                da.Fill(ds);
                conn.Close();
            }
            return ds.Tables[0];
        }

        /// <summary>
        /// Runs the Insert query from writing a review or checking into a business
        /// </summary>
        /// <param name="info">What type of class this insert is using</param>
        /// <param name="query">Insert statement</param>
        public static void RunInsert(BaseInfo info, string query)
        {
            if (info is Review review)
            {
                using (var conn = new NpgsqlConnection(Query.BuildConnectionString()))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("reviewid", review.Reviewid);
                        cmd.Parameters.AddWithValue("userid", review.Userid);
                        cmd.Parameters.AddWithValue("businessid", review.Businessid);
                        cmd.Parameters.AddWithValue("stars", review.Stars);
                        cmd.Parameters.AddWithValue("reviewdate", DateTime.Today);
                        cmd.Parameters.AddWithValue("reviewtext", review.Reviewtext);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            else if (info is Business business)
            {
                using (var conn = new NpgsqlConnection(Query.BuildConnectionString()))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("businessid", business.Businessid);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }
    }
}
