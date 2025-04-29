using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
namespace AvaloniaCalendarView;
/// <summary>
/// Represents an event that can be displayed on the calendar view.
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// The title of the event.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The DateTime object representing when the event starts.
    /// </summary>
    public required DateTime Start { get; set; }

    /// <summary>
    /// The DateTime object representing when the event ends.
    /// </summary>
    public required DateTime End { get; set; }

    /// <summary>
    /// The Brush that paints the background of the event.
    /// </summary>
    public IBrush BackgroundBrush { get; set; } = Brushes.Blue;

    /// <summary>
    /// The Brush that paints the foreground of the event.
    /// </summary>
    public IBrush DateForegroundBrush { get; set; } = Brushes.White;

    /// <summary>
    /// Whether or not this event can be resized to the left, that is the start can be pushed back.
    /// </summary>
    public bool ResizeLeft { get; set; } = false;

    /// <summary>
    /// Whether or not this event can be resized to the right, that is the end can be pushed forward.
    /// </summary>
    public bool ResizeRight { get; set; } = false;

    ///<summary>
    /// Whether or not this event can be moved.
    /// </summary>
    public bool Mobile { get; set; } = false;

}

internal class EventDrawer(IEnumerable<CalendarEvent> events, DrawingCanvas canvas, int cellDuration, int dayStartHour, ViewType viewType)
{
    public void DrawEvents(Grid col, DateTime columnDate)
    {
        var eventsOnThisDay = events.Where(p =>
            columnDate.Date >= p.Start.Date &&
            columnDate.Date <= p.End.Date &&
            (p.End - p.Start).TotalHours < 24);

        int spaceForMultiDayEvents = (int)canvas.SpaceForMultiDayEvents;
        if (Grid.GetColumn(col) == 1)
        {
            DrawMultiDayEvents(col, columnDate, spaceForMultiDayEvents);
        }

        foreach (var _event in eventsOnThisDay)
        {
            var intersections = eventsOnThisDay.Where(p =>
                _event.Start < p.End && p.Start < _event.End).ToArray();

            int gridColumns = intersections.Count();
            TimeOnly hourStart = _event.Start.Date == columnDate.Date
                ? TimeOnly.FromDateTime(_event.Start)
                : new(0, 0);

            TimeOnly hourEnd = _event.End.Date == columnDate.Date
                ? TimeOnly.FromDateTime(_event.End)
                : new(23, 59);

            int gridStartInMinutes = dayStartHour * 60;
            int hourStartInMinutes = hourStart.Hour * 60 + hourStart.Minute;
            int hourEndInMinutes = hourEnd.Hour * 60 + hourEnd.Minute;

            int indexOfFirstCell = (hourStartInMinutes - gridStartInMinutes) / cellDuration;
            int indexOfLastCell = (hourEndInMinutes - gridStartInMinutes) / cellDuration;

            var collection = col.Children.Where(p => p is Border).ToList();
            if (indexOfFirstCell >= collection.Count) continue;

            bool truncateTop = false;
            if (indexOfFirstCell < 0 && indexOfLastCell < 0) continue;
            if (indexOfFirstCell < 0)
            {
                indexOfFirstCell = 0;
                truncateTop = true;
            }

            var firstCell = (Border)collection[indexOfFirstCell];
            firstCell.PropertyChanged += (s, e) =>
            {
                if (e.Property == Border.BoundsProperty)
                {
                    var context = new CanvasDrawContext(
                        _event,
                        firstCell.Bounds,
                        Grid.GetColumn(col),
                        indexOfFirstCell,
                        indexOfLastCell,
                        gridColumns,
                        Array.IndexOf(intersections, _event),
                        columnDate,
                        truncateTop,
                        spaceForMultiDayEvents
                    );

                    DrawEventOnCanvas(context);
                }
            };
        }
    }

    private void DrawMultiDayEvents(Grid col, DateTime columnDate, int spaceForMultiDayEvents)
    {
        var endOfWeek = viewType == ViewType.Week ? columnDate.AddDays(6) : columnDate;
        var multiDayEventsOnThisWeek = events.Where(p =>
            p.Start.Date <= endOfWeek.Date &&
            columnDate.Date <= p.End.Date &&
            (p.End - p.Start).TotalHours >= 24).ToArray();

        foreach (var _event in multiDayEventsOnThisWeek)
        {
            DateTime intersectionStart = _event.Start.Date > columnDate.Date ? _event.Start.Date : columnDate.Date;
            DateTime intersectionEnd = _event.End.Date < endOfWeek ? _event.End.Date : endOfWeek;

            var distance = (intersectionEnd.Date - intersectionStart.Date).TotalDays + 1;
            var beginning = (intersectionStart.Date - columnDate.Date).TotalDays;
            bool canResizeLeft = intersectionStart == _event.Start.Date;
            var collection = col.Children.Where(p => p is Border).ToList();
            var firstCell = (Border)collection[0];

            firstCell.PropertyChanged += (_, e) =>
            {
                if (e.Property == Border.BoundsProperty)
                {
                    canvas.Children.RemoveAll(canvas.Children
                        .OfType<EventBorder>()
                        .Where(b => b.Event == _event));

                    var bounds = firstCell.Bounds;
                    var height = 30;
                    var width = bounds.Width * distance;
                    var y = Array.IndexOf(multiDayEventsOnThisWeek, _event) * height;
                    var x = beginning * bounds.Width;

                    var border = new EventBorder
                    {
                        Height = height,
                        Width = width,
                        Event = _event,
                        Background = DullBackgroundBrush(_event.BackgroundBrush),
                        HoverBackground = _event.BackgroundBrush,
                        DullBackground = DullBackgroundBrush(_event.BackgroundBrush),
                        ResizeLeft = _event.ResizeLeft && viewType != ViewType.Day && canResizeLeft, //cant resize multiday events in day view
                        ResizeRight = _event.ResizeRight && viewType != ViewType.Day,
                        CanMove = _event.Mobile,
                        CornerRadius = new CornerRadius(
                            intersectionStart.Date == _event.Start.Date ? 5 : 0,
                            5,
                            5,
                            intersectionStart.Date == _event.Start.Date ? 5 : 0
                        ),
                        Type = EventBorderType.Multiday,
                        CellHeight = bounds.Height,
                        CellWidth = bounds.Width,
                        ColumnDate = columnDate,
                        CellDuration = cellDuration,
                        BorderThickness = new Thickness(1),
                        BorderBrush = _event.BackgroundBrush,
                    };

                    var childContent = new StackPanel();
                    var eventText = new TextBlock
                    {
                        Text = $"{_event.Title} ({_event.Start.ToString()} - {_event.End.ToString()})",
                        TextAlignment = TextAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };

                    childContent.Children.Add(eventText);
                    border.Child = childContent;

                    Canvas.SetTop(border, y);
                    Canvas.SetLeft(border, x);

                    canvas.Children.Add(border);
                }
            };
        }
    }

    private void DrawEventOnCanvas(CanvasDrawContext context)
    {
        canvas.Children.RemoveAll(canvas.Children
            .OfType<EventBorder>()
            .Where(b => b.Column == context.column && b.Event == context._event));

        double x = context.bounds.X + ((context.column - 1) * context.bounds.Width) + 7;
        double y = context.bounds.Y - 2 + context.spaceForMultiDayEvents;
        double height = context.bounds.Height * ((context.indexOfLastCell - context.indexOfFirstCell) + 1);
        double width = context.bounds.Width - 10;

        double startCorrectionOffset = 0;
        bool canResizeUp = false;
        bool canResizeDown = true;
        if (context.columnDate.Date == context._event.Start.Date && !context.truncateTop)
        {
            var cellDate = new TimeOnly(dayStartHour, 0).AddMinutes(context.indexOfFirstCell * cellDuration);
            var diff = (TimeOnly.FromDateTime(context._event.Start) - cellDate).TotalMinutes;
            var offset = (diff / cellDuration) * context.bounds.Height;
            y += offset;
            startCorrectionOffset = offset;
            canResizeUp = true;
        }

        if (context.columnDate.Date == context._event.End.Date)
        {
            var cellDate = new TimeOnly(dayStartHour, 0).AddMinutes(context.indexOfLastCell * cellDuration);
            var diff = cellDuration - (TimeOnly.FromDateTime(context._event.End) - cellDate).TotalMinutes;
            var offset = (diff / cellDuration) * context.bounds.Height;
            height -= offset + startCorrectionOffset;
            canResizeDown = true;
        }

        if (context.numberOfGridColumns > 1)
        {
            width /= context.numberOfGridColumns;
            x += context.eventGridColumn * width;
        }
        //weird edgecase when start and end are equal. Have to resolve that.
        if (height < 0)
        {
            // Console.WriteLine(context._event.ToString());
            return;
        }
        var border = new EventBorder
        {
            Height = height,
            Width = width,
            Event = context._event,
            HoverBackground = context._event.BackgroundBrush,
            DullBackground = DullBackgroundBrush(context._event.BackgroundBrush),
            Background = DullBackgroundBrush(context._event.BackgroundBrush),
            ResizeDown = canResizeDown && context._event.ResizeRight,
            ResizeUp = canResizeUp && context._event.ResizeLeft,
            CanMove = context._event.Mobile,
            CornerRadius = new CornerRadius(context.truncateTop ? 0 : 10, context.truncateTop ? 0 : 10, 10, 10),
            BorderThickness = new Thickness(1, context.truncateTop ? 0 : 1, 1, 1),
            BorderBrush = context._event.BackgroundBrush,
            Type = EventBorderType.SingleDay,
            CellHeight = context.bounds.Height,
            CellWidth = context.bounds.Width,
            Column = context.column,
            ColumnDate = context.columnDate,
            CellDuration = cellDuration,
            DayStartHour = dayStartHour
        };

        var titleText = new TextBlock
        {
            Text = context._event.Title,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 15
        };

        var timeText = new TextBlock
        {
            Text = $"{context._event.Start:d, MMMM yyyy h:mm tt} - {context._event.End:d, MMM yyyy h:mm tt}",
            TextAlignment = TextAlignment.Center,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap
        };

        var contentPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        contentPanel.Children.Add(titleText);
        contentPanel.Children.Add(timeText);
        border.Child = contentPanel;

        Canvas.SetTop(border, y);
        Canvas.SetLeft(border, x);
        canvas.Children.Add(border);
    }

    private static IBrush DullBackgroundBrush(IBrush inputBrush)
    {
        if (inputBrush is ISolidColorBrush scBrush)
        {
            var dullColor = Color.FromArgb(
                scBrush.Color.A,
                (byte)(scBrush.Color.R * 0.6 + 80 * 0.4),
                (byte)(scBrush.Color.G * 0.6 + 80 * 0.4),
                (byte)(scBrush.Color.B * 0.6 + 80 * 0.4)
            );
            return new SolidColorBrush(dullColor);
        }

        return inputBrush;
    }
}

internal class EventBorder : Border
{
    public int Column { get; set; }
    public IBrush HoverBackground { get; set; } = Brushes.Blue;
    public IBrush DullBackground { get; set; } = Brushes.Blue;
    public EventBorderType Type { get; set; }
    public required CalendarEvent Event { get; set; }
    private DrawingCanvas? _canvas; //handle to the parent canvas
    private CalendarView? _calendarView; //handle to parent calendarview

    //movement properties
    public bool ResizeUp { get; set; } = false;
    public bool ResizeDown { get; set; } = false;
    public bool ResizeRight { get; set; } = false;
    public bool ResizeLeft { get; set; } = false;
    public bool CanMove { get; set; } = false;

    //for determining position
    public double CellHeight { get; set; }
    public double CellWidth { get; set; }
    public DateTime ColumnDate { get; set; }
    public int CellDuration { get; set; }
    public int DayStartHour { get; set; }

    private bool _resizeMode = false;
    private bool _moveMode = false;
    private ResizeContext? _resizeContext = null;
    private MoveContext? _moveContext = null;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _canvas = (DrawingCanvas?)this.FindAncestorOfType<Canvas>();
        _calendarView = (CalendarView?)this.FindAncestorOfType<CalendarView>();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        IlluminateAllEventBorders(true);
        base.OnPointerEntered(e);
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_resizeMode && !_moveMode)
        {
            SetResizeContext(e);
        }
        if (_resizeContext is not null && _resizeMode)
        {
            HandleResize(e);
        }

        if (_moveContext is not null && _moveMode)
        {
            HandleMove(e);
        }

    }

    private void HandleResize(PointerEventArgs e)
    {
        if (_resizeContext == null) return;
        (double x, double y) coords = (e.GetPosition(_canvas).X, e.GetPosition(_canvas).Y);
        if (Type == EventBorderType.SingleDay)
        {
            //can only be resized up and down
            DateTime? destination = CoordsToDateTime(coords);
            if (destination is null) return;
            if (_resizeContext.Edge == BorderEdge.Top)
            {
                ResizeStart(destination.Value);
            }
            else
            {
                ResizeEnd(destination.Value);
            }
        }
        else
        {
            //can only be resized left and right
            DateTime? destination = CoordsToDateTime(coords);
            if (destination is null) return;
            if (_resizeContext.Edge == BorderEdge.Left)
            {
                ResizeStart(destination.Value);
            }
            else
            {
                ResizeEnd(destination.Value);
            }
        }
    }

    private void HandleMove(PointerEventArgs e)
    {
        (double x, double y) coords = (e.GetPosition(_canvas).X, e.GetPosition(_canvas).Y);
        var moveTo = CoordsToDateTime(coords);
        if (moveTo is null) return;
        var diff = (moveTo.Value - _moveContext!.moveFrom).TotalHours;
        _calendarView!.DateEvents.FirstOrDefault(p => p == Event)!.Start = _moveContext.originalStart.AddHours(diff);
        _calendarView!.DateEvents.FirstOrDefault(p => p == Event)!.End = _moveContext.originalEnd.AddHours(diff);
    }
    private void ResizeStart(DateTime destination)
    {
        if (_calendarView is null) return;
        if (Type == EventBorderType.SingleDay)
        {
            if (destination > Event.End || (Event.End - destination).TotalMinutes < 30) return;
            var diff = destination - Event.Start;
            _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.Start = _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.Start.AddHours(diff.TotalHours);
        }
        else
        {
            if (destination > Event.End) return;
            destination = destination.AddDays(-1); //idk
            var diff = (destination - Event.Start).TotalDays;
            _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.Start = _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.Start.AddDays(diff);
        }
    }

    private void ResizeEnd(DateTime destination)
    {
        if (_calendarView is null) return;
        if (Type == EventBorderType.SingleDay)
        {
            if (destination < Event.Start || (destination - Event.Start).TotalMinutes < 30 || _calendarView is null) return;
            var diff = destination - Event.End;
            _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.End = _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.End.AddHours(diff.TotalHours);
        }
        else
        {
            if (destination < Event.Start) return;
            destination = destination.AddDays(-1);
            var diff = (destination - Event.End).TotalDays;
            _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.End = _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.End.AddDays(diff);
            var _event = _calendarView.DateEvents.FirstOrDefault(p => p == Event);
            if ((_event!.End - _event.Start).TotalHours < 1)
            {
                _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.Start = new(_event.Start.Year, _event.Start.Month, _event.Start.Day, 0, 0, 0);
                _calendarView.DateEvents.FirstOrDefault(p => p == Event)!.End = new(_event.Start.Year, _event.Start.Month, _event.Start.Day, 12, 0, 0);
            }
        }

    }
    //convert coordinates on the canvas to datetime by getting the datetime of the cell at that coordinate
    private DateTime? CoordsToDateTime((double x, double y) coord)
    {
        if (_canvas is null) return null;
        //bound guards
        if (
            coord.y < (Type == EventBorderType.SingleDay ? _canvas.SpaceForMultiDayEvents : 0) ||
            coord.y > _canvas.Bounds.Height
        ) return null;
        if (coord.x < 0 || coord.x > _canvas.Bounds.Width) return null;
        double y = coord.y - _canvas.SpaceForMultiDayEvents;
        var height = _canvas.Bounds.Height;
        var vertical_distance = (int)(y / CellHeight);
        var horizontal_distance = (int)(coord.x / CellWidth);
        DateTime horizontal_day = ColumnDate.AddDays(horizontal_distance - Column + 1).Date;
        TimeOnly vertical_time = new TimeOnly(DayStartHour, 0).AddMinutes(vertical_distance * CellDuration);
        DateTime day = new(horizontal_day.Year, horizontal_day.Month, horizontal_day.Day, vertical_time.Hour, vertical_time.Minute, 0);
        return day;
    }
    private void SetResizeContext(PointerEventArgs e)
    {
        var edge = DetermineEdge(e);
        switch (edge)
        {
            case BorderEdge.Bottom:
                if (ResizeDown)
                {
                    Cursor = Cursor.Parse("SizeNorthSouth");
                    _resizeContext = new(BorderEdge.Bottom);
                    _moveContext = null;
                }
                break;
            case BorderEdge.Top:
                if (ResizeUp)
                {
                    Cursor = Cursor.Parse("SizeNorthSouth");
                    _resizeContext = new(BorderEdge.Top);
                    _moveContext = null;
                }
                break;
            case BorderEdge.Left:
                if (ResizeLeft)
                {
                    Cursor = Cursor.Parse("SizeWestEast");
                    _resizeContext = new(BorderEdge.Left);
                    _moveContext = null;
                }
                break;
            case BorderEdge.Right:
                if (ResizeRight)
                {
                    Cursor = Cursor.Parse("SizeWestEast");
                    _resizeContext = new(BorderEdge.Right);
                    _moveContext = null;
                }
                break;
            default:
                _resizeContext = null;
                if (CanMove)
                {
                    Cursor = Cursor.Parse("DragMove");
                    DateTime? from = CoordsToDateTime((e.GetPosition(_canvas).X, e.GetPosition(_canvas).Y));
                    if (from is null) return;
                    _moveContext = new(from.Value, Event.Start, Event.End);
                }
                else
                {
                    Cursor = Cursor.Default;
                }
                break;
        }

    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        IlluminateAllEventBorders(false);
        base.OnPointerExited(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (_resizeContext is not null)
        {
            //go into resize mode
            _resizeMode = true;
        }
        if (_moveContext is not null)
        {
            //go into move mode
            _moveMode = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _resizeMode = false;
        _moveMode = false;
        _resizeContext = null;
        _moveContext = null;
        _calendarView!.ForceRender();
    }
    private BorderEdge? DetermineEdge(PointerEventArgs e)
    {
        int allowance = 10;
        double height = Bounds.Height;
        double width = Bounds.Width;
        var pos = e.GetPosition(this);
        //top
        if (pos.Y <= allowance)
        {
            return BorderEdge.Top;
        }
        //bottom
        else if (height - pos.Y <= allowance)
        {
            return BorderEdge.Bottom;
        }
        //left
        else if (pos.X <= allowance)
        {
            return BorderEdge.Left;
        }
        else if (width - pos.X <= allowance)
        {
            return BorderEdge.Right;
        }
        return BorderEdge.Center;
    }

    private void IlluminateAllEventBorders(bool illuminate = true)
    {
        if (_canvas is null) return;
        var borders = _canvas.Children.OfType<EventBorder>().Where(p => p.Event == Event).ToList();
        foreach (EventBorder border in borders)
        {
            border.Background = illuminate ? border.HoverBackground : border.DullBackground;
        }
    }
}

internal record CanvasDrawContext(
    CalendarEvent _event,
    Rect bounds,
    int column,
    int indexOfFirstCell,
    int indexOfLastCell,
    int numberOfGridColumns,
    int eventGridColumn,
    DateTime columnDate,
    bool truncateTop,
    int spaceForMultiDayEvents
);

internal enum BorderEdge
{
    Top,
    Right,
    Bottom,
    Left,
    Center
}

internal enum EventBorderType
{
    Multiday,
    SingleDay,
}
internal record ResizeContext(
    BorderEdge Edge
);
internal record MoveContext(
    DateTime moveFrom,
    DateTime originalStart,
    DateTime originalEnd
);
