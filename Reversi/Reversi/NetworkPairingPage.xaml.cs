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
        public class EPButton : ViewCell
        {
            public long Timeout = 0;
            public ulong PeerID = 0;
            public Button button;

            public EPButton() : base()
            {
                button = new Button();
                button.TextColor = Color.White;
                button.BorderWidth = 5;
                button.BackgroundColor = Color.Transparent;
                button.HorizontalOptions = LayoutOptions.Fill;
                button.Margin = new Thickness(10, 10, 10, 10);
                var stackLayout = new StackLayout();
                stackLayout.Children.Add(button);
                View = stackLayout;
            }
        }

        int titleLabelDotAnimation = 0;

        string defaultName;
        ulong id;


        Task backgroundWorker;
        bool cancelingBackground = false;

        UdpClient udpClient;
        System.Threading.Thread udpReceiveThread;

        ListView listView;

        ObservableCollection<EPButton> buttons = new ObservableCollection<EPButton>();

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

            listView = new ListView();
            listView.ItemTemplate = new DataTemplate(typeof(EPButton));
            listView.ItemsSource = buttons;
            listView.BackgroundColor = Color.Transparent;

            listContainer.Children.Add(listView);

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
                        if (IDstr == id.ToString())
                        {
                            Dispatcher.BeginInvokeOnMainThread(() =>
                            {
                                EPButton btn = null;
                                foreach (var v in buttons)
                                {
                                    if (v.PeerID == ulong.Parse(IDstr))
                                    {
                                        btn = v;
                                    }
                                }
                                if (btn == null)
                                {
                                    btn = new EPButton();
                                    btn.PeerID = ulong.Parse(IDstr);
                                    buttons.Add(btn);
                                }
                                btn.button.Text = Namestr;
                                btn.Timeout = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();


                            });
                        }
                    }
                }
            }
        }
    }
}