using SocialNetAdvisor.ViewModels;
using System.Windows;
namespace SocialNetAdvisor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel != null)
        {
            await viewModel.Loaded(webView);
        }
    }
}