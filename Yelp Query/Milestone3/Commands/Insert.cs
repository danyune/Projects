using System;
using System.Linq;
using Team7GUI.Information;

namespace Team7GUI.Commands
{
    class Insert : ICommand
    {
        public string Query { get; set; }
        private BaseInfo ThisObject { get; set; }

        public Insert(BaseInfo type)
        {
            ThisObject = type;
        }
        public void Execute()
        {
            if (ThisObject is Business business)
            {
                CheckIn(business);
            }
            else if (ThisObject is Review review)
            {
                SubmitReview(review);
            }
        }

        /// <summary>
        /// Inserts a checkin for a business
        /// </summary>
        /// <param name="businessid">Business id to check into</param>
        private static void CheckIn(Business business)
        {
            string query = "insert into businesscheckins (day, checkintime, businessid, count) " +
                "values (default, default, @businessid, default)" +
                "on conflict (day, checkintime, businessid) do update set count = businesscheckins.count + 1";

            ExecuteQuery.RunInsert(business, query);
        }

        /// <summary>
        /// Inserts a written review into the database
        /// </summary>
        /// <param name="review">Review written</param>
        private static void SubmitReview(Review review)
        {
            string reviewid = GenerateId();
            string query = "select reviewid from review where reviewid = '" + reviewid + "'";
            bool duplicate = true;

            // Check to see (however unlikely) if our reviewid generated already exists
            while (duplicate)
            {
                if (ExecuteQuery.RunSelectQuery(query).Rows.Count == 0)
                {
                    duplicate = false;
                }
                else
                {
                    reviewid = GenerateId();
                }
            }

            review.Reviewid = reviewid;
            query = "INSERT INTO review (reviewid, userid, businessid, stars, reviewdate, reviewtext) " +
                "VALUES (@reviewid, @userid, @businessid, @stars, @reviewdate, @reviewtext)"
                + "ON CONFLICT (reviewid) DO NOTHING;";
            ExecuteQuery.RunInsert(review, query);
        }

        /// <summary>
        /// Generate a random 22-digit ID that userid, reviewid, and businessid all seem to use
        /// </summary>
        /// <returns></returns>
        private static string GenerateId()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Enumerable.Repeat(chars, 22)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
