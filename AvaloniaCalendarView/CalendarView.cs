namespace AvaloniaCalendarView;
using Avalonia;
using Avalonia.Controls;

public class CalendarView : ContentControl
{
    public static readonly StyledProperty<DateTime> ViewDateProperty = AvaloniaProperty.Register<CalendarView, DateTime>(nameof(ViewDate), DateTime.Now);
    public static readonly StyledProperty<ViewType> ViewTypeProperty = AvaloniaProperty.Register<CalendarView, ViewType>(nameof(ViewType));

    public DateTime ViewDate
    {
        get => GetValue(ViewDateProperty);
        set => SetValue(ViewDateProperty, value);
    }

    public ViewType ViewType
    {
        get => GetValue(ViewTypeProperty);
        set => SetValue(ViewTypeProperty, value);
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
                if (change.NewValue is DateTime newdate)
                {
                    ViewDate = newdate;
                    Content = GetView();
                }
                break;
            case nameof(ViewType):
                if (change.NewValue is ViewType newtype)
                {
                    ViewType = newtype;
                    Content = GetView();
                }
                break;
        }
    }
    private ICalendarView GetView()
    {
        switch (ViewType)
        {
            case ViewType.Month:
                return new MonthView.MonthView(ViewDate);
            case ViewType.Week:
                return new WeekView.WeekView(ViewDate);
            default: return new MonthView.MonthView(ViewDate);
        }
    }
}

internal interface ICalendarView
{
    public DateTime ViewDate { get; }
}
public enum ViewType
{
    Month,
    Week,
    Day
}
