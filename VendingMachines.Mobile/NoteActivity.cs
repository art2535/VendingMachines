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
    private TextInputEditText _title, _body;
    private ImageView _photoPreview;
    private View _placeholderAddPhoto;
    private FloatingActionButton _fabSave, _fabCancel;

    private NotesRequest? _currentEvent;
    private bool _isEditMode = false;

    private Uri? _selectedMediaUri;
    private bool _isVideoSelected = false;
    private string? _localFilePath;

    private const int REQUEST_PICK_MEDIA = 200;

    protected override int ToolbarTitleResourceId => Resource.String.app_name;
    protected override int GetSelectedNavItemId() => Resource.Id.nav_archive;

    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_note);

        _title = FindViewById<TextInputEditText>(Resource.Id.title)!;
        _body = FindViewById<TextInputEditText>(Resource.Id.body)!;
        _photoPreview = FindViewById<ImageView>(Resource.Id.photo_preview)!;
        _placeholderAddPhoto = FindViewById(Resource.Id.placeholder_add_photo)!;
        _fabSave = FindViewById<FloatingActionButton>(Resource.Id.fab_save)!;
        _fabCancel = FindViewById<FloatingActionButton>(Resource.Id.fab_cancel)!;

        var eventId = Intent?.GetIntExtra("EventId", -1) ?? -1;
        if (eventId == -1)
        {
            Toast.MakeText(this, "Ошибка: событие не выбрано", ToastLength.Long)?.Show();
            Finish();
            return;
        }

        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
        {
            Finish();
            return;
        }

        _currentEvent = await LoadEventAsync(token, eventId);
        if (_currentEvent == null)
        {
            Toast.MakeText(this, "Ошибка загрузки события", ToastLength.Long)?.Show();
            Finish();
            return;
        }

        FillForm();

        _fabSave.Click += async (s, e) =>
        {
            if (_isEditMode) await SaveChangesAsync();
            else EnterEditMode();
        };

        _fabCancel.Click += (s, e) =>
        {
            if (_isEditMode) CancelEdit();
            else Finish();
        };

        _photoPreview.Click += (s, e) => OpenGalleryPicker();
        _placeholderAddPhoto.Click += (s, e) => OpenGalleryPicker();
    }

    private void OpenGalleryPicker()
    {
        if (!_isEditMode)
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
            Log.Error("Gallery", "Ошибка: " + ex);
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
                Toast.MakeText(this, "Не удалось получить путь к файлу", ToastLength.Long)?.Show();
                return;
            }

            AddMediaToGallery(_localFilePath, false);

            _photoPreview.Visibility = ViewStates.Visible;
            _placeholderAddPhoto.Visibility = ViewStates.Gone;

            try
            {
                var bitmap = BitmapFactory.DecodeFile(_localFilePath);
                _photoPreview.SetImageBitmap(bitmap);
            }
            catch (Exception ex)
            {
                Log.Error("Preview", ex.ToString());
                _photoPreview.SetImageResource(Resource.Drawable.ic_logo);
            }

            if (!_isEditMode) EnterEditMode();
        }
    }

    private void FillForm()
    {
        if (_currentEvent == null) return;

        _title.Text = _currentEvent.EventType ?? "—";

        var description = _currentEvent.Description ?? "—";
        var deviceInfo = new StringBuilder();

        if (_currentEvent.Device != null)
        {
            deviceInfo.AppendLine($"Модель: {_currentEvent.Device.DeviceModel}");
            deviceInfo.AppendLine($"Адрес: {_currentEvent.Device.Location}");
            deviceInfo.AppendLine($"Компания: {_currentEvent.Device.Company}");
            if (_currentEvent.Device.DeviceStatus != null)
                deviceInfo.AppendLine($"Статус: {_currentEvent.Device.DeviceStatus}");
        }

        if (_currentEvent.EventDate != default)
            deviceInfo.AppendLine($"Дата события: {_currentEvent.EventDate:dd.MM.yyyy HH:mm}");

        if (!description.Contains("Модель:") && deviceInfo.Length > 0)
            _body.Text = $"{description}\n\n{deviceInfo}";
        else
            _body.Text = description;

        if (!string.IsNullOrEmpty(_currentEvent.PhotoUrl) && File.Exists(_currentEvent.PhotoUrl))
        {
            _photoPreview.Visibility = ViewStates.Visible;
            _placeholderAddPhoto.Visibility = ViewStates.Gone;
            _photoPreview.SetImageBitmap(BitmapFactory.DecodeFile(_currentEvent.PhotoUrl));
        }
        else
        {
            _photoPreview.Visibility = ViewStates.Gone;
            _placeholderAddPhoto.Visibility = ViewStates.Visible;
        }
    }

    private async Task SaveChangesAsync()
    {
        if (_currentEvent == null) return;

        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updatedEvent = new NotesRequest
            {
                Id = _currentEvent.Id,
                EventType = _title.Text?.Trim() ?? "",
                Description = _body.Text?.Trim() ?? "",
                EventDate = _currentEvent.EventDate,
                Device = _currentEvent.Device,
                PhotoUrl = _localFilePath ?? _currentEvent.PhotoUrl
            };

            var response = await client.PutAsJsonAsync(
                $"http://192.168.1.77:5321/api/events/{_currentEvent.Id}", updatedEvent);

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
                var error = await response.Content.ReadAsStringAsync();
                Log.Error("Save", $"Ошибка: {response.StatusCode} — {error}");
                Toast.MakeText(this, "Ошибка сохранения", ToastLength.Long)?.Show();
            }
        }
        catch (Exception ex)
        {
            Log.Error("Save", ex.ToString());
            Toast.MakeText(this, "Ошибка сети", ToastLength.Long)?.Show();
        }
    }

    private string? GetRealPathFromUri(Uri uri)
    {
        try
        {
            var cursor = ContentResolver?.Query(uri, null, null, null, null);
            if (cursor?.MoveToFirst() == true)
            {
                var pathIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                if (pathIndex != -1)
                    return cursor.GetString(pathIndex);
            }
        }
        catch { }
        return uri.Path;
    }

    private void EnterEditMode()
    {
        _isEditMode = true;
        _title.Enabled = true;
        _body.Enabled = true;
        _fabSave.SetImageResource(Resource.Drawable.ic_add);
        _fabCancel.SetImageResource(Resource.Drawable.ic_cancel);
        Title = "Редактирование";
    }

    private void CancelEdit()
    {
        _isEditMode = false;
        _title.Enabled = false;
        _body.Enabled = false;
        _fabSave.SetImageResource(Resource.Drawable.ic_edit);
        _fabCancel.SetImageResource(Resource.Drawable.ic_cancel);
        _selectedMediaUri = null;
        FillForm();
        Title = "Событие";
    }

    private async Task<NotesRequest?> LoadEventAsync(string token, int id)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"http://192.168.1.77:5321/api/events/{id}");
            if (!response.IsSuccessStatusCode) 
                return null;
            
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<NotesRequest>(json, options);
        }
        catch (Exception ex)
        {
            Log.Error("LoadEvent", ex.ToString());
            return null;
        }
    }
}