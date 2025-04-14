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

    public DayView(DateTime _dateTime, IEnumerable<CalendarEvent> _dateEvents)
    {
        ViewDate = _dateTime;
        DateEvents = _dateEvents;
        _eventDrawer = new(DateEvents, DrawingCanvas);
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
        string gridstring = string.Concat(Enumerable.Repeat("*,", 48)).TrimEnd(',');
        Grid MainGrid = new();
        TimeOnly hour = new(0, 0);
        Grid dategrid = new() { ColumnDefinitions = new("80,*") };
        Grid timegrid = new() { RowDefinitions = new(gridstring) };
        Grid cellgrid = new() { RowDefinitions = new(gridstring) };
        Grid.SetColumn(timegrid, 0);
        Grid.SetColumn(cellgrid, 1);
        for (int i = 0; i < 48; i++)
        {
            Thickness thickness = new(1, i == 0 ? 1 : 0, 1, 1);
            Border colBorder = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness, Padding = new(5) };
            colBorder.Classes.Add("avalonia_calendar_view_gridcell");
            Panel p = new() { Height = 20 };
            if ((i == 0 || i % 2 == 0))
            {
                if (i != 0)
                {
                    hour = hour.AddHours(1);
                }
                var squaretime = hour.ToString("h tt");
                var textblock = new TextBlock() { Text = squaretime, TextAlignment = Avalonia.Media.TextAlignment.Center, FontSize = 15 };
                p.Children.Add(textblock);
            }
            colBorder.Child = p;
            Grid.SetRow(colBorder, i);
            timegrid.Children.Add(colBorder);

            Border cell = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness };
            Grid.SetRow(cell, i);
            cellgrid.Children.Add(cell);
        }
        //this is for the eventdrawer
        dategrid.Children.Add(timegrid);
        dategrid.Children.Add(cellgrid);
        MainGrid.Children.Add(dategrid);
        cellgrid.Children.Add(DrawingCanvas);
        Content = MainGrid;
        _eventDrawer.DrawEvents(cellgrid, ViewDate);
    }
}
