using Common.Connectors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using SocialNetAdvisor.Helpers;
using SocialNetAdvisor.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
namespace SocialNetAdvisor.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    private IWebDriver _driver;
    private const string _facebookUrl = "https://www.facebook.com/";
    private ISuggestionConnector _suggestionConnector;

    public ObservableCollection<SuggestionItem> Suggestions { get; set; } = new ObservableCollection<SuggestionItem>();


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

    public MainViewModel()
    {
        SuggestCommand = new AsyncRelayCommand(Suggest);
    }

    public IAsyncRelayCommand SuggestCommand { get; }

    internal void Loaded()
    {
        LoadDriver(_facebookUrl);
       _suggestionConnector = new SuggestionApiConnector();
        _suggestionConnector.Initialize();
    }

    private void LoadDriver(string driverName)
    {
        var options = new ChromeOptions();
        ChromeDriverService service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        options.AddExcludedArguments(new List<string> { "enable-automation" });
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.67 Safari/537.36");
        //options.AddArgument("--user-data-dir=C:/Users/benja/AppData/Local/Google/Chrome/User Data");
        options.AddArgument("--profile-directory=Default");
        options.AddArguments("--disable-notifications");
        _driver = new ChromeDriver(service, options);
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        _driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
        _driver.Manage().Window.Size = new System.Drawing.Size((int)(screenWidth / 2), (int)screenHeight);

        if (HasCookie(driverName))
        {
            _driver.Navigate().GoToUrl(driverName);
            LoadCookie(_driver, driverName);
            Wait.S(3);
        }
        else
        {
            _driver.Navigate().GoToUrl(driverName);
        }
        SaveCookie(driverName).Wait();
    }

    private async Task Suggest(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Progress = 0;
        Suggestions.Clear();
        var contextBySelectedText = await GetContextBySelectedText();
        var contextByIdenfiedPost = await GetContextByIdenfiedPost();

        var contexts = new List<string>
        {
            contextBySelectedText,
            contextByIdenfiedPost,
            contextByIdenfiedPost
        };

        if (contexts.Any(p => !string.IsNullOrEmpty(p)))
        {
            contexts.Add(contextBySelectedText);
        }
        else
        {
            contexts.Add(await GetContextByFullPageContent());
        }

        Progress = 50;
        var availableContexts = contexts.Where(p => !string.IsNullOrEmpty(p)).ToList();
        var i = 0;
        foreach (var context in availableContexts)
        {

            var suggestion = new SuggestionItem();
            Suggestions.Add(suggestion);

            try
            {
                await foreach (var suggestionPart in _suggestionConnector.GetSuggestion(context))
                {
                    suggestion.Text = suggestion.Text + suggestionPart;
                }
            }
            catch (Exception ex)
            {
                suggestion.Text = "Error on client side";
            }
            File.WriteAllText($"suggestion{i}.txt", $"{context}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{suggestion.Text}");
            i++;
            Progress = 50 + (i * 50 / availableContexts.Count);
        }
        IsLoading = false;
    }

    private async Task<string> GetContextByIdenfiedPost()
    {
        try
        {
            return await Task.Run(() =>
            {
                var justText = GetBodyText(_driver);
                var originalPosts = GetOriginalPosts(justText);
                var selectedText = GetSelection(_driver);
                var identifiedPost = FindOriginalPost(selectedText, originalPosts);
                var identifiedReplies = string.Empty;

                if (identifiedPost == null)
                {
                    SelectAll(_driver);
                    selectedText = GetSelection(_driver);
                    var currentPost = selectedText.Split(" · ").LastOrDefault();
                    var currentPostReplies = currentPost.Split("Répondre").Select(CleanText).ToList();
                    currentPostReplies.Remove(currentPostReplies.Last());
                    if (currentPostReplies.Any())
                    {
                        var sample = currentPostReplies.First().Split(Environment.NewLine).First().Trim();
                        identifiedPost = FindOriginalPost(sample, originalPosts);
                        if (identifiedPost != null)
                        {
                            currentPostReplies.Remove(currentPostReplies.FirstOrDefault());
                        }
                    }

                    identifiedReplies = string.Join(Environment.NewLine + Environment.NewLine + "New comment : " + Environment.NewLine, currentPostReplies);
                }
                return identifiedPost + identifiedReplies;
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
            return await Task.Run(() => GetBodyText(_driver));
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
            return await Task.Run(() => GetSelection(_driver));
        }
        catch (Exception)
        {

        }
        return string.Empty;
    }

    private void SelectAll(IWebDriver driver)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var actions = new Actions(driver);
            bool isMacOS = System.Runtime.InteropServices.RuntimeInformation
                        .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);

            if (isMacOS)
            {
                actions.KeyDown(Keys.Command).SendKeys("a").KeyUp(Keys.Command).Perform();
            }
            else
            {
                actions.KeyDown(Keys.Control).SendKeys("a").KeyUp(Keys.Control).Perform();
            }
        });
    }

    private string GetBodyText(IWebDriver driver)
    {
        IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
        string script = "return document.querySelector('body').innerText;";
        string bodyText = (string)jsExecutor.ExecuteScript(script);
        return bodyText;
    }

    private string GetSelection(IWebDriver driver)
    {
        IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
        string script = "return window.getSelection().toString();";
        string selectedText = (string)jsExecutor.ExecuteScript(script);
        return selectedText;
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
                state.Stop(); // Arrête la boucle dès qu'un résultat est trouvé
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

    private Task SaveCookie(string driverName)
    {
        var cookies = _driver.Manage().Cookies.AllCookies.Select(c => new SerializableCookie
        {
            Name = c.Name,
            Value = c.Value,
            Domain = c.Domain,
            Path = c.Path,
            Expiry = c.Expiry,
            IsHttpOnly = c.IsHttpOnly,
            SameSite = c.SameSite,
            Secure = c.Secure
        }).ToList();

        string json = JsonSerializer.Serialize(cookies);
        File.WriteAllText(GetCookieName(driverName), json);
        return Task.CompletedTask;
    }

    private bool HasCookie(string driverName)
    {
        return File.Exists(GetCookieName(driverName));
    }

    private string GetCookieName(string driverName)
    {
        return $"cookies{Regex.Replace(driverName, "[^A-Za-z0-9 -]", "")}.json";
    }

    private void LoadCookie(IWebDriver driver, string driverName)
    {
        string json = File.ReadAllText(GetCookieName(driverName));
        var cookies = JsonSerializer.Deserialize<List<SerializableCookie>>(json);

        if (cookies != null)
        {
            foreach (var sc in cookies)
            {
                Cookie cookie = new Cookie(sc.Name, sc.Value, sc.Domain, sc.Path, sc.Expiry, sc.Secure, sc.IsHttpOnly, sc.SameSite);
                driver.Manage().Cookies.AddCookie(cookie);
            }
        }

        driver.Navigate().Refresh();
    }

    internal void Closing()
    {
        _driver.Quit();
    }
}
