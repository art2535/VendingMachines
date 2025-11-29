using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using VendingMachines.Mobile.DTOs;

namespace VendingMachines.Mobile.Adapters;

public class ArchiveAdapter : RecyclerView.Adapter
{
    private readonly List<NotesRequest?> _originalItems;
    private readonly List<NotesRequest?> _displayItems;

    public ArchiveAdapter(List<NotesRequest?> items)
    {
        _originalItems = new List<NotesRequest?>(items);
        _displayItems = new List<NotesRequest?>(items);
    }

    public override int ItemCount => _displayItems.Count;

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var itemView = LayoutInflater.From(parent.Context)
            .Inflate(Resource.Layout.report_item, parent, false);
        return new ArchiveViewHolder(itemView);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        var item = _displayItems[position];
        var vh = holder as ArchiveViewHolder;

        vh!.Type.Text = item?.EventType ?? "—";
        vh.Date.Text = item?.EventDate?.ToString("dd.MM.yyyy HH:mm") ?? "—";

        // Клик по карточке
        vh.ItemView.Click -= ItemView_Click;
        vh.ItemView.Click += ItemView_Click;

        void ItemView_Click(object? sender, EventArgs e)
        {
            if (item?.Id == null) return;

            var intent = new Intent(vh.ItemView.Context, typeof(NoteActivity));
            intent.PutExtra("EventId", item.Id);
            vh.ItemView.Context.StartActivity(intent);
        }
    }
    
    public void UpdateData(List<NotesRequest?> newItems)
    {
        _originalItems.Clear();
        _originalItems.AddRange(newItems);
        _displayItems.Clear();
        _displayItems.AddRange(newItems);
        NotifyDataSetChanged();
    }
}

public class ArchiveViewHolder : RecyclerView.ViewHolder
{
    public ImageView Preview { get; }
    public MaterialTextView Type { get; }
    public MaterialTextView Date { get; }
    public MaterialButton ExportButton { get; }

    public ArchiveViewHolder(View itemView) : base(itemView)
    {
        Preview = itemView.FindViewById<ImageView>(Resource.Id.preview);
        Type = itemView.FindViewById<MaterialTextView>(Resource.Id.type);
        Date = itemView.FindViewById<MaterialTextView>(Resource.Id.date);
        ExportButton = itemView.FindViewById<MaterialButton>(Resource.Id.btn_export);
    }
}