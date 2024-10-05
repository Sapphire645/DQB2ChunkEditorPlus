using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls;

public partial class BlockTile : UserControl
{
    public int Id { get; set; } = 0;
    public Tile Tile { get; set; } = new();

    public BlockTile()
    {
        InitializeComponent();
        DataContext = this;
    }
    public BlockTile(Tile Tile)
    {
        InitializeComponent();
        DataContext = this;
        this.Tile = Tile;
    }
}


