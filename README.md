WIP Avalonia control that can display events on a calendar view



https://github.com/user-attachments/assets/8a58f9b2-0b17-4c0d-a4eb-f5de9da434e5


# Features
- Month View, Week View And Day View
- Custom Day Start Hour, Day End Hour and Hour Duration for week and day views
- Multiday events
- Resize and move events
  

# Usage 
See the 'sandbox' application for usage 
```
<Window
    x:Class="TestApplication.Views.MainWindow"
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:calendar="using:AvaloniaCalendarView"
    ...
            <ScrollViewer
                HorizontalAlignment="Stretch"
                VerticalAlignment="Stretch">
                <calendar:CalendarView
                    Margin="10,0,10,0"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    DateEvents="{Binding Events}"
                    ViewDate="{Binding ViewDate}"
                    ViewType="{Binding ViewType}" />
            </ScrollViewer>

```
