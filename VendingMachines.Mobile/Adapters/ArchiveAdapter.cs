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

        vh!.Type.Text = item?.EventType ?? "Неизвестно";
        vh.Date.Text = item?.EventDate.ToString();
        vh.Preview.SetImageResource(Resource.Drawable.ic_logo);
    }

    public void FilterAndSort(string query)
    {
        _displayItems.Clear();

        if (_originalItems == null || _originalItems.Count == 0)
        {
            NotifyDataSetChanged();
            return;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            _displayItems.AddRange(_originalItems.Where(x => x != null));
        }
        else
        {
            query = query.ToLower();

            if (query.Contains("дата") || query.Contains("date"))
            {
                _displayItems.AddRange(_originalItems
                    .Where(x => x?.EventDate != null)
                    .OrderByDescending(x => x!.EventDate));
            }
            else if (query.Contains("тип") || query.Contains("type"))
            {
                _displayItems.AddRange(_originalItems
                    .Where(x => x?.EventType != null)
                    .OrderBy(x => x!.EventType));
            }
            else
            {
                _displayItems.AddRange(_originalItems
                    .Where(x => x?.EventType != null && x.EventType.ToLower().Contains(query)));
            }
        }

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