using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Windows;
using Team7GUI.Information;

namespace Team7GUI.UsersPage
{
    /// <summary>
    /// Interaction logic for UpdateUserInfo.xaml
    /// </summary>
    public partial class UpdateUserInfo : Window
    {
        private Users currentUser;

        private double Latitude { get; set; }

        private double Longitude { get; set; }

        public UpdateUserInfo(Users users = null)
        {
            InitializeComponent();
            currentUser = users;
            InitializeElements();
        }

        /// <summary>
        /// Fill in name and disable submit button
        /// </summary>
        private void InitializeElements()
        {
            NameBox.Text = currentUser.Name;
            SubmitButton.IsEnabled = false;
        }

        /// <summary>
        /// Close the edit user window
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Query virtualearth to get a latitude and longitude based on partial or full address provided
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void CheckCoordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (double latitude, double longitude) = GetLatLong($"{AddressBox.Text} {CityBox.Text} {StateBox.Text} {ZipBox.Text}");
                Latitude = latitude;
                Longitude = longitude;
                LatitudeBox.Text = Math.Round(latitude,4).ToString();
                LongitudeBox.Text = Math.Round(longitude,4).ToString();
                SubmitButton.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("Error converting address to latitude and longitude", "Error");
            }
        }

        /// <summary>
        /// Submit only name, latitude, and longitude changes. No address is stored on the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            currentUser.Name = NameBox.Text;
            currentUser.Latitude = Latitude;
            currentUser.Longitude = Longitude;
            try
            {
                Query.UpdateUserInformation(currentUser);
                MessageBox.Show("Updated user information successfully!", "Success");
            }
            catch
            {
                MessageBox.Show("Failed to update database for some reason", "Error");
            }
            Close();
        }

        /// <summary>
        /// Get latitude and longitude based on address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>Lat, Long</returns>
        public (double latitude, double longitude) GetLatLong(string address)
        {
            string url = "http://dev.virtualearth.net/REST/v1/Locations?query=" + address + "&key=ArVh14HJTS5NYlLej7SaUdGNP0YR6zMVjMizCx4S-qpwo_nmK9sBvHsH3xfwXqau";
            double latitude = 0;
            double longitude = 0;
            using (var client = new WebClient())
            {
                // Get the result and parse out the coordinates
                string response = client.DownloadString(url);
                JsonSerializer ser = new JsonSerializer();
                JObject jObject = JObject.Parse(response);
                string la = jObject["resourceSets"][0]["resources"][0]["point"]["coordinates"].First.ToString();
                string lo = jObject["resourceSets"][0]["resources"][0]["point"]["coordinates"].Last.ToString();

                // Assign coordinates to return, if they are not existent then assign to 0
                latitude = double.TryParse(la, out double lat) ? lat : 0;
                longitude = double.TryParse(lo, out double lon) ? lon : 0;
            }

            return (latitude, longitude);
        }
    }
}