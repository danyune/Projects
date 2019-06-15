using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Team7GUI.Information;

namespace Team7GUI.BusinessPage
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BusinessUserControl : UserControl, INotifyPropertyChanged
    {
        public BusinessUserControl()
        {
            InitializeComponent();
            RunBusinessInitialization();
        }

        // Variables to use
        public event PropertyChangedEventHandler PropertyChanged;
        private CollectionViewSource businessCollection = new CollectionViewSource();
        private string _orderby = string.Empty;

        /// <summary>
        /// Wasn't sure how to do this in XAML, so they're just sorting mechanics and creating a new event
        /// </summary>
        private void RunBusinessInitialization()
        {
            AddStates();

            // Need to initialize a source so I can access CollectionViewSource properties on first search
            businessCollection.Source = new List<Business>();

            // Create a new event for when businessGrid ItemsSource refreshes
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpd != null)
            {
                dpd.AddValueChanged(businessGrid, BusinessGridSourceUpdate);
            }

            // Statebox properties
            stateBox.Items.SortDescriptions.Add(new SortDescription("State", ListSortDirection.Ascending));
            stateBox.DisplayMemberPath = "State";

            // Citybox properties
            cityBox.Items.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
            cityBox.DisplayMemberPath = "City";

            // Zipbox properties
            zipBox.Items.SortDescriptions.Add(new SortDescription("Zipcode", ListSortDirection.Ascending));
            zipBox.DisplayMemberPath = "Zipcode";

            // Categorybox properties
            categoryBox.Items.SortDescriptions.Add(new SortDescription("Total", ListSortDirection.Descending));

            // Attributebox properties
            attributesFilterList.Items.SortDescriptions.Add(new SortDescription("Total", ListSortDirection.Descending));
        }

        /// <summary>
        /// Update the counts of businesses, categories selected, and attributes selected.
        /// </summary>
        private void UpdateCounts()
        {
            numberOfBusinesses.Text = "Number of Businesses found: " + businessGrid.Items.Count.ToString();
            SelectedCategoryCount.Text = categoryBox.SelectedItems.Count.ToString();
            SelectedAttributesCount.Text = attributesFilterList.SelectedItems.Count.ToString();
        }

        /// <summary>
        /// Populates the State dropdown menu
        /// </summary>
        private void AddStates()
        {
            stateBox.ClearValue(ItemsControl.ItemsSourceProperty);
            stateBox.ItemsSource = Query.BaseQuery("business", new Dictionary<string, string>() { { "Select", "distinct state" }, { "OrderBy", "State" } });
        }

        /// <summary>
        /// Basically reset all the checkmarks
        /// </summary>
        /// <param name="mode">true or false</param>
        private void ChangeBusinessStates(bool mode)
        {
            // Enable or disable elements based on choice
            filterMeal.IsEnabled = mode;
            filterPrice.IsEnabled = mode;
            filterAttributes.IsEnabled = mode;
            filterDistance.IsEnabled = mode;
        }

        /// <summary>
        /// Open Map of selected businesses on business tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BusinessMap_Click(object sender, RoutedEventArgs e)
        {
            // Only show pins for selected business(es)
            if (businessGrid.SelectedItems.Count > 0)
            {
                new Maps.MapData(businessGrid.SelectedItems.Cast<Business>().Distinct().ToList());
            }
            // No business selected, so show all businesses pins
            else
            {
                new Maps.MapData(businessGrid.ItemsSource.Cast<Business>().Distinct().ToList());
            }
        }

        /// <summary>
        /// When a state is chosen on the dropdown combobox
        /// </summary>
        /// <param name="sender">N/a</param>
        /// <param name="e">N/a</param>
        private void StateBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (stateBox.SelectedValue is Business business)
            {
                // Call clear so if user changes state after already having results, clears everything
                cityBox.ClearValue(ItemsControl.ItemsSourceProperty);
                zipBox.ClearValue(ItemsControl.ItemsSourceProperty);
                ChangeStateOrCityOrZip();
                ChangeBusinessStates(false);

                cityBox.ItemsSource = Query.BaseQuery("business", new Dictionary<string, string>()
                {
                    { "Select", "distinct city, state" },
                    { "State", business.State }
                });
            }
        }

        /// <summary>
        /// When a user selects a city
        /// </summary>
        /// <param name="sender">N/a</param>
        /// <param name="e">N/a</param>
        private void CityBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cityBox.SelectedValue is Business business)
            {
                zipBox.ClearValue(ItemsControl.ItemsSourceProperty);
                ChangeStateOrCityOrZip();
                ChangeBusinessStates(false);

                zipBox.ItemsSource = Query.BaseQuery("business", new Dictionary<string, string>()
                {
                    { "Select", "distinct city, state, zipcode" },
                    { "State", business.State },
                    { "City", business.City }
                });
            }
        }

        /// <summary>
        /// When user selects a zip code
        /// </summary>
        /// <param name="sender">N/a</param>
        /// <param name="e">N/a</param>
        private void ZipBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (zipBox.SelectedValue is Business business)
            {
                ChangeStateOrCityOrZip();
            }
        }

        /// <summary>
        /// When user selects a business on the search results
        /// </summary>
        /// <param name="sender">N/a</param>
        /// <param name="e">N/a</param>
        private void BusinessGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            friendsReviews.ClearValue(ItemsControl.ItemsSourceProperty);
            if (businessGrid.SelectedItem is Business selectedBusiness)
            {
                Dictionary<string, string> businessInformation = Query.BusinessInformation(selectedBusiness);

                // Activates the business information area. Stays gray until a proper business is selected
                businessInfoGroup.IsEnabled = true;
                businessInfoBox.Text = new StringBuilder().AppendLine(selectedBusiness.Name).AppendLine(selectedBusiness.Address).AppendLine(selectedBusiness.City + ", " + selectedBusiness.State + " " + selectedBusiness.Zipcode).ToString();
                businessAttributesTextBox.Text = businessInformation["attributes"];
                businessHoursTextBox.Text = businessInformation["hours"];
                businessCategoriesTextBox.Text = businessInformation["categories"];
                friendsReviews.ItemsSource = Query.BusinessFriendsReviews(selectedBusiness, Tag as Users);
            }

            // Means the business grid was cleared (such as changing state or city), so we deactivate the business info area
            else
            {
                businessInfoGroup.IsEnabled = false;
                businessInfoBox.Text = string.Empty;
                businessAttributesTextBox.Text = string.Empty;
                businessHoursTextBox.Text = string.Empty;
                businessCategoriesTextBox.Text = string.Empty;
            }

            // Restore visible elements to default
            friendsReviews.Visibility = Visibility.Visible;
            writeReviewGroup.Visibility = Visibility.Hidden;
            writeReviewButton.Content = "Write a Review";
        }

        /// <summary>
        /// When a category filter is added or removed
        /// </summary>
        /// <param name="sender">Listbox</param>
        /// <param name="e">N/A</param>
        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBusinessList();
        }

        /// <summary>
        /// When user selects an attribute
        /// </summary>
        /// <param name="sender">Listbox</param>
        /// <param name="e">N/A</param>
        private void AttributesFilterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBusinessList();
        }

        /// <summary>
        /// The Write Review button simply changes the interface to show a textbox and rating dropdown. Same button will go back to default
        /// </summary>
        /// <param name="sender">N/a</param>
        /// <param name="e">N/a</param>
        private void WriteReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (writeReviewButton.Content.ToString() == "Write a Review")
            {
                friendsReviews.Visibility = Visibility.Hidden;
                writeReviewGroup.Visibility = Visibility.Visible;
                writeReviewButton.Content = "      Back to\nFriends Reviews";
            }
            else
            {
                writeReviewBox.Clear();
                ratingComboBox.SelectedIndex = 0;
                friendsReviews.Visibility = Visibility.Visible;
                writeReviewGroup.Visibility = Visibility.Hidden;
                writeReviewButton.Content = "Write a Review";
            }
        }

        /// <summary>
        /// When user wants to submit review
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void SubmitReviewButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrEmpty(writeReviewBox.Text))
            {
                MessageBox.Show("Review must have a comment", "Comment required");
                return;
            }
            if (ratingComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Review must have a rating", "Rating required");
                return;
            }

            Review review = new Review
            {
                Userid = ((Users)Tag).Userid,
                Businessid = ((Business)businessGrid.SelectedItem).Businessid,
                Stars = Convert.ToDouble(ratingComboBox.Text),
                Reviewtext = writeReviewBox.Text
            };

            try
            {
                Query.SubmitReview(review);
            }
            catch
            {
                MessageBox.Show("Review submission failure", "Failure");
            }
            finally
            {
                ratingComboBox.SelectedIndex = 0;
            }

            // Stores current business selected
            int currentBusinessIndex = businessGrid.SelectedIndex;

            // Invoke a refresh of businessgrid
            UpdateBusinessList();

            // Restore business selected after submitting review
            businessGrid.SelectedIndex = currentBusinessIndex;

            MessageBox.Show("Successfully submitted review", "Success!");
        }

        /// <summary>
        /// Add select businesses to favorite
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void AddToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Tag is Users users)
            {
                try
                {
                    Query.AddToFavorites(users, businessGrid.SelectedItems.Cast<Business>().Distinct().ToList());
                    MessageBox.Show("Successfully added to favorites!", "Success");
                    NotifyPropertyChanged("Favorites");
                }
                catch
                {
                    MessageBox.Show("Failed to add to favorites", "Error");
                }
            }
        }

        /// <summary>
        /// Open Reviews window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            Reviews reviewpage = new Reviews(((Business)businessGrid.SelectedItem).Businessid)
            {
                Owner = Window.GetWindow(this)
            };
            reviewpage.Show();
        }

        /// <summary>
        /// Open Checkins Chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowCheckInsButton_Click(object sender, RoutedEventArgs e)
        {
            if (businessGrid.SelectedItem is Business business)
            {
                Checkins.Checkins checkinpage = new Checkins.Checkins(business)
                {
                    Owner = Window.GetWindow(this)
                };
                checkinpage.Show();
            }
        }

        /// <summary>
        /// Checks into the business
        /// </summary>
        /// <param name="sender">N/a</param>
        /// <param name="e">N/a</param>
        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Query.CheckIn(businessGrid.SelectedItem as Business);
                MessageBox.Show("Checked in Successfully!", "Success!");
            }
            catch
            {
                MessageBox.Show("Failed to check in!", "Failure!");
            }

            // Stores current business selected
            int currentBusinessIndex = businessGrid.SelectedIndex;

            // Invoke a refresh of businessgrid
            UpdateBusinessList();

            // Restore business selected after checking in
            businessGrid.SelectedIndex = currentBusinessIndex;
        }

        /// <summary>
        /// Called when the businessGrid itemssource changes
        /// </summary>
        /// <param name="sender">businessGrid</param>
        /// <param name="e">itemssource changed event</param>
        private void BusinessGridSourceUpdate(object sender, EventArgs e)
        {
            // Won't run if grid is cleared, such as changing state or city
            if (businessGrid.Items.Count > 0)
            {
                // Tuple of structures to store information of which checkmarks to enable
                (bool[] prices, Dictionary<string, bool> meals) = Query.BusinessPriceMealsList(businessCollection.View.Cast<BaseInfo>().ToList());
                priceOneCheck.IsEnabled = prices[0];
                priceTwoCheck.IsEnabled = prices[1];
                priceThreeCheck.IsEnabled = prices[2];
                priceFourCheck.IsEnabled = prices[3];

                breakfastCheck.IsEnabled = meals["breakfast"];
                brunchCheck.IsEnabled = meals["brunch"];
                dessertCheck.IsEnabled = meals["dessert"];
                dinnerCheck.IsEnabled = meals["dinner"];
                lateNightCheck.IsEnabled = meals["latenight"];
                lunchCheck.IsEnabled = meals["lunch"];
            }
        }

        /// <summary>
        /// Single event method for when any checkmark is clicked for prices and meals
        /// </summary>
        /// <param name="sender">Checkbox</param>
        /// <param name="e">N/A</param>
        private void Check_Click(object sender, RoutedEventArgs e)
        {
            UpdateBusinessList();
        }

        /// <summary>
        /// Update business grid with given filters from the GUI
        /// </summary>
        private void UpdateBusinessList(List<string> additionalFilters = null)
        {
            // Just getting the state, city, and zip from the zipcode selection
            if (zipBox.SelectedItem is Business business)
            {
                ChangeBusinessStates(true);

                // Build the list of selected categories and attributes that are selected so we know what to set as selected again
                List<BusinessCategory> categories = BuildCategoryList();
                List<BusinessAttribute> attributes = BuildAttributeList();

                // Reset businessgrid with new filters
                businessCollection.Source = Query.UpdateBusinessGrid(business, categories, BuildPriceChecks(), BuildMealsChecks(), attributes, (Users)Tag, _orderby, additionalFilters);
                businessGrid.ItemsSource = businessCollection.View;

                // Gotta unsubscribe and resubscribe after changing the categories list to prevent infinite loop
                categoryBox.SelectionChanged -= CategoryBox_SelectionChanged;
                categoryBox.ClearValue(ItemsControl.ItemsSourceProperty);
                categoryBox.ItemsSource = Query.BusinessCategories(businessCollection.View.Cast<BaseInfo>().ToList());

                foreach (BusinessCategory category in categories)
                {
                    categoryBox.SelectedItems.Add(category);
                }
                categoryBox.SelectionChanged += CategoryBox_SelectionChanged;

                // Gotta unsubscribe and resubscribe after changing the attributes list to prevent infinite loop
                attributesFilterList.SelectionChanged -= AttributesFilterList_SelectionChanged;
                attributesFilterList.ClearValue(ItemsControl.ItemsSourceProperty);
                attributesFilterList.ItemsSource = Query.BusinessAttributes(businessCollection.View.Cast<BaseInfo>().ToList());

                foreach (BusinessAttribute attribute in attributes)
                {
                    attributesFilterList.SelectedItems.Add(attribute);
                }
                attributesFilterList.SelectionChanged += AttributesFilterList_SelectionChanged;
                businessGrid.SelectedItems.Clear();

                // Determine which price checkmark is checked, if any
                string BuildPriceChecks()
                {
                    if (priceOneCheck.IsChecked == true)
                    {
                        return "1";
                    }
                    else if (priceTwoCheck.IsChecked == true)
                    {
                        return "2";
                    }
                    else if (priceThreeCheck.IsChecked == true)
                    {
                        return "3";
                    }
                    else if (priceFourCheck.IsChecked == true)
                    {
                        return "4";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }

                // Determine which meals are checked
                List<string> BuildMealsChecks()
                {
                    List<string> mealsBuilder = new List<string>();
                    if (breakfastCheck.IsChecked == true)
                    {
                        mealsBuilder.Add("breakfast");
                    }
                    if (brunchCheck.IsChecked == true)
                    {
                        mealsBuilder.Add("brunch");
                    }
                    if (dessertCheck.IsChecked == true)
                    {
                        mealsBuilder.Add("dessert");
                    }
                    if (dinnerCheck.IsChecked == true)
                    {
                        mealsBuilder.Add("dinner");
                    }
                    if (lateNightCheck.IsChecked == true)
                    {
                        mealsBuilder.Add("latenight");
                    }
                    if (lunchCheck.IsChecked == true)
                    {
                        mealsBuilder.Add("lunch");
                    }

                    return mealsBuilder;
                }
            }

            businessGrid.Items.Refresh();
            UpdateCounts();

        }

        /// <summary>
        /// Build which categories are selected
        /// </summary>
        /// <returns>List of categories</returns>
        private List<BusinessCategory> BuildCategoryList() => categoryBox.SelectedItems.Cast<BusinessCategory>().Distinct().ToList();

        /// <summary>
        /// Build which attributes are selected
        /// </summary>
        /// <returns>List of attributes</returns>
        private List<BusinessAttribute> BuildAttributeList() => attributesFilterList.SelectedItems.Cast<BusinessAttribute>().Distinct().ToList();

        /// <summary>
        /// Unselect all attributes
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void ResetAttributes_Click(object sender, RoutedEventArgs e)
        {
            attributesFilterList.UnselectAll();
        }

        /// <summary>
        /// Unselect all categories
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void ResetCategories_Click(object sender, RoutedEventArgs e)
        {
            categoryBox.UnselectAll();
        }

        /// <summary>
        /// Reset all filters and show all businesses in a zipcode
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void ResetBusinesses_Click(object sender, RoutedEventArgs e)
        {
            ResetInterface();
        }

        /// <summary>
        /// Notify main window that Business selected changed
        /// </summary>
        /// <param name="propertyName">currentBusiness</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Just show what current user is
        /// </summary>
        /// <param name="sender">TabItem Business</param>
        /// <param name="e">Got Focus</param>
        private void BusinessUserControl_GotFocus(object sender, RoutedEventArgs e) => tempUserBox.Text = $"Current User: {((Users)Tag).Name}";
        
        /// <summary>
        /// Clicks a header in the businessGrid
        /// </summary>
        /// <param name="sender">DataGridColumnHeader</param>
        /// <param name="e">clicked</param>
        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridColumnHeader dgch)
            {
                // If the header is blue or not colored, then set to orange and display in ascending order
                if (dgch.Background == null || dgch.Background.ToString().Equals("#FF00FFFF"))
                {
                    ResetBusinessGridHeaders();
                    dgch.Background = new SolidColorBrush(Colors.Orange);
                    _orderby = dgch.Column.SortMemberPath;
                    UpdateBusinessList();
                }
                // Header is orange, so clicking orange changes to Aqua and displays in descending order
                else
                {
                    dgch.Background = new SolidColorBrush(Colors.Aqua);
                    _orderby = dgch.Column.SortMemberPath + " desc ";
                    UpdateBusinessList();
                }
            }
        }

        /// <summary>
        /// Remove backgrounds from all headers in businessGrid
        /// </summary>
        private void ResetBusinessGridHeaders()
        {
            foreach (DataGridColumn dgc in businessGrid.Columns)
            {
                DataGridColumnHeader dgch = GetHeader(dgc, businessGrid);
                if (dgch != null)
                {
                    dgch.Background = null;
                }
            }
            _orderby = string.Empty;

            // Get the header element of each column
            DataGridColumnHeader GetHeader(DataGridColumn column, DependencyObject reference)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(reference); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(reference, i);

                    DataGridColumnHeader colHeader = child as DataGridColumnHeader;
                    if ((colHeader != null) && (colHeader.Column == column))
                    {
                        return colHeader;
                    }

                    colHeader = GetHeader(column, child);
                    if (colHeader != null)
                    {
                        return colHeader;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Distance filtering the businesses
        /// </summary>
        /// <param name="sender">ComboBoxItem</param>
        /// <param name="e">Information</param>
        private void DistanceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (distanceBox.SelectedItem is ComboBoxItem cbi)
            {
                string[] distanceBoxContents = cbi.Content.ToString().Split(' ');
                if (distanceBoxContents.Length > 0)
                {
                    if (double.TryParse(distanceBoxContents[0], out double distance))
                    {
                        UpdateBusinessList(new List<string>() { $"distance < {distance}" });
                        //List<BaseInfo> businesses = businessGrid.ItemsSource.Cast<Business>().ToList().Where(x => x.Distance <= distance).Cast<BaseInfo>().ToList();
                        //RefreshBusinessGrid(businesses);
                    }
                    else
                    {
                        UpdateBusinessList();
                    }
                }
            }
        }

        /// <summary>
        /// Reset item sources when a state, city, or zip changes
        /// </summary>
        private void ChangeStateOrCityOrZip()
        {
            businessGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            categoryBox.ClearValue(ItemsControl.ItemsSourceProperty);
            attributesFilterList.ClearValue(ItemsControl.ItemsSourceProperty);
            ResetInterface();
        }

        /// <summary>
        /// Only reset filters, checks, and sorting
        /// </summary>
        private void ResetInterface()
        {
            _orderby = string.Empty;
            attributesFilterList.UnselectAll();
            categoryBox.UnselectAll();
            ResetBusinessGridHeaders();
            priceOneCheck.IsChecked = false;
            priceTwoCheck.IsChecked = false;
            priceThreeCheck.IsChecked = false;
            priceFourCheck.IsChecked = false;
            breakfastCheck.IsChecked = false;
            brunchCheck.IsChecked = false;
            dessertCheck.IsChecked = false;
            dinnerCheck.IsChecked = false;
            lateNightCheck.IsChecked = false;
            lunchCheck.IsChecked = false;
            distanceBox.SelectedIndex = 0;
            UpdateBusinessList();
        }

        /// <summary>
        /// Reinitializes businessgrid item source based on an IEnumerable list of businesses
        /// </summary>
        /// <param name="businesses"></param>
        private void RefreshBusinessGrid(IEnumerable<BaseInfo> businesses)
        {
            SortDescriptionCollection sdc = new SortDescriptionCollection();
            businessCollection.Source = businesses;
            businessGrid.Items.SortDescriptions.ToList().ForEach(x => sdc.Add(x));
            businessGrid.ItemsSource = businessCollection.View;
            sdc.ToList().ForEach(x => businessGrid.Items.SortDescriptions.Add(x));
            businessGrid.Items.Refresh();
        }
    }
}