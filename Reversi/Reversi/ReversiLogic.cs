using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
namespace Reversi
{
    public class ReversiLogic
    {
        public enum Turn
        {
            None, Black, White
        }

        public int[,] GameGrid { get { return gameGrid.Clone() as int[,]; } }
        public Turn WhoTurn { get { return whoTurn; } }
        public int StackHeight { get { return gameStack.Count; } }

        int[,] gameGrid = new int[8, 8];
        Stack<KeyValuePair<int[,], Turn>> gameStack = new Stack<KeyValuePair<int[,], Turn>>();
        Turn whoTurn = Turn.Black;


        public List<KeyValuePair<int, int>> getCanPut(Turn nowMoving)
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

        //<<row,col>,diff>
        public List<KeyValuePair<KeyValuePair<int, int>, int>> getCanPutWithDiff(Turn nowMoving)
        {

            List<KeyValuePair<KeyValuePair<int, int>, int>> ret = new List<KeyValuePair<KeyValuePair<int, int>, int>>();
            for (int i = 0; i <= gameGrid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= gameGrid.GetUpperBound(1); j++)
                {
                    if (gameGrid[i, j] == -1)
                    {
                        int[,] nextGrid;
                        int amount = 0;
                        switch (nowMoving)
                        {
                            case Turn.Black:

                                nextGrid = getNextGrid(i, j, Turn.Black);
                                if (nextGrid != null)
                                {
                                    for (int ii = 0; ii <= nextGrid.GetUpperBound(0); ii++)
                                    {
                                        for (int jj = 0; jj <= nextGrid.GetUpperBound(0); jj++)
                                        {
                                            if (nextGrid[ii, jj] != gameGrid[ii, jj])
                                            {
                                                amount++;
                                            }
                                        }
                                    }
                                    ret.Add(new KeyValuePair<KeyValuePair<int, int>, int>(
                                        new KeyValuePair<int, int>(i, j),
                                        amount));
                                }


                                break;
                            case Turn.White:

                                nextGrid = getNextGrid(i, j, Turn.White);
                                if (nextGrid != null)
                                {

                                    if (nextGrid != null)
                                    {
                                        for (int ii = 0; ii <= nextGrid.GetUpperBound(0); ii++)
                                        {
                                            for (int jj = 0; jj <= nextGrid.GetUpperBound(0); jj++)
                                            {
                                                if (nextGrid[ii, jj] != gameGrid[ii, jj])
                                                {
                                                    amount++;
                                                }
                                            }
                                        }
                                        ret.Add(new KeyValuePair<KeyValuePair<int, int>, int>(
                                            new KeyValuePair<int, int>(i, j),
                                            amount));
                                    }

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

        public void InitGame()
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
        }

        public void UndoGame()
        {
            if (gameStack.Count >= 2)
            {
                gameStack.Pop();
                var top = gameStack.Pop();

                gameGrid = top.Key;
                whoTurn = top.Value;

                updateGameInfo();
            }
        }

        public void PutDot(int row, int col)
        {
            
            var next = getNextGrid(row, col, whoTurn);
            if (next != null)
            {
                gameGrid = next;
                whoTurn = (whoTurn == Turn.Black) ? Turn.White : Turn.Black;

                var nextCanMove = getCanPut(whoTurn);
                if (nextCanMove.Count > 0)
                {
                    updateGameInfo();
                }
                else
                {
                    whoTurn = (whoTurn == Turn.Black) ? Turn.White : Turn.Black;
                    nextCanMove = getCanPut(whoTurn);
                    if (nextCanMove.Count > 0)
                    {
                        updateGameInfo();
                    }
                    else
                    {
                        updateGameInfo();
                        finishGame();
                    }
                }
            }
        }



        private void updateGameInfo()
        {
            gameStack.Push(new KeyValuePair<int[,], Turn>(gameGrid.Clone() as int[,], whoTurn));

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
            BroadChanged?.Invoke(this, blackCount, whiteCount);
        }

        private void finishGame()
        {
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


            if (blackCount > whiteCount)
            {
                GameFinished?.Invoke(this, Turn.Black);
            }
            else if (whiteCount > blackCount)
            {
                GameFinished?.Invoke(this, Turn.White);
            }
            else
            {
                GameFinished?.Invoke(this, Turn.None);
            }
        }

        public delegate void BroadChangedHandler(object sender, int blackAmount, int whiteAmount);
        public event BroadChangedHandler BroadChanged;

        public delegate void GameFinishedHandler(object sender, Turn whoWon);
        public event GameFinishedHandler GameFinished;
    }
}
