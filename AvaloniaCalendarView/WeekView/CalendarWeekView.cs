using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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
            Border pb = new() { BorderThickness = thickness, BorderBrush = Brushes.Gray, MinHeight = 30, Padding = new(0,5,0,5) };
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
        Grid dateGrid = new() { ColumnDefinitions = new("Auto,*,*,*,*,*,*,*") };
        Grid.SetRow(dateGrid, 1);
        string gridstring = string.Concat(Enumerable.Repeat("*,", 48)).TrimEnd(',');
        for (int i = 0; i < 8; i++)
        {
            Grid pGrid = new() { RowDefinitions = new(gridstring) };
            Grid.SetColumn(pGrid, i);
            SetColumnContent(pGrid, i == 0 ? new() : columnDates[i - 1]);
            dateGrid.Children.Add(pGrid);
        }
        MainGrid.Children.Add(dateGrid);
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
            var eventsOnThisHour = DateEvents.Where(p =>
              celldate >= p.Start && celldate <= p.End
            );
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
            string gridconfig = string.Concat(Enumerable.Repeat("*,", eventsOnThisHour.Count())).TrimEnd(',');
            var colGrid = new Grid() { ColumnDefinitions = new(gridconfig), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch };
            int index = 0;
            foreach (var _event in eventsOnThisHour)
            {
                Border eventBorder = new() { Background = _event.BackgroundBrush, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch };
                eventBorder.BorderBrush = Brushes.Red;
                Grid.SetColumn(eventBorder, index);
                colGrid.Children.Add(eventBorder);
                colBorder.BorderThickness = new(0);
                index++;
            }
            p.Children.Add(colGrid);
            colBorder.Child = p;
            Grid.SetRow(colBorder, row);
            _col.Children.Add(colBorder);
            hour = hour.AddMinutes(30);
        }
    }
}
