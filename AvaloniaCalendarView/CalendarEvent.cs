using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;

namespace AvaloniaCalendarView
{
    /// <summary>
    /// Represents an event that can be displayed on the calendar view.
    /// </summary>
    public class CalendarEvent
    {
        /// <summary>
        /// The title of the event.
        /// </summary>
        public required string Title;

        /// <summary>
        /// The DateTime object representing when the event starts.
        /// </summary>
        public required DateTime Start;

        /// <summary>
        /// The DateTime object representing when the event ends.
        /// </summary>
        public required DateTime End;

        /// <summary>
        /// The Brush that paints the background of the event.
        /// </summary>
        public IBrush BackgroundBrush = Brushes.Blue;

        /// <summary>
        /// The Brush that paints the foreground of the event.
        /// </summary>
        public IBrush? DateForegroundBrush;

        /// <summary>
        /// Whether or not this event can be resized on the view.
        /// </summary>
        public bool Resizable = false;

        /// <summary>
        /// A unique identifier for the event.
        /// </summary>
        public Guid EventID { get; } = Guid.CreateVersion7();

        /// <summary>
        /// Returns a desaturated version of the background brush.
        /// </summary>
        public IBrush DullBackgroundBrush()
        {
            if (BackgroundBrush is ISolidColorBrush scBrush)
            {
                var dullColor = Color.FromArgb(
                    scBrush.Color.A,
                    (byte)(scBrush.Color.R * 0.6 + 80 * 0.4),
                    (byte)(scBrush.Color.G * 0.6 + 80 * 0.4),
                    (byte)(scBrush.Color.B * 0.6 + 80 * 0.4)
                );
                return new SolidColorBrush(dullColor);
            }
            else return BackgroundBrush;
        }
    }

    internal class EventDrawer(IEnumerable<CalendarEvent> dateEvents, Canvas drawingCanvas, int cellDuration, int dayStartHour)
    {
        private readonly IEnumerable<CalendarEvent> DateEvents = dateEvents;
        private readonly Canvas DrawingCanvas = drawingCanvas;
        private readonly int CellDuration = cellDuration;
        private readonly int DayStartHour = dayStartHour;

        public void DrawEvents(Grid col, DateTime columnDate)
        {
            var eventsOnThisDay = DateEvents.Where(p =>
                columnDate.Date >= p.Start.Date && columnDate.Date <= p.End.Date
            );

            int gridColumns = eventsOnThisDay.Count();
            var arrayOfGuids = eventsOnThisDay.Select(p => p.EventID).ToArray();

            var hourStartEndList = eventsOnThisDay.Select(p => new List<TimeOnly> {
                p.Start.Date == columnDate.Date ? TimeOnly.FromDateTime(p.Start) : new(0,0),
                p.End.Date == columnDate.Date ? TimeOnly.FromDateTime(p.End) : new(23,59)
            }).ToList();

            foreach (var _event in eventsOnThisDay)
            {
                TimeOnly hourStart = _event.Start.Date == columnDate.Date ? TimeOnly.FromDateTime(_event.Start) : new(0, 0);
                TimeOnly hourEnd = _event.End.Date == columnDate.Date ? TimeOnly.FromDateTime(_event.End) : new(23, 59);

                int gridStartInMinutes = DayStartHour * 60;
                int hourStartInMinutes = hourStart.Hour * 60 + hourStart.Minute;
                int hourEndInMinutes = hourEnd.Hour * 60 + hourEnd.Minute;

                int indexOfFirstCell = (hourStartInMinutes - gridStartInMinutes) / CellDuration;
                int indexOfLastCell = (hourEndInMinutes - gridStartInMinutes) / CellDuration;

                var collection = col.Children.Where(p => p is Border).ToList();
                if (indexOfFirstCell >= collection.Count) continue;

                bool truncateTop = false;
                if (indexOfFirstCell < 0 && indexOfLastCell < 0) continue;
                if (indexOfFirstCell < 0) { indexOfFirstCell = 0; truncateTop = true; }

                var firstCell = (Border)collection[indexOfFirstCell];

                var filteredHourStartEndList = hourStartEndList
                    .Where(p => !(p[0] == hourStart && p[1] == hourEnd))
                    .ToList();

                firstCell.PropertyChanged += (s, e) =>
                {
                    if (e.Property == Border.BoundsProperty)
                    {
                        DrawEventOnCanvas(
                            _event,
                            firstCell.Bounds,
                            Grid.GetColumn(col),
                            indexOfFirstCell,
                            indexOfLastCell,
                            gridColumns,
                            Array.IndexOf(arrayOfGuids, _event.EventID),
                            columnDate,
                            filteredHourStartEndList,
                            new List<TimeOnly> { hourStart, hourEnd },
                            truncateTop
                        );
                    }
                };
            }
        }

        private void DrawEventOnCanvas(
            CalendarEvent calendarEvent,
            Rect bounds,
            int column,
            int indexOfFirstCell,
            int indexOfLastCell,
            int numberOfGridColumns,
            int eventGridColumn,
            DateTime columnDate,
            List<List<TimeOnly>> hourStartEndList,
            List<TimeOnly> hourStartEnd,
            bool truncateTop)
        {
            DrawingCanvas.Children.RemoveAll(
                DrawingCanvas.Children
                    .OfType<EventBorder>()
                    .Where(b => b.Column == column && b.Guid == calendarEvent.EventID)
            );

            double x = (bounds.X + ((column - 1) * bounds.Width)) + 7;
            double y = bounds.Y - 2;
            double height = bounds.Height + bounds.Height * (indexOfLastCell - indexOfFirstCell);
            double width = bounds.Width - 10;

            double startCorrectionOffset = 0;
            if (columnDate.Date == calendarEvent.Start.Date && !truncateTop)
            {
                var celldate = new TimeOnly(DayStartHour, 0).AddMinutes(indexOfFirstCell * CellDuration);
                var diff = (TimeOnly.FromDateTime(calendarEvent.Start) - celldate).TotalMinutes;
                double offset = (diff / CellDuration) * bounds.Height;
                y += offset;
                startCorrectionOffset = offset;
            }

            if (columnDate.Date == calendarEvent.End.Date)
            {
                var celldate = new TimeOnly(DayStartHour, 0).AddMinutes(indexOfLastCell * CellDuration);
                var diff = CellDuration - (TimeOnly.FromDateTime(calendarEvent.End) - celldate).TotalMinutes;
                double offset = (diff / CellDuration) * bounds.Height;
                height -= offset + startCorrectionOffset;
            }

            int intersections = hourStartEndList.Count(p =>
                hourStartEnd[0] < p[1] && p[0] < hourStartEnd[1]
            );

            if (numberOfGridColumns > 1 && intersections > 0)
            {
                width /= numberOfGridColumns;
                x += eventGridColumn * width;
            }

            var border = new EventBorder
            {
                Height = height,
                Width = width,
                Background = calendarEvent.DullBackgroundBrush(),
                CornerRadius = new CornerRadius(truncateTop ? 0 : 10, truncateTop ? 0 : 10, 10, 10),
                BoxShadow = new BoxShadows(new BoxShadow { Color = Colors.Black, IsInset = true }),
                Guid = calendarEvent.EventID,
                BorderThickness = new Thickness(5, truncateTop ? 0 : 5, 5, 5),
                BorderBrush = calendarEvent.BackgroundBrush,
                Column = column
            };

            var titleText = new TextBlock
            {
                Text = calendarEvent.Title,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 15
            };

            var timeText = new TextBlock
            {
                Text = $"{calendarEvent.Start:d, MMMM yyyy h:mm tt} - {calendarEvent.End:d, MMM yyyy h:mm tt}",
                TextAlignment = TextAlignment.Center,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };

            var contentPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };

            contentPanel.Children.Add(titleText);
            contentPanel.Children.Add(timeText);
            border.Child = contentPanel;

            Canvas.SetTop(border, y);
            Canvas.SetLeft(border, x);
            DrawingCanvas.Children.Add(border);
        }
    }

    internal class EventBorder : Border
    {
        /// <summary>
        /// The column index of the date grid (represents the day of the grid).
        /// </summary>
        public int Column;

        /// <summary>
        /// Unique identifier for the associated calendar event.
        /// </summary>
        public Guid Guid;
    }
}
