﻿using GamHub.Models;
using GamHub.Views;
using MvvmHelpers;
using Rg.Plugins.Popup.Extensions;
using System.Linq;

namespace GamHub.ViewModels
{
    public class EditFeedViewModel : BaseViewModel
    {
        private Feed _feed;
        private string _initialKeyWords;

        public Feed Feed
        {
            get { return _feed; }
            set
            {
                _feed = value;
                OnPropertyChanged(nameof(Feed));
            }
        }
        private FeedsViewModel _context;

        public FeedsViewModel Context
        {
            get { return _context; }
            set
            {
                _context = value;
                OnPropertyChanged(nameof(Feed));
            }
        }
        private RenameFeedPopUp _page;
        private int index;

        public RenameFeedPopUp Page
        {
            get { return _page; }
            set
            {
                _page = value;
                OnPropertyChanged(nameof(Feed));
            }
        }
        public Microsoft.Maui.Controls.Command Validate => new Microsoft.Maui.Controls.Command(async () =>
        {

            // Remove feed
            Context.UpdateCurrentFeed(_feed);
            // update the feed
            App.SqLiteConn.Update(_feed);

            _context.ListHasBeenUpdated = true;

            if (_initialKeyWords != _feed.Keywords)
                _feed.IsLoaded = false;

            // Close the page
            await App.Current.MainPage.Navigation.PopAsync();

            System.Collections.ObjectModel.ObservableCollection<Feed> feeds = _context.Feeds;
            _context.CurrentFeedIndex = index = feeds.IndexOf(feeds.FirstOrDefault(feed => feed.Id == _feed.Id));
            _context.FeedTabs[index].Title = _feed.Title;

            //_context.UpdateOrders.Add(new UpdateOrder
            //{
            //    Feed = _feed,
            //    Update = UpdateOrder.FeedUpdate.Edit
                
            //});
        });

        public Microsoft.Maui.Controls.Command Cancel => new Microsoft.Maui.Controls.Command(async () =>
        {
            // Close the page

            //_context.CurrentFeedIndex = _context.Feeds.IndexOf(_feed);
            await App.Current.MainPage.Navigation.PopAsync();

        });
        public EditFeedViewModel( Feed feed, FeedsViewModel vm )
        {
            _feed = feed;
            _initialKeyWords = feed.Keywords;
            _context = vm;
        }
    }
}
