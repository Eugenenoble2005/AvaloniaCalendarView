using Avalonia.Media;

namespace AvaloniaCalendarView;

///<summary>
/// Represents an event that can be displayed on the calendar view.
/// </summary>
public class CalendarEvent
{
    ///<summary>
    /// The title of the event
    /// </summary>
    public required string Title;

    ///<summary>
    /// The DateTime object representing when the event starts.
    /// </summary>
    public required DateTime Start;

    ///<summary>
    /// The DateTime object representing when the event ends.
    /// </summary>
    public required DateTime End;

    ///<summary>
    /// The Brush that paints the background of the event
    /// </summary>
    public IBrush BackgroundBrush = Brushes.Blue;

    ///<summary>
    /// The Brush that paints the foreground of the event
    ///</summary>
    public IBrush? DateForegroundBrush;

    ///<summary>
    // Whether or not this event can be resized on the view
    // </summary>
    public bool Resizable = false;

    public Guid EventID { get; } = Guid.CreateVersion7();

    public IBrush DullBackgroundBrush()
    {
        if (BackgroundBrush is ISolidColorBrush scBrush)
        {
            var dullColor = Color.FromArgb(
                scBrush.Color.A, // preserve alpha
                (byte)(scBrush.Color.R * 0.6 + 80 * 0.4), // mix with gray
                (byte)(scBrush.Color.G * 0.6 + 80 * 0.4),
                (byte)(scBrush.Color.B * 0.6 + 80 * 0.4)
            );
            return new SolidColorBrush(dullColor);
        }
        else return BackgroundBrush;
    }

}
