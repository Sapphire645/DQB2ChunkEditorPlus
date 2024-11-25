using System;
using System.Collections.Generic;
using DQBChunkEditor.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DQBChunkEditor.Controls
{
    /// <summary>
    /// Interaction logic for ReducedList.xaml
    /// </summary>
    public partial class ReducedListHorizontal : UserControl
    {
        public event Action<TileData> SelectedTileUpdate;

        public event EventHandler ButtonClicked;

        private BlockTileSquare BlockSelected;

        public List<BlockTileSquare> BlockList;
        private List<short> IdList;
        public BlockTileSquare clickedButton { get; set; }
        public ReducedListHorizontal()
        {
            InitializeComponent();
            IdList = new List<short>();
            BlockList = new List<BlockTileSquare>();
        }
        public void AddToList(TileData Block)
        {
            var ID = Block.Id;
            if (!IdList.Contains(ID))
            {
                var blockTile = new BlockTileSquare
                {
                    Id = ID,
                    Tile = new ObservableProperty<TileData> { Value = Block }
                };
                IdList.Add(ID);
                blockTile.TileButton.Click += (_, _) => { Button_Click(blockTile); };
                blockTile.TileButton.MouseRightButtonDown += (_, _) => { Button_RightClick(blockTile); };
                blockTile.TileButton.Height = 40;
                Grid.Children.Add(blockTile);
                BlockList.Add(blockTile);
            }
        }
        private void Button_ClickSelected(object sender)
        {
            clickedButton = sender as BlockTileSquare;

            if (clickedButton != null)
            {
                AddToList(clickedButton.Tile.Value);  
            }
        }
        private void Button_Click(object sender)
        {
            clickedButton = sender as BlockTileSquare;

            if (clickedButton != null)
            {
                SelectedTileUpdate?.Invoke(clickedButton.Tile.Value);
            }
        }
        private void Button_RightClick(object sender)
        {
            clickedButton = sender as BlockTileSquare;

            IdList.Remove((short)clickedButton.Id);
            Grid.Children.Remove(clickedButton);
            BlockList.Remove(clickedButton);
        }
    }
}
