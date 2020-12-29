using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Reversi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NetworkPlayPage : ContentPage
    {
        TcpClient tcpClient;
        System.Threading.Thread tcpReceiveThread;
        ulong PSK = 0;
        string PeerNickName = "";


        ReversiButton[,] BTNGrid = new ReversiButton[8, 8];
        ReversiLogic reversi = new ReversiLogic();

        ReversiLogic.Turn playerRole = ReversiLogic.Turn.Black;
        bool undoing = false;

        public delegate void PeerDisconnectedHandler();
        public event PeerDisconnectedHandler PeerDisconnected;

        public delegate void PeerDataComingHandler(string data);
        public event PeerDataComingHandler PeerDataComing;

        public NetworkPlayPage(TcpClient client, ulong psk, string peerNickname, bool init)
        {
            InitializeComponent();
            tcpClient = client;
            PSK = psk;
            PeerNickName = peerNickname;

            tcpReceiveThread = new System.Threading.Thread(receiveingTCPPacket);
            tcpReceiveThread.IsBackground = true;
            tcpReceiveThread.Start();
                       
            Disappearing += clearBackground;
            PeerDataComing += OnPeerDataComing;
            PeerDisconnected += OnPeerDisconnected;

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

            if (init)
            {
                initGame();
            }
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
            string telegram = "INIT, You: ";
            if (playerRole == ReversiLogic.Turn.Black)
            {
                LabelSelfName.Text = "○: ";
                LabelPeerName.Text = "●: ";
                telegram += "White";
                await DisplayAlert("你是 黑色", "你是 黑色", "好喔");
            }
            else
            {
                LabelSelfName.Text = "●: ";
                LabelPeerName.Text = "○: ";
                telegram += "Black";
                await DisplayAlert("你是 白色", "你是 白色", "好喔");                
            }
            LabelSelfName.Text += "你";
            LabelPeerName.Text += PeerNickName;

            sendTCPPacket(telegram);
        }
      
        private void onReceivedInit(string data)
        {
            string localTurnStr = data.Split(',')[1].Substring(6);
            Dispatcher.BeginInvokeOnMainThread( async() =>
            {
                if (localTurnStr == "Black")
                {
                    LabelSelfName.Text = "○: ";
                    LabelPeerName.Text = "●: ";
                    playerRole = ReversiLogic.Turn.Black;
                    await DisplayAlert("你是 黑色", "你是 黑色", "好喔");
                }
                else
                {
                    LabelSelfName.Text = "●: ";
                    LabelPeerName.Text = "○: ";
                    playerRole = ReversiLogic.Turn.White;
                    await DisplayAlert("你是 白色", "你是 白色", "好喔");
                }
                LabelSelfName.Text += "你";
                LabelPeerName.Text += PeerNickName;
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
            else 
            {
                
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

      

        private async void OnPeerDisconnected()
        {
            await DisplayAlert("對方離開了", null, "好喔");
            Navigation.PopModalAsync();
        }

        private void OnPeerDataComing(string data)
        {
            if(data.Contains(","))
            {
                string select = data.Split(',')[0];
                if(select=="INIT")
                {
                    onReceivedInit(data);
                }
            }
        }

        private void clearBackground(object sender, EventArgs args)
        {
            try
            {
                tcpReceiveThread.Abort();
            }
            catch (Exception ex)
            {

            }
            try
            {
                tcpClient.Close();
            }
            catch (Exception ex)
            {

            }

        }

        private void receiveingTCPPacket()
        {
            while (true)
            {
                var stream = tcpClient.GetStream();
                byte[] buffer = new byte[2048];
                int byteReceived = 0;
                do
                {
                    try
                    {
                        byteReceived += stream.Read(buffer, byteReceived, buffer.Length);
                    }
                    catch
                    {

                    }
                } while (stream.DataAvailable);

                if (byteReceived <= 0)
                {
                    Dispatcher.BeginInvokeOnMainThread(() =>
                    {
                        System.Threading.Thread.Sleep(100);
                        PeerDisconnected?.Invoke();
                    });
                    break;
                }
                else
                {
                    string inStr = aesDecryptBase64(Encoding.UTF8.GetString(buffer).Trim('\0'), PSK);
                    if (inStr != "")
                    {
                        PeerDataComing?.Invoke(inStr.Trim('\0'));
                    }
                }
            }
        }

        private async void sendTCPPacket(string data)
        {
            string telegramStr = aesEncryptBase64(data, PSK);
            if (telegramStr != "")
            {
                await tcpClient.GetStream().WriteAsync(Encoding.UTF8.GetBytes(telegramStr), 0, Encoding.UTF8.GetByteCount(telegramStr));
            }

        }
        public static string aesEncryptBase64(string SourceStr, ulong Key)
        {
            string encrypt = "";
            try
            {
                var aes = new System.Security.Cryptography.AesCryptoServiceProvider();
                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                var sha256 = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] key = sha256.ComputeHash(BitConverter.GetBytes(Key));
                byte[] iv = md5.ComputeHash(BitConverter.GetBytes(Key));
                aes.Key = key;
                aes.IV = iv;

                byte[] dataByteArray = Encoding.UTF8.GetBytes(SourceStr);
                using (var ms = new System.IO.MemoryStream())
                using (var cs = new System.Security.Cryptography.CryptoStream(ms, aes.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    encrypt = Convert.ToBase64String(ms.ToArray());
                }
            }
            catch (Exception e)
            {

            }
            return encrypt;
        }

        public static string aesDecryptBase64(string SourceStr, ulong Key)
        {
            string decrypt = "";
            try
            {
                var aes = new System.Security.Cryptography.AesCryptoServiceProvider();
                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                var sha256 = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] key = sha256.ComputeHash(BitConverter.GetBytes(Key));
                byte[] iv = md5.ComputeHash(BitConverter.GetBytes(Key));
                aes.Key = key;
                aes.IV = iv;

                byte[] dataByteArray = Convert.FromBase64String(SourceStr);
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, aes.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
            }
            return decrypt;
        }
    }
}