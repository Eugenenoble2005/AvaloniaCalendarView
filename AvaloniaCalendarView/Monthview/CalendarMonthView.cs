namespace AvaloniaCalendarView.MonthView;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaCalendarView;
internal class MonthView : ContentControl, ICalendarView
{
    private readonly String[] _daysArray = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    public MonthView(DateTime _dateTime)
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
        Grid MainGrid = new() { RowDefinitions = new("Auto,*") };
        //grid for the day names
        Grid dayNameGrid = new() { ColumnDefinitions = new("*,*,*,*,*,*,*"), Background = Brushes.Transparent };
        Grid.SetRow(dayNameGrid, 0);
        for (int i = 0; i < 7; i++)
        {
            Grid pGrid = new() { Width = 200, Background = Brushes.Transparent };
            pGrid.Children.Add(new TextBlock() { Text = _daysArray[i], TextAlignment = Avalonia.Media.TextAlignment.Center, FontWeight = FontWeight.Bold });
            Grid.SetColumn(pGrid, i);
            dayNameGrid.Children.Add(pGrid);
        }
        MainGrid.Children.Add(dayNameGrid);
        //grid for the actual dates
        Grid dateGrid = new() { ColumnDefinitions = new("*,*,*,*,*,*,*"), Background = Brushes.Transparent };
        Grid.SetRow(dateGrid, 1);
        for (int i = 0; i < 7; i++)
        {
            Grid pGrid = new() { RowDefinitions = new("*,*,*,*,*"), Background = Brushes.Transparent };
            Grid.SetColumn(pGrid, i);
            SetDateColumnContent(pGrid);
            dateGrid.Children.Add(pGrid);
        }
        MainGrid.Children.Add(dateGrid);
        Content = MainGrid;
    }
    public DateTime ViewDate { get; }

    private void SetDateColumnContent(Grid col)
    {
        int index = Grid.GetColumn(col);
        for (int i = 0; i < 5; i++)
        {
            Thickness thickness = new(1, 1, index == 6 ? 1 : 0, i == 4 ? 1 : 0);
            Border colBorder = new() { BorderBrush = new SolidColorBrush(Colors.White), BorderThickness = thickness, Background = Brushes.Transparent };
            Panel p = new() { Background = Brushes.Transparent };
            var squaredate = CalculateGridSquareDate(i, index, out bool outOfBounds);
            var textblock = new TextBlock() { Text = squaredate, TextAlignment = Avalonia.Media.TextAlignment.Right, Margin = new(10), FontSize = 20 };
            if (outOfBounds)
            {
                textblock.Foreground = new SolidColorBrush(Colors.Gray);
            }
            p.Children.Add(textblock);
            colBorder.Child = p;
            Grid.SetRow(colBorder, i);
            col.Children.Add(colBorder);
        }
    }

    private string CalculateGridSquareDate(int row, int col, out bool outOfBounds)
    {
        var month = ViewDate.Month;
        var year = ViewDate.Year;
        int days_in_month = DateTime.DaysInMonth(year, month);
        var firstDay = (int)new DateTime(year, month, 1).DayOfWeek;
        var lastDay = (int)new DateTime(year, month, days_in_month).DayOfWeek;

        //out of bounds, previous month
        if (row == 0 && col < firstDay)
        {
            var newdate = ViewDate.AddMonths(-1);
            days_in_month = DateTime.DaysInMonth(newdate.Year, newdate.Month);
            outOfBounds = true;
            return (days_in_month - (firstDay - col) + 1).ToString();
        }
        //out of bounds, next month
        if (row == 4 && col > lastDay)
        {
            outOfBounds = true;
            return (col - lastDay).ToString();
        }
        outOfBounds = false;
        return row == 0 ? ((col - firstDay) + 1).ToString() : ((((row - 1) * 7) + col + (6 - firstDay) + 2).ToString());
    }
}
