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
        public NetworkPlayPage(TcpClient client)
        {
            InitializeComponent();
            tcpClient = client;
        }
    }
}