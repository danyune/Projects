using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Team7GUI.Information;

namespace Team7GUI.Maps
{
    internal static class InfoBlock
    {
        private static SolidColorBrush background = new SolidColorBrush(Colors.Violet)
        {
            Opacity = 0.75
        };

        private static TextBlock textblock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(5),
            Background = background,
            FontSize = 16,
            FontWeight = FontWeights.UltraBold
        };

        private static Border border = new Border
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            Child = textblock
        };

        public static Border Build(BaseInfo info)
        {
            if (info is Business business)
            {
                textblock.Text = GenerateBusiness(business);
            }
            else if (info is Users users)
            {
                textblock.Text = GenerateUsers(users);
            }
            else
            {
                textblock.Text = string.Empty;
            }
            return border;
        }

        /// <summary>
        /// Build a Users TextBlock text with just name and addrses
        /// </summary>
        /// <param name="users">User</param>
        /// <returns>string</returns>
        private static string GenerateUsers(Users users)
        {
            StringBuilder userText = new StringBuilder();
            userText.AppendLine(users.Name);
            userText.AppendLine(GetAddress(users.Latitude, users.Longitude));
            return userText.ToString();
        }

        /// <summary>
        /// Build a Business TextBlock text with Name, Address, Reviewcount, Reviewrating, and Check-Ins
        /// </summary>
        /// <param name="currentBusiness">Business</param>
        /// <returns>string</returns>
        private static string GenerateBusiness(Business currentBusiness)
        {
            StringBuilder businessText = new StringBuilder();
            businessText.AppendLine(currentBusiness.Name);
            businessText.AppendLine(currentBusiness.Address);
            businessText.AppendLine("Review Count: " + currentBusiness.Reviewcount);
            businessText.AppendLine("Review Rating: " + currentBusiness.Reviewrating);
            businessText.Append("Check-Ins: " + currentBusiness.Numcheckins);
            return businessText.ToString();
        }

        /// <summary>
        /// Estimate address based on latitude and longitude
        /// </summary>
        /// <param name="latitude">user latitude</param>
        /// <param name="longitude">user longitude</param>
        /// <returns>address string</returns>
        public static string GetAddress(double latitude, double longitude)
        {
            string address = string.Empty;
            string url = $"http://dev.virtualearth.net/REST/v1/Locations/{latitude},{longitude}?&key=ArVh14HJTS5NYlLej7SaUdGNP0YR6zMVjMizCx4S-qpwo_nmK9sBvHsH3xfwXqau";

            using (var client = new WebClient())
            {
                // Get the result and parse out the coordinates
                string response = client.DownloadString(url);
                JsonSerializer ser = new JsonSerializer();
                JObject jObject = JObject.Parse(response);
                address = jObject["resourceSets"][0]["resources"][0]["name"].ToString();
            }

            return address;
        }
    }
}