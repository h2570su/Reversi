using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Reversi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public class ReversiButton : Button
    {
        public int gameRow { get; private set; } = -1;
        public int gameCol { get; private set; } = -1;
        public ReversiButton(int row, int col) : base()
        {
            gameRow = row;
            gameCol = col;
        }
    }
    enum Turn
    {
        Black, White
    }
    public partial class LocalPlayPage : ContentPage
    {
        ReversiButton[,] BTNGrid = new ReversiButton[8, 8];
        int[,] gameGrid = new int[8, 8];
        Turn whoTurn = Turn.Black;
        public LocalPlayPage()
        {
            InitializeComponent();
            for (int i = 0; i <= BTNGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= BTNGrid.GetUpperBound(1); j++)
                {
                    BTNGrid[i, j] = new ReversiButton(i, j);
                    var btn = BTNGrid[i, j];
                    mainGameGrid.Children.Add(btn, j, i);
                    btn.BackgroundColor = Color.Black;
                    btn.Padding = new Thickness(0);
                    btn.TextColor = Color.White;
                    btn.VerticalOptions = LayoutOptions.FillAndExpand;
                    btn.HorizontalOptions = LayoutOptions.FillAndExpand;
                    btn.FontSize = 32;

                    btn.Clicked += reversiBTNClicked;
                }
            }

            for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                {
                    gameGrid[i, j] = -1;
                }
            }

            gameGrid[3, 3] = 0;
            gameGrid[3, 4] = 1;
            gameGrid[4, 3] = 1;
            gameGrid[4, 4] = 0;
            whoTurn = Turn.Black;
            updateGameViewBTNs();

        }

        protected void reversiBTNClicked(object sender, EventArgs args)
        {

        }

        private void updateGameViewBTNs()
        {
            for (int i = 0; i <= BTNGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= BTNGrid.GetUpperBound(1); j++)
                {
                    var btn = BTNGrid[i, j];
                    btn.BackgroundColor = Color.Black;
                    switch (gameGrid[i, j])
                    {
                        case 0:
                            {
                                btn.Text = "○";
                                break;
                            }
                        case 1:
                            {
                                btn.Text = "●";
                                break;
                            }
                        default:
                            {
                                btn.Text = "";
                                break;
                            }
                    }
                }
            }
        }

        private void updateGameViewBTNcanMove()
        {
            var list = getCanPut();
            foreach(var v in list)
            {
                BTNGrid[v.Key, v.Value].BackgroundColor = Color.Blue;
            }
        }
        List<KeyValuePair<int, int>> getCanPut()
        {
            Turn nowMoving = whoTurn;
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                {
                    if (gameGrid[i, j] == -1)
                    {
                        switch (nowMoving)
                        {
                            case Turn.Black:
                                if (getNextGrid(i, j, Turn.Black) != null)
                                {
                                    ret.Add(new KeyValuePair<int, int>(i, j));
                                }
                                break;
                            case Turn.White:
                                if (getNextGrid(i, j, Turn.White) != null)
                                {
                                    ret.Add(new KeyValuePair<int, int>(i, j));
                                }
                                break;
                        }
                    }
                }
            }
            return ret;
        }

        int[,] getNextGrid(int row, int col, Turn turn)
        {
            switch (turn)
            {
                case Turn.Black:
                    {
                        for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
                        {
                            for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                            {
                                if (gameGrid[i, j] == 0)
                                {
                                    if (i == row || j == col || (Math.Sqrt((row - i) * (row - i) + (col - j) * (col - j)) % Math.Sqrt(2)) < 0.001)
                                    {
                                        return gameGrid;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case Turn.White:
                    {
                        for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
                        {
                            for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                            {
                                if (gameGrid[i, j] == 1)
                                {
                                    if (i == row || j == col || (Math.Sqrt((row - i) * (row - i) + (col - j) * (col - j)) % Math.Sqrt(2)) <= 0.001)
                                    {
                                        return gameGrid;
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
            return null;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            updateGameViewBTNcanMove();
        }
    }
}