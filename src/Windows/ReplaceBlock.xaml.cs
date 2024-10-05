using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DQB2ChunkEditor.Controls;
using DQB2ChunkEditor.Models;
using DQB2ChunkEditor.Windows;
using System.Windows.Controls.Primitives;
using System.Security.Policy;
using System.Collections.Generic;
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace DQB2ChunkEditor.Windows;

public partial class ReplaceBlock : Window
{
    private bool First { get; set; } = false;
    private bool Second { get; set; } = false;
    public short FirstId { get; set; } = -1;
    public short SecondId { get; set; } = -1;

    private BlockTile FirstBlock = new BlockTile();
    private BlockTile SecondBlock = new BlockTile();

    private BlockGrid ButtonGridFirst;
    private bool Resize = false;

    public ReplaceBlock(BlockGrid ButtonGridFirst)
    {
        InitializeComponent();

        Grid.SetColumn(ButtonGridFirst, 0);
        Grid.SetRow(ButtonGridFirst, 0);
        ButtonGridFirst.VerticalAlignment = VerticalAlignment.Top;
        GridAdd.Children.Add(ButtonGridFirst);

        FirstBlock.TileButton.Click += (_, _) => { SetFirstBlock_OnClick(FirstBlock); };
        SecondBlock.TileButton.Click += (_, _) => { SetSecondBlock_OnClick(SecondBlock); };

        ButtonGridFirst.ButtonClicked += ButtonClick;

        this.ButtonGridFirst = ButtonGridFirst;
        this.SizeChanged += OnWindowSizeChanged;
        this.ResizeMode = ResizeMode.NoResize;

        Stack1.Children.Add(FirstBlock);
        Stack2.Children.Add(SecondBlock);
        SizeChangedOp(330.0, 500.0);
    }

    private void Resize_OnClick(object sender, EventArgs e)
    {
        this.ResizeMode = ResizeMode.CanResize;
        Resize = true;
        this.Darnit.Visibility = Visibility.Collapsed;
    }
    public void ButtonClick(object sender, EventArgs e)
    {
        var block = (sender as BlockGrid).clickedButton;

        if (First)
        {
            Stack1.Children.Remove(FirstBlock);
            FirstBlock = new BlockTile(block.Tile);
            FirstBlock.TileButton.Click += (_, _) => { SetFirstBlock_OnClick(FirstBlock); };
            Stack1.Children.Add(FirstBlock);
            FirstId = FirstBlock.Tile.Id;
        }
        else if (Second)
        {
            Stack2.Children.Remove(SecondBlock);
            SecondBlock = new BlockTile(block.Tile);
            SecondBlock.TileButton.Click += (_, _) => { SetSecondBlock_OnClick(SecondBlock); };
            Stack2.Children.Add(SecondBlock);
            SecondId = SecondBlock.Tile.Id;
        }
    }
    protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Resize)
        {
            SizeChangedOp(e.NewSize.Height, e.NewSize.Width);
        }   
    }

    private void SizeChangedOp(double newWindowHeight, double newWindowWidth)
    {
        GridAdd.RowDefinitions[0].Height = new GridLength((newWindowHeight - 140));
        GridAdd.ColumnDefinitions[0].Width = new GridLength((newWindowWidth - 10));
        GridAdd.Height = newWindowHeight - 10;
        GridAdd.Width = newWindowWidth;
        ButtonGridFirst.ScrollView.Height = (newWindowHeight - 160);
        ButtonGridFirst.ScrollView.Width = (newWindowWidth - 20);
        ButtonGridFirst.Grid.Columns = Convert.ToInt16(newWindowWidth) / 100;
    }
    private void SetFirstBlock_OnClick(BlockTile Tile)
    {
        First = true;
        Second = false;
    }

    private void SetSecondBlock_OnClick(BlockTile Tile)
    {
        First = false;
        Second = true;
    }
    private void OkButton_OnClick(Object sender,  RoutedEventArgs e)
    {
        if (FirstId > -1 && SecondId > -1)
        {
            DialogResult = true;
        }
    }
}
