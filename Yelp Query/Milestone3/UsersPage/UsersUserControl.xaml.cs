using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Team7GUI.Information;

namespace Team7GUI.UsersPage
{
    /// <summary>
    /// Interaction logic for UsersUserControl.xaml
    /// </summary>
    public partial class UsersUserControl : UserControl, INotifyPropertyChanged
    {
        public UsersUserControl()
        {
            InitializeComponent();
            RunUsersInitialization();
        }

        private Users currentUser;

        public Users CurrentUser
        {
            get => currentUser;
            set
            {
                if (currentUser == value)
                {
                    return;
                }

                currentUser = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Populate favorite business grid
        /// </summary>
        /// <param name="currentUser"></param>
        public void FillFavoriteBusinessGrid()
        {
            favoritesGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            favoritesGrid.ItemsSource = Query.FavoriteBusiness(currentUser);
        }

        /// <summary>
        /// Initialize some properties for the Users page
        /// </summary>
        private void RunUsersInitialization()
        {
            // Searched users ordering
            selectUserListBox.Items.SortDescriptions.Add(new SortDescription("SearchResult", ListSortDirection.Ascending));
            selectUserListBox.DisplayMemberPath = "SearchResult";

            // Users friends list
            friendsGrid.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // Users favorite businesses
            favoritesGrid.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // Users recent reviews
            recentReviewsGrid.Items.SortDescriptions.Add(new SortDescription("Businessname", ListSortDirection.Ascending));

            // Friends reviews
            friendsReviewsGrid.Items.SortDescriptions.Add(new SortDescription("Reviewdate", ListSortDirection.Descending));

            // Make elements all disabled until a user is selected
            ChangeGridStates(false);
        }

        private void FillUserList()
        {
            // Clear results and disable information sections until a valid result chosen
            ChangeGridStates(false);
            ClearGrids();
            selectUserListBox.DisplayMemberPath = "SearchResult";
            selectUserListBox.ClearValue(ItemsControl.ItemsSourceProperty);
            selectUserListBox.Items.Clear();
            selectUserListBox.ItemsSource = Query.SearchUsers(selectUserSearchTextBox.Text);

            // If search name was not found
            if (selectUserListBox.Items.Count == 0)
            {
                selectUserListBox.DisplayMemberPath = string.Empty;
                selectUserListBox.ClearValue(ItemsControl.ItemsSourceProperty);
                selectUserListBox.Items.Add("No results found");
            }

            void ClearGrids()
            {
                // Reset User Information to empty
                userName.Text = string.Empty;
                userStars.Text = string.Empty;
                userFans.Text = string.Empty;
                userYelpingSince.Text = string.Empty;
                userFunny.Text = string.Empty;
                userCool.Text = string.Empty;
                userUseful.Text = string.Empty;
                userLatitude.Text = string.Empty;
                userLongitude.Text = string.Empty;

                // Clear dataGrids
                friendsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
                favoritesGrid.ClearValue(ItemsControl.ItemsSourceProperty);
                friendsReviewsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
                recentReviewsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            }
        }

        /// <summary>
        /// Fill in information grids based off input user
        /// </summary>
        /// <param name="users">Selected User</param>
        private void FillAllSections(Users users)
        {
            CurrentUser = users;
            ChangeGridStates(true);
            FillUserInformation();
            FillFavoriteBusinessGrid();
            FillFriendsInformation();
            FillRecentReviewsGrid();
            FillFriendsReviewsGrid();
        }

        /// <summary>
        /// Populates the users results based on search parameter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectSearchButton_Click(object sender, RoutedEventArgs e)
        {
            FillUserList();
        }

        /// <summary>
        /// Clears the default text in the search box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectUserSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear the "<search a user>" dialogue
            selectUserSearchTextBox.Clear();
        }

        /// <summary>
        /// If user clicks to search but types nothing (such as clicking another tab)
        /// </summary>
        /// <param name="sender">N/A</param>
        /// <param name="e">N/A</param>
        private void SelectUserSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (selectUserListBox.Items.Count == 0 && string.IsNullOrEmpty(selectUserSearchTextBox.Text))
            {
                selectUserSearchTextBox.Text = "<Search a user>";
            }
        }

        /// <summary>
        /// Actor selects a user and that currentUser populates all the fields and grids
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectUserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Result must be selected to populate. If user selects the 'No results found', will not trigger
            if (selectUserListBox.SelectedItem is Users users)
            {
                FillAllSections(users);
            }
        }

        /// <summary>
        /// Opens a map of the current users location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserShowMapButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectUserListBox.SelectedItem is Users users)
            {
                new Maps.MapData(users);
            }
        }

        /// <summary>
        /// Enables or Disables all grid elements except the search a user area
        /// </summary>
        /// <param name="mode">True or false</param>
        private void ChangeGridStates(bool mode)
        {
            userInformationGroup.IsEnabled = mode;
            favoritesGroup.IsEnabled = mode;
            friendsGroup.IsEnabled = mode;
            recentReviewsGroup.IsEnabled = mode;
            friendsReviewsGroup.IsEnabled = mode;
        }

        /// <summary>
        /// Populate user information based off the instance of a User
        /// </summary>
        /// <param name="currentUser">Selected user from the search list results</param>
        private void FillUserInformation()
        {
            userName.Text = currentUser.Name;
            userStars.Text = currentUser.Averagestars.ToString("0.00");
            userFans.Text = currentUser.Fans.ToString();
            userYelpingSince.Text = currentUser.Yelpingsince;
            userFunny.Text = currentUser.Funny.ToString();
            userCool.Text = currentUser.Cool.ToString();
            userUseful.Text = currentUser.Useful.ToString();
            userLatitude.Text = Math.Round(currentUser.Latitude, 4).ToString();
            userLongitude.Text = Math.Round(currentUser.Longitude, 4).ToString();
        }

        /// <summary>
        /// Populate the friends data grid
        /// </summary>
        /// <param name="currentUser"></param>
        private void FillFriendsInformation()
        {
            friendsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            friendsGrid.ItemsSource = Query.FriendsInformation(currentUser);
        }

        /// <summary>
        /// Populate the Friends Reviews
        /// </summary>
        /// <param name="currentUser"></param>
        private void FillFriendsReviewsGrid()
        {
            friendsReviewsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            friendsReviewsGrid.ItemsSource = Query.FriendsReviews(currentUser);
        }

        /// <summary>
        /// Populates the Recent Reviews
        /// </summary>
        /// <param name="currentUser"></param>
        private void FillRecentReviewsGrid()
        {
            recentReviewsGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            recentReviewsGrid.ItemsSource = Query.RecentReviews(currentUser);
        }

        /// <summary>
        /// Opens Map of business that was double clicked on users tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FavoritesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && e.ChangedButton.ToString().Equals("Left"))
            {
                if (favoritesGrid.SelectedItem is Business currentBusiness)
                {
                    new Maps.MapData(currentBusiness);
                }
            }
        }

        /// <summary>
        /// User clicks update to update name and coordinates
        /// </summary>
        /// <param name="sender">Button</param>
        /// <param name="e">N/A</param>
        private void UserUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateUserInfo uui = new UpdateUserInfo(currentUser)
            {
                Owner = Window.GetWindow(this)
            };
            uui.ShowDialog();

            FillUserList();
            selectUserListBox.SelectedItem = selectUserListBox.ItemsSource.Cast<Users>().First(x => x.Userid.Equals(currentUser.Userid));
            FillAllSections(selectUserListBox.SelectedItem as Users);
        }

        /// <summary>
        /// Clicks the ContextMenu Remove option
        /// </summary>
        /// <param name="sender">ContextMenu</param>
        /// <param name="e">n/a</param>
        private void RemoveFromfavorite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Query.RemoveFromFavorites(currentUser, favoritesGrid.SelectedItem as Business);
                FillFavoriteBusinessGrid();
                MessageBox.Show("Successfully removed from favorites", "Success");
            }
            catch
            {
                MessageBox.Show("Failed to remove favorite", "Error");
            }
        }

        /// <summary>
        /// Just so the contextmenu will close
        /// </summary>
        /// <param name="sender">n/a</param>
        /// <param name="e">n/a</param>
        private void FavoritesGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            favoritesGrid.ContextMenu.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Gray out the option if not right clicked while on a row
        /// </summary>
        /// <param name="sender">n/a</param>
        /// <param name="e">n/a</param>
        private void FavoritesGrid_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            favoritesGrid.ContextMenu.IsEnabled = true;
            favoritesGrid.ContextMenu.Visibility = Visibility.Visible;
            if (!(e.OriginalSource is TextBlock))
            {
                favoritesGrid.ContextMenu.IsEnabled = false;
            }
        }

        /// <summary>
        /// Notifies subscribers that User selection changed
        /// </summary>
        /// <param name="propertyName">currentUser</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}