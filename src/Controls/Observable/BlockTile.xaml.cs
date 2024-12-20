﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls;

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


