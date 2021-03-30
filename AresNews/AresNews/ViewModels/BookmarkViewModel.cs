﻿using AresNews.Models;
using AresNews.Views;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AresNews.ViewModels
{
    public class BookmarkViewModel :BaseViewModel
    {
        // Property list of articles
        private ObservableCollection<Article> _bookmarks;

        public ObservableCollection<Article> Bookmarks
        {
            get { return _bookmarks; }
            set
            {
                _bookmarks = value;
                OnPropertyChanged();
            }
        }
        

        public Command GoToDetail
        {
            get
            {
                return new Command(async (id) =>
                {
                    var articlePage = new ArticlePage(_bookmarks.FirstOrDefault(art => art.Id == id.ToString()));

                    /*Task.Run(async () =>*/
                    await App.Current.MainPage.Navigation.PushAsync(articlePage);/*);*/
                }); ;
            }
        }
        // Command to add a Bookmark
        private Command _addBookmark;

        public Command AddBookmark
        {
            get { return _addBookmark; }
        }
        // Command to add a Bookmark
        private Command _shareArticle;

        public Command ShareArticle
        {
            get { return _shareArticle; }
        }
        public BookmarkViewModel()
        {

            Bookmarks = new ObservableCollection<Article>(App.SqLiteConn.Table<Article>().Reverse());


            _addBookmark = new Command((id) =>
            {
                //App.StartDb();

                // Get the article
                var article = _bookmarks.FirstOrDefault(art => art.Id == id.ToString());
                
                App.SqLiteConn.Delete(article);

                // Marked the article as saved
                Bookmarks.Remove(article);


            });


            // Set command to share an article
            _shareArticle = new Command(async (id) =>
            {
                // Get selected article
                var article = _bookmarks.FirstOrDefault(art => art.Id == id.ToString());

                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri = article.Url,
                    Title = "Share this article",
                    Subject = article.Title,
                    Text = article.Title
                });
            });
        }
    }
}
