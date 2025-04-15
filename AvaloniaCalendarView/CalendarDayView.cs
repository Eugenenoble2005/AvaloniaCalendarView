using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace AvaloniaCalendarView.DayView;
internal class DayView : ContentControl, ICalendarView
{
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get; set; }

    private readonly EventDrawer _eventDrawer;

    public Canvas DrawingCanvas = new() { };
    private readonly uint _hourDuration = 60;
    private uint _cellDuration => _hourDuration / 2;

    public DayView(DateTime _dateTime, IEnumerable<CalendarEvent> _dateEvents, uint hourDuration)
    {
        _hourDuration = hourDuration;
        ViewDate = _dateTime;
        DateEvents = _dateEvents;
        _eventDrawer = new(DateEvents, DrawingCanvas, (int)_cellDuration);
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
        int numberOfRows = (int)Math.Ceiling((double)(24 * 60) / _cellDuration);
        string gridstring = string.Concat(Enumerable.Repeat("*,", numberOfRows)).TrimEnd(',');
        Grid MainGrid = new();
        TimeOnly hour = new(0, 0);
        Grid dategrid = new() { ColumnDefinitions = new("80,*") };
        Grid timegrid = new() { RowDefinitions = new(gridstring) };
        Grid cellgrid = new() { RowDefinitions = new(gridstring) };
        Grid.SetColumn(timegrid, 0);
        Grid.SetColumn(cellgrid, 1);
        for (int i = 0; i < numberOfRows; i++)
        {
            Thickness thickness = new(1, i == 0 ? 1 : 0, 1, 1);
            Border colBorder = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness, Padding = new(5) };
            colBorder.Classes.Add("avalonia_calendar_view_gridcell");
            Panel p = new() { Height = 20 };
            if ((i == 0 || i % 2 == 0))
            {
                if (i != 0)
                {
                }
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
        dategrid.Children.Add(cellgrid);
        MainGrid.Children.Add(dategrid);
        cellgrid.Children.Add(DrawingCanvas);
        Content = MainGrid;
        _eventDrawer.DrawEvents(cellgrid, ViewDate);
    }
}
