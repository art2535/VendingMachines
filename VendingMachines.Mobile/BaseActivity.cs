using Android.Content;
using Android.Provider;
using Android.Views;
using AndroidX.AppCompat.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using static Google.Android.Material.Navigation.NavigationBarView;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public abstract class BaseActivity : AppCompatActivity
{
    protected BottomNavigationView? BottomNav;
    private AlertDialog? _logoutDialog;

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
            // Удаляем токен
            var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
            prefs.Edit().Remove("JWT_TOKEN").Apply();

            // Переходим на экран входа
            var intent = new Intent(this, typeof(LoginActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);

            Finish(); // Завершаем текущую активность
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
        Intent? intent = mode switch
        {
            "photo" => new Intent(MediaStore.ActionImageCapture),
            "video" => new Intent(MediaStore.ActionVideoCapture),
            _ => null
        };

        if (intent != null)
        {
            StartActivityForResult(intent, mode == "photo" ? REQUEST_CAMERA_PHOTO : REQUEST_CAMERA_VIDEO);
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (resultCode == Result.Ok)
        {
            switch (requestCode)
            {
                case REQUEST_CAMERA_PHOTO:
                    Toast.MakeText(this, "Фото снято!", ToastLength.Short)?.Show();
                    break;
                case REQUEST_CAMERA_VIDEO:
                    Toast.MakeText(this, "Видео записано!", ToastLength.Short)?.Show();
                    break;
            }
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

    protected string GetJwtToken()
    {
        var prefs = GetSharedPreferences("UserPrefs", FileCreationMode.Private);
        var token = prefs?.GetString("JWT_TOKEN", null);
        return token ?? string.Empty;
    }
}
