using System;
using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaCalendarView;
using System.Collections.Generic;
using Avalonia.Media;
using TestApplication.ViewModels;
using Avalonia.Controls;
using System.Collections.ObjectModel;
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
    public List<CalendarEvent> _events = new();

    public void Today()
    {
        ViewDate = DateTime.Now;
    }
    public MainWindowViewModel()
    {
        Random();
    }
    public void Random()
    {
        List<CalendarEvent> p = new();
        //randomize multiday events
        for (int i = 0; i < 5; i++)
        {
            var random = new Random();
            // 1. Generate a random start date between some reasonable range
            DateTime start = RandomDate(random, DateTime.Now.AddDays(-7), DateTime.Now.AddDays(7));
            int hoursOffset = random.Next(0, 24 * 30);
            int minutesOffset = random.Next(0, 60);
            DateTime end = start
                                 .AddHours(hoursOffset)
                                 .AddMinutes(minutesOffset);
            p.Add(new()
            {
                Title = $"Event {i}",
                Start = start,
                End = end,
                BackgroundBrush = new SolidColorBrush(Color.Parse($"#{random.Next(0x1000000):X6}"))
            });
        }
        //randomize singleday events
        var startFrom = DateTime.Now.AddDays(-7);
        for (int i = 0; i < 25; i++)
        {
            var random = new Random();
            var endAt = startFrom.AddHours(random.Next(22)).AddMinutes(random.Next(0, 59));
            p.Add(new()
            {
                Title = $"Less Event {i}",
                Start = startFrom,
                End = endAt,
                BackgroundBrush = new SolidColorBrush(Color.Parse($"#{random.Next(0x1000000):X6}"))
            });
            startFrom = new(startFrom.Year, startFrom.Month, startFrom.Day);
            startFrom = startFrom.AddDays(1).AddHours(random.Next(23)).AddMinutes(59);
        }
        Events = p;
    }
    static DateTime RandomDate(Random rng, DateTime from, DateTime to)
    {
        var range = (to - from).Days;
        return from.AddDays(rng.Next(range)).AddHours(rng.Next(24)).AddMinutes(rng.Next(60));
    }
    public void Month() => ViewType = ViewType.Month;
    public void Week() => ViewType = ViewType.Week;
    public void Day() => ViewType = ViewType.Day;
}
