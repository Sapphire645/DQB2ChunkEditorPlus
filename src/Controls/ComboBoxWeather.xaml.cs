using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls;

public partial class ComboBoxWeather : UserControl
{
    public int Id { get; set; } = 0;
    public Weather Weather { get; set; } = new();

    public ComboBoxWeather()
    {
        InitializeComponent();
        DataContext = this;
    }
}


