using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VendingMachines.API.DTOs.Devices;
using VendingMachines.Desktop.Models;
using VendingMachines.Desktop.Services;

namespace VendingMachines.Desktop.Account.Pages
{
    /// <summary>
    /// Логика взаимодействия для VendingMachinesPage.xaml
    /// </summary>
    public partial class VendingMachinesPage : Page
    {
        private readonly string _url = "https://localhost:7270/api/devices";
        private readonly string _jwtToken;
        private readonly HttpClient _httpClient;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount = 0;

        public VendingMachinesPage(string jwtToken)
        {
            InitializeComponent();
            _jwtToken = jwtToken;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_url)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadDevicesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonAddItem_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddVendingMachinePage(_jwtToken, null));
        }

        private void ButtonExportData_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить файл как...",
                FileName = "export",
                Filter = "PDF файлы (*.pdf)|*.pdf|CSV файлы (*.csv)|*.csv|HTML файлы (*.html;*.htm)|*.html;*.htm",
                DefaultExt = "pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                switch (Path.GetExtension(saveFileDialog.FileName)?.ToLower())
                {
                    case ".pdf":
                        ExportService.ExportToPdf(saveFileDialog.FileName);
                        break;
                    case ".csv":
                        ExportService.ExportToCsv(saveFileDialog.FileName);
                        break;
                    case ".html":
                    case ".htm":
                        ExportService.ExportToHtml(saveFileDialog.FileName);
                        break;
                    default:
                        MessageBox.Show("Неподдерживаемый формат файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void ButtonEditItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DeviceListItem device)
            {
                NavigationService.Navigate(new AddVendingMachinePage(_jwtToken, device));
            }
        }

        private async void ButtonDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DeviceListItem device)
            {
                if (MessageBox.Show($"Удалить запись с ID = {device.Id}?", "Предупреждение", MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await _httpClient.DeleteAsync($"{_url}/{device.Id}");

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw new Exception(errorContent);
                        }

                        MessageBox.Show("Запись успешно удалена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        Page_Loaded(sender, e);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void ButtonDetachModem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DeviceListItem device)
            {
                if (MessageBox.Show($"Отвязать модем от аппарата?", "Предупреждение", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await _httpClient.PatchAsync($"{_url}/{device.Id}/detach-modem", null);

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw new Exception(errorContent);
                        }

                        Page_Loaded(sender, e);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void PaginationCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaginationCombobox.SelectedItem is ComboBoxItem selected)
            {
                _pageSize = int.Parse(selected.Content.ToString());
                _currentPage = 1;
                await LoadDevicesAsync();
            }
        }

        private async Task LoadDevicesAsync(string? nameFilter = null)
        {
            try
            {
                int offset = (_currentPage - 1) * _pageSize;
                var response = !string.IsNullOrEmpty(nameFilter) 
                    ? await _httpClient.GetAsync($"{_url}?limit={_pageSize}&offset={offset}&nameFilter={nameFilter}")
                    : await _httpClient.GetAsync($"{_url}?limit={_pageSize}&offset={offset}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Ошибка при загрузке данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PagedDeviceResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    return;

                _totalCount = result.TotalCount;
                DataListView.ItemsSource = result.Items;
                CountTextBlock.Text = $"Всего найдено {_totalCount} шт.";

                int start = (_currentPage - 1) * _pageSize + 1;
                int end = Math.Min(_currentPage * _pageSize, _totalCount);
                PaginationTextBlock.Text = $"Записи с {start} до {end} из {_totalCount} записей";

                RenderPagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderPagination()
        {
            PaginationPanel.Children.Clear();

            int totalPages = (int)Math.Ceiling((double)_totalCount / _pageSize);
            if (totalPages <= 1) return;

            // Кнопка "Назад"
            var prevButton = new Button
            {
                Content = "<",
                Margin = new Thickness(3),
                IsEnabled = _currentPage > 1
            };
            prevButton.Click += async (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    await LoadDevicesAsync();
                }
            };
            PaginationPanel.Children.Add(prevButton);

            // Кнопки страниц
            for (int i = 1; i <= totalPages; i++)
            {
                if (i > 1 && i < totalPages && Math.Abs(i - _currentPage) > 2)
                {
                    if (i == 2 || i == totalPages - 1)
                    {
                        PaginationPanel.Children.Add(new TextBlock { Text = "...", Margin = new Thickness(5, 0, 5, 0) });
                    }
                    continue;
                }

                var pageButton = new Button
                {
                    Content = i.ToString(),
                    Margin = new Thickness(2),
                    Background = i == _currentPage ? Brushes.LightBlue : Brushes.Transparent
                };
                pageButton.Click += async (s, e) =>
                {
                    _currentPage = int.Parse(((Button)s).Content.ToString());
                    await LoadDevicesAsync();
                };
                PaginationPanel.Children.Add(pageButton);
            }

            // Кнопка "Вперёд"
            var nextButton = new Button
            {
                Content = ">",
                Margin = new Thickness(3),
                IsEnabled = _currentPage < totalPages
            };
            nextButton.Click += async (s, e) =>
            {
                if (_currentPage < totalPages)
                {
                    _currentPage++;
                    await LoadDevicesAsync();
                }
            };
            PaginationPanel.Children.Add(nextButton);
        }

        private async void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = FilterTextBox.Text.Trim();
            await LoadDevicesAsync(filter);
        }
    }
}
