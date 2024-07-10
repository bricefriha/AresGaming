﻿using GamHub.Models;
using GamHub.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace GamHub.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EditFeedPage : ContentPage
	{
		public EditFeedPage (Feed feed, FeedsViewModel vm)
		{
			InitializeComponent ();
			BindingContext = new EditFeedViewModel(feed, vm);
		}

        private void Button_Clicked(object sender, EventArgs e)
        {

        }
    }
}