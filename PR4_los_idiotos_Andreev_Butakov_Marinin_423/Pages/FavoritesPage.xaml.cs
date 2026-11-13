
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class FavoritesPage : Page
    {
        private List<ProductCardModel> _favoriteProducts =
            new List<ProductCardModel>();

        public FavoritesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFavoritesView();

            // TODO: Здесь будет загрузка избранного пользователя через API.
            // После получения товаров вызываем SetFavorites(products).
        }

        public void SetFavorites(IEnumerable<ProductCardModel> products)
        {
            _favoriteProducts = (products ?? new List<ProductCardModel>())
                .Where(product => product != null)
                .Select(PrepareProductForDisplay)
                .ToList();

            UpdateFavoritesView();
        }

        private ProductCardModel PrepareProductForDisplay(
            ProductCardModel product)
        {
            product.ImagePath = GetImagePath(product.ImagePath);

            if (string.IsNullOrWhiteSpace(product.Category))
            {
                product.Category = "Без категории";
            }

            if (string.IsNullOrWhiteSpace(product.AvailableText))
            {
                product.AvailableText = product.IsAvailable &&
                                        product.StockQuantity > 0
                    ? "В наличии"
                    : "Нет в наличии";
            }

            if (string.IsNullOrWhiteSpace(product.PackageText))
            {
                product.PackageText = product.ItemsPerPackage.HasValue &&
                                      product.ItemsPerPackage.Value > 1
                    ? "В упаковке: " + product.ItemsPerPackage.Value + " шт."
                    : "Обычный товар";
            }

            if (string.IsNullOrWhiteSpace(product.RatingText))
            {
                product.RatingText = product.Rating > 0
                    ? "★ " + product.Rating.ToString("0.0")
                    : "Пока нет оценок";
            }

            return product;
        }

        private string GetImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return imagePath;
            }

            string fileName = Path.GetFileName(
                imagePath.Replace((char)92, '/'));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            return "/PR4_los_idiotos_Andreev_Butakov_Marinin_423;component/Assets/Products/" +
                   Uri.EscapeDataString(fileName);
        }

        private void UpdateFavoritesView()
        {
            FavoritesItemsControl.ItemsSource = _favoriteProducts;

            bool hasFavorites = _favoriteProducts.Count > 0;

            FavoritesItemsControl.Visibility = hasFavorites
                ? Visibility.Visible
                : Visibility.Collapsed;

            EmptyText.Visibility = hasFavorites
                ? Visibility.Collapsed
                : Visibility.Visible;

            StatusText.Text = hasFavorites
                ? "Товаров в избранном: " + _favoriteProducts.Count
                : "В избранном пока нет товаров.";
        }

        private ProductCardModel GetProductFromButton(object sender)
        {
            Button button = sender as Button;

            return button == null
                ? null
                : button.DataContext as ProductCardModel;
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ProductCardModel product = GetProductFromButton(sender);

            if (product != null)
            {
                NavigationService.Navigate(
                    new ProductDetailsPage(product));
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            ProductCardModel product = GetProductFromButton(sender);

            if (product == null)
            {
                return;
            }

            if (!product.IsAvailable || product.StockQuantity <= 0)
            {
                StatusText.Text = "Этот товар сейчас недоступен для покупки.";
                return;
            }

            // TODO: Здесь будет вызов API добавления товара в корзину.

            StatusText.Text =
                "Добавление товара в корзину будет подключено через API.";
        }

        private void RemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            ProductCardModel product = GetProductFromButton(sender);

            if (product == null)
            {
                return;
            }

            // TODO: Здесь будет вызов API удаления товара из избранного.
            // После успешного ответа API нужно удалить товар из _favoriteProducts
            // и вызвать UpdateFavoritesView().

            StatusText.Text =
                "Удаление из избранного будет подключено через API.";
        }
    }
}
