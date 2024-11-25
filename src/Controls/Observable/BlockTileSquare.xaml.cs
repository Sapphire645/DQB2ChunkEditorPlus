using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQBChunkEditor.Models;

namespace DQBChunkEditor.Controls;

public partial class BlockTileSquare : UserControl
{
    public int Id { get; set; } = 0;
    public ObservableProperty<TileData> Tile { get; set; } = new();

    public BlockTileSquare()
    {
        InitializeComponent();
        DataContext = this;
    }
}


