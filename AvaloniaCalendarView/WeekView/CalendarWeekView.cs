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
        foreach (var _event in eventsOnThisDay)
        {
            //obtain start and end hours relative to the current column
            TimeOnly hourStart = _event.Start.Date == columnDate.Date ? TimeOnly.FromDateTime(_event.Start) : new(0, 0);
            TimeOnly hourEnd = _event.End.Date == columnDate.Date ? TimeOnly.FromDateTime(_event.End) : new(23, 59);
            int indexOfFirstCell = hourStart.Hour * 2 + (hourStart.Minute >= 30 ? 1 : 0);
            int indexOfLastCell = hourEnd.Hour * 2 + (hourEnd.Minute >= 30 ? 1 : 0);
            var firstcell = (Border)col.Children.Where(p => p is Border colBorder).ToList()[indexOfFirstCell];
            firstcell.PropertyChanged += (s, e) =>
            {
                if (e.Property == Border.BoundsProperty)
                {
                    DrawEventOnCanvas(_event, firstcell.Bounds, Grid.GetColumn(col), indexOfFirstCell, indexOfLastCell, gridColumns);
                }
            };
        }
    }
    private void DrawEventOnCanvas(CalendarEvent _event, Rect bounds, int column, int indexOfFirstCell, int indexOfLastCell, int numberOfGridColumns = 0)
    {
        //prevent drawing the same border more than once
        DrawingCanvas.Children.RemoveAll(DrawingCanvas.Children.Where(b => b is EventBorder bE && (bE.Guid == _event.EventID && bE.Column == column)));
        var x = (bounds.X + ((column - 1) * bounds.Width)) + 7;
        var y = bounds.Y - 2;
        var height = bounds.Height + (bounds.Height * (indexOfLastCell - indexOfFirstCell));
        var width = bounds.Width - 10;

        if (numberOfGridColumns > 1)
        {
            width = width / numberOfGridColumns;
            //how many borders have already been drawn in this column?
            int count = DrawingCanvas.Children.Where(p => p is EventBorder bE && bE.Column == column).Count();
            Console.WriteLine(count);
            x += (count * width);
        }
        EventBorder p = new()
        {
            Height = height,
            Width = width,
            Background = _event.BackgroundBrush,
            CornerRadius = new(10),
            BoxShadow = new(new() { Color = Colors.Black, IsInset = true }),
            Guid = _event.EventID,
            Column = column
        };
        Canvas.SetTop(p, y);
        Canvas.SetLeft(p, x);
        DrawingCanvas.Children.Add(p);
    }
}

internal class EventBorder : Border
{
    //the column of the date grid, represents the day of the grid
    public int Column;
    public Guid Guid;
}
