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

                AddressTextBox.Text = device.Address ?? string.Empty;
                PlaceTextBox.Text = device.Place ?? string.Empty;

                if (!string.IsNullOrEmpty(device.Company))
                {
                    var index = _companies.FindIndex(c =>
                        string.Equals(c.Name, device.Company, StringComparison.OrdinalIgnoreCase));

                    if (index != -1)
                        MachineManufacturerComboBox.SelectedIndex = index;
                }

                if (!string.IsNullOrEmpty(device.Model))
                {
                    string modelNameFromApi = device.Model.Trim();

                    var index = _deviceModels.FindIndex(m =>
                        m.Name.Trim().Equals(modelNameFromApi, StringComparison.OrdinalIgnoreCase));

                    if (index != -1)
                    {
                        ModelComboBox.SelectedIndex = index;
                    }
                }

                if (device.ModemId.HasValue && device.ModemId.Value > 0)
                {
                    var index = _modems.FindIndex(m => m.Id == device.ModemId);
                    if (index != -1)
                        ModemComboBox.SelectedIndex = index;
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
                var companiesResponse = await _client.GetAsync("https://localhost:7270/api/companies");
                companiesResponse.EnsureSuccessStatusCode();
                var companies = await companiesResponse.Content.ReadFromJsonAsync<List<Company>>();
                _companies = companies ?? new List<Company>();
                MachineManufacturerComboBox.ItemsSource = _companies.Select(c => c.Name).ToList();

                var modelsResponse = await _client.GetAsync($"{_url}/devicemodels");
                modelsResponse.EnsureSuccessStatusCode();
                var models = await modelsResponse.Content.ReadFromJsonAsync<List<DeviceModel>>();
                _deviceModels = models ?? new List<DeviceModel>();
                ModelComboBox.ItemsSource = _deviceModels.Select(m => m.Name).ToList();

                var modemsResponse = await _client.GetAsync($"{_url}/modems");
                modemsResponse.EnsureSuccessStatusCode();
                var modems = await modemsResponse.Content.ReadFromJsonAsync<List<Modem>>();
                _modems = modems ?? new List<Modem>();
                ModemComboBox.ItemsSource = _modems.Select(m => m.SerialNumber).ToList();
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
                if (string.IsNullOrWhiteSpace(AddressTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PlaceTextBox.Text) ||
                    MachineManufacturerComboBox.SelectedItem == null ||
                    ModelComboBox.SelectedItem == null ||
                    ModemComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля (отмечены *)",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedModelName = ModelComboBox.SelectedItem?.ToString();
                var selectedCompanyName = MachineManufacturerComboBox.SelectedItem?.ToString();

                var dto = new DeviceUpdateDto
                {
                    Id = _deviceListItem?.Id ?? 0,
                    DeviceModelId = GetIdFromName(selectedModelName, _deviceModels),
                    CompanyId = GetIdFromName(selectedCompanyName, _companies),
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
                        Model = selectedModelName ?? string.Empty,
                        Company = selectedCompanyName ?? "—",
                        ModemId = updatedDevice.ModemId,
                        Modem = ModemComboBox.SelectedItem?.ToString(),
                        Address = updatedDevice.Location?.InstallationAddress ?? AddressTextBox.Text,
                        Place = updatedDevice.Location?.PlaceDescription ?? PlaceTextBox.Text,
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
