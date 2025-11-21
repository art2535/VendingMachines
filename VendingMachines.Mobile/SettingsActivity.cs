namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public class SettingsActivity : BaseActivity
{
    protected override int ToolbarTitleResourceId => Resource.String.app_name;
    protected override int GetSelectedNavItemId() => Resource.Id.nav_home;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_settings);
    }
}