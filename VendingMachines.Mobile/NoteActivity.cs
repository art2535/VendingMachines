using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Util;
using Android.Views;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using VendingMachines.Mobile.DTOs;
using Uri = Android.Net.Uri;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public class NoteActivity : BaseActivity
{
    private TextInputEditText _title = null!;
    private TextInputEditText _body = null!;
    private ImageView _photoPreview = null!;
    private View _placeholderAddPhoto = null!;
    private FloatingActionButton _fabSave = null!;
    private FloatingActionButton _fabCancel = null!;

    private NotesRequest? _currentEvent;
    private bool _isEditMode = false;

    private Uri? _selectedMediaUri;
    private string? _localFilePath;
    private string? _deviceInfoHint;

    private const int REQUEST_PICK_MEDIA = 200;

    protected override int ToolbarTitleResourceId => Resource.String.app_name;
    protected override int GetSelectedNavItemId() => Resource.Id.nav_archive;

    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_note);

        InitializeViews();

        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
        {
            Finish();
            return;
        }

        var eventId = Intent?.GetIntExtra("EventId", -1) ?? -1;
        var deviceId = Intent?.GetIntExtra("DeviceId", -1) ?? -1;
        var isNewNote = Intent?.GetBooleanExtra("IsNewNote", false) == true;

        if (isNewNote && deviceId > 0)
        {
            Title = "Новая заметка";
            _isEditMode = true;

            _title.Enabled = true;
            _body.Enabled = true;

            _fabSave.SetImageResource(Resource.Drawable.ic_add);
            _fabCancel.SetImageResource(Resource.Drawable.ic_cancel);

            var deviceJson = Intent?.GetStringExtra("SelectedDevice");
            DeviceRequest? device = null;

            if (!string.IsNullOrEmpty(deviceJson))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                device = JsonSerializer.Deserialize<DeviceRequest>(deviceJson, options);
            }

            if (device == null)
            {
                await LoadAndDisplayDeviceInfoAsync(token, deviceId);
            }
            else
            {
                _currentEvent = new NotesRequest
                {
                    Device = device,
                    EventDate = DateTime.Now
                };
                ShowDeviceAsHint(device);
            }
        }
        else if (eventId > 0)
        {
            _currentEvent = await LoadEventAsync(token, eventId);
            if (_currentEvent == null)
            {
                Toast.MakeText(this, "Ошибка загрузки события", ToastLength.Long)?.Show();
                Finish();
                return;
            }

            FillFormForExistingEvent();
            EnterViewMode();
        }
        else
        {
            Toast.MakeText(this, "Ошибка: не указан аппарат или событие", ToastLength.Short)?.Show();
            Finish();
            return;
        }

        SetupClickListeners();
    }

    private void ShowDeviceAsHint(DeviceRequest device)
    {
        RunOnUiThread(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Модель: {device.DeviceModel ?? "—"}");
            sb.AppendLine($"Адрес: {device.Location ?? "—"}");
            sb.AppendLine($"Компания: {device.Company ?? "—"}");
            sb.AppendLine($"Дата события: {DateTime.Now:dd.MM.yyyy HH:mm}");

            _deviceInfoHint = sb.ToString().Trim();

            _body.TextChanged -= OnBodyTextChanged;
            _body.TextChanged += OnBodyTextChanged;

            _body.Text = "";
            UpdateBodyHint();
        });
    }

    private void OnBodyTextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        UpdateBodyHint();
    }

    private void UpdateBodyHint()
    {
        if (!string.IsNullOrWhiteSpace(_body.Text))
        {
            _body.Hint = "Описание";
        }
        else if (!string.IsNullOrWhiteSpace(_deviceInfoHint))
        {
            _body.Hint = _deviceInfoHint;
            _body.SetHintTextColor(Color.ParseColor("#8A000000"));
        }
        else
        {
            _body.Hint = "Описание";
        }
    }

    private void InitializeViews()
    {
        _title = FindViewById<TextInputEditText>(Resource.Id.title)!;
        _body = FindViewById<TextInputEditText>(Resource.Id.body)!;
        _photoPreview = FindViewById<ImageView>(Resource.Id.photo_preview)!;
        _placeholderAddPhoto = FindViewById(Resource.Id.placeholder_add_photo)!;
        _fabSave = FindViewById<FloatingActionButton>(Resource.Id.fab_save)!;
        _fabCancel = FindViewById<FloatingActionButton>(Resource.Id.fab_cancel)!;
    }

    private void SetupClickListeners()
    {
        _fabSave.Click += async (s, e) =>
        {
            var isNew = Intent?.GetBooleanExtra("IsNewNote", false) == true;

            if (isNew)
                await CreateNewNoteAsync();
            else if (_isEditMode)
                await SaveChangesAsync();
            else
                EnterEditMode();
        };

        _fabCancel.Click += (s, e) =>
        {
            var isNew = Intent?.GetBooleanExtra("IsNewNote", false) == true;

            if (isNew)
                Finish();
            else if (_isEditMode)
                CancelEdit();
            else
                Finish();
        };

        _photoPreview.Click += (s, e) => OpenGalleryPicker();
        _placeholderAddPhoto.Click += (s, e) => OpenGalleryPicker();
    }

    private async Task LoadAndDisplayDeviceInfoAsync(string token, int deviceId)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{API_URL}/api/devices/{deviceId}");
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var device = JsonSerializer.Deserialize<DeviceRequest>(json, options);

            if (device != null && _currentEvent != null)
            {
                _currentEvent.Device = device;
                ShowDeviceAsHint(device);
            }
        }
        catch (Exception ex)
        {
            Log.Error("NoteActivity", "Ошибка загрузки данных аппарата: " + ex);
        }
    }

    private void FillFormForExistingEvent()
    {
        if (_currentEvent == null) return;

        _title.Text = _currentEvent.EventType ?? "—";

        var userDescription = _currentEvent.Description?.Trim() ?? "";

        var deviceInfo = new StringBuilder();
        if (_currentEvent.Device != null)
        {
            deviceInfo.AppendLine($"Модель: {_currentEvent.Device.DeviceModel ?? "—"}");
            deviceInfo.AppendLine($"Адрес: {_currentEvent.Device.Location ?? "—"}");
            deviceInfo.AppendLine($"Компания: {_currentEvent.Device.Company ?? "—"}");
            if (_currentEvent.EventDate != default)
                deviceInfo.AppendLine($"Дата события: {_currentEvent.EventDate:dd.MM.yyyy HH:mm}");
        }

        _deviceInfoHint = deviceInfo.ToString().Trim();

        _body.Text = userDescription;

        if (!string.IsNullOrEmpty(_currentEvent.PhotoUrl) && System.IO.File.Exists(_currentEvent.PhotoUrl))
        {
            _photoPreview.Visibility = ViewStates.Visible;
            _placeholderAddPhoto.Visibility = ViewStates.Gone;
            try
            {
                var bitmap = BitmapFactory.DecodeFile(_currentEvent.PhotoUrl);
                _photoPreview.SetImageBitmap(bitmap);
            }
            catch
            {
                _photoPreview.SetImageResource(Resource.Drawable.ic_no_photo);
            }
        }
        else
        {
            _photoPreview.Visibility = ViewStates.Gone;
            _placeholderAddPhoto.Visibility = ViewStates.Visible;
        }

        _body.TextChanged -= OnBodyTextChanged;
        _body.TextChanged += OnBodyTextChanged;
        UpdateBodyHint();
    }

    private void EnterViewMode()
    {
        _isEditMode = false;
        _title.Enabled = false;
        _body.Enabled = false;
        _fabSave.SetImageResource(Resource.Drawable.ic_edit);
        _fabCancel.SetImageResource(Resource.Drawable.ic_cancel);
        Title = "Событие";
    }

    private void EnterEditMode()
    {
        _isEditMode = true;
        _title.Enabled = true;
        _body.Enabled = true;
        _fabSave.SetImageResource(Resource.Drawable.ic_add);
        _fabCancel.SetImageResource(Resource.Drawable.ic_cancel);
        Title = "Редактирование";

        _body.TextChanged += OnBodyTextChanged;
        UpdateBodyHint();
    }

    private void CancelEdit()
    {
        if (Intent?.GetBooleanExtra("IsNewNote", false) == true)
        {
            Finish();
            return;
        }

        _isEditMode = false;
        _title.Enabled = false;
        _body.Enabled = false;
        _fabSave.SetImageResource(Resource.Drawable.ic_edit);
        _fabCancel.SetImageResource(Resource.Drawable.ic_cancel);
        FillFormForExistingEvent();
        Title = "Событие";
    }

    private async Task CreateNewNoteAsync()
    {
        if (_currentEvent?.Device?.Id == null) return;

        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token)) return;

        var newEvent = new NotesRequest
        {
            EventType = _title.Text?.Trim() ?? "Без типа",
            Description = _body.Text?.Trim(),
            EventDate = DateTime.UtcNow,
            PhotoUrl = _localFilePath,
            DeviceId = _currentEvent.Device.Id,
            Device = null
        };
        
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync($"{API_URL}/api/events", newEvent);

            if (response.IsSuccessStatusCode)
            {
                Toast.MakeText(this, "Заметка создана!", ToastLength.Long)?.Show();
                var intent = new Intent(this, typeof(ArchiveActivity));
                intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                intent.PutExtra("RefreshArchive", true);
                StartActivity(intent);
                Finish();
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                Log.Error("NoteActivity", err);
                Toast.MakeText(this, $"Ошибка создания: {err}", ToastLength.Long)?.Show();
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "Ошибка сети", ToastLength.Long)?.Show();
            Log.Error("CreateNote", ex.ToString());
        }
    }

    private async Task SaveChangesAsync()
    {
        if (_currentEvent == null) return;

        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token)) return;

        var updated = new NotesRequest
        {
            Id = _currentEvent.Id,
            EventType = _title.Text?.Trim() ?? "",
            Description = _body.Text?.Trim(),
            EventDate = _currentEvent.EventDate,
            PhotoUrl = _localFilePath ?? _currentEvent.PhotoUrl,
            Device = _currentEvent.Device
        };

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsJsonAsync($"{API_URL}/api/events/{_currentEvent.Id}", updated);

            if (response.IsSuccessStatusCode)
            {
                Toast.MakeText(this, "Сохранено!", ToastLength.Long)?.Show();
                var intent = new Intent(this, typeof(ArchiveActivity));
                intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                intent.PutExtra("RefreshArchive", true);
                StartActivity(intent);
                Finish();
            }
            else
            {
                Toast.MakeText(this, "Ошибка сохранения", ToastLength.Long)?.Show();
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "Ошибка сети", ToastLength.Long)?.Show();
            Log.Error("SaveNote", ex.ToString());
        }
    }

    private void OpenGalleryPicker()
    {
        if (!_isEditMode && Intent?.GetBooleanExtra("IsNewNote", false) != true)
        {
            Toast.MakeText(this, "Включите режим редактирования", ToastLength.Short)?.Show();
            return;
        }

        var intent = new Intent(Intent.ActionPick);
        intent.SetType("*/*");
        intent.PutExtra(Intent.ExtraMimeTypes, new[] { "image/*", "video/*" });

        try
        {
            StartActivityForResult(Intent.CreateChooser(intent, "Выберите фото или видео"), REQUEST_PICK_MEDIA);
        }
        catch (Exception ex)
        {
            Log.Error("Gallery", ex.ToString());
            Toast.MakeText(this, "Галерея недоступна", ToastLength.Short)?.Show();
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == REQUEST_PICK_MEDIA && resultCode == Result.Ok && data?.Data != null)
        {
            _selectedMediaUri = data.Data;
            _localFilePath = GetRealPathFromUri(_selectedMediaUri);

            if (string.IsNullOrEmpty(_localFilePath))
            {
                Toast.MakeText(this, "Не удалось получить файл", ToastLength.Long)?.Show();
                return;
            }

            _photoPreview.Visibility = ViewStates.Visible;
            _placeholderAddPhoto.Visibility = ViewStates.Gone;

            try
            {
                var bitmap = BitmapFactory.DecodeFile(_localFilePath);
                _photoPreview.SetImageBitmap(bitmap);
            }
            catch
            {
                _photoPreview.SetImageResource(Resource.Drawable.ic_logo);
            }
        }
    }

    private string? GetRealPathFromUri(Uri uri)
    {
        try
        {
            var cursor = ContentResolver?.Query(uri, null, null, null, null);
            if (cursor?.MoveToFirst() == true)
            {
                var idx = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                if (idx != -1) return cursor.GetString(idx);
            }
            cursor?.Close();
        }
        catch { }
        return uri.Path;
    }

    private async Task<NotesRequest?> LoadEventAsync(string token, int id)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.GetAsync($"{API_URL}/api/events/{id}");
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<NotesRequest>(json, opt);
        }
        catch (Exception ex)
        {
            Log.Error("LoadEvent", ex.ToString());
            return null;
        }
    }
}