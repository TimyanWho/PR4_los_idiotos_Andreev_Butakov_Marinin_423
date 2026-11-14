using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class FavoritesPage : Page
    {
        private List<ProductCardModel> _favoriteProducts = new List<ProductCardModel>();

        public FavoritesPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ApiService.Instance.IsAuthenticated)
            {
                StatusText.Text = "Войдите в аккаунт, чтобы просматривать избранное.";
                return;
            }

            try
            {
                StatusText.Text = "Загрузка избранного...";
                var products = await ApiService.Instance.Favorites.GetFavoritesAsync();
                SetFavorites(products);
            }
            catch
            {
                StatusText.Text = "Не удалось загрузить избранное. Проверьте подключение к API.";
            }
        }

        public void SetFavorites(IEnumerable<ProductCardModel> products)
        {
            _favoriteProducts = (products ?? new List<ProductCardModel>())
                .Where(product => product != null)
                .Select(PrepareProductForDisplay)
                .ToList();

            UpdateFavoritesView();
        }

        private ProductCardModel PrepareProductForDisplay(ProductCardModel product)
        {
            product.ImagePath = GetImagePath(product.ImagePath);
            if (string.IsNullOrWhiteSpace(product.Category)) product.Category = "Без категории";
            return product;
        }

        private string GetImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return null;

            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return imagePath;
            }

            string fileName = Path.GetFileName(imagePath.Replace((char)92, '/'));
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            return "/PR4_los_idiotos_Andreev_Butakov_Marinin_423;component/Assets/Products/" +
                   Uri.EscapeDataString(fileName);
        }

        private void UpdateFavoritesView()
        {
            FavoritesItemsControl.ItemsSource = _favoriteProducts;
            bool hasFavorites = _favoriteProducts.Count > 0;

            FavoritesItemsControl.Visibility = hasFavorites ? Visibility.Visible : Visibility.Collapsed;
            EmptyText.Visibility = hasFavorites ? Visibility.Collapsed : Visibility.Visible;
            StatusText.Text = hasFavorites
                ? "Товаров в избранном: " + _favoriteProducts.Count
                : "В избранном пока нет товаров.";
        }

        private ProductCardModel GetProductFromButton(object sender)
        {
            return (sender as Button)?.DataContext as ProductCardModel;
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ProductCardModel product = GetProductFromButton(sender);
            if (product != null)
            {
                NavigationService.Navigate(new ProductDetailsPage(product));
            }
        }

        private async void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            ProductCardModel product = GetProductFromButton(sender);
            if (product == null) return;

            if (!product.IsAvailable || product.StockQuantity <= 0)
            {
                StatusText.Text = "Этот товар сейчас недоступен для покупки.";
                return;
            }

            try
            {
                bool moved = await ApiService.Instance.Favorites.MoveFavoriteToCartAsync(product.Id);
                if (moved)
                {
                    _favoriteProducts.Remove(product);
                    UpdateFavoritesView();
                    StatusText.Text = $"Товар «{product.Name}» перенесён в корзину.";
                }
                else
                {
                    StatusText.Text = "Не удалось перенести товар в корзину.";
                }
            }
            catch
            {
                StatusText.Text = "Ошибка соединения при переносе в корзину.";
            }
        }


        private async void RemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            ProductCardModel product = GetProductFromButton(sender);
            if (product == null) return;

            try
            {
                bool removed = await ApiService.Instance.Favorites.RemoveFavoriteAsync(product.Id);
                if (removed)
                {
                    _favoriteProducts.Remove(product);
                    UpdateFavoritesView();
                }
                else
                {
                    StatusText.Text = "Не удалось удалить товар из избранного.";
                }
            }
            catch
            {
                StatusText.Text = "Ошибка соединения при удалении из избранного.";
            }
        }
    }
}
