using Android.Content;
using Google.Android.Material.AppBar;
using Google.Android.Material.Card;
using System.Net.Http.Headers;
using static AndroidX.AppCompat.Widget.Toolbar;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public class DashboardActivity : BaseActivity
{
    protected override int ToolbarTitleResourceId => Resource.String.app_name;
    protected override int GetSelectedNavItemId() => Resource.Id.nav_home;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_dashboard);

        var dashboardMenu = FindViewById<MaterialToolbar>(Resource.Id.toolbar_main);
        if (dashboardMenu != null)
        {
            dashboardMenu.MenuItemClick += DashboardMenu_MenuItemClick;
        }

        var photoCardView = FindViewById<MaterialCardView>(Resource.Id.card_photo);
        var videoCardView = FindViewById<MaterialCardView>(Resource.Id.card_video);
        if (photoCardView != null && videoCardView != null)
        {
            photoCardView.Click += (sender, e) => OpenCamera("photo");
            videoCardView.Click += (sender, e) => OpenCamera("video");
        }

        var archiveCardView = FindViewById<MaterialCardView>(Resource.Id.card_archive);
        var noteCardView = FindViewById<MaterialCardView>(Resource.Id.card_note);
        if (archiveCardView != null && noteCardView != null)
        {
            archiveCardView.Click += (sender, e) => StartActivity(typeof(ArchiveActivity));
            noteCardView.Click += (sender, e) => StartActivity(typeof(NoteActivity));
        }
    }

    private async void DashboardMenu_MenuItemClick(object? sender, MenuItemClickEventArgs e)
    {
        switch (e.Item.ItemId)
        {
            case Resource.Id.action_settings:
                StartActivity(typeof(SettingsActivity));
                break;
            case Resource.Id.action_logout:
                if (await LogoutAsync())
                {
                    var intent = new Intent(this, typeof(LoginActivity));
                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                    StartActivity(intent);
                    Finish();
                }
                break;
        }
    }

    private async Task<bool> LogoutAsync()
    {
        using (var httpClient = new HttpClient())
        {
            var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
            var token = prefs?.GetString("JWT_TOKEN", null);

            if (string.IsNullOrEmpty(token))
            {
                Toast.MakeText(this, "Токен пуст", ToastLength.Short)?.Show();
                return false;
            }

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync("http://192.168.1.77:5321/api/auth/logout", null);

            if (!response.IsSuccessStatusCode)
            {
                Toast.MakeText(this, $"Ошибка сервера: {response.StatusCode}", ToastLength.Short)?.Show();
                return false;
            }

            var editor = prefs?.Edit();
            editor?.Remove("JWT_TOKEN");
            editor?.Apply();
        }

        return true;
    }
}