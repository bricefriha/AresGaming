﻿using GamHubApp.Core;
using GamHubApp.Models;
using GamHubApp.Models.Http.Payloads;
using GamHubApp.Models.Http.Responses;
using CustardApi.Objects;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif
namespace GamHubApp.Services
{
    public class Fetcher
    {
        public static string ProdHost { get; } = "api.gamhub.io";
        private static string _dateFormat = "dd-MM-yyy_HH:mm:ss";
        private Session CurrentSession { get; set; }
        public Service WebService { get; private set; }
        public User UserData { get; set; }
        public Fetcher()
        {
#if DEBUG_LOCALHOST
            // Set webservice
            WebService = new Service(host: AppConstant.Localhost,
                                    port: 255,
                                   sslCertificate: false);
#else
            // Set webservice
            WebService = new Service(host: ProdHost,
                                   sslCertificate: true);
#endif
        }
        /// <summary>
        /// Get the last 2 months worth of feed
        /// </summary>
        /// <returns>last 2 months worth of feed</returns>
        public async Task<Collection<Article>> GetMainFeedUpdate()
        {
            try
            {

                return await this.WebService.Get<Collection<Article>>(controller: "feeds",
                                                                   action: "update",
                                                                   parameters: new string[] { DateTime.Now.AddMonths(-2).ToString(_dateFormat) },
                                                                   jsonBody: null,
                                                                   unSuccessCallback: e => _ = HandleHttpException(e));
            }
            catch (Exception ex)
            {

#if DEBUG
                throw ex;
#else
                return null;
#endif
            }
        }
        /// <summary>
        /// Get all the sources
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Collection<Source>> GetSources()
        {
            return await WebService.Get<Collection<Source>>(controller: "sources", action: "getAll", unSuccessCallback: async (e) =>
            {
#if DEBUG || DEBUG_LOCALHOST
                throw new Exception(await e.Content.ReadAsStringAsync());
#endif
            });
        }
        /// <summary>
        /// Get the feed of an article
        /// </summary>
        /// <param name="keywords">keywords of the feed</param>
        /// <param name="timeUpdate">time of the last update (if applicable)</param>
        /// <param name="needUpdate">does the feed need an update</param>
        /// <returns></returns>
        public async Task<Collection<Article>> GetFeedArticles(string keywords, string timeUpdate = null, bool needUpdate = false)
        {
            try
            {
                // Convert the spaces to make it url friendly
                keywords = keywords.Replace(' ', '+');
                return await WebService.Get<Collection<Article>>(controller: "feeds",
                                                                 action: needUpdate ? "update" : null,
                                                                 parameters: needUpdate ? [timeUpdate] : [timeUpdate, keywords],
                                                                 unSuccessCallback: async (err) =>
                                                                 {
                                                                     string call = err.RequestMessage.RequestUri.OriginalString;
                                                                     string content = await err.RequestMessage.Content.ReadAsStringAsync();
#if DEBUG
                                                                     throw new Exception(await err.Content.ReadAsStringAsync());
#endif                                                           
                                                                 });
            }
            catch (Exception ex)
            {

#if DEBUG
                Debug.WriteLine(ex);

#endif

                return null;
            }
        }
        /// <summary>
        /// Get a brand new discord session from a resfresh token
        /// </summary>
        /// <param name="refreshToken">refreshToken given by discord auth</param>
        /// <returns></returns>
        public async Task<Session> RefreshDiscordSession(string refreshToken)
        {
            try
            {
                RefreshDiscordPayload payload = new()
                {
                    RefreshToken = refreshToken
                };
                RefreshSessionResponse res = await WebService.Post<RefreshSessionResponse>(controller: "auth",
                                                                                            action: "discord/refresh_token",
                                                                                            jsonBody: JsonConvert.SerializeObject(payload),
                                                                                            unSuccessCallback: e => _ = HandleHttpException(e));

                if (res == null)
                {
                    return null;
                }
                return res.Session;
            }
            catch (Exception ex)
            {

#if DEBUG
                throw ex;
#else
                return null; 
#endif
            }
        }
        /// <summary>
        /// Get the lastest articles since given date
        /// </summary>
        /// <param name="dateUpdate">given date as "dd-MM-yyy_HH:mm:ss"</param>
        /// <returns>lastest articles the date provided</returns>
        public async Task<Collection<Article>> GetMainFeedUpdate(string dateUpdate)
        {
            try
            {

                return await this.WebService.Get<Collection<Article>>(controller: "feeds",
                                                                   action: "update",
                                                                   parameters: new string[] { dateUpdate },
                                                                   jsonBody: null,
                                                                   unSuccessCallback: e => _ = HandleHttpException(e));
            }
            catch (Exception ex)
            {

#if DEBUG
                throw ex;
#else
                return null; 
#endif
            }
        }
        /// <summary>
        /// Get the chunk articles from given date
        /// </summary>
        /// <param name="dateFrom">date from which the last article was published as "dd-MM-yyy_HH:mm:ss"</param>
        /// <param name="length">length of the chunk in hour</param>
        /// <returns>chunk articles the date provided</returns>
        public async Task<Collection<Article>> GetFeedChunk(DateTime dateFrom, int length)
        {
            try
            {
                //dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, dateFrom.Hour, dateFrom.Minute, 0);
                string[] parameters = new string[]
                {
                   dateFrom.AddHours(-length).ToString("dd-MM-yyy_HH:mm:ss"),
                   dateFrom.AddMinutes(-1).ToString("dd-MM-yyy_HH:mm:ss"),
                };
                return await this.WebService.Get<Collection<Article>>(controller: "feeds",
                                                                   parameters: parameters,
                                                                   jsonBody: null,
                                                                   unSuccessCallback: e => _ = HandleHttpException(e));
            }
            catch (Exception ex)
            {

#if DEBUG
                throw ex;
#else
                return null; 
#endif
            }
        }
        /// <summary>
        /// Save all the tokens of a session and expiration
        /// </summary>
        /// <param name="newSession"></param>
        public void SaveSession (Session newSession)
        {
            // Keep the current session
            CurrentSession = newSession;

            // Save sensitive data
            var accessTask = SecureStorage.SetAsync(nameof(Session.AccessToken), newSession.AccessToken);
            var refreshTask = SecureStorage.SetAsync(nameof(Session.RefreshToken), newSession.RefreshToken);
            var tokenTypeTask = SecureStorage.SetAsync(nameof(Session.TokenType), newSession.TokenType);

            _ = Task.WhenAll(accessTask, refreshTask, tokenTypeTask);

            // Save regular data about the session
            Preferences.Set(nameof(Session.ExpiresIn), newSession.ExpiresIn);
            Preferences.Set(nameof(Session.Created), newSession.Created);
        }
        /// <summary>
        /// Restore the last session if any
        /// </summary>
        public async Task RestoreSession ()
        {
            // Save regular data about the session
            int exp = Preferences.Get(nameof(Session.ExpiresIn), Int16.MinValue);
            string refreshToken = await SecureStorage.GetAsync(nameof(Session.RefreshToken)).ConfigureAwait(false);

            // If there was no session just leave
            if (string.IsNullOrEmpty(refreshToken)) return;

            DateTime dt = Preferences.Get(nameof(Session.Created), DateTime.MinValue);

            // Check if the token expired
            if ((DateTime.UtcNow - dt).TotalSeconds >= exp)
            {
                // Refresh the token
                SaveSession(await RefreshDiscordSession(refreshToken));

                return;
            }
            if (!(App.Current as App).RecoverUserInfo())
            {
                // Kill the previous session
                KillSession();
                return;

            }
            // Save sensitive data
            var accessTask = SecureStorage.GetAsync(nameof(Session.AccessToken));
            var tokenTypeTask = SecureStorage.GetAsync(nameof(Session.TokenType));

            await Task.WhenAll(accessTask, tokenTypeTask);

            // Fill up the current session with the data gathered from the previous one
            CurrentSession = new()
            {
                AccessToken = accessTask.Result,
                RefreshToken = refreshToken,
                TokenType = tokenTypeTask.Result,
                ExpiresIn = exp
            };


        }
        /// <summary>
        /// Adding a hook to an article
        /// </summary>
        /// <param name="article">article hooked</param>
        public async Task RegisterHook(Article article)
        {
            var headers = new Dictionary<string, string>
            {
                { "x-api-key", AppConstant.MonitoringKey},
            };
            var paramss = new string[] { article.MongooseId };

            await WebService.Post(controller: "monitor",
                                                action: "register",
                                                singleUseHeaders: headers,
                                                parameters: paramss);
        }

        /// <summary>
        /// Kill a session
        /// </summary>
        public void KillSession()
        {
            // Empty the property
            CurrentSession = null;

            // Clear all data stored
            SecureStorage.Remove(nameof(Session.AccessToken));
            SecureStorage.Remove(nameof(Session.TokenType));


        }
        /// <summary>
        /// Method to handle exceptions
        /// </summary>
        /// <param name="err"></param>
        /// <exception cref="Exception"></exception>
        private async Task HandleHttpException(HttpResponseMessage err)
        {
#if DEBUG
            throw new Exception(await err.Content.ReadAsStringAsync());
#endif
        }
    }
}