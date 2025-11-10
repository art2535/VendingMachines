using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace VendingMachines.Mobile;

[Activity(Label = "@string/app_name")]
public class NoteActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_note);

        var cancelButton = FindViewById<FloatingActionButton>(Resource.Id.fab_cancel);
        if (cancelButton != null)
        {
            cancelButton.Click += (_, _) =>
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle("Подтверждение");
                builder.SetMessage("Вы действительно хотите отменить изменения?");
                builder.SetCancelable(true);

                builder.SetPositiveButton("Да", (_, _) =>
                {
                    StartActivity(typeof(DashboardActivity));
                });

                builder.SetNegativeButton("Нет", (_, _) =>
                {
                    Toast.MakeText(this, "Отменено", ToastLength.Short)?.Show();
                });

                var dialog = builder.Create();
                dialog.Show();
            };
        }

        var saveButton = FindViewById<FloatingActionButton>(Resource.Id.fab_add_note);
        if (saveButton != null)
        {
            saveButton.Click += (_, _) =>
            {
                Toast.MakeText(this, "Заметка добавлена", ToastLength.Short)?.Show();
                StartActivity(typeof(DashboardActivity));
            };
        }
    }
}