namespace AvaloniaCalendarView;
using Avalonia;
using Avalonia.Controls;

public class CalendarView : ContentControl
{
    public static readonly StyledProperty<DateTime> ViewDateProperty = AvaloniaProperty.Register<CalendarView, DateTime>(nameof(ViewDate), DateTime.Now);
    public static readonly StyledProperty<ViewType> ViewTypeProperty = AvaloniaProperty.Register<CalendarView, ViewType>(nameof(ViewType));
    public static readonly StyledProperty<IEnumerable<CalendarEvent>> DateEventsProperty = AvaloniaProperty.Register<CalendarView, IEnumerable<CalendarEvent>>(nameof(DateEvents));
    public static readonly StyledProperty<uint> HourDurationProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(HourDuration), 60);
    public static readonly StyledProperty<uint> DayStartHourProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(DayStartHour), 0);
    public static readonly StyledProperty<uint> DayEndHourProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(DayEndHour), 23);
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
        set
        {
            if (value < 10)
            {
                throw new Exception("Hour duration is too small. Minimum of 10 minutes");
            }
            SetValue(HourDurationProperty, value);
        }
    }

    public uint DayStartHour
    {
        get => GetValue(DayStartHourProperty);
        set
        {
            if (value > 24 || value > DayEndHour)
            {
                throw new Exception($"Day start hour must be less than 24 and less than Day End Hour currently set to {DayEndHour}");
            }
            SetValue(DayStartHourProperty, value);
        }
    }
    public uint DayEndHour
    {
        get => GetValue(DayEndHourProperty);
        set
        {
            if (value > 24 || value < DayStartHour)
            {
                throw new Exception($"Day start hour must be less than 24 and greater than Day Start Hour currently set to {DayStartHour}");
            }
            SetValue(DayEndHourProperty, value);
        }
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
        switch (ViewType)
        {
            case ViewType.Month:
                return new MonthView.MonthView(ViewDate, DateEvents);
            case ViewType.Week:
                return new WeekView.WeekView(ViewDate, DateEvents, HourDuration, DayStartHour, DayEndHour);
            case ViewType.Day:
                return new DayView.DayView(ViewDate, DateEvents, HourDuration, DayStartHour, DayEndHour);
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
