using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Reversi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NetworkPairingPage : ContentPage
    {
        public class EPButton : Button
        {
            public long Timeout = 0;
            public ulong PeerID = 0;

            public IPAddress PeerIP;

            public EPButton() : base()
            {
                TextColor = Color.White;
                BorderWidth = 5;
                BorderColor = Color.White;
                BackgroundColor = Color.DarkSlateGray;
                HorizontalOptions = LayoutOptions.FillAndExpand;
                Margin = new Thickness(10, 10, 10, 10);
            }
        }

        int titleLabelDotAnimation = 0;

        string defaultName;
        ulong id;


        Task backgroundWorker;
        bool cancelingBackground = false;

        UdpClient udpClient;
        System.Threading.Thread udpReceiveThread;

        TcpListener tcpListener;
        System.Threading.Thread tcpReceiveThread;

        public NetworkPairingPage()
        {
            InitializeComponent();
            id = (uint)DateTime.Now.Ticks;
            defaultName = "路人_" + id.ToString();
            EntryPlayerName.Placeholder = defaultName;

            backgroundWorker = new Task(() => { backgroundFunc(); });
            backgroundWorker.Start();

            Disappearing += clearBackground;


            udpClient = new UdpClient(37654);
            udpReceiveThread = new System.Threading.Thread(receiveingUDPPacket);
            udpReceiveThread.IsBackground = true;
            udpReceiveThread.Start();

            tcpListener = new TcpListener(IPAddress.Any, 37654);
            tcpReceiveThread = new System.Threading.Thread(receiveingTCPPacket);
            tcpReceiveThread.IsBackground = true;
            tcpReceiveThread.Start();
        }

        private void clearBackground(object sender, EventArgs args)
        {
            if (backgroundWorker != null || backgroundWorker.IsCompleted == false)
            {
                cancelingBackground = true;
                backgroundWorker.Wait();
            }
            
            try
            {
                udpReceiveThread.Abort();
            }
            catch (Exception ex)
            {

            }
            try
            {
                tcpReceiveThread.Abort();
            }
            catch (Exception ex)
            {

            }
            tcpListener.Stop();
        }


        private void backgroundFunc()
        {
            while (!cancelingBackground)
            {
                System.Threading.Thread.Sleep(200);
                string newTitle = "配對";

                for (int i = 0; i < titleLabelDotAnimation; i++)
                {
                    newTitle += ".";
                }
                titleLabelDotAnimation++;
                titleLabelDotAnimation %= 7;

                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    LabelTitle.Text = newTitle;
                    List<EPButton> toRemove = new List<EPButton>();
                    foreach (var v in EPButtons.Children)
                    {
                        EPButton b = v as EPButton;
                        if (b.Timeout < ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds())
                        {
                            toRemove.Add(b);
                        }
                    }
                    foreach (var v in toRemove)
                    {
                        v.Clicked -= BTNPeer_Clicked;
                        EPButtons.Children.Remove(v);
                    }
                });

                string telegram = string.Format("ID: {0}, Name: {1}", id.ToString(), (EntryPlayerName.Text != "") ? EntryPlayerName.Text : defaultName);
                udpClient.SendAsync(Encoding.UTF8.GetBytes(telegram), Encoding.UTF8.GetByteCount(telegram), new IPEndPoint(IPAddress.Broadcast, 37654));
            }
        }

        private async void receiveingUDPPacket()
        {
            while (true)
            {
                var result = await udpClient.ReceiveAsync();

                string receiveStr = Encoding.UTF8.GetString(result.Buffer);
                if (result.RemoteEndPoint.Port == 37654)
                {
                    if (receiveStr.Contains("ID: ") && receiveStr.Contains(", Name: "))
                    {
                        string IDstr = receiveStr.Split(',')[0].Substring(4);
                        string Namestr = receiveStr.Split(',')[1].Substring(7);
                        if (IDstr != id.ToString())
                        {
                            Dispatcher.BeginInvokeOnMainThread(() =>
                            {
                                EPButton btn = null;
                                foreach (var v in EPButtons.Children)
                                {
                                    EPButton b = v as EPButton;
                                    if (b.PeerID == ulong.Parse(IDstr))
                                    {
                                        btn = b;
                                    }
                                }
                                if (btn == null)
                                {
                                    btn = new EPButton();
                                    btn.PeerID = ulong.Parse(IDstr);
                                    EPButtons.Children.Add(btn);
                                    btn.Clicked += BTNPeer_Clicked;
                                }
                                btn.Text = Namestr;
                                btn.PeerIP = result.RemoteEndPoint.Address;
                                btn.Timeout = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() + 10;
                            });
                        }
                    }
                }
            }
        }

        private void receiveingTCPPacket()
        {
            tcpListener.Start();
            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                byte[] buffer = new byte[2048];
                int byteReceived = client.GetStream().Read(buffer, 0, buffer.Length);

                string receiveStr = Encoding.UTF8.GetString(buffer);
                receiveStr = receiveStr.Trim('\0');
                if (receiveStr.Contains("ID: ") && receiveStr.Contains(", Name: ") && receiveStr.Contains(", To: "))
                {
                    string IDstr = receiveStr.Split(',')[0].Substring(4);
                    string Namestr = receiveStr.Split(',')[1].Substring(7);
                    string Tostr = receiveStr.Split(',')[2].Substring(5);
                    if (Tostr == id.ToString())
                    {
                        string OKstr = "OK..";
                        client.GetStream().Write(Encoding.UTF8.GetBytes(OKstr), 0, Encoding.UTF8.GetByteCount(OKstr));
                        Dispatcher.BeginInvokeOnMainThread(() =>
                        {
                            Navigation.PopModalAsync();
                            Navigation.PushModalAsync(new NetworkPlayPage(client));
                        });
                    }

                }
            }
        }
        protected void BTNPeer_Clicked(object sender, EventArgs args)
        {
            TcpClient tcpClient = new TcpClient();
            EPButton btn = sender as EPButton;
            try
            {
                tcpClient.Connect(btn.PeerIP, 37654);
                int trying = 0;
                do
                {
                    string telegram = string.Format("ID: {0}, Name: {1}, To: {2}", id.ToString(), (EntryPlayerName.Text != "") ? EntryPlayerName.Text : defaultName, btn.PeerID.ToString());
                    tcpClient.GetStream().Write(Encoding.UTF8.GetBytes(telegram), 0, Encoding.UTF8.GetByteCount(telegram));

                    byte[] buffer = new byte[2048];
                    tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer);
                    response = response.Trim('\0');
                    if (response.Contains("OK.."))
                    {
                        Dispatcher.BeginInvokeOnMainThread(() =>
                        {
                            Navigation.PopModalAsync();
                            Navigation.PushModalAsync(new NetworkPlayPage(tcpClient));
                        });
                    }
                    else
                    {
                        trying++;
                    }
                } while (trying < 5);
            }
            catch
            {
                EPButtons.Children.Remove(btn);
            }
        }
    }
}