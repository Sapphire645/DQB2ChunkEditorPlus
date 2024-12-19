using DQB2ChunkEditor.Models;
using DQB2ChunkEditor.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DQB2ChunkEditor.Controls
{
    /// <summary>
    /// Interaction logic for TileList.xaml
    /// </summary>
    public partial class MenuList : UserControl
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler ButtonClicked;
        public event EventHandler ButtonRightClicked;

        private List<BlockTile> BlockListFilter1;
        private List<BlockTile> BlockListFilter2;
        private List<BlockTile> BlockListFilter3;
        private List<BlockTile> BlockListFilter4;
        private List<BlockTile> BlockListFull;

        public BlockTile clickedButton { get; set; }
        public MenuList(List<TileData> ListFilter1, string Filter1,
            List<TileData> ListFilter2, string Filter2,
            List<TileData> ListFilter3, string Filter3)
        {
            BlockListFilter1 = new List<BlockTile>();
            BlockListFilter2 = new List<BlockTile>();
            BlockListFilter3 = new List<BlockTile>();
            BlockListFull = new List<BlockTile>();
            InitializeComponent();
            ButtonGrid.ColumnDefinitions[3].Width = new GridLength(0); ;
            FilterOne.Content = Filter1;
            FilterTwo.Content = Filter2;
            FilterThree.Content = Filter3;
            CreateButtons(BlockListFilter1, ListFilter1);
            CreateButtons(BlockListFilter2, ListFilter2);
            CreateButtons(BlockListFilter3, ListFilter3);
            AddBlocks(BlockListFilter1);
            AddBlocks(BlockListFilter2);
        }
        public MenuList(List<TileData> ListFilter1, string Filter1,
    List<TileData> ListFilter2, string Filter2,
    List<TileData> ListFilter3, string Filter3, List<TileData> ListFilter4, string Filter4)
        {
            BlockListFilter1 = new List<BlockTile>();
            BlockListFilter2 = new List<BlockTile>();
            BlockListFilter3 = new List<BlockTile>();
            BlockListFilter4 = new List<BlockTile>();
            BlockListFull = new List<BlockTile>();
            InitializeComponent();
            FilterOne.Content = Filter1;
            FilterTwo.Content = Filter2;
            FilterThree.Content = Filter3;
            FilterFour.Content = Filter4;
            FilterFour.Visibility = Visibility.Visible;
            CreateButtons(BlockListFilter1, ListFilter1);
            CreateButtons(BlockListFilter2, ListFilter2);
            CreateButtons(BlockListFilter3, ListFilter3);
            CreateButtons(BlockListFilter4, ListFilter4);
            AddBlocks(BlockListFilter1);
            AddBlocks(BlockListFilter2);
            AddBlocks(BlockListFilter3);
        }
        private void CreateButtons(List<BlockTile> ButtonList, List<TileData> BlockList)
        {
            for (int i = 0; i < BlockList.Count; i++)
            {
                var blockTile = new BlockTile
                {
                    Id = BlockList[i].Id,
                    Tile = new ObservableProperty<TileData>
                    {
                        Value = BlockList[i]
                    }
                };
                blockTile.TileButton.Click += (_, _) => { Button_Click(blockTile); };
                blockTile.TileButton.MouseRightButtonDown += (_, _) => { Button_RightClick(blockTile); };
                blockTile.TileButton.Height = 40;
                ButtonList.Add(blockTile);
            }
        }
        private void Button_Click(object sender)
        {
            clickedButton = sender as BlockTile;

            if (clickedButton != null)
            {
                ButtonClicked?.Invoke(clickedButton, EventArgs.Empty);
            }
        }
        private void Button_RightClick(object sender)
        {
            clickedButton = sender as BlockTile;

            if (clickedButton != null)
            {
                ButtonRightClicked?.Invoke(clickedButton, EventArgs.Empty);
            }
        }

        private void FilterOne_Checked(object sender, RoutedEventArgs e){AddBlocks(BlockListFilter1);}
        private void FilterOne_Unchecked(object sender, RoutedEventArgs e){ RemoveBlocks(BlockListFilter1); }
        private void FilterTwo_Checked(object sender, RoutedEventArgs e) { AddBlocks(BlockListFilter2); }
        private void FilterTwo_Unchecked(object sender, RoutedEventArgs e) { RemoveBlocks(BlockListFilter2); }
        private void FilterThree_Checked(object sender, RoutedEventArgs e) { AddBlocks(BlockListFilter3); }
        private void FilterThree_Unchecked(object sender, RoutedEventArgs e) { RemoveBlocks(BlockListFilter3); }
        private void FilterFour_Checked(object sender, RoutedEventArgs e) { AddBlocks(BlockListFilter4); }
        private void FilterFour_Unchecked(object sender, RoutedEventArgs e) { RemoveBlocks(BlockListFilter4); }

        private void AddBlocks(List<BlockTile> List)
        {
            foreach (var block in List)
            { 
                BlockListFull.Add(block);
            }
            if (Grid != null)
            {
                SortItems();
                var a = TextBoxFilter.Text.ToLower();
                FilterItems(a); 
            }
        }
        private void RemoveBlocks(List<BlockTile> List)
        {
            foreach (var block in List)
            {
                Grid.Children.Remove(block);
                BlockListFull.Remove(block);
            }
        }
        private void SortItems()
        {
            BlockListFull = BlockListFull.OrderBy(child => ((BlockTile)child).Id).ToList();

            Grid.Children.Clear();

            foreach (var child in BlockListFull)
            {
                Grid.Children.Add(child);
            }
        }
        private void FilterItems(string Filter)
        {
            Grid.Children.Clear();
            foreach (var child in BlockListFull)
            {
                var a = child.Tile.Value.Name.ToLower();
                if (a.Contains(Filter))
                    Grid.Children.Add(child);
            }
        }
        private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var a = TextBoxFilter.Text.ToLower();
            FilterItems(a);
        }
    }
}
