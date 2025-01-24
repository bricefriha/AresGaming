﻿using GamHubApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace GamHubApp.ViewModels;

public class ArticleViewModel : BaseViewModel
{
    private CancellationTokenSource _cts;
    private Article _selectedArticle;

    public Article SelectedArticle
    {
        get { return _selectedArticle; }
        set 
        { 
            _selectedArticle = value;
            OnPropertyChanged();
        }
    }
    private bool _isMenuOpen;

    public bool IsMenuOpen
    {
        get { return _isMenuOpen; }
        set 
        {
            _isMenuOpen = value;
            OnPropertyChanged(nameof(IsMenuOpen));
        }
    }
    public ObservableCollection<Deal> Deals { get; set; }


    /// <summary>
    /// Time spend reading the article
    /// </summary>
    public Stopwatch TimeSpent { get; set; }

    public Command Browse 
    { 
        get 
        {
            return new Command(async () => await Browser.OpenAsync(_selectedArticle.Url, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Default,
            }));
        }
    }
    private string _ttsIcon;

    public string TtsIcon
    {
        get 
        { 
            return _ttsIcon; 
        }
        set 
        {
            _ttsIcon = value; 
            OnPropertyChanged(nameof(TtsIcon));
        }
    }
    private string _ttsColour;

    public string TtsColour
    {
        get 
        { 
            return _ttsColour; 
        }
        set 
        {
            _ttsColour = value; 
            OnPropertyChanged(nameof(TtsColour));
        }
    }

    public Command PlayTextToSpeech 
    { 
        get 
        {
            return new Command<string>(async (text) =>
            {
                try
                {
                    // Stop the tts if already launched
                    if (_audioIsPlaying)
                    {
                        StopTtS();
                        return;
                    }

                    _cts = new CancellationTokenSource();

                    // indicator text to speech done
                    bool ttsDone = false;

                    // Run text to speech
                    await Task.Factory.StartNew(async () =>
                    {
                        await TextToSpeech.SpeakAsync(SelectedArticle.TextSnipet, _cts.Token);
                        ttsDone = true;
                    });
                    AudioIsPlaying = !_audioIsPlaying;

                    // Change the icon
                    if (_audioIsPlaying)
                        await Task.Run(() =>
                        {
                            TtsColour = "#222326";
                            while (_audioIsPlaying && !ttsDone)
                            {
                                //TtsIcon = "\uf6a8";
                                //Thread.Sleep(500);
                                TtsIcon = "\uf028";
                                Thread.Sleep(500);
                                TtsIcon = "\uf027";
                                Thread.Sleep(500);
                            }
                            StopTtS();
                        });
                } 
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }
    }

    /// <summary>
    /// Stop text to speach
    /// </summary>
    public void StopTtS()
    {
        CancelSpeech();
        // Reset icon
        TtsIcon = "\uf028";
        TtsColour = "#36383c";

        // Mark the audio as not playin
        AudioIsPlaying = false;
    }

    private bool  _audioIsPlaying;

    public bool AudioIsPlaying
    {
        get 
        {
            return _audioIsPlaying; 
        }
        set 
        { 
            _audioIsPlaying = value; 
            OnPropertyChanged(nameof(AudioIsPlaying));
        }
    }

    public ArticleViewModel(Article article)
    {
        Deals = new ObservableCollection<Deal>((App.Current as App).Deals.Where(deal => 
        {
            for (int i = 0; i < article.Categories?.Count(); i++) 
                if (deal.Title.ToLower().Contains(article.Categories[i].ToLower()) 
                ||
                deal.Description.ToLower().Contains(article.Categories[i].ToLower()))
                    return true;
            return false;
        }).ToList());
        _ttsIcon = "\uf028";
        _ttsColour = "#36383c";

#if !DEBUG
        // Register Hook
        _ =(App.Current as App).DataFetcher.RegisterHook(article);
#endif

        _selectedArticle = article;

        TimeSpent = new Stopwatch();
        TimeSpent.Start();
    }
    /// <summary>
    /// Cancel text to speech
    /// </summary>
    public void CancelSpeech()
    {
        if (_cts?.IsCancellationRequested ?? true)
            return;

        _cts.Cancel();
    }
}
