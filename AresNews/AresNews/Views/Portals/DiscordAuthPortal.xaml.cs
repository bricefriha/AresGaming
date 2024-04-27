﻿using AresNews.Core;
using AresNews.Models;
using AresNews.Models.Http.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AresNews.Views.Portals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DiscordAuthPortal : ContentPage
    {

        public Action<AuthResponse> CallBack { get; private set; }
        public string Url { get; set; }
        public bool? Result { get; set; } = null;
        public static readonly App CurrentApp = (App.Current as App);
        public DiscordAuthPortal(Action<AuthResponse> callBack)
        {
            InitializeComponent();

            Url = $"https://discord.com/oauth2/authorize?client_id={AppConstant.DiscordClientId}&response_type=code&redirect_uri=https%3A%2F%2F{AppConstant.ApiHost}%2Fauth%2Fdiscord&scope=email+identify+connections";
            CallBack = callBack;
            BindingContext = this;
        }

        private async void DiscordPortal_Navigated(object sender, WebNavigatedEventArgs e)
        {
            // Catch the navigation to the api
            if (e.Url.Contains($"//{AppConstant.ApiHost}/auth/discord"))
            {
                // Get data returned
                string value = Regex.Unescape(await DiscordPortal.EvaluateJavaScriptAsync("document.getElementsByTagName(\"pre\")[0].innerHTML"));
                var res = JsonConvert.DeserializeObject<AuthResponse>(value);

                // Indicate a result is being returned
                Result = true;
                CallBack(res);

                // Navigate back
                CurrentApp.MainPage.Navigation.RemovePage(this);
            }
        }

        private void DiscordPortal_Navigating(object sender, WebNavigatingEventArgs e)
        {
            // Catch the navigation to the api
            if (e.Url.Contains($"//{AppConstant.ApiHost}/auth/discord"))
            {
                DiscordPortal.IsVisible = false;
            }
        }
    }
}