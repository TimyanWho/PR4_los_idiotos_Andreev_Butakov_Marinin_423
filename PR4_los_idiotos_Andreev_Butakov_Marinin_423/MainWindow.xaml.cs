using System.Windows;
//using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //MainFrame.Navigate(new CatalogPage());
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
           // MainFrame.Navigate(new CatalogPage());
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Страницы входа и регистрации добавим на следующем этапе.",
                "GMWOG",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Favorites_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            
        }

    }
}