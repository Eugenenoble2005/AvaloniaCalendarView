using System;
using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaCalendarView;
using System.Collections.Generic;
using Avalonia.Media;
namespace TestApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    [ObservableProperty]
    public DateTime _viewDate = DateTime.Now;

    [ObservableProperty]
    public ViewType _viewType = ViewType.Month;

    public void NextDate()
    {
        if (ViewType == ViewType.Month)
        {
            ViewDate = ViewDate.AddMonths(1);
        }
        else if (ViewType == ViewType.Week)
        {
            ViewDate = ViewDate.AddDays(7);
        }
        else if (ViewType == ViewType.Day)
        {
            ViewDate = ViewDate.AddDays(1);
        }
    }

    public void PrevDate()
    {
        if (ViewType == ViewType.Month)
        {
            ViewDate = ViewDate.AddMonths(-1);
        }
        else if (ViewType == ViewType.Week)
        {
            ViewDate = ViewDate.AddDays(-7);
        }
        else if (ViewType == ViewType.Day)
        {
            ViewDate = ViewDate.AddDays(-1);
        }
    }
    [ObservableProperty]
    public List<CalendarEvent> _events = new() {
          new() {
            Title = "Event 1",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(3).AddHours(1),
            BackgroundBrush = Brushes.CornflowerBlue
        },
        new() {
            Title = "Event 2",
            Start = DateTime.Now.AddDays(-1),
            End = DateTime.Now.AddDays(4),
            BackgroundBrush = Brushes.DarkRed
        },
        new() {
            Title = "Event 3",
            Start = new(2025,4,4),
            End = new(2025,4,6),
            BackgroundBrush = Brushes.DarkSeaGreen
        },
        new() {
            Title = "Event 4",
            Start = new(2025,5,2),
            End = new(2025,5,3),
            BackgroundBrush = Brushes.LightSalmon
        },
        new() {
            Title = "Event 5",
            Start = new(2025,3,30),
            End = new(2025,3,30),
        }
    };
    public void Today()
    {
        ViewDate = DateTime.Now;
    }
    public MainWindowViewModel()
    {
        for (int i = 0; i < 25; i++)
        {
            Events.Add(
                new()
                {
                    Title = $"Event {i}",
                    Start = DateTime.Now,
                    End = DateTime.Now.AddDays(2),
                    BackgroundBrush = Brushes.Green
                }
            );
        }
    }
    public void Month() => ViewType = ViewType.Month;
    public void Week() => ViewType = ViewType.Week;
    public void Day() => ViewType = ViewType.Day;
}
