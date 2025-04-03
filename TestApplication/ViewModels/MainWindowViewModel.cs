using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    [ObservableProperty]
    public DateTime _viewDate = DateTime.Now;

    public void NextDate()
    {
        ViewDate = ViewDate.AddMonths(1);
    }
    
    public void PrevDate()
    {
        ViewDate = ViewDate.AddMonths(-1);
    }

    public void Today() {
        ViewDate = DateTime.Now;
    }

}
