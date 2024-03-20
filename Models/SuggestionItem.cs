using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace SocialNetAdvisor.Models;

public class SuggestionItem
{
    public string Text { get; set; }

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
