using DQB2ChunkEditor.Controls;
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
    /// Interaction logic for BlockGrid.xaml
    /// </summary>
    /// 
    public partial class BlockGrid : UserControl
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler ButtonClicked;

        private List<BlockTile> mBlockList;
        public List<BlockTile> BlockList { get => mBlockList; set { mBlockList = value; CreateButtonsFromList(); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlockList))); } }
        public BlockTile clickedButton { get; set; }
        public BlockGrid()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        /// <summary>
        /// Creates the blocks on the selector
        /// </summary>
        public void CreateButtons()
        {
            if (mBlockList == null)
            {
                mBlockList = new List<BlockTile>();
            }
                for (int i = 0; i < MainWindow.TileList.Count; i++)
            {
                var blockTile = new BlockTile
                {
                    Id = i,
                    Tile = MainWindow.TileList[i]
                };

                blockTile.TileButton.Click += (_, _) => { Button_Click(blockTile); };
                blockTile.TileButton.Height = 40;
                mBlockList.Add(blockTile);
                Grid.Children.Add(blockTile);
            }
        }
        public void CreateButtonsFromList()
        {
            for (int i = 0; i < mBlockList.Count; i++)
            {
                Grid.Children.Add(mBlockList[i]);
            }
        }
        public void Button_Click(object sender)
        {
            clickedButton = sender as BlockTile;

            if (clickedButton != null)
            {
                ButtonClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool IsLiquid(BlockTile block)
        {
            if (mBlockList != null)
            {
                if ((block.Tile.Name.ToLower().Contains("water") || block.Tile.Name.Contains("Plasma") || block.Tile.Name.Contains("Bottomless") || block.Tile.Name.Contains("Poison ") || block.Tile.Name.Contains("Lava "))
                    && block.Tile.Id != 593 && block.Tile.Id != 17 && block.Tile.Id != 86) { return true; }
                else { return false; }
            }
            return false;
        }

        private void CheckUsed_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (block.Tile.Used == "Used")
                    {
                        Grid.Children.Remove(block);
                    }
                }
            }
        }
        private void CheckUsed_Checked(object sender, RoutedEventArgs e)
        {
            if(mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (Grid.Children.Contains(block))
                    {
                        Grid.Children.Remove(block);
                        Grid.Children.Add(block);
                    }
                    else if (block.Tile.Used == "Used")
                    {
                        if (CheckLiquid.IsChecked == true || IsLiquid(block) == false)
                        {
                            Grid.Children.Add(block);
                        }

                    }
                }
            }

        }
        private void CheckUnused_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (block.Tile.Used == "Unused")
                    {
                        Grid.Children.Remove(block);
                    }
                }
            }
        }
        private void CheckUnused_Checked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (Grid.Children.Contains(block))
                    {
                        Grid.Children.Remove(block);
                        Grid.Children.Add(block);
                    }
                    else if (block.Tile.Used == "Unused")
                    {
                        if ((CheckLiquid.IsChecked == true || IsLiquid(block) == false) && (CheckDefault.IsChecked == true || block.Tile.Name.Contains("Default Block") == false))
                        {
                            Grid.Children.Add(block);
                        }
                    }
                }
            }
        }
        private void CheckLiquid_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (IsLiquid(block))
                    {
                        Grid.Children.Remove(block);
                    }
                }
            }
        }
        private void CheckLiquid_Checked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (Grid.Children.Contains(block))
                    {
                        Grid.Children.Remove(block);
                        Grid.Children.Add(block);
                    }
                    else if (IsLiquid(block))
                    {
                        if ((CheckDefault.IsChecked == true || block.Tile.Name.Contains("Default Block") == false) && (CheckUsed.IsChecked == true || block.Tile.Used != "Used") && (CheckUnused.IsChecked == true || block.Tile.Used != "Unused"))
                        {
                            Grid.Children.Add(block);
                        }
                    }
                }
            }
        }
        private void CheckDefault_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (block.Tile.Name.Contains("Default Block"))
                    {
                        Grid.Children.Remove(block);
                    }
                }
            }
        }
        private void CheckDefault_Checked(object sender, RoutedEventArgs e)
        {
            if (mBlockList != null)
            {
                foreach (var block in mBlockList)
                {
                    if (Grid.Children.Contains(block))
                    {
                        Grid.Children.Remove(block);
                        Grid.Children.Add(block);
                    }
                    else if (block.Tile.Name.Contains("Default Block"))
                    {
                        if ((CheckDefault.IsChecked == true || block.Tile.Name.Contains("Default Block") == false) && (CheckUsed.IsChecked == true || block.Tile.Used != "Used") && (CheckUnused.IsChecked == true || block.Tile.Used != "Unused"))
                        {
                            Grid.Children.Add(block);
                        }
                    }
                }
            }
        }
    }
}
