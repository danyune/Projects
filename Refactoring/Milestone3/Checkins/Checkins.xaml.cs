using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;
using Team7GUI.Information;

namespace Team7GUI.Checkins
{
    /// <summary>
    /// Interaction logic for Checkins.xaml
    /// </summary>
    public partial class Checkins : Window
    {
        public Checkins(Business business)
        {
            InitializeComponent();
            LoadBarChartData(business);
        }

        /// <summary>
        /// Retrieves Dictionary of days and values for checkins
        /// </summary>
        /// <param name="business">business</param>
        private void LoadBarChartData(Business business)
        {
            ((ColumnSeries)checkinChart.Series[0]).ItemsSource = Query.GetCheckIns(business);
        }
    }
}