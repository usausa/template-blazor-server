namespace Template.BlazorServer.Host.Components.Pages.MusselSheet;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

internal sealed class SheetOrder : INotifyPropertyChanged
{
    private readonly List<SheetOrderLine> watched = [];

    public SheetOrder()
    {
        Lines.CollectionChanged += OnLinesChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public DateOnly? OrderDate
    {
        get;
        set
        {
            field = value;
            OnChanged();
        }
    }

    public string? Customer
    {
        get;
        set
        {
            field = value;
            OnChanged();
        }
    }

    public string? Department
    {
        get;
        set
        {
            field = value;
            OnChanged();
        }
    }

    public ObservableCollection<SheetOrderLine> Lines { get; } = [];

    public int LineCount => Lines.Count;

    public decimal Total => Lines.Sum(static x => x.Amount ?? 0m);

    private void OnLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var line in watched)
        {
            line.PropertyChanged -= OnLineChanged;
        }

        watched.Clear();
        watched.AddRange(Lines);
        foreach (var line in watched)
        {
            line.PropertyChanged += OnLineChanged;
        }

        OnChanged(nameof(LineCount));
        OnChanged(nameof(Total));
    }

    private void OnLineChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (String.IsNullOrEmpty(e.PropertyName) || (e.PropertyName == nameof(SheetOrderLine.Amount)))
        {
            OnChanged(nameof(Total));
        }
    }

    private void OnChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal sealed class SheetOrderLine : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Name
    {
        get;
        set
        {
            field = value;
            OnChanged();
        }
    }

    public decimal? Quantity
    {
        get;
        set
        {
            field = value;
            OnChanged();
            OnChanged(nameof(Amount));
        }
    }

    public decimal? Price
    {
        get;
        set
        {
            field = value;
            OnChanged();
            OnChanged(nameof(Amount));
        }
    }

    public decimal? Amount => Quantity * Price;

    private void OnChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
