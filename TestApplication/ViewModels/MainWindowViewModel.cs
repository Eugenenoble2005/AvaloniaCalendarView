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
    public ViewType _viewType = ViewType.Week;

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
            Title = "a 3 day event",
            BackgroundBrush = Brushes.Green,
            Start = new DateTime(2025,4,8,4,43,0),
            End = new(2025,4,10,7,29,0)
        },
        new(){
            Title="a long event",
            BackgroundBrush = Brushes.Blue,
            Start = new(2025,4,27,5,34,0),
            End = new(2025,5,3,23,55,0),
        },
        new(){
            Title = "A draggable event",
            BackgroundBrush = Brushes.Peru,
            Start = new(2025,4,9,6,40,0),
            End= new(2025,4,9,9,15,0)
        },
        new(){
            Title = "A draggable event",
            BackgroundBrush = Brushes.Blue,
            Start = new(2025,4,9,5,25,0),
            End= new(2025,4,9,9,48,0)
        },
        new(){
            Title = "My Event",
            BackgroundBrush = Brushes.Red,
            Start = new(2025,4,10,7,30,0),
            End= new(2025,4,10,13,36,0)
        }
    };
    public void Today()
    {
        ViewDate = DateTime.Now;
    }
    public MainWindowViewModel()
    {
    }
    public void Month() => ViewType = ViewType.Month;
    public void Week() => ViewType = ViewType.Week;
    public void Day() => ViewType = ViewType.Day;
}
