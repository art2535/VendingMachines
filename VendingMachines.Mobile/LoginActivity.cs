using Android.Content;
using Android.Util;
using AndroidX.AppCompat.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using System.Net;
using System.Text;
using System.Text.Json;
using VendingMachines.Mobile.DTOs;

namespace VendingMachines.Mobile
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class LoginActivity : AppCompatActivity
    {
        private TextInputLayout? layoutEmail;
        private TextInputEditText? email;
        private TextInputLayout? layoutPassword;
        private TextInputEditText? password;
        private MaterialButton? loginButton;
        private TextView? forgotPassword;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
            var token = prefs?.GetString("JWT_TOKEN", null);

            if (!string.IsNullOrEmpty(token))
            {
                if (IsTokenValid(token))
                {
                    var intent = new Intent(this, typeof(DashboardActivity));
                    StartActivity(intent);
                    return;
                }
            }

            SetContentView(Resource.Layout.activity_login);

            var toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar!.Title = GetString(Resource.String.app_name);
            }

            layoutEmail = FindViewById<TextInputLayout>(Resource.Id.layout_email);
            email = FindViewById<TextInputEditText>(Resource.Id.email);

            layoutPassword = FindViewById<TextInputLayout>(Resource.Id.layout_password);
            password = FindViewById<TextInputEditText>(Resource.Id.password);

            loginButton = FindViewById<MaterialButton>(Resource.Id.login_button);
            if (loginButton != null)
            {
                loginButton.Click += LoginButton_ClickAsync;
            }

            forgotPassword = FindViewById<TextView>(Resource.Id.forgot_password);
            if (forgotPassword != null)
            {
                forgotPassword.Click += (_, _) =>
                    Toast.MakeText(this, "Функция восстановления пароля не реализована", 
                        ToastLength.Short)?.Show();
            }
        }

        private async void LoginButton_ClickAsync(object? sender, EventArgs e)
        {
            if (!ValidateInputs(out string emailText, out string passwordText))
            {
                return;
            }
            await LoginAsync(emailText, passwordText);
        }

        private static bool IsTokenValid(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) 
                return false;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) 
                    return false;

                var payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: 
                        payload += "=="; 
                        break;
                    case 3: 
                        payload += "="; 
                        break;
                }

                var bytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(bytes);

                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("exp", out var expProp)) 
                    return false;

                var expUnix = expProp.GetInt64();
                var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

                return expDate > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateInputs(out string emailText, out string passwordText)
        {
            emailText = email?.Text?.Trim() ?? string.Empty;
            passwordText = password?.Text?.Trim() ?? string.Empty;
            layoutEmail!.Error = string.Empty;
            layoutPassword!.Error = string.Empty;

            if (string.IsNullOrEmpty(emailText) && string.IsNullOrEmpty(passwordText))
            {
                layoutEmail.Error = "Email не может быть пустым";
                layoutPassword.Error = "Пароль не может быть пустым";
                return false;
            }
            if (string.IsNullOrEmpty(emailText))
            {
                layoutEmail.Error = "Email не может быть пустым";
                return false;
            }
            if (string.IsNullOrEmpty(passwordText))
            {
                layoutPassword.Error = "Пароль не может быть пустым";
                return false;
            }
            if (!Patterns.EmailAddress.Matcher(emailText).Matches())
            {
                layoutEmail.Error = "Email не может быть пустым";
                return false;
            }

            return true;
        }

        private async Task LoginAsync(string emailText, string passwordText)
        {
            try
            {
                using var httpClient = new HttpClient();

                var loginRequest = new LoginRequest
                {
                    Email = emailText,
                    Password = passwordText
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                /* Адреса для URL API:
                 * 192.168.1.77 - IP-адрес дом
                 * 172.20.10.2 - IP-адрес колледж (телефон iPhone)
                 * 5221 - порт HTTP
                 * 7270 - порт HTTPS
                 */

                var response = await httpClient.PostAsync("http://172.20.10.2:5321/api/auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Toast.MakeText(this, "Пользователь не авторизован", ToastLength.Short)?.Show();
                        return;
                    }

                    Toast.MakeText(this, $"Ошибка сервера: {response.StatusCode}", ToastLength.Short)?.Show();
                    return;
                }

                var requestJson = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserRequest>(requestJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (user == null || string.IsNullOrEmpty(user.Token))
                {
                    Toast.MakeText(this, "Неверный логин или пароль", ToastLength.Short)?.Show();
                    return;
                }

                Toast.MakeText(this, "Авторизация прошла успешно", ToastLength.Short)?.Show();

                SaveJwtToken(user.Token);

                var intent = new Intent(this, typeof(DashboardActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Ошибка авторизации: {ex.Message}", ToastLength.Long)?.Show();
            }
        }

        private void SaveJwtToken(string jwtToken)
        {
            var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
            var editor = prefs?.Edit();
            editor?.PutString("JWT_TOKEN", jwtToken);
            editor?.Apply();
        }
    }
}