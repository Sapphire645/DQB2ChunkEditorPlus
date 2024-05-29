using System;
using System.Windows;
using System.Windows.Media;


namespace DQB2ChunkEditor.Windows;

public partial class ConfirmChoice : Window
{
    public string Text { get; set; } = "N/A";
    public bool Confirmed = false;

    public ConfirmChoice()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OkButton_OnClick(Object sender,  RoutedEventArgs e)
    {
        Confirmed = true;
        DialogResult = true;
    }

    private void CancelButton_OnClick(Object sender,  RoutedEventArgs e)
    {
        DialogResult = true;
    }
}