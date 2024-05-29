using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Models;
using DQB2ChunkEditor.Windows;

namespace DQB2ChunkEditor.Windows;

public partial class ReplaceBlock : Window
{
    public ObservableCollection<ComboBoxTile> TileComboBoxListCopy { get; set; } = MainWindow.TileComboBoxList;
    public ObservableProperty<Tile> SelectedTileFirst { get; set; } = new();
    public ObservableProperty<Tile> SelectedTileSecond { get; set; } = new();
    public short FirstId { get; set; } = -1;
    public short SecondId { get; set; } = -1;

    public ReplaceBlock()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void SetFirstBlock_OnClick(Object sender, SelectionChangedEventArgs e)
    {
        FirstId = ((ComboBoxTile)e.AddedItems[0]!).Tile.Id;
    }

    private void SetSecondBlock_OnClick(Object sender, SelectionChangedEventArgs e)
    {
        SecondId = ((ComboBoxTile)e.AddedItems[0]!).Tile.Id;
    }
    private void OkButton_OnClick(Object sender,  RoutedEventArgs e)
    {
        if (FirstId > -1 && SecondId > -1)
        {
            DialogResult = true;
        }
    }
}
