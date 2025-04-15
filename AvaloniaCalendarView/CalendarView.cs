namespace AvaloniaCalendarView;
using Avalonia;
using Avalonia.Controls;

public class CalendarView : ContentControl
{
    public static readonly StyledProperty<DateTime> ViewDateProperty = AvaloniaProperty.Register<CalendarView, DateTime>(nameof(ViewDate), DateTime.Now);
    public static readonly StyledProperty<ViewType> ViewTypeProperty = AvaloniaProperty.Register<CalendarView, ViewType>(nameof(ViewType));
    public static readonly StyledProperty<IEnumerable<CalendarEvent>> DateEventsProperty = AvaloniaProperty.Register<CalendarView, IEnumerable<CalendarEvent>>(nameof(DateEvents));
    public static readonly StyledProperty<uint> HourDurationProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(HourDuration), 60);

    public DateTime ViewDate
    {
        get => GetValue(ViewDateProperty);
        set => SetValue(ViewDateProperty, value);
    }

    public IEnumerable<CalendarEvent> DateEvents
    {
        get => GetValue(DateEventsProperty);
        set => SetValue(DateEventsProperty, value);
    }
    public ViewType ViewType
    {
        get => GetValue(ViewTypeProperty);
        set => SetValue(ViewTypeProperty, value);
    }

    public uint HourDuration
    {
        get => GetValue(HourDurationProperty);
        set => SetValue(HourDurationProperty, value);
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
            case nameof(DateEvents):
                if (change.NewValue is IEnumerable<CalendarEvent> newevents)
                {
                    DateEvents = newevents;
                    Content = GetView();
                }
                break;
        }
    }
    private ICalendarView GetView()
    {
        if(HourDuration < 10) {
            throw new Exception("Hour Duration is too small, minimum value of 10 is accepted");
        }
        switch (ViewType)
        {
            case ViewType.Month:
                return new MonthView.MonthView(ViewDate, DateEvents);
            case ViewType.Week:
                return new WeekView.WeekView(ViewDate, DateEvents, HourDuration);
            case ViewType.Day:
                return new DayView.DayView(ViewDate, DateEvents, HourDuration);
            default: return new MonthView.MonthView(ViewDate, DateEvents);
        }
    }
}

internal interface ICalendarView
{
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get; set; }
}
public enum ViewType
{
    Month,
    Week,
    Day
}
