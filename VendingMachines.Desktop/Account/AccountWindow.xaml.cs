using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls.Primitives;
using VendingMachines.API.DTOs.Auth;
using VendingMachines.Desktop.Account.Pages;

namespace VendingMachines.Desktop.Account
{
    /// <summary>
    /// Логика взаимодействия для AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private readonly string _accountName;
        private readonly string _role;
        private readonly string _token;
        private readonly string _email;
        private readonly string _password;
        private readonly string _url = "https://localhost:7270/api/auth";

        public AccountWindow()
        {
            InitializeComponent();
        }

        public AccountWindow(string accountName, string role, string token, string email, string password)
        {
            InitializeComponent();

            _accountName = accountName;
            _role = role;
            _token = token;
            _email = email;
            _password = password;

            MainFrame.Navigate(new MainPage(_token));

            AccountName.Text = _accountName;
            AccountRole.Text = _role;
            PagesTextBlock.Text = string.Empty;
        }

        private void UserDropdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private async void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Информация", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                try
                {
                    var exitRequest = new ExitRequest
                    {
                        Email = _email,
                        Password = _password,
                        Token = _token
                    };

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                        var response = await httpClient.PostAsJsonAsync($"{_url}/logout", exitRequest);

                        if (!response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}\n{content}");
                        }
                    }

                    var mainWindow = new MainWindow();
                    Close();
                    mainWindow.Show();
                }
                catch (HttpRequestException httpRequestExeption)
                {
                    ShowMessageBox(httpRequestExeption.Message);
                }
                catch (TaskCanceledException)
                {
                    ShowMessageBox("Превышено время ожидания ответа от сервера.");
                }
                catch (Exception ex)
                {
                    ShowMessageBox("Ошибка - " + ex.Message);
                }
            }
        }

        private static void ShowMessageBox(string message, string caption = "Ошибка",
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Error)
        {
            MessageBox.Show(message, caption, button, image);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MainPage(_token));
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DetailReportsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InventoryAccountingButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VendingMachinesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PagesTextBlock.Text = "Администрирование/Торговые автоматы";
            MainFrame.Navigate(new VendingMachinesPage(_token));
        }
    }
}
