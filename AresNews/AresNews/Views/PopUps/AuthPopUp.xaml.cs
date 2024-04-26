﻿
using AresNews.Views.Portals;
using Rg.Plugins.Popup.Pages;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace AresNews.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AuthPopUp : PopupPage
    {
		public AuthPopUp()
		{
			InitializeComponent ();
		}

        public App CurrentApp { get; private set; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            CurrentApp = App.Current as App;
            // prevent the page from collapsing when the keyboard appears
            HasSystemPadding = false;


        }

        private async void Discord_Clicked(object sender, System.EventArgs e)
        {
            // Close this popup
            CurrentApp.ClosePopUp(this);
            DiscordAuthPortal discordAuthPortal = new DiscordAuthPortal();

            while (discordAuthPortal.Result == null)
            {
                await Task.Delay(100);
            }

            //if ((discordAuthPortal.Result ?? false))
               

            await App.Current.MainPage.Navigation.PushAsync(discordAuthPortal);
        }
    }
}