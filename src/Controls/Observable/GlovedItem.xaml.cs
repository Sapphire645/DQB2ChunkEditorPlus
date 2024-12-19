using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls;

public partial class GlovedItem : UserControl
{
    public int Id { get; set; } = 0;
    public ObservableProperty<ItemInstance> Tile { get; set; } = new();

    public GlovedItem()
    {
        InitializeComponent();
        DataContext = this;
    }
}


