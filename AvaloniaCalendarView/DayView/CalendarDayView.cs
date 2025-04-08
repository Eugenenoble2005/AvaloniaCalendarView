using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace AvaloniaCalendarView.DayView;
internal class DayView : ContentControl, ICalendarView
{
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public DayView(DateTime _dateTime)
    {
        ViewDate = _dateTime;
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
        Grid MainGrid = new() { RowDefinitions = new(gridstring) };
        TimeOnly hour = new(0, 0);
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
                var textblock = new TextBlock() { Text = squaretime, TextAlignment = Avalonia.Media.TextAlignment.Left, FontSize = 15 };
                p.Children.Add(textblock);
            }
            colBorder.Child = p;
            Grid.SetRow(colBorder, i);
            MainGrid.Children.Add(colBorder);
        }
        Content = MainGrid;
    }
}
