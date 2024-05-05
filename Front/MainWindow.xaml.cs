using MaterialDesignThemes.Wpf;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SocialNetAdvisor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
        }
    }
    private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TextBox tBox = (TextBox)sender;
            DependencyProperty prop = TextBox.TextProperty;

            BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
            if (binding != null) { binding.UpdateSource(); }
        }
    }

    private void CoreWebView2_ContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel != null)
        {
            var menuItems = e.MenuItems;
            CoreWebView2ContextMenuItem suggestMenuItem = webView.CoreWebView2.Environment.CreateContextMenuItem(
                "Suggest a response", null, CoreWebView2ContextMenuItemKind.Command);

            suggestMenuItem.CustomItemSelected += (sender, e) => {
                viewModel.SuggestCommand.Execute(null);
            };

            menuItems.Insert(0, suggestMenuItem);
        }
    }
}