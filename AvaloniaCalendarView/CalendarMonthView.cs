namespace AvaloniaCalendarView;

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
internal class MonthView : ContentControl, ICalendarView
{
    private readonly String[] _daysArray = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    public MonthView(DateTime _dateTime, IEnumerable<CalendarEvent> _dateEvents)
    {
        ViewDate = _dateTime;
        DateEvents = _dateEvents;
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
            Grid pGrid = new() { RowDefinitions = new("*,*,*,*,*") };
            pGrid.Classes.Add("avalonia_calendar_view_gridcolumn");
            Grid.SetColumn(pGrid, i);
            SetDateColumnContent(pGrid);
            dateGrid.Children.Add(pGrid);
        }
        MainGrid.Children.Add(dateGrid);
        Content = MainGrid;
    }
    public DateTime ViewDate { get; }
    public IEnumerable<CalendarEvent> DateEvents { get; set; }

    private void SetDateColumnContent(Grid _col)
    {
        int col = Grid.GetColumn(_col);
        int numberOfRows = ViewDate.Month == 2 && new DateOnly(ViewDate.Year, ViewDate.Month, 1).DayOfWeek == 0 ? 4 : 5;
        for (int row = 0; row < numberOfRows; row++)
        {
            Thickness thickness = new(1, 1, col == 6 ? 1 : 0, row == numberOfRows - 1 ? 1 : 0);
            Border colBorder = new() { BorderBrush = Brushes.Gray, BorderThickness = thickness };
            colBorder.Classes.Add("avalonia_calendar_view_gridcell");
            Grid pWrapperGrid = new();
            Panel p = new() { Background = Brushes.Transparent };
            var squaredate = CalculateGridSquareDate(row, col, out bool outOfBounds, out string? direction);
            var textblock = new TextBlock() { Text = squaredate, TextAlignment = Avalonia.Media.TextAlignment.Right, Margin = new(10), FontSize = 20 };
            if (outOfBounds)
            {
                textblock.Foreground = new SolidColorBrush(Colors.Gray);
            }
            p.Children.Add(textblock);

            //Calculate and display the number of events on this day of this month of this year
            DateTime dateOfThisCell;
            if (outOfBounds)
            {
                dateOfThisCell = ViewDate.AddMonths(direction == "next" ? +1 : -1);
                dateOfThisCell = new(dateOfThisCell.Year, dateOfThisCell.Month, int.Parse(squaredate));
            }
            else
            {
                dateOfThisCell = new(ViewDate.Year, ViewDate.Month, int.Parse(squaredate));
            }
            //get all events that fall on this date, ignoring time
            var eventsOnThisDate = DateEvents.Where(p =>
                dateOfThisCell.Date >= p.Start.Date &&
                dateOfThisCell.Date <= p.End.Date
            );
            if (eventsOnThisDate.Count() > 0)
            {
                Border eventCounterTextBlockBorder = new()
                {
                    CornerRadius = new(100),
                    Padding = new(5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    Background = Brushes.Coral,
                    Margin = new(10),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                };
                var eventCounterTextBlock = new TextBlock()
                {
                    Text = eventsOnThisDate.Count().ToString(),
                    FontSize = 15,
                };
                eventCounterTextBlockBorder.Child = eventCounterTextBlock;
                p.Children.Add(eventCounterTextBlockBorder);
            }
            //draw the events
            WrapPanel eventsHolder = new() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom };
            foreach (CalendarEvent _event in eventsOnThisDate)
            {
                //tiny blob that represents the event
                Border eventBlob = new() { Height = 10, Width = 10, CornerRadius = new(100), Background = _event.BackgroundBrush, Margin = new(1) };
                ToolTip.SetTip(eventBlob, _event.Title);
                eventsHolder.Children.Add(eventBlob);
            }
            p.Children.Add(eventsHolder);
            colBorder.Child = p;
            pWrapperGrid.Children.Add(colBorder);
            if (outOfBounds)
            {
                //draw a grey overlay over this cell
                Panel overlay = new() { Background = new SolidColorBrush(Colors.Black, 0.5) };
                pWrapperGrid.Children.Add(overlay);
            }
            Grid.SetRow(pWrapperGrid, row);
            _col.Children.Add(pWrapperGrid);
        }
    }
    ///<summary>
    /// Gets the date that will be displayed on the month grid cell
    /// </summary>
    private string CalculateGridSquareDate(int row, int col, out bool outOfBounds, out string? direction)
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
            direction = "prev";
            return (days_in_month - (firstDay - col) + 1).ToString();
        }
        //out of bounds, next month
        if (row == 4 && col > lastDay)
        {
            outOfBounds = true;
            direction = "next";
            return (col - lastDay).ToString();
        }
        outOfBounds = false;
        direction = null;
        return row == 0 ? ((col - firstDay) + 1).ToString() : ((((row - 1) * 7) + col + (6 - firstDay) + 2).ToString());
    }
}
