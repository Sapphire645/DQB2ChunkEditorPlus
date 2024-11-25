using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DQBChunkEditor.Models;

namespace DQBChunkEditor.Controls;

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


