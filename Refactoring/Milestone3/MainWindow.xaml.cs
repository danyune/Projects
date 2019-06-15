using System.ComponentModel;
using System.Windows;
using Team7GUI.BusinessPage;
using Team7GUI.OwnerInfoPage;
using Team7GUI.UsersPage;

namespace Team7GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            businessTab.IsEnabled = false;
            FillTabs();
        }

        // Store to access them in all methods
        private UsersUserControl userpage;
        private BusinessUserControl businesspage;
        private OwnerInfo ownerpage;

        /// <summary>
        /// Initialize the tabs to have the proper user controls
        /// </summary>
        private void FillTabs()
        {
            // Enable a propertychanged event when user selects a different user
            userpage = new UsersUserControl();
            usersTab.Content = userpage;
            userpage.PropertyChanged += UserSelectionChanged;

            // Enable a propertychanged event when a user adds a favorite
            businesspage = new BusinessUserControl();
            businessTab.Content = businesspage;
            businesspage.PropertyChanged += BusinessSelectionChanged;

            ownerpage = new OwnerInfo();
            ownerTab.Content = ownerpage;
        }

        /// <summary>
        /// When a new user is selected on the userspage, store it in the businesspage
        /// </summary>
        /// <param name="sender">Userpage</param>
        /// <param name="e">N/A</param>
        private void UserSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            businesspage.Tag = userpage.CurrentUser;
            businessTab.IsEnabled = true;
        }

        /// <summary>
        /// When a new business is selected on the businesspage, store it in the userpage
        /// </summary>
        /// <param name="sender">Businesspage</param>
        /// <param name="e">N/A</param>
        private void BusinessSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Favorites")
            {
                userpage.FillFavoriteBusinessGrid();
            }
        }
    }
}