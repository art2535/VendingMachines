using System.Net.Http.Headers;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using VendingMachines.Mobile.DTOs;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace VendingMachines.Mobile.Adapters;

public class ArchiveAdapter : RecyclerView.Adapter
{
    private readonly List<NotesRequest?> _items;
    private readonly Context _context;
    private readonly string _apiUrl;

    public ArchiveAdapter(List<NotesRequest?> items, Context context)
    {
        _items = items ?? new List<NotesRequest?>();
        _context = context;
        _apiUrl = Application.Context.Resources!.GetString(Resource.String.api_base_url);
    }

    public override int ItemCount => _items.Count;

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var itemView = LayoutInflater.From(parent.Context)!
            .Inflate(Resource.Layout.report_item, parent, false);
        return new ArchiveViewHolder(itemView);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        var item = _items[position];
        var vh = (ArchiveViewHolder)holder;

        vh.Type.Text = item?.EventType ?? "—";
        vh.Date.Text = item?.EventDate?.ToString("dd.MM.yyyy HH:mm") ?? "—";

        if (!string.IsNullOrEmpty(item?.PhotoUrl) && File.Exists(item.PhotoUrl))
        {
            try
            {
                var bitmap = BitmapFactory.DecodeFile(item.PhotoUrl);
                vh.Preview.SetImageBitmap(bitmap);
            }
            catch
            {
                vh.Preview.SetImageResource(Resource.Drawable.ic_no_photo);
            }
        }
        else
        {
            vh.Preview.SetImageResource(Resource.Drawable.ic_no_photo);
        }

        vh.ItemView.Tag = new Java.Lang.Integer(position);
        vh.DeleteButton.Tag = new Java.Lang.Integer(position);

        vh.ItemView.Click -= OnItemClick;
        vh.ItemView.Click += OnItemClick;

        vh.DeleteButton.Click -= OnDeleteClick;
        vh.DeleteButton.Click += OnDeleteClick;
    }

    private void OnItemClick(object? sender, EventArgs e)
    {
        if (sender is View view && view.Tag is Java.Lang.Integer integerTag)
        {
            int position = integerTag.IntValue();
            if (position < 0 || position >= _items.Count) 
                return;

            var item = _items[position];
            if (item?.Id == null) 
                return;

            var intent = new Intent(_context, typeof(NoteActivity));
            intent.PutExtra("EventId", item.Id);
            intent.AddFlags(ActivityFlags.ClearTop);
            _context.StartActivity(intent);
        }
    }

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        if (sender is View view && view.Tag is Java.Lang.Integer integerTag)
        {
            int position = integerTag.IntValue();
            if (position < 0 || position >= _items.Count) 
                return;

            var item = _items[position];
            if (item?.Id == null) 
                return;

            ShowDeleteConfirmationDialog(item, position);
        }
    }

    private void ShowDeleteConfirmationDialog(NotesRequest? item, int position)
    {
        var builder = new AlertDialog.Builder(_context);
        var alert = builder
            .SetTitle("Удалить событие?")
            .SetMessage("Это действие нельзя отменить")
            .SetPositiveButton("Удалить", (IDialogInterfaceOnClickListener)null!)
            .SetNegativeButton("Отмена", (IDialogInterfaceOnClickListener)null!)
            .Create();

        alert.Show();

        var positiveButton = alert.GetButton((int)DialogButtonType.Positive);
        var negativeButton = alert.GetButton((int)DialogButtonType.Negative);

        positiveButton!.Click += async (s, ev) =>
        {
            positiveButton.Enabled = false;

            if (_context is not ArchiveActivity archiveActivity)
            {
                Toast.MakeText(_context, "Ошибка доступа", ToastLength.Short)?.Show();
                alert.Dismiss();
                return;
            }

            var token = archiveActivity.GetJwtToken();
            if (string.IsNullOrEmpty(token))
            {
                Toast.MakeText(_context, "Не авторизован", ToastLength.Short)?.Show();
                alert.Dismiss();
                return;
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.DeleteAsync($"{_apiUrl}/api/events/{item.Id}");

                if (response.IsSuccessStatusCode)
                {
                    Toast.MakeText(_context, "Событие удалено", ToastLength.Short)?.Show();

                    _items.RemoveAt(position);
                    NotifyItemRemoved(position);
                    NotifyItemRangeChanged(position, _items.Count);
                }
                else
                {
                    Toast.MakeText(_context, "Ошибка удаления", ToastLength.Short)?.Show();
                }
            }
            catch
            {
                Toast.MakeText(_context, "Нет сети", ToastLength.Short)?.Show();
            }
            finally
            {
                alert.Dismiss();
            }
        };

        negativeButton!.Click += (s, ev) =>
        {
            alert.Dismiss();
        };
    }

    public void UpdateData(List<NotesRequest?> newItems)
    {
        _items.Clear();
        if (newItems != null)
            _items.AddRange(newItems);
        NotifyDataSetChanged();
    }
}

public class ArchiveViewHolder : RecyclerView.ViewHolder
{
    public ImageView Preview { get; }
    public MaterialTextView Type { get; }
    public MaterialTextView Date { get; }
    public MaterialButton ExportButton { get; }
    public MaterialButton DeleteButton { get; }

    public ArchiveViewHolder(View itemView) : base(itemView)
    {
        Preview = itemView.FindViewById<ImageView>(Resource.Id.preview)!;
        Type = itemView.FindViewById<MaterialTextView>(Resource.Id.type)!;
        Date = itemView.FindViewById<MaterialTextView>(Resource.Id.date)!;
        ExportButton = itemView.FindViewById<MaterialButton>(Resource.Id.btn_export)!;
        DeleteButton = itemView.FindViewById<MaterialButton>(Resource.Id.btn_delete)!;
    }
}