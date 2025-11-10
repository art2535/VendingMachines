using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using VendingMachines.Desktop.Account;
using VendingMachines.API.DTOs.Account;
using VendingMachines.API.DTOs.Auth;

namespace VendingMachines.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _url = "https://localhost:7270/api/auth";

        public MainWindow()
        {
            InitializeComponent();

            //var accountWindow = new AccountWindow();
            //Close();
            //accountWindow.Show();
        }

        private async void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            ErrorEmailTextBlock.Text = ErrorPasswordTextBlock.Text = string.Empty;

            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
            {
                ErrorEmailTextBlock.Text = "Email не может быть пустым";
                ErrorPasswordTextBlock.Text = "Пароль не может быть пустым";
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                ErrorEmailTextBlock.Text = "Email не может быть пустым";
                return;
            }
            else if (string.IsNullOrEmpty(password))
            {
                ErrorPasswordTextBlock.Text = "Пароль не может быть пустым";
                return;
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var loginRequest = new LoginRequest
                    {
                        Email = email,
                        Password = password
                    };

                    var response = await httpClient.PostAsJsonAsync($"{_url}/login", loginRequest);
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}\n{content}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserRequest>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new Exception("Не удалось получить данные пользователя.");

                    var accountWindow = new AccountWindow($"{userInfo.LastName} {userInfo.FirstName.Substring(0, 1)}. " +
                        $"{userInfo.MiddleName.Substring(0, 1)}.", userInfo.RoleName, userInfo.Token, 
                        userInfo.Email, userInfo.Password);
                    Close();
                    accountWindow.Show();
                }
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

        private static void ShowMessageBox(string message, string caption = "Ошибка", 
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Error)
        {
            MessageBox.Show(message, caption, button, image);
        }
    }
}