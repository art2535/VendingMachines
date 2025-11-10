using LiveCharts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VendingMachines.Desktop.Services;

namespace VendingMachines.Desktop.Account.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly ApiService _apiService;
        private bool _isByAmount = true;
        private readonly string _jwtToken;

        public MainPage(string token)
        {
            InitializeComponent();

            NetworkStatusChart.Series[0].Values = new ChartValues<int>();
            NetworkStatusChart.Series[1].Values = new ChartValues<int>();

            _jwtToken = token;
            _apiService = new ApiService(_jwtToken);
            LoadDataAsync();
        }

        private void ByAmountButton_Click(object sender, RoutedEventArgs e)
        {
            _isByAmount = true;
            ByAmountButton.Background = Brushes.Blue;
            ByAmountButton.Foreground = Brushes.White;
            ByQuantityButton.Background = Brushes.White;
            ByQuantityButton.Foreground = Brushes.Black;
            LoadSalesDataAsync();
        }

        private void ByQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            _isByAmount = false;
            ByQuantityButton.Background = Brushes.Blue;
            ByQuantityButton.Foreground = Brushes.White;
            ByAmountButton.Background = Brushes.White;
            ByAmountButton.Foreground = Brushes.Black;
            LoadSalesDataAsync();
        }

        private async void LoadSalesDataAsync()
        {
            var startDate = new DateTime(2025, 1, 15);
            var endDate = new DateTime(2025, 1, 31);
            var salesData = await _apiService.GetSalesTrendAsync(startDate, endDate, _isByAmount);

            var values = salesData.Select(s => s.Value).ToArray();
            SalesChart.Series[0].Values = new ChartValues<decimal>(values);
            SalesAxisX.Labels = salesData.Select(s => DateTime.Parse(s.Date).ToString("dd.MM")).ToArray();

            var minValue = values.Min() * 0.9m;
            var maxValue = values.Max() * 1.1m;

            SalesChart.AxisY[0].MinValue = (double)minValue;
            SalesChart.AxisY[0].MaxValue = (double)maxValue;
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Эффективность и состояние сети
                var networkStatus = await _apiService.GetNetworkStatusAsync();
                EfficiencyGauge.Value = networkStatus.Efficiency;
                NetworkStatusChart.Series[0].Values = new ChartValues<int> { networkStatus.Active };
                NetworkStatusChart.Series[1].Values = new ChartValues<int> { networkStatus.Inactive };

                // Сводка
                var summary = await _apiService.GetSummaryAsync();
                MoneyInTA.Text = $"{summary.MoneyInTA:F2} р.";
                ChangeInTA.Text = $"{summary.ChangeInTA:F2} р.";
                RevenueToday.Text = $"{summary.RevenueToday:F2} р.";
                RevenueYesterday.Text = $"{summary.RevenueYesterday:F2} р.";
                CollectedToday.Text = $"{summary.CollectedToday:F2} р.";
                CollectedYesterday.Text = $"{summary.CollectedYesterday:F2} р.";
                Serviced.Text = $"{summary.ServicedToday}/{summary.ServicedYesterday}";

                // Динамика продаж
                LoadSalesDataAsync();

                // Новости
                var notifications = await _apiService.GetNotificationsAsync();
                NotificationsList.ItemsSource = notifications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
    }
}
