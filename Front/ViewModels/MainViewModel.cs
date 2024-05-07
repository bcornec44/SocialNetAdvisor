using Common.Connectors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
namespace SocialNetAdvisor.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    private WebView2 _webView;
    private ISuggestionConnector _suggestionConnector;
    private string _personality = "";

    private string _suggestionHtml;
    public string SuggestionHtml
    {
        get => _suggestionHtml;
        set => SetProperty(ref _suggestionHtml, value);
    }

    private string _selectedTextHtml;
    public string SelectedTextHtml
    {
        get => _selectedTextHtml;
        set => SetProperty(ref _selectedTextHtml, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private int _progress;
    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    private string _defaultUrl = "https://www.google.com";
    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            if (!value.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && !value.StartsWith("c:", StringComparison.InvariantCultureIgnoreCase))
            {
                Url = "https://www.google.com/search?q=" + value;
            }
            if (SetProperty(ref _url, value))
            {
                try
                {
                    _webView.CoreWebView2.Navigate(value);
                }
                catch (Exception)
                {
                    Url = "https://www.google.com/search?q=" + value;
                }
            }
        }
    }

    private bool _showSuggestions;
    public bool ShowSuggestions
    {
        get => _showSuggestions;
        set
        {
            _showSuggestions = value;
            OnPropertyChanged(nameof(ShowSuggestions));
            OnPropertyChanged(nameof(ShowWebView));
        }
    }

    public bool ShowWebView => !_showSuggestions;


    public MainViewModel()
    {
        ShowSuggestions = false;
        SuggestCommand = new AsyncRelayCommand(Suggest);
        SuggestAgainCommand = new AsyncRelayCommand(SuggestAgain);
        ChangePersonalityAndSuggestCommand = new AsyncRelayCommand<string>(ChangePersonalityAndSuggest);
        CancelCommand = new RelayCommand(Cancel);
        CopyCommand = new RelayCommand(Copy);
        GoToUrlCommand = new RelayCommand(GoToUrl);
        GoToCommand = new RelayCommand<string>(GoTo);
        HomeCommand = new RelayCommand(Home);
    }

    public IAsyncRelayCommand SuggestCommand { get; }
    public IAsyncRelayCommand SuggestAgainCommand { get; }
    public IAsyncRelayCommand ChangePersonalityAndSuggestCommand { get; }
    public IRelayCommand CopyCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand GoToUrlCommand { get; }
    public ICommand GoToCommand { get; }
    public ICommand SettingsCommand { get; }


    internal async Task Loaded(WebView2 webView)
    {
        _webView = webView;
        await LoadWebView();
        _suggestionConnector = new SuggestionMockConnector();
        _suggestionConnector.Initialize();
    }

    public async Task LoadWebView()
    {
        await _webView.EnsureCoreWebView2Async(null).ContinueWith(task =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webView.CoreWebView2.PermissionRequested += (sender, args) =>
                {
                    if (args.PermissionKind == CoreWebView2PermissionKind.Notifications)
                    {
                        args.State = CoreWebView2PermissionState.Deny;
                    }
                };

                Home();
            });
        });
    }

    private void Cancel()
    {
        SuggestCommand.Cancel();
        ShowSuggestions = false;
    }

    private void Copy()
    {
        Clipboard.SetText(SuggestionHtml);        
        Cancel();
    }

    private async Task ChangePersonalityAndSuggest(string personality, CancellationToken cancellationToken)
    {
        _personality = personality;
        await SuggestAgain(cancellationToken);
    }

    private async Task SuggestAgain(CancellationToken cancellationToken)
    {
        SuggestCommand.Cancel();
        var i = 0;
        while (IsLoading)
        {
            await Task.Delay(100, cancellationToken);
            i++;
            if (i > 10000)
            {
                throw new Exception("couldnt stop previous suggestion");
            }
        }

        SuggestCommand.Execute(null);
    }

    private async Task Suggest(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Progress = 0;
        SuggestionHtml = string.Empty;
        var contextBySelectedText = await GetContextBySelectedText();
        var contextByIdenfiedPost = await GetContextByIdenfiedPost();
        SelectedTextHtml = contextBySelectedText;
        if (!string.IsNullOrEmpty(contextByIdenfiedPost))
        {
            SelectedTextHtml = contextByIdenfiedPost;
        }

        SelectedTextHtml = SelectedTextHtml.Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>").Replace("\\n", "\r\n");

        ShowSuggestions = true;
        Progress = 50;

        try
        {
            await foreach (var suggestionPart in _suggestionConnector.GetSuggestion(SelectedTextHtml, _personality, cancellationToken))
            {
                SuggestionHtml += suggestionPart;
            }
        }
        catch (Exception ex)
        {
            SuggestionHtml = "Error on client side";
        }
        File.WriteAllText($"suggestion.txt", $"{SelectedTextHtml}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{SuggestionHtml}");

        Progress = 100;
        IsLoading = false;
    }

    /// TODO mapper les boutons, corriger l'affichage des textes

    private async Task<string> GetContextByIdenfiedPost()
    {
        try
        {
            return await Task.Run(async () =>
            {
                var justText = await GetBodyText();
                var originalPosts = GetOriginalPosts(justText);
                var selectedText = await GetSelection();
                var identifiedPost = FindOriginalPost(selectedText, originalPosts);
                return identifiedPost;
            });
        }
        catch (Exception)
        {

        }
        return string.Empty;
    }

    private async Task<string> GetContextByFullPageContent()
    {
        try
        {
            return await Task.Run(GetBodyText);
        }
        catch (Exception)
        {

        }
        return string.Empty;
    }

    private async Task<string> GetContextBySelectedText()
    {
        try
        {
            return await Task.Run(GetSelection);
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
        }
        return string.Empty;
    }

    private async Task<string> GetBodyText()
    {
        var script = "document.body.innerText";
        string pageText = await Application.Current.Dispatcher.Invoke(async () =>
        {
            return await _webView.CoreWebView2.ExecuteScriptAsync(script);
        });
        return pageText.Trim('"');
    }

    private async Task<string> GetSelection()
    {
        string script = "window.getSelection().toString()";
        string pageText = await Application.Current.Dispatcher.Invoke(async () =>
        {
            return await _webView.CoreWebView2.ExecuteScriptAsync(script);
        });
        return pageText.Trim('"');
    }

    private ConcurrentBag<string> GetOriginalPosts(string input)
    {
        var splittedByCommentActionLabel = input.Split("Commenter");
        var posts = new ConcurrentBag<string>();
        Parallel.ForEach(splittedByCommentActionLabel, post =>
        {
            var postCleaned = CleanText(post);
            posts.Add(postCleaned);
        });
        return posts;
    }

    private string FindOriginalPost(string sample, ConcurrentBag<string> messages)
    {
        string result = null;
        Parallel.ForEach(messages, (msg, state) =>
        {
            if (msg.Contains(sample))
            {
                result = msg;
                state.Stop();
            }
        });

        return result;
    }

    private string CleanText(string input)
    {
        input = input.Split("Toutes les réactions").FirstOrDefault();
        input = CleanFirstText(input, Environment.NewLine);
        input = CleanFirstText(input, "Partager");
        input = CleanFirstText(input, Environment.NewLine);
        input = CleanFirstText(input, "Écrivez un commentaire…");
        input = CleanFirstText(input, Environment.NewLine);
        input = CleanFirstText(input, "Écrivez un commentaire public...");
        input = CleanFirstText(input, Environment.NewLine);

        input = CleanLastText(input, Environment.NewLine);
        input = CleanLastText(input, "Ça m’intéresse");
        input = CleanLastText(input, Environment.NewLine);
        input = CleanLastText(input, "J’aime");
        input = CleanLastText(input, Environment.NewLine);
        input.Replace(Environment.NewLine, "<br/>");
        return input;
    }

    private string CleanFirstText(string source, string firstText)
    {
        if (source == null || firstText == null)
        {
            throw new ArgumentNullException(source == null ? nameof(source) : nameof(firstText));
        }

        while (source.StartsWith(firstText))
        {
            source = source.Substring(firstText.Length);
        }

        return source;
    }

    private string CleanLastText(string source, string lastText)
    {
        if (source == null || lastText == null)
        {
            throw new ArgumentNullException(source == null ? nameof(source) : nameof(lastText));
        }

        while (source.EndsWith(lastText))
        {
            source = source.Substring(0, source.Length - lastText.Length);
        }

        return source;
    }
    
    private void GoToUrl()
    {
        OnPropertyChanged(nameof(Url));
    }

    private void GoTo(string? url)
    {
        Url = url;
    }

    private void Home()
    {
        Url = _defaultUrl;
    }
}
