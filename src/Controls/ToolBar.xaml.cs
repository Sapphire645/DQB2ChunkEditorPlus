using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.RightsManagement;
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
using DQB2ChunkEditor.Models;
using DQB2ChunkEditor.Windows;
using static System.Net.WebRequestMethods;

namespace DQB2ChunkEditor.Controls
{
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    /// 

    public partial class ToolBar : UserControl
    {
        public ushort tool = 0;
        public Cursor paintCursor;
        public event Action<List<BlockTileSquare>,short> ReplaceBlocksCommand;
        public event Action<ObservableProperty<Tile>> ReturnSelectedTile;
        public event Action AreaRemovalCommand;
        public event Action<bool> PrintOutCommand;
        public TabList menuList;
        public ToolBar()
        {
            InitializeComponent();
            LoadCustomCursor();
            NewBlock.TileButton.Click += (_,_) => { Replace2_Click(NewBlock); } ;
            ReplaceList.SelectedTileUpdate += OnSelectedTileUpdate;
        }

        private void Select_Check(object sender, RoutedEventArgs e)
        {
            tool = 0;
            Mouse.OverrideCursor = null;
            if (Pencil != null)
            {
                Pencil.Visibility = Visibility.Collapsed;
                Select.Visibility = Visibility.Visible;
                Replace.Visibility = Visibility.Collapsed;
            }


        }
        private void Paint_Check(object sender, RoutedEventArgs e)
        {
            tool = 1;
            Pencil.Visibility = Visibility.Collapsed;
            Select.Visibility = Visibility.Visible;
            Replace.Visibility = Visibility.Collapsed;
        }
        private void Pencil_Check(object sender, RoutedEventArgs e)
        {
            tool = 2;
            Mouse.OverrideCursor = null;
            Select.Visibility = Visibility.Collapsed;
            Pencil.Visibility = Visibility.Visible;
            Replace.Visibility = Visibility.Collapsed;
        }
        private void Replace_Check(object sender, RoutedEventArgs e)
        {
            tool = 3;
            Mouse.OverrideCursor = null;
            Select.Visibility = Visibility.Collapsed;
            Pencil.Visibility = Visibility.Collapsed;
            Replace.Visibility = Visibility.Visible;
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
                menuList.FavouriteList.SelectedToList(i.Tile);
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
        public void AddToList(ObservableProperty<Tile> tile)
        {
            if (SelectBlocks2.IsChecked == true)
            {
                NewBlock.Tile.Value = tile.Value;
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
            AreaRemovalCommand?.Invoke();
        }
        private void PrintOut_Check(object sender, RoutedEventArgs e)
        {
            bool check = (sender as Button).Equals(PrintData2);
            PrintOutCommand?.Invoke(check);
        }
        private void LoadCustomCursor()
        {
            var cursorImagePath = "pack://application:,,,/Images/Cursor/Paste.cur";
            paintCursor = new Cursor(Application.GetResourceStream(new Uri(cursorImagePath)).Stream);
        }
        private void OnSelectedTileUpdate(ObservableProperty<Tile> tile)
        {
            menuList.FavouriteList.SelectedToList(tile);
        }

    }
}
