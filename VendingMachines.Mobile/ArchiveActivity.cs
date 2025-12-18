using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using Java.Lang;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using VendingMachines.Mobile.Adapters;
using VendingMachines.Mobile.DTOs;
using Exception = System.Exception;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public class ArchiveActivity : BaseActivity
{
    private ArchiveAdapter? _adapter;
    private Handler? _searchHandler;
    private Runnable? _searchRunnable;

    private RecyclerView? _recyclerView;
    private MaterialAutoCompleteTextView? _searchInput;
    private FloatingActionButton? _fabAddNote;
    internal string _currentSearchQuery = string.Empty;

    protected override int ToolbarTitleResourceId => Resource.String.app_name;
    protected override int GetSelectedNavItemId() => Resource.Id.nav_archive;

    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_archive);
        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
        {
            Toast.MakeText(this, "Не авторизован", ToastLength.Long)?.Show();
            Finish();
            return;
        }
        _recyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
        _searchInput = FindViewById<MaterialAutoCompleteTextView>(Resource.Id.search);
        _fabAddNote = FindViewById<FloatingActionButton>(Resource.Id.fab_add_note);

        _recyclerView!.SetLayoutManager(new LinearLayoutManager(this));
        _recyclerView.HasFixedSize = true;
        _adapter = new ArchiveAdapter(new List<NotesRequest>(), this);
        _recyclerView.SetAdapter(_adapter);

        _fabAddNote!.Click += async (_, _) => await OpenNewNoteWithDevicePicker();

        await RefreshEventsAsync(token);
        _searchHandler = new Handler(Looper.MainLooper!);
        _searchInput!.TextChanged += (_, _) =>
        {
            _searchHandler?.RemoveCallbacks(_searchRunnable!);
            _searchRunnable = new Runnable(async () =>
            {
                _currentSearchQuery = _searchInput.Text?.Trim() ?? "";
                await RefreshEventsAsync(token, search: _currentSearchQuery);
            });
            _searchHandler.PostDelayed(_searchRunnable, 500);
        };
    }

    protected override async void OnResume()
    {
        base.OnResume();
        if (Intent?.GetBooleanExtra("RefreshArchive", false) == true)
        {
            var token = GetJwtToken();
            if (!string.IsNullOrEmpty(token))
            {
                await RefreshEventsAsync(token, search: _currentSearchQuery);
            }
            Intent?.RemoveExtra("RefreshArchive");
        }
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
            Log.Error("Archive", ex.ToString());
            Toast.MakeText(this, "Ошибка загрузки", ToastLength.Short)?.Show();
        }
    }

    internal async Task RefreshEventsAsync(string token, string? search = null)
    {
        try
        {
            var events = await LoadEventsAsync(token, search);
            RunOnUiThread(() =>
            {
                _adapter?.UpdateData(events);
                if (events.Count == 0)
                {
                    Toast.MakeText(this, "Нет событий по запросу", ToastLength.Short)?.Show();
                }
            });
        }
        catch (Exception ex)
        {
            RunOnUiThread(() =>
                Toast.MakeText(this, "Ошибка загрузки", ToastLength.Short)?.Show());
            Log.Error("Archive", ex.ToString());
        }
    }

    private async Task<List<NotesRequest?>> LoadEventsAsync(string token, string? search = null,
        string? eventType = null, DateTime? date = null, string sortBy = "date", string sortOrder = "desc")
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var uri = new UriBuilder($"{API_URL}/api/events");
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(search))
                query["search"] = search.Trim();
            if (!string.IsNullOrWhiteSpace(eventType))
                query["eventType"] = eventType.Trim();
            if (date.HasValue)
                query["date"] = date.Value.ToString("yyyy-MM-dd");
            query["sortBy"] = sortBy;
            query["sortOrder"] = sortOrder;
            uri.Query = query.ToString();
            var response = await client.GetAsync(uri.ToString());
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Error("API", $"Ошибка: {response.StatusCode} — {error}");
                return new List<NotesRequest?>();
            }
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<NotesRequest?>>(json, options) ?? new List<NotesRequest?>();
        }
        catch (Exception ex)
        {
            Log.Error("Archive", ex.ToString());
            RunOnUiThread(() =>
                Toast.MakeText(this, "Ошибка загрузки: " + ex.Message, ToastLength.Long)?.Show());
            return new List<NotesRequest?>();
        }
    }
}
