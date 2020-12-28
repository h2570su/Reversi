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
    
    public partial class LocalPlayPage : ContentPage
    {
        ReversiButton[,] BTNGrid = new ReversiButton[8, 8];
        ReversiLogic reversi = new ReversiLogic();

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
            reversi.BroadChanged += onGameBroadChanged;
            reversi.GameFinished += onGameFinished;
            reversi.InitGame();
            
        }


        private void updateGameViewBTNs(int[,] gameGrid)
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

        private void updateGameViewBTNcanMove(List<KeyValuePair<int, int>> list)
        {
            foreach (var v in list)
            {
                var btn = BTNGrid[v.Key, v.Value];
                btn.BackgroundColor = Color.Blue;                
            }
        }

        private void updateGameViewInfo(ReversiLogic reversi, int blackAmount, int whiteAmount)
        {          
            LabelGameTurn.Text = (reversi.WhoTurn == ReversiLogic.Turn.Black) ? "黑子下" : "白子下";

            LabelBlackScore.Text = string.Format("○: {0,2:00}", blackAmount);
            LabelWhiteScore.Text = string.Format("●: {0,2:00}", whiteAmount);
        }




        protected void onGameBroadChanged(object sender, int blackAmount, int whiteAmount)
        {
            updateGameViewBTNs((sender as ReversiLogic).GameGrid);
            updateGameViewBTNcanMove((sender as ReversiLogic).getCanPut((sender as ReversiLogic).WhoTurn));
            updateGameViewInfo(sender as ReversiLogic, blackAmount, whiteAmount);
        }

        protected async void onGameFinished(object sender, ReversiLogic.Turn whoWon)
        {
            updateGameViewBTNs((sender as ReversiLogic).GameGrid);
            string winner;
            switch (whoWon)
            {
                case ReversiLogic.Turn.None:
                    winner = "平手";
                    break;
                case ReversiLogic.Turn.Black:
                    winner = "黑的";
                    break;
                case ReversiLogic.Turn.White:
                    winner = "白的";
                    break;
                default:
                    winner = "平手";
                    break;
            }
            await DisplayAlert("誰贏了?", winner, "好喔");
            bool anotherGameSel = await DisplayAlert("再來一局?", "你要再來一局嗎?", "好", "不要");
            if (anotherGameSel)
            {
                reversi.InitGame();
            }
            else
            {
                bool quitSel = await DisplayAlert("不然呢?", "你想要怎樣", "退出", "待著");
                if (quitSel)
                {
                    Navigation.PopModalAsync();
                }
                else
                {
                    //Non action
                }
            }
        }



        protected void BTNUndo_Clicked(object sender, EventArgs e)
        {
            reversi.UndoGame();
        }

        protected void reversiBTNClicked(object sender, EventArgs args)
        {
            var btn = sender as ReversiButton;
            if (btn != null)
            {
                if (btn.BackgroundColor == Color.Blue)
                {
                    int row = btn.gameRow;
                    int col = btn.gameCol;
                    reversi.PutDot(row, col);                    
                }
            }
        }

        private async void BTNRestart_Clicked(object sender, EventArgs e)
        {            
            if (await DisplayAlert("重來", "你要重來嗎?", "好", "不要"))
            {
                reversi.InitGame();
            }
        }
    }
}