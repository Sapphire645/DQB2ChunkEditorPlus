using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQBChunkEditor.Models;

namespace DQBChunkEditor.Controls;

public partial class BlockTile : UserControl
{
    public int Id { get; set; } = 0;
    public ObservableProperty<TileData> Tile { get; set; } = new();

    public BlockTile()
    {
        InitializeComponent();
        DataContext = this;
    }
}


