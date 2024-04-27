using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SocialNetAdvisor.ViewModels
{
    public partial class SuggestionViewModel : ObservableObject
    {
        private string _selectedTextHtml;
        public string SelectedTextHtml
        {
            get => _selectedTextHtml;
            set => SetProperty(ref _selectedTextHtml, value);
        }

        private string _suggestedResponse;
        public string SuggestedResponse
        {
            get => _suggestedResponse;
            set => SetProperty(ref _suggestedResponse, value);
        }

        public SuggestionViewModel(string initialText)
        {
            SelectedTextHtml = initialText;
            GenerateCommand = new AsyncRelayCommand(GenerateResponse);
            CopyCommand = new RelayCommand(CopyResponseToClipboard);
            CloseDialogCommand = new RelayCommand(CloseDialog);
        }

        public IAsyncRelayCommand GenerateCommand { get; }
        public IRelayCommand CopyCommand { get; }
        public IRelayCommand CloseDialogCommand { get; }

        public async Task GenerateResponse()
        {
            IsLoading = true;
            await Task.Delay(1000);
            SuggestedResponse = "This is a simulated response based on the input: " + SelectedTextHtml;
            IsLoading = false;
        }

        public void CopyResponseToClipboard()
        {
            System.Windows.Clipboard.SetText(SuggestedResponse);
            CloseDialog();
        }

        public void CloseDialog()
        {
            // Close the dialog logic here. This might involve calling a method on the view or using a service
            // This part depends on how your dialog is being displayed. For Material Design in XAML, you might use DialogHost.CloseDialogCommand
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
    }
}
