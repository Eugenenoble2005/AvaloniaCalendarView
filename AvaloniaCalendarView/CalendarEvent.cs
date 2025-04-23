using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace AvaloniaCalendarView;
/// <summary>
/// Represents an event that can be displayed on the calendar view.
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// The title of the event.
    /// </summary>
    public required string Title;

    /// <summary>
    /// The DateTime object representing when the event starts.
    /// </summary>
    public required DateTime Start;

    /// <summary>
    /// The DateTime object representing when the event ends.
    /// </summary>
    public required DateTime End;

    /// <summary>
    /// The Brush that paints the background of the event.
    /// </summary>
    public IBrush BackgroundBrush = Brushes.Blue;

    /// <summary>
    /// The Brush that paints the foreground of the event.
    /// </summary>
    public IBrush? DateForegroundBrush;

    /// <summary>
    /// Whether or not this event can be resized on the view.
    /// </summary>
    public bool Resizable = false;

    /// <summary>
    /// A unique identifier for the event.
    /// </summary>
    public Guid EventID { get; } = Guid.CreateVersion7();
}

internal class EventDrawer(IEnumerable<CalendarEvent> events, Canvas canvas, int cellDuration, int dayStartHour, ViewType viewType)
{
    public void DrawEvents(Grid col, DateTime columnDate, int spaceForMultiDayEvents)
    {
        var eventsOnThisDay = events.Where(p =>
            columnDate.Date >= p.Start.Date &&
            columnDate.Date <= p.End.Date &&
            (p.End - p.Start).TotalHours < 24);

        if (Grid.GetColumn(col) == 1)
        {
            DrawMultiDayEvents(col, columnDate, spaceForMultiDayEvents);
        }

        foreach (var _event in eventsOnThisDay)
        {
            var intersections = eventsOnThisDay.Where(p =>
                _event.Start < p.End && p.Start < _event.End);

            int gridColumns = intersections.Count();
            var arrayOfGuids = intersections.Select(p => p.EventID).ToArray();

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
                        Array.IndexOf(arrayOfGuids, _event.EventID),
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
            (p.End - p.Start).TotalHours >= 24);

        var arrayOfGuids = multiDayEventsOnThisWeek.Select(p => p.EventID).ToArray();

        foreach (var _event in multiDayEventsOnThisWeek)
        {
            DateTime intersectionStart = _event.Start.Date > columnDate.Date ? _event.Start.Date : columnDate.Date;
            DateTime intersectionEnd = _event.End.Date < endOfWeek ? _event.End.Date : endOfWeek;

            var distance = (intersectionEnd.Date - intersectionStart.Date).TotalDays + 1;
            var beginning = (intersectionStart.Date - columnDate.Date).TotalDays;

            var collection = col.Children.Where(p => p is Border).ToList();
            var firstCell = (Border)collection[0];

            firstCell.PropertyChanged += (_, e) =>
            {
                if (e.Property == Border.BoundsProperty)
                {
                    canvas.Children.RemoveAll(canvas.Children
                        .OfType<EventBorder>()
                        .Where(b => b.Guid == _event.EventID));

                    var bounds = firstCell.Bounds;
                    var height = 30;
                    var width = bounds.Width * distance;
                    var y = Array.IndexOf(arrayOfGuids, _event.EventID) * height;
                    var x = beginning * bounds.Width;

                    var border = new EventBorder
                    {
                        Height = height,
                        Width = width,
                        Background = DullBackgroundBrush(_event.BackgroundBrush),
                        Guid = _event.EventID,
                        CornerRadius = new CornerRadius(5),
                        BorderThickness = new Thickness(1),
                        BorderBrush = _event.BackgroundBrush,
                    };

                    var childContent = new StackPanel();
                    var eventText = new TextBlock
                    {
                        Text = _event.Title,
                        TextAlignment = TextAlignment.Center
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
            .Where(b => b.Column == context.column && b.Guid == context._event.EventID));

        double x = context.bounds.X + ((context.column - 1) * context.bounds.Width) + 7;
        double y = context.bounds.Y - 2 + context.spaceForMultiDayEvents;
        double height = context.bounds.Height * ((context.indexOfLastCell - context.indexOfFirstCell) + 1);
        double width = context.bounds.Width - 10;

        double startCorrectionOffset = 0;
        if (context.columnDate.Date == context._event.Start.Date && !context.truncateTop)
        {
            var cellDate = new TimeOnly(dayStartHour, 0).AddMinutes(context.indexOfFirstCell * cellDuration);
            var diff = (TimeOnly.FromDateTime(context._event.Start) - cellDate).TotalMinutes;
            var offset = (diff / cellDuration) * context.bounds.Height;
            y += offset;
            startCorrectionOffset = offset;
        }

        if (context.columnDate.Date == context._event.End.Date)
        {
            var cellDate = new TimeOnly(dayStartHour, 0).AddMinutes(context.indexOfLastCell * cellDuration);
            var diff = cellDuration - (TimeOnly.FromDateTime(context._event.End) - cellDate).TotalMinutes;
            var offset = (diff / cellDuration) * context.bounds.Height;
            height -= offset + startCorrectionOffset;
        }

        if (context.numberOfGridColumns > 1)
        {
            width /= context.numberOfGridColumns;
            x += context.eventGridColumn * width;
        }

        var border = new EventBorder
        {
            Height = height,
            Width = width,
            Background = DullBackgroundBrush(context._event.BackgroundBrush),
            CornerRadius = new CornerRadius(context.truncateTop ? 0 : 10, context.truncateTop ? 0 : 10, 10, 10),
            Guid = context._event.EventID,
            BorderThickness = new Thickness(1, context.truncateTop ? 0 : 1, 1, 1),
            BorderBrush = context._event.BackgroundBrush,
            Column = context.column
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
    public int Column;
    public Guid Guid;
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
