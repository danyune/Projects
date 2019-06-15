using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;
using Team7GUI.Information;

namespace Team7GUI.OwnerInfoPage
{
    /// <summary>
    /// Interaction logic for OwnerInfo.xaml
    /// </summary>
    public partial class OwnerInfo : UserControl
    {
        public OwnerInfo()
        {
            InitializeComponent();
            businessTextBoxes = new List<TextBox>() { businessName, businessAddress, businessCity, businessState, businessZipcode };
            hoursTextBoxes = new List<TextBox>() { SundayO, SundayC, MondayO, MondayC, TuesdayO, TuesdayC, WednesdayO, WednesdayC, ThursdayO, ThursdayC, FridayO, FridayC, SaturdayO, SaturdayC };
            ChangeGridStates(false);
            ChangeHoursLock(true);
            ChangeBusinessInfoLock(true);
        }

        // Store what TextBoxes each area has so I don't have to repeat long code
        private List<TextBox> businessTextBoxes;

        private List<TextBox> hoursTextBoxes;

        // So I can easily store which textboxes in hours have been changed and set them as parameters for upsert in SQL
        private HashSet<TextBox> changedHours = new HashSet<TextBox>();

        /// <summary>
        /// Enable or Disable grids based on a bool
        /// </summary>
        /// <param name="mode">true or false</param>
        private void ChangeGridStates(bool mode)
        {
            HoursGroup.IsEnabled = mode;
            businessInfoGroup.IsEnabled = mode;
            recentReviewsGroup.IsEnabled = mode;
            CheckInsGroup.IsEnabled = mode;
        }

        /// <summary>
        /// Makes business information read only based on a bool
        /// </summary>
        /// <param name="mode">true or false</param>
        private void ChangeBusinessInfoLock(bool mode)
        {
            CancelBusinessInfoUpdate.IsEnabled = !mode;

            if (mode)
            {
                UpdateBusiness.Content = "Update";
                CancelBusinessInfoUpdate.Visibility = Visibility.Hidden;
            }
            else
            {
                UpdateBusiness.Content = "Submit";
                CancelBusinessInfoUpdate.Visibility = Visibility.Visible;
            }
            businessTextBoxes.ForEach(x => {
                x.Background = BackgroundColor(mode);
                x.IsReadOnly = mode;
            });
        }

        /// <summary>
        /// Makes hours information read only based on a bool
        /// </summary>
        /// <param name="mode">true or false</param>
        private void ChangeHoursLock(bool mode)
        {
            CancelHoursUpdate.IsEnabled = !mode;
            if (mode)
            {
                UpdateHours.Content = "Update";
                CancelHoursUpdate.Visibility = Visibility.Hidden;
            }
            else
            {
                UpdateHours.Content = "Submit";
                CancelHoursUpdate.Visibility = Visibility.Visible;
            }

            hoursTextBoxes.ForEach(x => {
                x.Background = BackgroundColor(mode);
                x.IsReadOnly = mode;
            });
        }

        /// <summary>
        /// Provides white or transparent to set background
        /// </summary>
        /// <param name="mode">true or false</param>
        /// <returns>white or transparent</returns>
        private SolidColorBrush BackgroundColor(bool mode)
        {
            return new SolidColorBrush(mode ? Color.FromArgb(0, 255, 255, 255) : Color.FromArgb(255, 255, 255, 255));
        }

        /// <summary>
        /// Populates the bar chart for checkins per day
        /// </summary>
        /// <param name="business"></param>
        private void LoadBarChartData(Business business)
        {
            ((ColumnSeries)checkinChart.Series[0]).ItemsSource = Query.GetCheckIns(business);
        }

        /// <summary>
        /// Clicks to search for a business
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void SelectSearchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Dictionary<string, string> filters = new Dictionary<string, string>();

            // Don't add filters if they're default text or empty
            if (!SearchBusinessNameBox.Text.Equals("<Name>") || string.IsNullOrEmpty(SearchBusinessNameBox.Text))
            {
                filters.Add("lower(name)", $"%{SearchBusinessNameBox.Text.ToLower()}%");
            }
            if (!SearchBusinessAddressBox.Text.Equals("<Address>") || string.IsNullOrEmpty(SearchBusinessAddressBox.Text))
            {
                filters.Add("lower(address)", $"%{SearchBusinessAddressBox.Text.ToLower()}%");
            }
            if (!SearchBusinessZipcodeBox.Text.Equals("<Zipcode>") || string.IsNullOrEmpty(SearchBusinessZipcodeBox.Text))
            {
                filters.Add("zipcode", $"%P{SearchBusinessZipcodeBox.Text}%");
            }

            // At least one field is filled out
            if (filters.Count > 0)
            {
                SearchBusinessListBox.ClearValue(ItemsControl.ItemsSourceProperty);
                SearchBusinessListBox.Items.Clear();
                SearchBusinessListBox.ItemsSource = Query.BaseQuery("business", filters);
            }

            // User clicked search with all default fields
            else
            {
                SearchBusinessListBox.ClearValue(ItemsControl.ItemsSourceProperty);
                SearchBusinessListBox.Items.Add("Need search criteria");
            }

            // If search name was not found
            if (SearchBusinessListBox.Items.Count == 0)
            {
                SearchBusinessListBox.ClearValue(ItemsControl.ItemsSourceProperty);
                SearchBusinessListBox.Items.Add("No results found");
            }
        }

        /// <summary>
        /// Clear current text in textbox when begin editing
        /// </summary>
        /// <param name="sender">Textbox</param>
        /// <param name="e">N/A</param>
        private void ClearTextBox(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                if (textbox.Text.StartsWith("<") && textbox.Text.EndsWith(">"))
                {
                    textbox.Clear();
                }
            }
        }

        /// <summary>
        /// Restore default text if nothing typed
        /// </summary>
        /// <param name="sender">Textbox</param>
        /// <param name="e">N/A</param>
        private void LeaveTextBox(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                if (string.IsNullOrEmpty(textbox.Text))
                {
                    if (textbox.Name == "SearchBusinessNameBox")
                    {
                        textbox.Text = "<Name>";
                    }
                    else if (textbox.Name == "SearchBusinessAddressBox")
                    {
                        textbox.Text = "<Address>";
                    }
                    else if (textbox.Name == "SearchBusinessZipcodeBox")
                    {
                        textbox.Text = "<Zipcode>";
                    }
                }
            }
        }

        /// <summary>
        /// If user selects a business, populate all grids based on that business
        /// </summary>
        /// <param name="sender">Listbox</param>
        /// <param name="e">N/A</param>
        private void SearchBusinessListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchBusinessListBox.SelectedItem is Business business)
            {
                ChangeGridStates(true);
                Tag = business;
                FillBusinessInformation(business);
                LoadBarChartData(business);
                FillRecentReviews(business);
                FillHoursInformation(business);
            }
        }

        /// <summary>
        /// Populates the hours from a given business
        /// </summary>
        /// <param name="business">Selected business</param>
        private void FillHoursInformation(Business business)
        {
            // List<BusinessHours> hoursList = Query.BusinessHours(business).Cast<BusinessHours>().Distinct().ToList();
            business.Hours = Query.BusinessHours(business).Cast<BusinessHours>().Distinct().ToList();
            foreach (BusinessHours hours in business.Hours)
            {
                // Fill in days based on businesshours attributes
                switch (hours.Day)
                {
                    case "Sunday":
                        SundayC.Text = hours.Closetime;
                        SundayO.Text = hours.Opentime;
                        break;

                    case "Monday":
                        MondayC.Text = hours.Closetime;
                        MondayO.Text = hours.Opentime;
                        break;

                    case "Tuesday":
                        TuesdayC.Text = hours.Closetime;
                        TuesdayO.Text = hours.Opentime;
                        break;

                    case "Wednesday":
                        WednesdayC.Text = hours.Closetime;
                        WednesdayO.Text = hours.Opentime;
                        break;

                    case "Thursday":
                        ThursdayC.Text = hours.Closetime;
                        ThursdayO.Text = hours.Opentime;
                        break;

                    case "Friday":
                        FridayC.Text = hours.Closetime;
                        FridayO.Text = hours.Opentime;
                        break;

                    case "Saturday":
                        SaturdayC.Text = hours.Closetime;
                        SaturdayO.Text = hours.Opentime;
                        break;
                }
            }

            // Any remaining hours boxes that have no hours are marked as 'N/A'
            foreach (TextBox element in HoursGrid.Children)
            {
                if (string.IsNullOrEmpty(element.Text))
                {
                    element.Text = "N/A";
                }
            }
        }

        /// <summary>
        /// Populate business information based on selected business
        /// </summary>
        /// <param name="business">business chosen</param>
        private void FillBusinessInformation(Business business)
        {
            businessName.Text = business.Name;
            businessAddress.Text = business.Address;
            businessCity.Text = business.City;
            businessState.Text = business.State;
            businessZipcode.Text = business.Zipcode.ToString();
            businessStars.Content = business.Stars;
            businessCheckIns.Content = business.Numcheckins;
            businessAvgRating.Content = business.Reviewrating;
            businessReviewCount.Content = business.Reviewcount;
        }

        /// <summary>
        /// Populate recent reviews sorted by date for the business
        /// </summary>
        /// <param name="business">business chosen</param>
        private void FillRecentReviews(Business business)
        {
            BusinessRecentReviewsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            BusinessRecentReviewsGrid.ItemsSource = Query.RecentBusinessReviews(business);
        }

        /// <summary>
        /// Called for every textbox that changes text to see if it differs from the original. Background changes to green if changing
        /// </summary>
        /// <param name="sender">Textbox</param>
        /// <param name="e">N/A</param>
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tag is Business business)
            {
                if (sender is TextBox textbox)
                {
                    Color color;
                    if (BusinessInfoChanged(business, textbox) || BusinessHoursChanged(business, textbox))
                    {
                        // White
                        color = Color.FromArgb(255, 255, 255, 255);
                        if (!textbox.Name.StartsWith("business"))
                        {
                            changedHours.Remove(textbox);
                        }
                    }
                    else
                    {
                        // A greenish color
                        color = Color.FromArgb(255, 153, 255, 153);
                        if (!textbox.Name.StartsWith("business"))
                        {
                            changedHours.Add(textbox);
                        }
                    }
                    textbox.Background = new SolidColorBrush(color);
                }
            }
        }

        /// <summary>
        /// Determines if a business hours box has changed
        /// </summary>
        /// <param name="business">Selected business</param>
        /// <param name="textbox">Which textbox changed</param>
        /// <returns>True or False</returns>
        private bool BusinessHoursChanged(Business business, TextBox textbox)
        {
            List<BusinessHours> theseHours = business.Hours;
            foreach (BusinessHours hours in theseHours)
            {
                string thisDay = hours.Day;
                if (textbox.Name == $"{thisDay}O")
                {
                    return textbox.Text == hours.Opentime;
                }
                if (textbox.Name == $"{thisDay}C")
                {
                    return textbox.Text == hours.Closetime;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if a business info box has changed
        /// </summary>
        /// <param name="business">Selected business</param>
        /// <param name="textbox">Which textbox changed</param>
        /// <returns>True or False</returns>
        private bool BusinessInfoChanged(Business business, TextBox textbox)
        {
            if (textbox.Name == "businessName")
            {
                return textbox.Text == business.Name;
            }
            else if (textbox.Name == "businessAddress")
            {
                return textbox.Text == business.Address;
            }
            else if (textbox.Name == "businessCity")
            {
                return textbox.Text == business.City;
            }
            else if (textbox.Name == "businessState")
            {
                return textbox.Text == business.State;
            }
            else if (textbox.Name == "businessZipcode")
            {
                return textbox.Text == business.Zipcode.ToString();
            }
            return false;
        }

        /// <summary>
        /// User clicks Update for business info, unlocking fields and unlocking "Submit" and "Cancel" buttons
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void UpdateBusiness_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Button says Update
                if (button.Content.ToString() == "Update")
                {
                    ChangeBusinessInfoLock(false);
                    businessTextBoxes.ForEach(x => x.TextChanged += TextChanged);
                }

                // Button says submit
                else
                {
                    ChangeBusinessInfoLock(true);
                    businessTextBoxes.ForEach(x => x.TextChanged -= TextChanged);
                    if (button.Name == "CancelBusinessInfoUpdate")
                    {
                        FillBusinessInformation((Business)Tag);
                    }

                    // Submit pushed
                    else
                    {
                        Business business = (Business)Tag;

                        business.Name = businessName.Text;
                        business.Address = businessAddress.Text;
                        business.City = businessCity.Text;
                        business.State = businessState.Text;

                        // If zip code is bad, maintain what it was so user can't put an invalid zipcode
                        business.Zipcode = int.TryParse(businessZipcode.Text, out int result) ? result : business.Zipcode;

                        try
                        {
                            Query.UpdateBusinessInformationOwnerPage(business);
                            MessageBox.Show("Information updated successfully!", "Success");
                        }
                        catch
                        {
                            MessageBox.Show("Failed to submit information change", "Error");
                        }

                        // Reinitialize the information boxes with new info
                        FillBusinessInformation(SearchBusinessListBox.SelectedItem as Business);
                    }
                }
            }
        }

        /// <summary>
        /// User clicks update for hours information, unlocking fields and unlocking "Submit" and "Cancel" buttons
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void UpdateHours_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Button says Update
                if (button.Content.ToString() == "Update")
                {
                    ChangeHoursLock(false);
                    hoursTextBoxes.ForEach(x => x.TextChanged += TextChanged);
                }

                // Button says Submit
                else
                {
                    ChangeHoursLock(true);
                    hoursTextBoxes.ForEach(x => x.TextChanged -= TextChanged);
                    if (button.Name == "CancelHoursUpdate")
                    {
                        FillHoursInformation((Business)Tag);
                    }

                    // Submit pushed
                    else
                    {
                        foreach (TextBox textbox in changedHours)
                        {
                            string day = textbox.Name.Substring(0, textbox.Name.Length - 1);
                            char change = textbox.Name[textbox.Name.Length - 1];
                            TimeSpan opentime = TimeSpan.Zero;
                            TimeSpan closetime = TimeSpan.Zero;
                            TextBox otherbox = null;

                            foreach (TextBox tb in HoursGrid.Children)
                            {
                                if (tb.Name.StartsWith(day) && !tb.Equals(textbox))
                                {
                                    otherbox = tb;
                                }
                            }

                            if (change.Equals('O'))
                            {
                                opentime = TimeSpan.Parse(textbox.Text);
                                closetime = TimeSpan.Parse(otherbox.Text);
                            }
                            else
                            {
                                closetime = TimeSpan.Parse(textbox.Text);
                                opentime = TimeSpan.Parse(otherbox.Text);
                            }

                            try
                            {
                                Query.UpdateHours(SearchBusinessListBox.SelectedItem as Business, day, opentime, closetime);
                                MessageBox.Show("Hours updated successfully!", "Success");
                            }
                            catch
                            {
                                MessageBox.Show("Failed to submit hours change", "Error");
                            }
                        }

                        // Refresh hours boxes with the business
                        FillHoursInformation(SearchBusinessListBox.SelectedItem as Business);
                    }
                }
            }
        }
    }
}