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
        Stack<KeyValuePair<int[,], Turn>> gameStack = new Stack<KeyValuePair<int[,], Turn>>();
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
            initGame();
        }      


        private void updateGameViewBTNs()
        {
            for (int i = 0; i <= BTNGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= BTNGrid.GetUpperBound(1); j++)
                {
                    var btn = BTNGrid[i, j];
                    btn.BackgroundColor = Color.Black;
                    //btn.IsEnabled = false;

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
                btn.IsEnabled = true;

            }
        }

        private void updateGameInfo()
        {
            gameStack.Push(new KeyValuePair<int[,], Turn>(gameGrid.Clone() as int[,], whoTurn));
            LabelGameTurn.Text = (whoTurn == Turn.Black)?"黑子下":"白子下";
            int blackCount = 0;
            int whiteCount = 0;
            for(int i=0;i<=gameGrid.GetUpperBound(0);i++)
            {
                for(int j=0;j<=gameGrid.GetUpperBound(1);j++)
                {
                    if(gameGrid[i,j]==0)
                    {
                        blackCount++;
                    }
                    else if (gameGrid[i, j] == 1)
                    {
                        whiteCount++;
                    }
                }
            }
            LabelBlackScore.Text = string.Format("○: {0,2:##}", blackCount);
            LabelWhiteScore.Text = string.Format("●: {0,2:##}", whiteCount);
        }



        private void initGame()
        {
            gameStack.Clear();
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
            updateGameInfo();
            updateGameViewBTNs();
            updateGameViewBTNcanMove(getCanPut(whoTurn));
        }

        private void undoGame()
        {
            if (gameStack.Count >= 2)
            {
                gameStack.Pop();
                var top = gameStack.Pop();
                //gameStack.Push(top);

                gameGrid = top.Key;
                whoTurn = top.Value;
                updateGameViewBTNs();
                updateGameViewBTNcanMove(getCanPut(whoTurn));
                updateGameInfo();
            }
        }

        private async void finishGame()
        {
            updateGameViewBTNs();
            updateGameInfo();
            int blackCount = 0;
            int whiteCount = 0;
            for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                {
                    if (gameGrid[i, j] == 0)
                    {
                        blackCount++;
                    }
                    else if (gameGrid[i, j] == 1)
                    {
                        whiteCount++;
                    }
                }
            }
            string winner;
            if(blackCount>whiteCount)
            {
                winner = "黑的";
            }
            else if(whiteCount>blackCount)
            {
                winner = "白的";
            }
            else
            {
                winner = "平手";
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
                    
                }
            }
        }


        List<KeyValuePair<int, int>> getCanPut(Turn nowMoving)
        {

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
            int[,] result = null;
            switch (turn)
            {
                case Turn.Black:
                    {
                        for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
                        {
                            for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                            {
                                if (gameGrid[i, j] == 0 && !(row == i && col == j))
                                {
                                    List<KeyValuePair<int, int>> modify = new List<KeyValuePair<int, int>>();

                                    //nextGrid[row, col] = 0;
                                    modify.Add(new KeyValuePair<int, int>(row, col));
                                    if (i == row)
                                    {
                                        int from = col;
                                        int to = j;
                                        if (from > to)
                                        {
                                            int temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to -= 1;
                                        from += 1;
                                        bool failed = false;
                                        if (from > to)
                                        {
                                            failed = true;
                                        }
                                        while (from <= to)
                                        {
                                            if (gameGrid[row, from] == 1)
                                            {
                                                //nextGrid[row, from] = 0;
                                                modify.Add(new KeyValuePair<int, int>(row, from));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from++;
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 0;
                                            }
                                        }
                                    }
                                    if (j == col)
                                    {
                                        int from = row;
                                        int to = i;
                                        if (from > to)
                                        {
                                            int temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to -= 1;
                                        from += 1;
                                        bool failed = false;
                                        if (from > to)
                                        {
                                            failed = true;
                                        }
                                        while (from <= to)
                                        {
                                            if (gameGrid[from, col] == 1)
                                            {
                                                modify.Add(new KeyValuePair<int, int>(from, col));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from++;
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 0;
                                            }
                                        }
                                    }
                                    if (((Double)(row - i) / (col - j)) == 1.0)
                                    {

                                        Point from = new Point(row, col);
                                        Point to = new Point(i, j);
                                        Point step = new Point(1, 1);
                                        if (from.X > to.X)
                                        {
                                            Point temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to = to.Subtract(step);
                                        from = from.Add(step);
                                        bool failed = false;
                                        if (from.X > to.X)
                                        {
                                            failed = true;
                                        }
                                        while (from.X <= to.X)
                                        {
                                            if (gameGrid[(int)from.X, (int)from.Y] == 1)
                                            {
                                                modify.Add(new KeyValuePair<int, int>((int)from.X, (int)from.Y));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from = from.Add(step);
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 0;
                                            }
                                        }
                                    }
                                    if (((Double)(row - i) / (col - j)) == -1.0)
                                    {
                                        Point from = new Point(row, col);
                                        Point to = new Point(i, j);
                                        Point step = new Point(1, -1);
                                        if (from.X > to.X)
                                        {
                                            Point temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to = to.Subtract(step);
                                        from = from.Add(step);
                                        bool failed = false;
                                        if (from.X > to.X)
                                        {
                                            failed = true;
                                        }
                                        while (from.X <= to.X)
                                        {
                                            if (gameGrid[(int)from.X, (int)from.Y] == 1)
                                            {
                                                modify.Add(new KeyValuePair<int, int>((int)from.X, (int)from.Y));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from = from.Add(step);
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 0;
                                            }
                                        }
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
                                if (gameGrid[i, j] == 1 && !(row == i && col == j))
                                {
                                    List<KeyValuePair<int, int>> modify = new List<KeyValuePair<int, int>>();

                                    //nextGrid[row, col] = 0;
                                    modify.Add(new KeyValuePair<int, int>(row, col));
                                    if (i == row)
                                    {
                                        int from = col;
                                        int to = j;
                                        if (from > to)
                                        {
                                            int temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to -= 1;
                                        from += 1;
                                        bool failed = false;
                                        if (from > to)
                                        {
                                            failed = true;
                                        }
                                        while (from <= to)
                                        {
                                            if (gameGrid[row, from] == 0)
                                            {
                                                //nextGrid[row, from] = 1;
                                                modify.Add(new KeyValuePair<int, int>(row, from));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from++;
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 1;
                                            }
                                        }
                                    }
                                    if (j == col)
                                    {
                                        int from = row;
                                        int to = i;
                                        if (from > to)
                                        {
                                            int temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to -= 1;
                                        from += 1;
                                        bool failed = false;
                                        if (from > to)
                                        {
                                            failed = true;
                                        }
                                        while (from <= to)
                                        {
                                            if (gameGrid[from, col] == 0)
                                            {
                                                modify.Add(new KeyValuePair<int, int>(from, col));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from++;
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 1;
                                            }
                                        }
                                    }
                                    if (((Double)(row - i) / (col - j)) == 1.0)
                                    {

                                        Point from = new Point(row, col);
                                        Point to = new Point(i, j);
                                        Point step = new Point(1, 1);
                                        if (from.X > to.X)
                                        {
                                            Point temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to = to.Subtract(step);
                                        from = from.Add(step);
                                        bool failed = false;
                                        if (from.X > to.X)
                                        {
                                            failed = true;
                                        }
                                        while (from.X <= to.X)
                                        {
                                            if (gameGrid[(int)from.X, (int)from.Y] == 0)
                                            {
                                                modify.Add(new KeyValuePair<int, int>((int)from.X, (int)from.Y));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from = from.Add(step);
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 1;
                                            }
                                        }
                                    }
                                    if (((Double)(row - i) / (col - j)) == -1.0)
                                    {
                                        Point from = new Point(row, col);
                                        Point to = new Point(i, j);
                                        Point step = new Point(1, -1);
                                        if (from.X > to.X)
                                        {
                                            Point temp = from;
                                            from = to;
                                            to = temp;
                                        }

                                        to = to.Subtract(step);
                                        from = from.Add(step);
                                        bool failed = false;
                                        if (from.X > to.X)
                                        {
                                            failed = true;
                                        }
                                        while (from.X <= to.X)
                                        {
                                            if (gameGrid[(int)from.X, (int)from.Y] == 0)
                                            {
                                                modify.Add(new KeyValuePair<int, int>((int)from.X, (int)from.Y));
                                            }
                                            else
                                            {
                                                failed = true;
                                            }
                                            from = from.Add(step);
                                        }
                                        if (!failed)
                                        {
                                            if (result == null)
                                            {
                                                result = gameGrid.Clone() as int[,];
                                            }
                                            foreach (var v in modify)
                                            {
                                                result[v.Key, v.Value] = 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
            return result;
        }


        protected void BTNUndo_Clicked(object sender, EventArgs e)
        {
            undoGame();
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
                    Turn nowTurn = whoTurn;
                    var next = getNextGrid(row, col, nowTurn);
                    if (next != null)
                    {
                        gameGrid = next;
                        whoTurn = (nowTurn == Turn.Black) ? Turn.White : Turn.Black;
                        updateGameViewBTNs();
                        var nextCanMove = getCanPut(whoTurn);
                        if (nextCanMove.Count > 0)
                        {
                            updateGameViewBTNcanMove(nextCanMove);
                            updateGameInfo();
                        }
                        else
                        {
                            whoTurn = (nowTurn == Turn.Black) ? Turn.White : Turn.Black;
                            nextCanMove = getCanPut(whoTurn);
                            if (nextCanMove.Count > 0)
                            {
                                updateGameViewBTNcanMove(nextCanMove);
                                updateGameInfo();
                            }
                            else
                            {
                                finishGame();
                            }
                        }
                    }

                }
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }
    }
}