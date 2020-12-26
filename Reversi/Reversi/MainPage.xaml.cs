using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Reversi
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected void BTN_LocalPlay_Clicked(object sender, EventArgs args)
        {
            Navigation.PushModalAsync(new LocalPlayPage());
        }

        protected void BTN_AiPlay_Clicked(object sender, EventArgs args)
        {
            Navigation.PushModalAsync(new AIPlayPage());
        }

        protected void BTN_LANPlay_Clicked(object sender, EventArgs args)
        {
            Navigation.PushModalAsync(new NetworkPairingPage());
        }

    }
}
