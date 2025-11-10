using Android.Content;
using Android.Provider;
using Android.Views;
using AndroidX.AppCompat.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using static Google.Android.Material.Navigation.NavigationBarView;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public abstract class BaseActivity : AppCompatActivity
{
    protected BottomNavigationView? BottomNav;

    protected const int REQUEST_CAMERA_PHOTO = 100;
    protected const int REQUEST_CAMERA_VIDEO = 101;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }

    protected abstract int ToolbarTitleResourceId { get; }

    public override void SetContentView(int layoutResID)
    {
        var fullView = LayoutInflater.Inflate(Resource.Layout.activity_base, null);
        var container = fullView.FindViewById<ViewGroup>(Resource.Id.content_container);

        LayoutInflater.Inflate(layoutResID, container, true);

        base.SetContentView(fullView);

        var toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar_main);
        if (toolbar != null)
        {
            toolbar!.SetTitle(ToolbarTitleResourceId);
        }

        BottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_nav);
        if (BottomNav != null)
        {
            BottomNav.SelectedItemId = GetSelectedNavItemId();
            BottomNav.ItemSelected += BottomNav_ItemSelected;
        }
    }

    protected abstract int GetSelectedNavItemId();

    private void BottomNav_ItemSelected(object? sender, ItemSelectedEventArgs e)
    {
        if (e.Item.ItemId == GetSelectedNavItemId())
        {
            return;
        }

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
}