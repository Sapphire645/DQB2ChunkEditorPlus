using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DQB2ChunkEditor.Windows;

public partial class ValueEditor : Window
{
    public uint Value{ get; set; } = 0;
    public string ResponseText => ValueText.Text;

    public string ImagePath  { get; set; } = "/Images/Gratitude.png";

    public string Text { get; set; } = "N/A";

    public ValueEditor()
    {
        InitializeComponent();
        DataContext = this;

        ValueText.Focus();
    }

    private void OkButton_OnClick(Object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void ValueText_OnKeyDown(Object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Return or Key.Enter)
        {
            DialogResult = true;
        }
    }
}
