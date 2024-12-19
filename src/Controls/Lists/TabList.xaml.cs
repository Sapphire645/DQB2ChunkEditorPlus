using DQB2ChunkEditor.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for SelectionList.xaml
    /// </summary>
    public partial class TabList : UserControl
    {
        public event Action<TileData> ReturnSelectedTile;

        public List<List<TileData>> BlockList;
        public List<List<TileData>> LiquidList;
        public List<List<TileData>> ObjectList;

        public MenuList BlockMenu;
        public MenuList LiquidMenu;
        public MenuList ObjectMenu;
        public TabList()
        {
            InitializeComponent();
            FavouriteList.SelectedTileUpdate += OnSelectedTileUpdate;
        }
        public void createTabList()
        {
            BlockMenu = new MenuList(BlockList.ElementAt(0), "Used", BlockList.ElementAt(1), "Unused", BlockList.ElementAt(2), "Indestructible", BlockList.ElementAt(3), "NULL");
            BlockMenu.ButtonClicked += ButtonClick;
            BlockMenu.ButtonRightClicked += ButtonRightClick;
            this.Blocks.Children.Add(BlockMenu);
            LiquidMenu = new MenuList(LiquidList.ElementAt(0), "Used", LiquidList.ElementAt(1), "Unused", LiquidList.ElementAt(2), "NULL");
            LiquidMenu.ButtonClicked += ButtonClick;
            LiquidMenu.ButtonRightClicked += ButtonRightClick;
            this.Liquids.Children.Add(LiquidMenu);
            ObjectMenu = new MenuList(ObjectList.ElementAt(0), "Used", ObjectList.ElementAt(1), "Unused", ObjectList.ElementAt(2), "NULL");
            ObjectMenu.ButtonClicked += ButtonClick;
            ObjectMenu.ButtonRightClicked += ButtonRightClick;
            this.Objects.Children.Add(ObjectMenu);
        }
        public void ScrollViewUpdate(double height) {
            foreach(var Menu in Blocks.Children)
            {
                ((MenuList)Menu).ScrollView.Height = height- 80;
            }
            foreach (var Menu in Liquids.Children)
            {
                ((MenuList)Menu).ScrollView.Height = height - 80;
            }
            foreach (var Menu in Objects.Children)
            {
                ((MenuList)Menu).ScrollView.Height = height - 80;
            }
        }
        public void ButtonClick(object sender, EventArgs e) {
            var i = sender as BlockTile;
            FavouriteList.SelectedToList(i.Tile.Value);
        }
        public void ButtonRightClick(object sender, EventArgs e)
        {
            var i = sender as BlockTile;
            FavouriteList.AddToList(i.Tile.Value);
        }
        private void OnSelectedTileUpdate(TileData tile)
        {
            ReturnSelectedTile?.Invoke(tile);
        }
    }
}
