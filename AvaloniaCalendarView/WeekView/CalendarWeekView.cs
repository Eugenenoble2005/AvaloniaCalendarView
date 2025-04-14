using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
namespace AvaloniaCalendarView.WeekView;
internal class WeekView : ContentControl, ICalendarView
{
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get; set; }

    private readonly String[] _daysArray = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };


    public WeekView(DateTime _dateTime, IEnumerable<CalendarEvent> _dateEvents)
    {
        ViewDate = _dateTime;
        DateEvents = _dateEvents;
        try
        {
            Initialize();
        }
        catch (Exception E)
        {
            Console.WriteLine(E);
        }
    }
    public Canvas DrawingCanvas = new() { Margin = new(80, 0, 0, 0) };
    private void Initialize()
    {
        Grid MainGrid = new() { RowDefinitions = new("Auto,*") };
        //grid for daynames
        Grid dayNameGrid = new() { ColumnDefinitions = new("Auto,*,*,*,*,*,*,*"), Background = Brushes.Transparent };
        Grid.SetRow(dayNameGrid, 0);
        int viewday = (int)ViewDate.DayOfWeek;
        var sunday = ViewDate.AddDays(0 - viewday);
        var columnDates = new List<DateTime>();
        for (int i = 0; i < 8; i++)
        {
            var date = sunday.AddDays(i - 1);
            Thickness thickness = new(1, 1, i == 7 ? 1 : 0, 1);
            Border pb = new() { BorderThickness = thickness, BorderBrush = Brushes.Gray, MinHeight = 30, Padding = new(0, 5, 0, 5) };
            if (i == 0)
            {
                Panel p = new() { Width = 80 };
                pb.Child = p;
                Grid.SetColumn(pb, 0);
                dayNameGrid.Children.Add(pb);
                continue;
            }
            columnDates.Add(date);
            StackPanel pl = new() { Width = 200, Background = Brushes.Transparent, Orientation = Avalonia.Layout.Orientation.Vertical };
            pl.Children.Add(new TextBlock() { Text = _daysArray[i - 1], TextAlignment = Avalonia.Media.TextAlignment.Center, FontWeight = FontWeight.Bold });
            pl.Children.Add(new TextBlock() { Text = date.ToString("MMM d"), TextAlignment = TextAlignment.Center });
            pb.Child = pl;
            Grid.SetColumn(pb, i);
            dayNameGrid.Children.Add(pb);
        }
        MainGrid.Children.Add(dayNameGrid);
        // 48x7 grid for hours
        Grid dateGridOuter = new();
        Grid dateGrid = new() { ColumnDefinitions = new("Auto,*,*,*,*,*,*,*") };
        Grid.SetRow(dateGridOuter, 1);
        string gridstring = string.Concat(Enumerable.Repeat("*,", 48)).TrimEnd(',');
        for (int i = 0; i < 8; i++)
        {
            Grid pGrid = new() { RowDefinitions = new(gridstring) };
            Grid.SetColumn(pGrid, i);
            SetColumnContent(pGrid, i == 0 ? new() : columnDates[i - 1]);
            DrawEvents(pGrid, i == 0 ? new() : columnDates[i - 1]);
            dateGrid.Children.Add(pGrid);
        }
        dateGridOuter.Children.Add(dateGrid);
        dateGridOuter.Children.Add(DrawingCanvas);
        MainGrid.Children.Add(dateGridOuter);
        Content = MainGrid;

    }
    private void SetColumnContent(Grid _col, DateTime columnDate)
    {
        int col = Grid.GetColumn(_col);
        TimeOnly hour = new(0, 0);
        for (int row = 0; row < 48; row++)
        {
            Thickness thickness = new(1, 0, col == 7 ? 1 : 0, 1);
            Border colBorder = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness, MinHeight = 30 };
            colBorder.Classes.Add("avalonia_calendar_view_gridcell");
            Panel p = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch };
            DateTime celldate = new(columnDate.Year, columnDate.Month, columnDate.Day, hour.Hour, hour.Minute, 0);
            if (row == 0 && col == 0)
            {
                p.Width = 80;
            }
            if ((row == 0 || row % 2 == 0))
            {
                if (col == 0)
                {
                    var squaretime = hour.ToString("h tt");
                    var textblock = new TextBlock() { Text = squaretime, TextAlignment = Avalonia.Media.TextAlignment.Center, FontSize = 15, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    p.Children.Add(textblock);
                }
            }
            colBorder.Child = p;
            Grid.SetRow(colBorder, row);
            _col.Children.Add(colBorder);
            hour = hour.AddMinutes(30);
        }
    }
    private void DrawEvents(Grid col, DateTime columnDate)
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
    double x = (bounds.X + ((column - 1) * bounds.Width)) + 7;
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
        BorderThickness = new(2),
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
