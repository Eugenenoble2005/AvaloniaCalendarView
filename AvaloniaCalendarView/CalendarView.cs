namespace AvaloniaCalendarView;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

public class CalendarView : ContentControl
{
    public static readonly StyledProperty<DateTime> ViewDateProperty = AvaloniaProperty.Register<CalendarView, DateTime>(nameof(ViewDate), DateTime.Now);
    public static readonly StyledProperty<ViewType> ViewTypeProperty = AvaloniaProperty.Register<CalendarView, ViewType>(nameof(ViewType));
    public static readonly StyledProperty<IEnumerable<CalendarEvent>> DateEventsProperty = AvaloniaProperty.Register<CalendarView, IEnumerable<CalendarEvent>>(nameof(DateEvents), new List<CalendarEvent>());
    public static readonly StyledProperty<uint> HourDurationProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(HourDuration), 60);
    public static readonly StyledProperty<uint> DayStartHourProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(DayStartHour), 0);
    public static readonly StyledProperty<uint> DayEndHourProperty = AvaloniaProperty.Register<CalendarView, uint>(nameof(DayEndHour), 23);

    //Events
    public static readonly RoutedEvent<EventResizedArgs> EventResizedEvent = RoutedEvent.Register<CalendarView, EventResizedArgs>(nameof(EventResized), RoutingStrategies.Direct);
    public static readonly RoutedEvent<EventMovedArgs> EventMovedEvent = RoutedEvent.Register<CalendarView, EventMovedArgs>(nameof(EventMoved), RoutingStrategies.Direct);

    public event EventHandler<EventResizedArgs> EventResized
    {
        add => AddHandler(EventResizedEvent, value);
        remove => RemoveHandler(EventResizedEvent, value);
    }
    public event EventHandler<EventMovedArgs> EventMoved
    {
        add => AddHandler(EventMovedEvent, value);
        remove => RemoveHandler(EventMovedEvent, value);
    }

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
                return new MonthView(ViewDate, DateEvents);
            case ViewType.Week:
                return new WeekView(ViewDate, DateEvents, HourDuration, DayStartHour, DayEndHour);
            case ViewType.Day:
                return new DayView(ViewDate, DateEvents, HourDuration, DayStartHour, DayEndHour);
            default: return new MonthView(ViewDate, DateEvents);
        }
    }

    internal void ForceRender()
    {
        Content = GetView();
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

public class EventResizedArgs : RoutedEventArgs
{
    public CalendarEvent CalendarEvent { get; }
    public (DateTime start, DateTime end) ResizedFrom { get; }
    public (DateTime start, DateTime end) ResizedTo { get; }

    public EventResizedArgs(RoutedEvent routedEvent, CalendarEvent calendarEvent, (DateTime start, DateTime end) resizedFrom, (DateTime start, DateTime end) resizedTo) : base(routedEvent)
    {
        CalendarEvent = calendarEvent;
        ResizedFrom = resizedFrom;
        ResizedTo = resizedTo;
    }
}

public class EventMovedArgs : RoutedEventArgs
{
    public CalendarEvent CalendarEvent { get; }
    public (DateTime start, DateTime end) MovedFrom { get; }
    public (DateTime start, DateTime end) MovedTo { get; }

    public EventMovedArgs(RoutedEvent routedEvent, CalendarEvent calendarEvent, (DateTime start, DateTime end) movedFrom, (DateTime start, DateTime end) movedTo) : base(routedEvent)
    {
        CalendarEvent = calendarEvent;
        MovedFrom = movedFrom;
        MovedTo = movedTo;
    }
}

