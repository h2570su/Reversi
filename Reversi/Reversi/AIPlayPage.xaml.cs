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
    public partial class AIPlayPage : ContentPage
    {
        enum Diffculty
        {
            Easy, Mediem, Hard, Random
        }


        ReversiButton[,] BTNGrid = new ReversiButton[8, 8];
        ReversiLogic reversi = new ReversiLogic();

        ReversiLogic.Turn playerRole = ReversiLogic.Turn.Black;
        Diffculty diffculty;
        bool doAI_Wait = true;
        bool undoing = false;

        public AIPlayPage()
        {
            InitializeComponent();

            BTNDiffcultyChange_Clicked(BTNEasy, null);

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
            initGame();
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
            LabelGameTurn.Text = (reversi.WhoTurn == playerRole) ? "玩家下" : "電腦下";
            LabelGameTurn.Text += "\n";
            LabelGameTurn.Text += (reversi.WhoTurn == ReversiLogic.Turn.Black) ? "(黑子)" : "(白子)";

            LabelBlackScore.Text = string.Format("○: {0,2:00}", blackAmount);
            LabelWhiteScore.Text = string.Format("●: {0,2:00}", whiteAmount);
        }

        private async void initGame()
        {
            playerRole = (DateTime.Now.Second % 2 == 0) ? ReversiLogic.Turn.Black : ReversiLogic.Turn.White;
            reversi.InitGame();
            if (playerRole == ReversiLogic.Turn.Black)
            {
                await DisplayAlert("你是 黑色", "你是 黑色", "好喔");
            }
            else
            {
                await DisplayAlert("你是 白色", "你是 白色", "好喔");
                Task.Run(() => { aiMove(ReversiLogic.Turn.Black); });
            }
        }

        private void aiMove(ReversiLogic.Turn nowMoving)
        {
            var canMove = reversi.getCanPutWithDiff(nowMoving);
            //Bigger in Front
            canMove.Sort((p2, p1) =>
            {
                return p1.Value.CompareTo(p2.Value);
            });

            int row = -1;
            int col = -1;
            switch (diffculty)
            {
                case Diffculty.Easy:
                    row = canMove[canMove.Count - 1].Key.Key;
                    col = canMove[canMove.Count - 1].Key.Value;
                    break;
                case Diffculty.Mediem:
                    row = canMove[canMove.Count / 2].Key.Key;
                    col = canMove[canMove.Count / 2].Key.Value;
                    break;
                case Diffculty.Hard:
                    row = canMove[0].Key.Key;
                    col = canMove[0].Key.Value;
                    break;
                case Diffculty.Random:
                    Random rand = new Random((int)DateTime.Now.Ticks);
                    int idx = rand.Next() % canMove.Count;
                    row = canMove[idx].Key.Key;
                    col = canMove[idx].Key.Value;
                    break;
            }
            if (doAI_Wait)
            {
                System.Threading.Thread.Sleep(300);
            }
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                reversi.PutDot(row, col);
            });
        }




        protected void onGameBroadChanged(object sender, int blackAmount, int whiteAmount)
        {
            updateGameViewInfo(sender as ReversiLogic, blackAmount, whiteAmount);
            updateGameViewBTNs((sender as ReversiLogic).GameGrid);
            var nowMoving = (sender as ReversiLogic).WhoTurn;
            if (nowMoving == playerRole)
            {
                updateGameViewBTNcanMove((sender as ReversiLogic).getCanPut((sender as ReversiLogic).WhoTurn));
            }
            else if (!undoing)
            {
                Task.Run(() => { aiMove(nowMoving); });
            }
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
                initGame();
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
            undoing = true;
            if (playerRole == ReversiLogic.Turn.Black || (playerRole == ReversiLogic.Turn.White && reversi.StackHeight >= 3))
            {
                do
                {
                    reversi.UndoGame();
                } while (reversi.WhoTurn != playerRole);
            }
            undoing = false;
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
                initGame();
            }
        }

        private void BTNDiffcultyChange_Clicked(object sender, EventArgs e)
        {
            var btn = sender as Button;

            BTNEasy.BackgroundColor = (btn != BTNEasy) ? Color.Transparent : Color.Blue;
            BTNMeid.BackgroundColor = (btn != BTNMeid) ? Color.Transparent : Color.Blue;
            BTNHard.BackgroundColor = (btn != BTNHard) ? Color.Transparent : Color.Blue;
            BTNRand.BackgroundColor = (btn != BTNRand) ? Color.Transparent : Color.Blue;

            if (btn == BTNEasy)
            {
                diffculty = Diffculty.Easy;
            }
            else if (btn == BTNMeid)
            {
                diffculty = Diffculty.Mediem;
            }
            else if (btn == BTNHard)
            {
                diffculty = Diffculty.Hard;
            }
            else if (btn == BTNRand)
            {
                diffculty = Diffculty.Random;
            }
        }

        private void BTNAIWait_Clicked(object sender, EventArgs e)
        {
            if ((sender as Button).BackgroundColor == Color.Transparent)
            {
                (sender as Button).BackgroundColor = Color.Blue;
                doAI_Wait = true;
            }
            else
            {
                (sender as Button).BackgroundColor = Color.Transparent;
                doAI_Wait = false;
            }
        }
    }
}