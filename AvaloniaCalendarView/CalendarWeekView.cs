using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace AvaloniaCalendarView;
internal class WeekView : ContentControl, ICalendarView
{
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get; set; }

    private readonly String[] _daysArray = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    private readonly EventDrawer _eventDrawer;
    private readonly uint _hourDuration = 60;
    private readonly uint _dayStartHour;
    private readonly uint _dayEndHour;
    private uint _cellDuration => _hourDuration / 2;
    private int _numberOfRows;
    public WeekView(DateTime _dateTime, IEnumerable<CalendarEvent> _dateEvents, uint hourDuration, uint dayStartHour, uint dayEndHour)
    {
        ViewDate = _dateTime;
        DateEvents = _dateEvents;
        _hourDuration = hourDuration;
        _dayStartHour = dayStartHour;
        _dayEndHour = dayEndHour;
        _eventDrawer = new(DateEvents, _canvas, (int)_cellDuration, (int)_dayStartHour, ViewType.Week);
        try
        {
            Initialize();
        }
        catch (Exception E)
        {
            Console.WriteLine(E);
        }
    }
    private DrawingCanvas _canvas = new() { Margin = new(80, 0, 0, 0), };
    private void Initialize()
    {
        Grid MainGrid = new() { RowDefinitions = new("Auto,*") };
        //grid for daynames
        Grid dayNameGrid = new() { ColumnDefinitions = new("Auto,*,*,*,*,*,*,*"), Background = Brushes.Transparent };
        Grid.SetRow(dayNameGrid, 0);
        int viewday = (int)ViewDate.DayOfWeek;
        //sunday is the first day of the week
        var sunday = ViewDate.AddDays(0 - viewday);
        var columnDates = new List<DateTime>();
        //obtain the day that has the contains the most number of events over a day
        int maxNumberOfMultiDayEventsInOneDay = DateEvents.Where(p => p.End.Date >= sunday.Date && p.Start.Date <= sunday.AddDays(6) && (p.End - p.Start).TotalHours >= 24).Count();
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
        //we have to predetermine this before we draw the events
        int spaceForMultiDayEvents = 30 * maxNumberOfMultiDayEventsInOneDay;
        _canvas.SpaceForMultiDayEvents = spaceForMultiDayEvents;
        MainGrid.Children.Add(dayNameGrid);
        Grid dateGridOuter = new();
        Grid dateGrid = new() { ColumnDefinitions = new("Auto,*,*,*,*,*,*,*"), Margin = new(0, spaceForMultiDayEvents, 0, 0) };
        Grid.SetRow(dateGridOuter, 1);
        _numberOfRows = (int)Math.Ceiling((double)(((_dayEndHour - _dayStartHour) + 1) * 60) / _cellDuration);
        string gridstring = string.Concat(Enumerable.Repeat("*,", _numberOfRows)).TrimEnd(',');
        for (int i = 0; i < 8; i++)
        {
            Grid pGrid = new() { RowDefinitions = new(gridstring) };
            Grid.SetColumn(pGrid, i);
            SetColumnContent(pGrid, i == 0 ? new() : columnDates[i - 1]);
            //draw events on a daily basis
            _eventDrawer.DrawEvents(pGrid, i == 0 ? new() : columnDates[i - 1]);
            dateGrid.Children.Add(pGrid);
        }
        dateGridOuter.Children.Add(dateGrid);
        dateGridOuter.Children.Add(_canvas);
        MainGrid.Children.Add(dateGridOuter);
        Content = MainGrid;
    }
    private void SetColumnContent(Grid _col, DateTime columnDate)
    {
        int col = Grid.GetColumn(_col);
        TimeOnly hour = new((int)_dayStartHour, 0);
        uint hoursBetween = (_dayEndHour - _dayStartHour) + 1;
        for (int row = 0; row < _numberOfRows; row++)
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
                    var squaretime = hour.ToString("h:mm tt");
                    var textblock = new TextBlock() { Text = squaretime, TextAlignment = Avalonia.Media.TextAlignment.Center, FontSize = 15, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    p.Children.Add(textblock);
                }
            }
            colBorder.Child = p;
            Grid.SetRow(colBorder, row);
            _col.Children.Add(colBorder);
            hour = hour.AddMinutes(_cellDuration);
        }
    }
}
internal class DrawingCanvas : Canvas
{
    //this is just used for resizing and moving
    public double SpaceForMultiDayEvents { get; set; }
}
