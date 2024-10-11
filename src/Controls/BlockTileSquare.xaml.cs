using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls;

public partial class BlockTileSquare : UserControl
{
    public int Id { get; set; } = 0;
    public ObservableProperty<Tile> Tile { get; set; } = new();

    public BlockTileSquare()
    {
        InitializeComponent();
        DataContext = this;
    }
}


