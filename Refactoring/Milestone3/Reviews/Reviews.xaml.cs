using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Team7GUI
{
    /// <summary>
    /// Interaction logic for Reviews.xaml
    /// </summary>
    public partial class Reviews : Window
    {
        public Reviews(string businessid)
        {
            InitializeComponent();
            WrapReviewTextColumn();
            reviewGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Reviewdate", System.ComponentModel.ListSortDirection.Descending));
            reviewGrid.ClearValue(ItemsControl.ItemsSourceProperty);
            reviewGrid.ItemsSource = Query.BaseQuery("review", new Dictionary<string, string>() { { "businessid", businessid } });
        }

        /// <summary>
        /// Enable wrapping just for text
        /// </summary>
        private void WrapReviewTextColumn()
        {
            // Allows the reviewtext section to wrap
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
            ((DataGridTextColumn)reviewGrid.Columns[3]).ElementStyle = style;
        }
    }
}
