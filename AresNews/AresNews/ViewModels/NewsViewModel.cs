﻿using AresNews.Models;
using HtmlAgilityPack;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using Xamarin.Forms;

namespace AresNews.ViewModels
{
    public class NewsViewModel : BaseViewModel
    {
        // Property list of articles
        private ObservableCollection<Article> _articles;

        public ObservableCollection<Article> Articles
        {
            get { return _articles; }
            set 
            { 
                _articles = value;
                OnPropertyChanged();
            }
        }
        // Command to add a Bookmark
        private Command _addBookmark;

        public Command AddBookmark
        {
            get { return _addBookmark; }
        }


        public NewsViewModel()
        {
            _articles = new ObservableCollection<Article>();

            _addBookmark = new Command((id) =>
               {
                   //App.StartDb();

                   // Get the article
                   var article = _articles.FirstOrDefault(art => art.Id == id.ToString());



                   // If the article is already in bookmarks
                   bool isSaved = article.IsSaved;

                   // Marked the article as saved
                   article.IsSaved = !article.IsSaved;

                   if (isSaved)
                       App.SqLiteConn.Delete(article);
                   else
                       // Insert it in database
                       App.SqLiteConn.Insert(article);


                   Articles[_articles.IndexOf(article)] = article;

                   
               });
            FetchArticles();

        }
        private void FetchArticles()
        {
            // move throu all the sources
            foreach (var source in App.Sources)
            {
                // Create the RSS reader
                var reader = XmlReader.Create(source.Url);

                SyndicationFeed feed = SyndicationFeed.Load(reader);

                
                reader.Close();

                // Add the news article of this feed one by one
                foreach (SyndicationItem item in feed.Items)
                {
                    // Get the main image
                    var image = GetImagesFromHTML(item)[0];

                    // include the article only if the link is not an ad and if we can get an image
                    string articleUrl = item.Links[0].Uri.OriginalString;

                    if (!string.IsNullOrEmpty(image) && articleUrl.Contains(source.Domain))
                    {
                        string id = item.Id;
                        _articles.Add(new Article
                        {
                            Id = id,
                            Title = item.Title.Text,
                            PublishDate = item.PublishDate.DateTime,
                            SourceName = source.Name,
                            Image = GetImagesFromHTML(item)[0],
                            Url = articleUrl,
                            IsSaved = App.SqLiteConn.Find<Article>(id) != null

                        });
                    }
                }

                // Reorder articles
                Articles = new ObservableCollection<Article>(_articles.OrderByDescending(a => a.PublishDate));
            }
        }
        /// <summary>
        /// Fetch an image from a url
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static List<string> GetImagesFromHTML(SyndicationItem item/*string url*/)
        {

            // Instanciate the list
            List<string> images = new List<string>();

            // Load the Html
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(item.Summary.Text);

            HtmlNodeCollection imageNodes = doc.DocumentNode.SelectNodes("//img");

            if (imageNodes != null && imageNodes.Count != 0)
            {
                foreach (HtmlNode node in imageNodes)
                {
                    images.Add(node.Attributes["src"].Value);
                }
            }
            else
            {
                // Load the webpage html
                using (WebClient client = new WebClient())
                {
                    doc.LoadHtml(client.DownloadString(item.Links[0].Uri.OriginalString));

                }

                // Now, using LINQ to get all Images
                //List<HtmlNode> imageNodes = null;
                imageNodes = doc.DocumentNode.SelectNodes("//img");
                //imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                //              where node.Name == "img"
                //              && node.Attributes["class"] != null
                //              && node.Attributes["class"].Value.StartsWith("img_")
                //              select node).ToList();

                foreach (HtmlNode node in imageNodes)
                {
                    var src = node.Attributes["src"].Value;
                    if (src.Contains(".png") || src.Contains(".jpg") || src.Contains(".jpeg"))
                        images.Add(src);

                }
                images.Add(string.Empty);
            }

            

            return images;
        }
    }
}
