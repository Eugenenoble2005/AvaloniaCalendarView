using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestApplication.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void AddEvent(object sender, RoutedEventArgs e)
    {
        var window = new AddEvent();
        window.ShowDialog(this);
    }
}
