using SocialNetAdvisor.ViewModels;
using System.Windows;
namespace SocialNetAdvisor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Width = screenWidth / 2;
        Height = screenHeight;
        Left = screenWidth / 2;
        Top = 0;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel != null)
        {
            Task.Run(viewModel.Loaded);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel != null)
        {
            viewModel.Closing();
        }
    }
}