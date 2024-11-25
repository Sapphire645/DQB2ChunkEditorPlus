using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQBChunkEditor.Models;

namespace DQBChunkEditor.Controls;

public partial class LayerTile : UserControl
{
    public short Id { get; set; }
    public ObservableProperty<Tile> Tile { get; set; } = new();

    public LayerTile()
    {
        InitializeComponent();
        DataContext = this;
    }
}
