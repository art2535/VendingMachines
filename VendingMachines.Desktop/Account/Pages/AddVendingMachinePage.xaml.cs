using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using VendingMachines.API.DTOs.Devices;
using VendingMachines.Core.Models;

namespace VendingMachines.Desktop.Account.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddVendingMachinePage.xaml
    /// </summary>
    public partial class AddVendingMachinePage : Page
    {
        private DeviceListItem? _deviceListItem;
        private readonly string _jwtToken;
        private readonly HttpClient _client;
        private readonly string _url = "https://localhost:7270/api/devices";

        private List<Company> _companies = new List<Company>();
        private List<DeviceModel> _deviceModels = new List<DeviceModel>();
        private List<Modem> _modems = new List<Modem>();

        private readonly string _status = "создано";

        public AddVendingMachinePage(string jwtToken, DeviceListItem? deviceListItem)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            _deviceListItem = deviceListItem;

            _client = new HttpClient
            {
                BaseAddress = new Uri(_url)
            };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            if (deviceListItem != null)
            {
                InfoTextBlock.Text = "Изменение торгового автомата";
                ButtonCreateMachine.Content = "Изменить";
                _status = "изменено";
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadComboBoxData();

                if (_deviceListItem != null)
                {
                    await PopulateFieldsForEdit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PopulateFieldsForEdit()
        {
            try
            {
                var response = await _client.GetAsync($"{_url}/{_deviceListItem.Id}");
                response.EnsureSuccessStatusCode();

                var device = await response.Content.ReadFromJsonAsync<DeviceListItem>();

                if (device == null)
                    return;

                // --- Текстовые поля ---
                MachineNametextBox.Text = device.Name ?? string.Empty;
                AddressTextBox.Text = device.Address ?? string.Empty;
                PlaceTextBox.Text = device.Place ?? string.Empty;
                MachineNumberTextBox.Text = device.Id.ToString();
                PriorityTextBox.Text = device.ServicePriority?.ToString() ?? string.Empty;

                // --- ComboBox: Производитель ---
                if (!string.IsNullOrEmpty(device.Company))
                {
                    MachineManufacturerComboBox.SelectedIndex =
                        MachineManufacturerComboBox.Items.IndexOf(device.Company);
                }

                // --- ComboBox: Модель ---
                if (!string.IsNullOrEmpty(device.Model))
                {
                    ModelComboBox.SelectedIndex =
                        ModelComboBox.Items.IndexOf(device.Model);
                }

                // --- ComboBox: Модем ---
                if (device.ModemId.HasValue && device.ModemId.Value > 0)
                {
                    var modem = _modems.FirstOrDefault(m => m.Id == device.ModemId);
                    if (modem != null)
                        ModemComboBox.SelectedIndex =
                            ModemComboBox.Items.IndexOf(modem.SerialNumber);
                }

                // --- ComboBox: Режим работы ---
                if (!string.IsNullOrEmpty(device.OperatingMode))
                {
                    OperatingModeComboBox.SelectedIndex =
                        OperatingModeComboBox.Items.IndexOf(device.OperatingMode);
                }

                // --- ComboBox: Часовой пояс ---
                if (!string.IsNullOrEmpty(device.TimeZone))
                {
                    TimeZoneComboBox.SelectedIndex =
                        TimeZoneComboBox.Items.IndexOf(device.TimeZone);
                }

                // --- ComboBox: Товарная матрица ---
                if (!string.IsNullOrEmpty(device.Matrix))
                {
                    MatrixComboBox.SelectedIndex =
                        MatrixComboBox.Items.IndexOf(device.Matrix);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка при получении данных устройства: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadComboBoxData()
        {
            try
            {
                // Загрузка списка производителей (Companies)
                var companiesResponse = await _client.GetAsync("https://localhost:7270/api/companies");
                companiesResponse.EnsureSuccessStatusCode();
                var companies = await companiesResponse.Content.ReadFromJsonAsync<List<Company>>();
                _companies = companies ?? new List<Company>();
                MachineManufacturerComboBox.ItemsSource = _companies.Select(c => c.Name).ToList();

                // Загрузка списка моделей (DeviceModels)
                var modelsResponse = await _client.GetAsync($"{_url}/devicemodels");
                modelsResponse.EnsureSuccessStatusCode();
                var models = await modelsResponse.Content.ReadFromJsonAsync<List<DeviceModel>>();
                _deviceModels = models ?? new List<DeviceModel>();
                ModelComboBox.ItemsSource = _deviceModels.Select(m => m.Name).ToList();

                // Загрузка списка модемов (Modems)
                var modemsResponse = await _client.GetAsync($"{_url}/modems");
                modemsResponse.EnsureSuccessStatusCode();
                var modems = await modemsResponse.Content.ReadFromJsonAsync<List<Modem>>();
                _modems = modems ?? new List<Modem>();
                ModemComboBox.ItemsSource = _modems.Select(m => m.SerialNumber).ToList();

                // Загрузка списка продуктов для товарной матрицы
                var productsResponse = await _client.GetAsync("https://localhost:7270/api/products");
                productsResponse.EnsureSuccessStatusCode();
                var products = await productsResponse.Content.ReadFromJsonAsync<List<Product>>();
                MatrixComboBox.ItemsSource = products?.Select(p => p.Name).ToList() ?? new List<string>();
                MatrixComboBox.SelectedIndex = 0;

                OperatingModeComboBox.ItemsSource = new List<string> { "Круглосуточно", "Дневной", "Ночной" };
                OperatingModeComboBox.SelectedIndex = 0;

                TimeZoneComboBox.ItemsSource = TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).ToList();
                TimeZoneComboBox.SelectedIndex = 0;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для выпадающих списков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonCreateMachine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(MachineNametextBox.Text) ||
                    MachineManufacturerComboBox.SelectedItem == null ||
                    ModelComboBox.SelectedItem == null ||
                    string.IsNullOrEmpty(AddressTextBox.Text) ||
                    string.IsNullOrEmpty(PlaceTextBox.Text) ||
                    string.IsNullOrEmpty(MachineNumberTextBox.Text) ||
                    ModemComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dto = new DeviceUpdateDto
                {
                    Id = _deviceListItem?.Id ?? 0,
                    DeviceModelId = GetIdFromName(ModelComboBox.SelectedItem?.ToString(), _deviceModels),
                    CompanyId = GetIdFromName(MachineManufacturerComboBox.SelectedItem?.ToString(), _companies),
                    ModemId = GetIdFromSerialNumber(ModemComboBox.SelectedItem?.ToString(), _modems),
                    InstallationDate = _deviceListItem?.InstallationDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    CreatedAt = _deviceListItem?.CreatedAt ?? DateTime.UtcNow,
                    Location = new LocationDto
                    {
                        InstallationAddress = AddressTextBox.Text,
                        PlaceDescription = PlaceTextBox.Text
                    }
                };

                HttpResponseMessage response;
                if (_deviceListItem != null)
                {
                    response = await _client.PutAsJsonAsync($"{_url}/{_deviceListItem.Id}", dto);
                }
                else
                {
                    response = await _client.PostAsJsonAsync(_url, dto);
                }

                response.EnsureSuccessStatusCode();

                var updatedDevice = await response.Content.ReadFromJsonAsync<Device>();
                if (updatedDevice != null)
                {
                    _deviceListItem = new DeviceListItem
                    {
                        Id = updatedDevice.Id,
                        Name = updatedDevice.DeviceModel?.Name ?? string.Empty,
                        Model = _deviceModels.FirstOrDefault(m => m.Id == updatedDevice.DeviceModelId)?.Name ?? string.Empty,
                        Company = _companies.FirstOrDefault(c => c.Id == updatedDevice.CompanyId)?.Name ?? "—",
                        ModemId = updatedDevice.ModemId ?? 0,
                        Address = updatedDevice.Location?.InstallationAddress ?? string.Empty,
                        Place = updatedDevice.Location?.PlaceDescription ?? string.Empty,
                        InstallationDate = updatedDevice.InstallationDate
                    };
                }

                MessageBox.Show($"Устройство успешно {_status}!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new VendingMachinesPage(_jwtToken));
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка при {_status.ToLower()} устройства: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка при {_status.ToLower()} устройства: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static int? GetIdFromName(string? name, List<Company> companies)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var company = companies.FirstOrDefault(c => c.Name == name);
            return company?.Id;
        }

        private static int? GetIdFromName(string? name, List<DeviceModel> models)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var model = models.FirstOrDefault(m => m.Name == name);
            return model?.Id;
        }

        private static int? GetIdFromSerialNumber(string? serialNumber, List<Modem> modems)
        {
            if (string.IsNullOrEmpty(serialNumber)) return null;
            var modem = modems.FirstOrDefault(m => m.SerialNumber == serialNumber);
            return modem?.Id;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Отменить изменения?", "Предупреждение", MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new VendingMachinesPage(_jwtToken));
            }
        }
    }
}
