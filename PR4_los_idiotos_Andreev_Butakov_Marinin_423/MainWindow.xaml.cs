using System.Windows;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new CatalogPage());
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var authPage = new AuthPage();
            authPage.AuthenticationCompleted += (s, args) => MainFrame.Navigate(new CatalogPage());
            MainFrame.Navigate(authPage);
        }
        private void Favorites_Click(object sender, RoutedEventArgs e)
        {
            ShowNotReady("Избранное");
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            ShowNotReady("Корзина");
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            ShowNotReady("История заказов");
        }

        private void ShowNotReady(string feature)
        {
            MessageBox.Show(
                feature + " будет реализовано на следующих этапах.",
                "GMWOG",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}