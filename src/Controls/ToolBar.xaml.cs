﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DQB2ChunkEditor.Models;

namespace DQB2ChunkEditor.Controls
{
    public class IslandData
    {
        public ushort IslandNumber { get; private set; }
        public short ChunkCount { get; private set; }
        public short VitrualChunkCount { get; private set; }
        public uint ItemCount { get; private set; }
        public uint VirtualItemCount { get; private set; }
        public uint SeaLevel { get; private set; }
        public void UpdateIslandData() { 
            IslandNumber = ChunkEditor.Island;
            ChunkCount = ChunkEditor.ChunkCount;
            VitrualChunkCount = ChunkEditor.VirtualChunkCount; 
            ItemCount = ChunkEditor.TemporalItemCount;
            VirtualItemCount = ChunkEditor.VirtualItemCount;
            SeaLevel = ChunkEditor.TemporalSeaLevel;
        }
    }
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    /// 

    public partial class ToolBar : UserControl
    {
        public ushort tool = 0;
        public Cursor paintCursor;
        public event Action<List<BlockTileSquare>,short> ReplaceBlocksCommand;
        public event Action<ObservableProperty<TileData>> ReturnSelectedTileData;
        public event Action<ObservableProperty<Tile>> ReturnSelectedTile;
        public event Action<bool> AreaRemovalCommand;
        public event Action<bool> PrintOutCommand;
        public TabList menuList;
        public ObservableProperty<IslandData> IslandDataOb { get; private set; } = new() { Value = new() };

        public ToolBar()
        {
            InitializeComponent();
            LoadCustomCursor();
            NewBlock.TileButton.Click += (_,_) => { Replace2_Click(NewBlock); } ;
            ReplaceList.SelectedTileUpdate += OnSelectedTileUpdate;
            GlovesList.SelectedTileUpdate += OnSelectedTileUpdate;
        }

        private void Select_Check(object sender, RoutedEventArgs e)
        {
            tool = 0;
            Mouse.OverrideCursor = null;
            if (Pencil != null)
            {
                Select.Visibility = Visibility.Visible;
                Pencil.Visibility = Visibility.Collapsed;
                Replace.Visibility = Visibility.Collapsed;
                Gloves.Visibility = Visibility.Collapsed;
                IslandData.Visibility = Visibility.Collapsed;
            }


        }
        private void Paint_Check(object sender, RoutedEventArgs e)
        {
            tool = 1;
            Select.Visibility = Visibility.Visible;
            Pencil.Visibility = Visibility.Collapsed;
            Replace.Visibility = Visibility.Collapsed;
            Gloves.Visibility = Visibility.Collapsed;
            IslandData.Visibility = Visibility.Collapsed;
        }
        private void Pencil_Check(object sender, RoutedEventArgs e)
        {
            tool = 2;
            Mouse.OverrideCursor = null;
            Select.Visibility = Visibility.Collapsed;
            Pencil.Visibility = Visibility.Visible;
            Replace.Visibility = Visibility.Collapsed;
            Gloves.Visibility = Visibility.Collapsed;
            IslandData.Visibility = Visibility.Collapsed;
        }

        private void Replace_Check(object sender, RoutedEventArgs e)
        {
            tool = 3;
            Mouse.OverrideCursor = null;
            Select.Visibility = Visibility.Collapsed;
            Pencil.Visibility = Visibility.Collapsed;
            Replace.Visibility = Visibility.Visible;
            Gloves.Visibility = Visibility.Collapsed;
            IslandData.Visibility = Visibility.Collapsed;
        }
        private void Gloves_Check(object sender, RoutedEventArgs e)
        {
            tool = 4;
            Mouse.OverrideCursor = null;
            Select.Visibility = Visibility.Collapsed;
            Pencil.Visibility = Visibility.Collapsed;
            Replace.Visibility = Visibility.Collapsed;
            Gloves.Visibility = Visibility.Visible;
            IslandData.Visibility = Visibility.Collapsed;
        }
        private void Island_Check(object sender, RoutedEventArgs e)
        {
            tool = 5;
            Mouse.OverrideCursor = null;
            IslandDataOb.Value.UpdateIslandData();
            IslandDataOb.NotifyValue();
            Select.Visibility = Visibility.Collapsed;
            Pencil.Visibility = Visibility.Collapsed;
            Replace.Visibility = Visibility.Collapsed;
            Gloves.Visibility = Visibility.Collapsed;
            IslandData.Visibility = Visibility.Visible;
        }
        private void Replace1_Click(object sender, RoutedEventArgs e)
        {
            if(SelectBlocks1.IsChecked == true)
            {
                SelectBlocks1.IsChecked = true;
                SelectBlocks2.IsChecked = false;
                NewBlock.TileButton.IsChecked = false;
            }

        }
        private void Replace2_Click(object sender)
        {
            if (SelectBlocks2.IsChecked == true || NewBlock.TileButton.IsChecked == true)
            {
                SelectBlocks2.IsChecked = true;
                SelectBlocks1.IsChecked = false;
            }
            var i = sender as BlockTile;
            if (i.Tile.Value != null)
            {
                menuList.FavouriteList.SelectedToList(i.Tile.Value);
            }
        }
        private void Replace2_Click(object sender, RoutedEventArgs e)
        {
            if (SelectBlocks2.IsChecked == true || NewBlock.TileButton.IsChecked == true)
            {
                SelectBlocks2.IsChecked = true;
                SelectBlocks1.IsChecked = false;
            }
        }
        public void AddToList(TileData tile)
        {
            if (SelectBlocks2.IsChecked == true)
            {
                NewBlock.Tile.Value = tile;
            }
            else
            {
                ReplaceList.AddToList(tile);
            }
            if (NewBlock.Tile.Value != null && ReplaceList.BlockList.Count > 0)
                Confirm.Background = SelectBlocks2.Background;
            else Confirm.Background = Acacac.Stroke;

        }
        private void Replace_Command(object sender, RoutedEventArgs e)
        {
            if(NewBlock.Tile.Value != null && ReplaceList.BlockList.Count > 0)
                ReplaceBlocksCommand?.Invoke(ReplaceList.BlockList, NewBlock.Tile.Value.Id);
        }
        private void AreaRemoval_Check(object sender, RoutedEventArgs e)
        {
            AreaRemovalCommand?.Invoke(false);
            AreaRemovalCommand?.Invoke(true);
        }
        private void PrintOut_Check(object sender, RoutedEventArgs e)
        {
            bool check = (sender as Button).Equals(PrintData2);
            PrintOutCommand?.Invoke(check);
        }
        private void KILL_ITEMS(object sender, RoutedEventArgs e)
        {
            ChunkEditor.KILLITEMS();
        }
        private void LoadCustomCursor()
        {
            var cursorImagePath = "pack://application:,,,/Images/Cursor/paste.cur";
            paintCursor = new Cursor(Application.GetResourceStream(new Uri(cursorImagePath)).Stream);
        }
        private void OnSelectedTileUpdate(TileData tile)
        {
            menuList.FavouriteList.SelectedToList(tile);
        }
        private void OnSelectedTileUpdate(ItemInstance tile)
        {
            menuList.FavouriteList.SelectedToList(tile.TileData);
        }
        private void Grab_OnClick(object sender, RoutedEventArgs e)
        {
            if (Place.IsChecked == true) { 
                Grab.IsChecked = true;
                Place.IsChecked = false;
            }
        }
        private void Place_OnClick(object sender, RoutedEventArgs e)
        {
            if (Grab.IsChecked == true)
            {
                Grab.IsChecked = false;
                Place.IsChecked = true;
            }
        }

        private void Refresh_Island_Data(object sender, RoutedEventArgs e)
        {
            IslandDataOb.Value.UpdateIslandData();
            IslandDataOb.NotifyValue();
        }

        private void FirstArea_Click(object sender, RoutedEventArgs e)
        {
            AreaRemovalCommand?.Invoke(false);
        }
        private void SecondArea_Click(object sender, RoutedEventArgs e)
        {
            AreaRemovalCommand?.Invoke(true);
        }
    }
}
