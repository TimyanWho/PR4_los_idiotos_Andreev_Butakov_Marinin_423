using System.Windows;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateMenuState();
            MainFrame.Navigate(new CatalogPage());
        }

        public void UpdateMenuState()
        {
            bool isAuth = ApiService.Instance.IsAuthenticated;

            FavoritesButton.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;
            CartButton.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;
            OrdersButton.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;

            LoginButton.Visibility = isAuth ? Visibility.Collapsed : Visibility.Visible;
            UserProfilePanel.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;

            UserNameText.Text = isAuth ? ApiService.Instance.CurrentUserLogin : string.Empty;

            AdminButton.Visibility =
            isAuth && ApiService.Instance.IsAdministrator
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var authPage = new AuthPage();
            authPage.AuthenticationCompleted += (s, args) =>
            {
                UpdateMenuState();
                MainFrame.Navigate(new CatalogPage());
            };
            MainFrame.Navigate(authPage);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ApiService.Instance.Auth.Logout();
            UpdateMenuState();
            MainFrame.Navigate(new CatalogPage());
        }

        private void Favorites_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new FavoritesPage());
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CartPage());
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiService.Instance.IsAdministrator)
            {
                MessageBox.Show(
                    "Эта страница доступна только администратору.",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                UpdateMenuState();
                return;
            }

            MainFrame.Navigate(new AdminPage());
        }
    }
}
