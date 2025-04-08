using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
namespace AvaloniaCalendarView.WeekView;
internal class WeekView : ContentControl, ICalendarView
{
    public DateTime ViewDate { get; }
    private readonly String[] _daysArray = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

    public WeekView(DateTime _dateTime)
    {
        ViewDate = _dateTime;
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
        for (int i = 0; i < 8; i++)
        {
            var date = sunday.AddDays(i - 1);
            Thickness thickness = new(1, 1, i == 7 ? 1 : 0, 1);
            Border pb = new() { BorderThickness = thickness, BorderBrush = Brushes.Gray, Padding = new(5) };
            if (i == 0)
            {
                Panel p = new() { Width = 80 };
                pb.Child = p;
                Grid.SetColumn(pb, 0);
                dayNameGrid.Children.Add(pb);
                continue;
            }
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
            SetColumnContent(pGrid);
            dateGrid.Children.Add(pGrid);
        }
        MainGrid.Children.Add(dateGrid);
        Content = MainGrid;
    }
    private void SetColumnContent(Grid col)
    {
        int index = Grid.GetColumn(col);
        TimeOnly hour = new(0, 0);
        for (int i = 0; i < 48; i++)
        {
            Thickness thickness = new(1, 0, index == 7 ? 1 : 0, 1);
            Border colBorder = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness, Padding = new(5) };
            Panel p = new() { Height = 20 };
            if (i == 0)
            {
                p.Width = 80;
            }
            if ((i == 0 || i % 2 == 0) && index == 0)
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
            col.Children.Add(colBorder);
        }
    }
}
