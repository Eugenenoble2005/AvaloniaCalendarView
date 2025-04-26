using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace AvaloniaCalendarView;
internal class DayView : ContentControl, ICalendarView
{
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get; set; }

    private readonly EventDrawer _eventDrawer;

    public DrawingCanvas _canvas = new() { };
    private readonly uint _hourDuration = 60;
    private uint _cellDuration => _hourDuration / 2;
    private uint _dayStartHour;
    private uint _dayEndHour;
    public DayView(DateTime _dateTime, IEnumerable<CalendarEvent> _dateEvents, uint hourDuration, uint dayStartHour, uint dayEndHour)
    {
        _hourDuration = hourDuration;
        ViewDate = _dateTime;
        DateEvents = _dateEvents;
        _dayStartHour = dayStartHour;
        _dayEndHour = dayEndHour;
        _eventDrawer = new(DateEvents, _canvas, (int)_cellDuration, (int)_dayStartHour, ViewType.Day);
        try
        {
            Initialize();
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex);
        }
    }

    private void Initialize()
    {
        int numberOfRows = (int)Math.Ceiling((double)(((_dayEndHour - _dayStartHour) + 1) * 60) / _cellDuration);
        string gridstring = string.Concat(Enumerable.Repeat("*,", numberOfRows)).TrimEnd(',');
        Grid MainGrid = new();
        int spaceForMultiDayEvents = DateEvents.Where(p => ViewDate.Date >= p.Start.Date && ViewDate.Date <= p.End.Date && (p.End - p.Start).TotalHours >= 24).Count() * 30;
        _canvas.SpaceForMultiDayEvents = spaceForMultiDayEvents;
        TimeOnly hour = new((int)_dayStartHour, 0);
        Grid dategrid = new() { ColumnDefinitions = new("80,*") };
        Grid timegrid = new() { RowDefinitions = new(gridstring), Margin = new(0, spaceForMultiDayEvents, 0, 0) };
        Grid cellgridouter = new();
        Grid cellgrid = new() { RowDefinitions = new(gridstring), Margin = new(0, spaceForMultiDayEvents, 0, 0) };
        Grid.SetColumn(timegrid, 0);
        Grid.SetColumn(cellgridouter, 1);
        Grid.SetColumn(cellgrid, 1);
        for (int i = 0; i < numberOfRows; i++)
        {
            Thickness thickness = new(1, 1, 1, 0);
            Border colBorder = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness, Padding = new(5) };
            colBorder.Classes.Add("avalonia_calendar_view_gridcell");
            Panel p = new() { Height = 20 };
            if ((i == 0 || i % 2 == 0))
            {
                var squaretime = hour.ToString("h:mm tt");
                var textblock = new TextBlock() { Text = squaretime, TextAlignment = Avalonia.Media.TextAlignment.Center, FontSize = 15 };
                p.Children.Add(textblock);
            }
            colBorder.Child = p;
            Grid.SetRow(colBorder, i);
            timegrid.Children.Add(colBorder);

            Border cell = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness };
            Grid.SetRow(cell, i);
            cellgrid.Children.Add(cell);
            hour = hour.AddMinutes(_cellDuration);
        }
        dategrid.Children.Add(timegrid);
        cellgridouter.Children.Add(cellgrid);
        dategrid.Children.Add(cellgridouter);
        MainGrid.Children.Add(dategrid);
        cellgridouter.Children.Add(_canvas);
        Content = MainGrid;
        _eventDrawer.DrawEvents(cellgrid, ViewDate);
    }
}
