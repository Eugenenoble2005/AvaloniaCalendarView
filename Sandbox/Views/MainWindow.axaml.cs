using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestApplication.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        (calendar as AvaloniaCalendarView.CalendarView).EventResized += (s, e) =>
        {
            //handle resize
        };
        (calendar as AvaloniaCalendarView.CalendarView).EventMoved += (s, e) =>
        {
            //handle move
        };
    }

}
