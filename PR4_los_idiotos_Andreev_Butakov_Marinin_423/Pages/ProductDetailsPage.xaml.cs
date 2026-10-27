using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;


namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class ProductDetailsPage : Page
    {
        public ProductDetailsPage(ProductCardModel product)
        {
            InitializeComponent();

            DataContext = product;
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (NavigationService == null)
            {
                return;
            }

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new CatalogPage());
            }
        }

        private void AuthRequiredAction_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button button = sender as Button;

            string action = button == null
                ? "Это действие"
                : button.Content.ToString();

            MessageBox.Show(
                action + " станет доступно после входа в аккаунт. " +
                "Подключим эту возможность вместе с авторизацией.",
                "GMWOG",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}