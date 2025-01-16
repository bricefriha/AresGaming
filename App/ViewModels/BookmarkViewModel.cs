﻿using GamHubApp.Models;
using SQLiteNetExtensions.Extensions;
using System.Collections.ObjectModel;

namespace GamHubApp.ViewModels
{
    public class BookmarkViewModel : BaseViewModel
    {
        // Property list of articles
        private ObservableCollection<Article> _bookmarks;
        private App _curr;

        public ObservableCollection<Article> Bookmarks
        {
            get { return _bookmarks; }
            set
            {
                _bookmarks = value;
                OnPropertyChanged();
            }
        }
        
        public BookmarkViewModel()
        {

            Bookmarks = new ObservableCollection<Article>(GetArticlesFromDb());

            // Handle if a article change sees a change of bookmark state
            MessagingCenter.Subscribe<Article>(this, "SwitchBookmark", (sender) =>
            {
                try
                {

                    Bookmarks = new ObservableCollection<Article>(GetArticlesFromDb());


                }
                catch (Exception ex)
                {

                    throw ex;
                }


            });
        }
        /// <summary>
        /// Get all the articles bookmarked from the local database
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Article> GetArticlesFromDb()
        {
            return App.SqLiteConn.GetAllWithChildren<Article>(recursive: true).Reverse<Article>();
        }
    }
}
