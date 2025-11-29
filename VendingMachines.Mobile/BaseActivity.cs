using Android.OS;
using Android.Util;
using Java.Text;
using AndroidX.Core.Content;
using Java.IO;
using Java.Lang;
using Java.Util;
using Android.Content;
using Android.Provider;
using Android.Views;
using AndroidX.AppCompat.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using static Google.Android.Material.Navigation.NavigationBarView;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Uri = Android.Net.Uri;
using File = Java.IO.File;
using Exception = System.Exception;
using Environment = Android.OS.Environment;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public abstract class BaseActivity : AppCompatActivity
{
    protected BottomNavigationView? BottomNav;
    private AlertDialog? _logoutDialog;
    
    private File? _currentPhotoFile;
    private File? _currentVideoFile;

    protected const int REQUEST_CAMERA_PHOTO = 100;
    protected const int REQUEST_CAMERA_VIDEO = 101;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
        {
            var intent = new Intent(this, typeof(LoginActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);
        }
    }

    protected abstract int ToolbarTitleResourceId { get; }
    protected abstract int GetSelectedNavItemId();

    public override void SetContentView(int layoutResID)
    {
        var fullView = LayoutInflater.Inflate(Resource.Layout.activity_base, null);
        var container = fullView.FindViewById<ViewGroup>(Resource.Id.content_container);
        LayoutInflater.Inflate(layoutResID, container, true);

        base.SetContentView(fullView);

        var toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar_main);
        if (toolbar != null)
        {
            toolbar.SetTitle(ToolbarTitleResourceId);
            toolbar.MenuItemClick += Toolbar_MenuItemClick;
        }

        BottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_nav);
        if (BottomNav != null)
        {
            BottomNav.SelectedItemId = GetSelectedNavItemId();
            BottomNav.ItemSelected += BottomNav_ItemSelected;
        }
    }

    private void Toolbar_MenuItemClick(object sender, MaterialToolbar.MenuItemClickEventArgs e)
    {
        if (e.Item.ItemId == Resource.Id.action_logout)
        {
            ShowLogoutDialog();
        }
    }

    private void ShowLogoutDialog()
    {
        if (_logoutDialog != null && _logoutDialog.IsShowing)
            return;

        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Выход");
        builder.SetMessage("Вы действительно хотите выйти из аккаунта?");
        builder.SetCancelable(true);

        builder.SetPositiveButton("Да", (_, _) =>
        {
            var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
            prefs.Edit().Remove("JWT_TOKEN").Apply();

            var intent = new Intent(this, typeof(LoginActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);

            Finish();
        });

        builder.SetNegativeButton("Нет", (_, _) => { });

        _logoutDialog = builder.Create();
        if (!IsFinishing)
            _logoutDialog.Show();
    }

    private void BottomNav_ItemSelected(object? sender, ItemSelectedEventArgs e)
    {
        if (e.Item.ItemId == GetSelectedNavItemId()) return;

        switch (e.Item.ItemId)
        {
            case Resource.Id.nav_home:
                StartActivity(typeof(DashboardActivity));
                break;
            case Resource.Id.nav_camera:
                OpenCamera("photo");
                break;
            case Resource.Id.nav_archive:
                StartActivity(typeof(ArchiveActivity));
                break;
        }
    }

    protected void OpenCamera(string mode)
    {
        _currentPhotoFile = _currentVideoFile = null;

        var intent = new Intent(mode == "photo" ? MediaStore.ActionImageCapture : MediaStore.ActionVideoCapture);

        Uri? fileUri = null;
        File? targetFile = null;

        if (mode == "photo")
        {
            targetFile = CreateImageFile();
            if (targetFile == null)
            {
                Toast.MakeText(this, "Ошибка создания файла", ToastLength.Long)?.Show(); 
                return;
            }
            _currentPhotoFile = targetFile;
            fileUri = FileProvider.GetUriForFile(this, 
                "com.companyname.VendingMachines.Mobile.fileprovider", targetFile);
        }
        else
        {
            targetFile = CreateVideoFile();
            if (targetFile == null) 
            {
                Toast.MakeText(this, "Ошибка создания файла", ToastLength.Long)?.Show(); 
                return;
            }
            _currentVideoFile = targetFile;
            fileUri = FileProvider.GetUriForFile(this, 
                "com.companyname.VendingMachines.Mobile.fileprovider", targetFile);
        }

        intent.PutExtra(MediaStore.ExtraOutput, fileUri);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

        try
        {
            StartActivityForResult(intent, mode == "photo" ? REQUEST_CAMERA_PHOTO : REQUEST_CAMERA_VIDEO);
        }
        catch (Exception ex)
        {
            Log.Error("Camera", "Не удалось открыть камеру: " + ex);
            Toast.MakeText(this, "Камера недоступна", ToastLength.Long)?.Show();
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        Log.Info("CameraResult", $"Request: {requestCode}, Result: {resultCode}");

        File? fileToProcess = null;
        bool isVideo = false;

        if (requestCode == REQUEST_CAMERA_PHOTO && _currentPhotoFile != null)
        {
            fileToProcess = _currentPhotoFile;
            isVideo = false;
        }
        else if (requestCode == REQUEST_CAMERA_VIDEO && _currentVideoFile != null)
        {
            fileToProcess = _currentVideoFile;
            isVideo = true;
        }

        if (fileToProcess != null && fileToProcess.Exists() && fileToProcess.Length() > 0)
        {
            AddMediaToGallery(fileToProcess.AbsolutePath, isVideo);
            Toast.MakeText(this, isVideo ? "Видео сохранено!" : "Фото сохранено в галерее!", 
                ToastLength.Long)?.Show();
        }
        else
        {
            Log.Warn("CameraResult", $"Файл не найден или пустой: {fileToProcess?.AbsolutePath}");
        }

        _currentPhotoFile = _currentVideoFile = null;
    }
    
    private File? CreateImageFile()
    {
        try
        {
            var timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.Default).Format(new Date());
            var fileName = $"VENDING_PHOTO_{timeStamp}_";
            var storageDir = GetExternalFilesDir(Environment.DirectoryPictures)!;
            return File.CreateTempFile(fileName, ".jpg", storageDir);
        }
        catch (Exception ex)
        {
            Log.Error("Camera", "Ошибка создания фото-файла: " + ex);
            return null;
        }
    }

    private File? CreateVideoFile()
    {
        try
        {
            var timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.Default).Format(new Date());
            var fileName = $"VENDING_VIDEO_{timeStamp}_";
            var storageDir = GetExternalFilesDir(Environment.DirectoryMovies)!;
            return File.CreateTempFile(fileName, ".mp4", storageDir);
        }
        catch (Exception ex)
        {
            Log.Error("Camera", "Ошибка создания видео-файла: " + ex);
            return null;
        }
    }

    protected void AddMediaToGallery(string filePath, bool isVideo)
    {
        try
        {
            var file = new File(filePath);
            if (!file.Exists()) 
                return;

            var values = new ContentValues();
            values.Put(MediaStore.IMediaColumns.DisplayName, file.Name);
            values.Put(MediaStore.IMediaColumns.Title, Path.GetFileNameWithoutExtension(file.Name));
            values.Put(MediaStore.IMediaColumns.DateAdded, JavaSystem.CurrentTimeMillis() / 1000);
            values.Put(MediaStore.IMediaColumns.Size, file.Length());
            values.Put(MediaStore.IMediaColumns.MimeType, isVideo ? "video/mp4" : "image/jpeg");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                values.Put(MediaStore.IMediaColumns.RelativePath,
                    isVideo ? Environment.DirectoryMovies + "/VendingMachines"
                            : Environment.DirectoryPictures + "/VendingMachines");
                values.Put(MediaStore.IMediaColumns.IsPending, 1);
            }

            var collection = isVideo
                ? MediaStore.Video.Media.ExternalContentUri
                : MediaStore.Images.Media.ExternalContentUri;

            var newUri = ContentResolver.Insert(collection, values);

            if (newUri != null && Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                using var input = new FileInputStream(file);
                using var output = ContentResolver.OpenOutputStream(newUri);
                if (output != null)
                {
                    var buffer = new byte[8192];
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                }

                values.Clear();
                values.Put(MediaStore.IMediaColumns.IsPending, 0);
                ContentResolver.Update(newUri, values, null, null);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Gallery", "Ошибка добавления в галерею: " + ex);
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (BottomNav != null)
        {
            BottomNav.SelectedItemId = GetSelectedNavItemId();
        }
    }
    
    protected override void OnDestroy()
    {
        _logoutDialog?.Dismiss();
        base.OnDestroy();
    }

    public string GetJwtToken()
    {
        var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
        var token = prefs?.GetString("JWT_TOKEN", null);
        return token ?? string.Empty;
    }
}
