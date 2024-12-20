﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQB2ChunkEditor.Models;
using static System.Net.Mime.MediaTypeNames;

namespace DQB2ChunkEditor.Controls;

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
