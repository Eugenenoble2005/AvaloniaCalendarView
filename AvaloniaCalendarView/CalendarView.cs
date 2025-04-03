namespace AvaloniaCalendarView;

using Avalonia;
using Avalonia.Controls;

public class CalendarView : ContentControl
{
    public static readonly StyledProperty<DateTime> ViewDateProperty = AvaloniaProperty.Register<CalendarView, DateTime>(nameof(ViewDate));

    public DateTime ViewDate
    {
        get => GetValue(ViewDateProperty);
        set => SetValue(ViewDateProperty, value);
    }
    public CalendarView()
    {
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        switch (change.Property.Name)
        {
            case nameof(ViewDate):
                Content = new MonthView.MonthView(ViewDate);
                break;
        }
    }
}

internal interface ICalendarView
{
    public DateTime ViewDate { get; }
}
