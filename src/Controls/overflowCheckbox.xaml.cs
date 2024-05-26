using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls;

public partial class overflowCheckbox : UserControl
{
    private bool checkClicked = true;
    public overflowCheckbox()
    {
        InitializeComponent();
        DataContext = this;
    }

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register("IsChecked", typeof(bool), typeof(overflowCheckbox), new PropertyMetadata(false));

    public bool IsChecked
    {
        get { return (bool)GetValue(IsCheckedProperty); }
        set{ checkClicked = false; 
             SetValue(IsCheckedProperty, value); 
             checkClicked = true;}
    }
    public event RoutedEventHandler CheckedChanged;

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkClicked)
                CheckedChanged?.Invoke(this, e);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkClicked)
                CheckedChanged?.Invoke(this, e);
        }

}