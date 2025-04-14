using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;

namespace AvaloniaCalendarView;

// This should probably be an interface
///<summary>
/// Represents an event that can be displayed on the calendar view.
/// </summary>
public class CalendarEvent
{
    ///<summary>
    /// The title of the event
    /// </summary>
    public required string Title;

    ///<summary>
    /// The DateTime object representing when the event starts.
    /// </summary>
    public required DateTime Start;

    ///<summary>
    /// The DateTime object representing when the event ends.
    /// </summary>
    public required DateTime End;

    ///<summary>
    /// The Brush that paints the background of the event
    /// </summary>
    public IBrush BackgroundBrush = Brushes.Blue;

    ///<summary>
    /// The Brush that paints the foreground of the event
    ///</summary>
    public IBrush? DateForegroundBrush;

    ///<summary>
    // Whether or not this event can be resized on the view
    // </summary>
    public bool Resizable = false;

    public Guid EventID { get; } = Guid.CreateVersion7();

    public IBrush DullBackgroundBrush()
    {
        if (BackgroundBrush is ISolidColorBrush scBrush)
        {
            var dullColor = Color.FromArgb(
                scBrush.Color.A, // preserve alpha
                (byte)(scBrush.Color.R * 0.6 + 80 * 0.4), // mix with gray
                (byte)(scBrush.Color.G * 0.6 + 80 * 0.4),
                (byte)(scBrush.Color.B * 0.6 + 80 * 0.4)
            );
            return new SolidColorBrush(dullColor);
        }
        else return BackgroundBrush;
    }

}
internal class EventDrawer(IEnumerable<CalendarEvent> DateEvents, Canvas DrawingCanvas)
{
    public void DrawEvents(Grid col, DateTime columnDate)
    {
        var eventsOnThisDay = DateEvents.Where(p =>
            columnDate.Date >= p.Start.Date && columnDate.Date <= p.End.Date
        );
        int gridColumns = eventsOnThisDay.Count();
        //when there are multiple events on one day, we will grid them and sort them by the GUID
        var arrayOfGuids = eventsOnThisDay.Select(p => p.EventID).ToArray();

        //this is used to look for intersections with other events on the same day so we can draw the event full width if it will not intersect with other events
        List<List<TimeOnly>> hourStartEndList = eventsOnThisDay.Select(p => new List<TimeOnly> {
            p.Start.Date == columnDate.Date ? TimeOnly.FromDateTime(p.Start) : new(0,0),
            p.End.Date == columnDate.Date ? TimeOnly.FromDateTime(p.End) : new(23,59)
        }).ToList();
        foreach (var _event in eventsOnThisDay)
        {
            //obtain start and end hours relative to the current column
            TimeOnly hourStart = _event.Start.Date == columnDate.Date ? TimeOnly.FromDateTime(_event.Start) : new(0, 0);
            TimeOnly hourEnd = _event.End.Date == columnDate.Date ? TimeOnly.FromDateTime(_event.End) : new(23, 59);
            int indexOfFirstCell = hourStart.Hour * 2 + (hourStart.Minute >= 30 ? 1 : 0);
            int indexOfLastCell = hourEnd.Hour * 2 + (hourEnd.Minute >= 30 ? 1 : 0);
            var firstcell = (Border)col.Children.Where(p => p is Border colBorder).ToList()[indexOfFirstCell];
            //remove the hourStart and hourEnd of this list from the list above
            var filteredHourStartEndList = hourStartEndList
                .Where(p => !(p[0] == hourStart && p[1] == hourEnd))
                .ToList();
            firstcell.PropertyChanged += (s, e) =>
            {
                if (e.Property == Border.BoundsProperty)
                {
                    DrawEventOnCanvas(_event,
                                    firstcell.Bounds,
                                    Grid.GetColumn(col),
                                    indexOfFirstCell,
                                    indexOfLastCell,
                                    gridColumns,
                                    Array.IndexOf(arrayOfGuids, _event.EventID),
                                    columnDate,
                                    filteredHourStartEndList,
                                    new List<TimeOnly>() { hourStart, hourEnd }
                                    );
                }
            };
        }
    }
    private void DrawEventOnCanvas(
        CalendarEvent calendarEvent,
        Rect bounds,
        int column,
        int indexOfFirstCell,
        int indexOfLastCell,
        int numberOfGridColumns,
        int eventGridColumn,
        DateTime columnDate,
        List<List<TimeOnly>> hourStartEndList,
        List<TimeOnly> hourStartEnd)
    {
        // Prevent drawing the same border more than once
        DrawingCanvas.Children.RemoveAll(
            DrawingCanvas.Children
                .OfType<EventBorder>()
                .Where(b => b.Column == column && b.Guid == calendarEvent.EventID)
        );

        // Calculate base position and size
        double x = (bounds.X + ((column - 1) * bounds.Width)) + 7; //column will always be one for dayview
        double y = bounds.Y - 2;
        double height = bounds.Height + (bounds.Height * (indexOfLastCell - indexOfFirstCell));
        double width = bounds.Width - 10;

        // Offset for partial hour start
        double startCorrectionOffset = 0;
        if (columnDate.Date == calendarEvent.Start.Date)
        {
            int anchor = calendarEvent.Start.Minute >= 30 ? 30 : 0;
            int diff = calendarEvent.Start.Minute - anchor;
            double offset = (diff / 30.0) * bounds.Height;
            y += offset;
            startCorrectionOffset = offset;
        }

        // Offset for partial hour end
        if (columnDate.Date == calendarEvent.End.Date)
        {
            int anchor = calendarEvent.End.Minute >= 30 ? 30 : 0;
            int diff = 30 - (calendarEvent.End.Minute - anchor);
            double offset = (diff / 30.0) * bounds.Height;
            height -= (offset + startCorrectionOffset);
        }

        // Calculate event stacking for overlaps
        int intersections = hourStartEndList.Count(p =>
            hourStartEnd[0] < p[1] && p[0] < hourStartEnd[1]
        );

        if (numberOfGridColumns > 1 && intersections > 0)
        {
            width /= numberOfGridColumns;
            x += eventGridColumn * width;
        }

        // Create event border
        var border = new EventBorder
        {
            Height = height,
            Width = width,
            Background = calendarEvent.DullBackgroundBrush(),
            CornerRadius = new(10),
            BoxShadow = new(new() { Color = Colors.Black, IsInset = true }),
            Guid = calendarEvent.EventID,
            BorderThickness = new(5),
            BorderBrush = calendarEvent.BackgroundBrush,
            Column = column
        };

        // Create event content
        var titleText = new TextBlock
        {
            Text = calendarEvent.Title,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 15
        };

        var timeText = new TextBlock
        {
            Text = $"{calendarEvent.Start:d, MMMM yyyy h:mm tt} - {calendarEvent.End:d, MMM yyyy h:mm tt}",
            TextAlignment = TextAlignment.Center,
            FontSize = 10,
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

        // Position and add to canvas
        Canvas.SetTop(border, y);
        Canvas.SetLeft(border, x);
        DrawingCanvas.Children.Add(border);
    }
}

internal class EventBorder : Border
{
    //the column of the date grid, represents the day of the grid
    public int Column;
    public Guid Guid;
}
