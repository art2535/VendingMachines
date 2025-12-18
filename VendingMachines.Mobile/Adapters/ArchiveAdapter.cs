using System.Net.Http.Headers;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using VendingMachines.Mobile.DTOs;

namespace VendingMachines.Mobile.Adapters;

public class ArchiveAdapter : RecyclerView.Adapter
{
    private readonly List<NotesRequest?> _items;

    private readonly Context _context;

    public ArchiveAdapter(List<NotesRequest?> items, Context context)
    {
        _items = items ?? new List<NotesRequest?>();
        _context = context;
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

        vh.ItemView.Click -= OnItemClick;
        vh.ItemView.Click += OnItemClick;

        vh.DeleteButton.Click -= OnDeleteClick;
        vh.DeleteButton.Click += OnDeleteClick;

        void OnItemClick(object? sender, EventArgs e)
        {
            if (item?.Id == null) return;

            var intent = new Intent(_context, typeof(NoteActivity));
            intent.PutExtra("EventId", item.Id);
            intent.AddFlags(ActivityFlags.ClearTop);
            _context.StartActivity(intent);
        }

        async void OnDeleteClick(object? sender, EventArgs e)
        {
            var apiUrl = Application.Context.Resources!.GetString(Resource.String.api_base_url);

            if (item?.Id == null) return;

            var builder = new AlertDialog.Builder(_context);
            builder.SetTitle("Удалить событие?")
                   .SetMessage("Это действие нельзя отменить")
                   .SetPositiveButton("Удалить", async (s, args) =>
                   {
                       if (_context is not BaseActivity baseActivity)
                       {
                           Toast.MakeText(_context, "Ошибка доступа", ToastLength.Short)?.Show();
                           return;
                       }

                       var token = baseActivity.GetJwtToken();
                       if (string.IsNullOrEmpty(token)) return;

                       try
                       {
                           using var client = new HttpClient();
                           client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                           var response = await client.DeleteAsync($"{apiUrl}/api/events/{item.Id}");

                           if (response.IsSuccessStatusCode)
                           {
                               Toast.MakeText(_context, "Удалено", ToastLength.Short)?.Show();

                               var pos = holder.AdapterPosition;
                               if (pos != RecyclerView.NoPosition)
                               {
                                   _items.RemoveAt(pos);
                                   NotifyItemRemoved(pos);
                                   NotifyItemRangeChanged(pos, _items.Count);
                               }
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
                   })
                   .SetNegativeButton("Отмена", (s, args) => { })
                   .Show();
        }
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