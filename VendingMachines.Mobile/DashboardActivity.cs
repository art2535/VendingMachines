using Android.Content;
using Google.Android.Material.AppBar;
using Google.Android.Material.Card;
using System.Net.Http.Headers;
using System.Text.Json;
using Android.Util;
using VendingMachines.Mobile.DTOs;
using static AndroidX.AppCompat.Widget.Toolbar;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name", MainLauncher = false)]
public class DashboardActivity : BaseActivity
{
    protected override int ToolbarTitleResourceId => Resource.String.app_name;
    protected override int GetSelectedNavItemId() => Resource.Id.nav_home;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_dashboard);

        var toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar_main);
        //toolbar!.MenuItemClick += DashboardMenu_MenuItemClick;

        FindViewById<MaterialCardView>(Resource.Id.card_photo)!.Click += (_, _) => OpenCamera("photo");
        FindViewById<MaterialCardView>(Resource.Id.card_video)!.Click += (_, _) => OpenCamera("video");

        FindViewById<MaterialCardView>(Resource.Id.card_archive)!.Click += (_, _) =>
            StartActivity(typeof(ArchiveActivity));

        FindViewById<MaterialCardView>(Resource.Id.card_note)!.Click += async (_, _) =>
            await OpenNewNoteWithDevicePicker();
    }

    private async Task OpenNewNoteWithDevicePicker()
    {
        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
        {
            Toast.MakeText(this, "Не авторизован", ToastLength.Short)?.Show();
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{API_URL}/api/devices");
            if (!response.IsSuccessStatusCode)
            {
                Toast.MakeText(this, "Не удалось загрузить аппараты", ToastLength.Short)?.Show();
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var wrapper = JsonSerializer.Deserialize<DevicesResponse>(json, options);
            if (wrapper?.Items == null || wrapper.Items.Count == 0)
            {
                Toast.MakeText(this, "Нет доступных аппаратов", ToastLength.Short)?.Show();
                return;
            }

            var devices = wrapper.Items.Select(dto => new DeviceRequest
            {
                Id = dto.Id,
                DeviceModel = dto.Model,
                Location = dto.Address,
                Company = dto.Company
            }).ToList();

            var displayItems = wrapper.Items.Select(d =>
                $"{d.Model}\n{d.Address}\n{d.Company}").ToArray();

            new AlertDialog.Builder(this)
                .SetTitle("Выберите аппарат")
                .SetItems(displayItems, (s, args) =>
                {
                    var selectedDevice = devices[args.Which];

                    var intent = new Intent(this, typeof(NoteActivity));
                    intent.PutExtra("DeviceId", selectedDevice.Id);
                    intent.PutExtra("IsNewNote", true);

                    intent.PutExtra("SelectedDevice", JsonSerializer.Serialize(selectedDevice));

                    StartActivity(intent);
                })
                .SetNegativeButton("Отмена", (s, e) => { })
                .Show();
        }
        catch (Exception ex)
        {
            Log.Error("Dashboard", ex.ToString());
            Toast.MakeText(this, "Ошибка загрузки", ToastLength.Short)?.Show();
        }
    }

    private async void DashboardMenu_MenuItemClick(object? sender, MenuItemClickEventArgs e)
    {
        switch (e.Item.ItemId)
        {
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

    public async Task<bool> LogoutAsync()
    {
        using var httpClient = new HttpClient();
        var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
        var token = prefs.GetString("JWT_TOKEN", null);

        if (string.IsNullOrEmpty(token))
        {
            Toast.MakeText(this, "Токен отсутствует", ToastLength.Short)?.Show();
            return false;
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync($"{API_URL}/api/auth/logout", null);

        if (!response.IsSuccessStatusCode)
        {
            Toast.MakeText(this, "Ошибка выхода", ToastLength.Short)?.Show();
            return false;
        }

        prefs.Edit()?.Remove("JWT_TOKEN")?.Apply();
        return true;
    }
}