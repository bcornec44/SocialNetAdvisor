using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace SocialNetAdvisor.Models;

public class SuggestionItem : ObservableObject
{

    private string _text;
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public IAsyncRelayCommand CopyCommand { get; }

    public SuggestionItem()
    {
        CopyCommand = new AsyncRelayCommand(CopyToClipboard);
    }

    private async Task CopyToClipboard()
    {
        Clipboard.SetText(Text);
    }
}
