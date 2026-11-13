using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class CatalogPage : Page
    {
        private List<ProductCardModel> _allProducts =
            new List<ProductCardModel>();

        private readonly ICatalogApiClient _catalogApiClient;
        private bool _isReady;
        private bool _isLoading;
        private bool _loadCompleted;

        public CatalogPage()
            : this(null)
        {
        }

        public CatalogPage(ICatalogApiClient catalogApiClient)
        {
            InitializeComponent();

            _catalogApiClient = catalogApiClient;
            RetryButton.Visibility = _catalogApiClient == null
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loadCompleted || _isLoading)
            {
                return;
            }

            if (_catalogApiClient == null)
            {
                StatusText.Text =
                    "Клиент каталога API ещё не подключён. " +
                    "После согласования API маршрута здесь появятся товары.";
                return;
            }

            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            if (_catalogApiClient == null || _isLoading)
            {
                return;
            }

            _isLoading = true;
            RetryButton.IsEnabled = false;
            StatusText.Text = "Загружаем каталог...";

            try
            {
                IReadOnlyList<ProductCardModel> products =
                    await _catalogApiClient.GetProductsAsync();

                _allProducts = (products ?? new List<ProductCardModel>())
                    .Where(product => product != null)
                    .Select(PrepareProductForDisplay)
                    .ToList();

                PopulateCategories();

                _isReady = true;
                _loadCompleted = true;
                ApplyFilters();
            }
            catch
            {
                StatusText.Text =
                    "Не удалось загрузить каталог. Проверьте подключение к API и повторите попытку.";
                ProductsItemsControl.ItemsSource =
                    new List<ProductCardModel>();
            }
            finally
            {
                _isLoading = false;
                RetryButton.IsEnabled = true;
            }
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

        private void PopulateCategories()
        {
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add("Все категории");

            foreach (string category in _allProducts
                .Select(product => product.Category)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name))
            {
                CategoryFilter.Items.Add(category);
            }

            CategoryFilter.SelectedIndex = 0;
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

            // The API returns the stored image path; the actual images are WPF Resources.
            string fileName = Path.GetFileName(
                imagePath.Replace((char)92, '/'));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            return "/PR4_los_idiotos_Andreev_Butakov_Marinin_423;component/Assets/Products/" +
                   Uri.EscapeDataString(fileName);
        }

        private void Filters_Changed(
            object sender,
            TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryFilter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void AvailableFilter_Changed(
            object sender,
            RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!_isReady)
            {
                return;
            }

            IEnumerable<ProductCardModel> result = _allProducts;

            string search = SearchBox.Text.Trim();

            if (search.Length > 0)
            {
                result = result.Where(p =>
                    p.Name.IndexOf(
                        search,
                        StringComparison.OrdinalIgnoreCase) >= 0);
            }

            string category = CategoryFilter.SelectedItem as string;

            if (!string.IsNullOrEmpty(category) &&
                category != "Все категории")
            {
                result = result.Where(p => p.Category == category);
            }

            decimal minPrice;
            decimal maxPrice;

            if (!TryParsePrice(MinPriceBox.Text, out minPrice) &&
                !string.IsNullOrWhiteSpace(MinPriceBox.Text))
            {
                StatusText.Text = "Проверьте минимальную цену.";
                ProductsItemsControl.ItemsSource =
                    new List<ProductCardModel>();
                return;
            }

            if (!TryParsePrice(MaxPriceBox.Text, out maxPrice) &&
                !string.IsNullOrWhiteSpace(MaxPriceBox.Text))
            {
                StatusText.Text = "Проверьте максимальную цену.";
                ProductsItemsControl.ItemsSource =
                    new List<ProductCardModel>();
                return;
            }

            if (!string.IsNullOrWhiteSpace(MinPriceBox.Text))
            {
                result = result.Where(p => p.Price >= minPrice);
            }

            if (!string.IsNullOrWhiteSpace(MaxPriceBox.Text))
            {
                result = result.Where(p => p.Price <= maxPrice);
            }

            if (!string.IsNullOrWhiteSpace(MinPriceBox.Text) &&
                !string.IsNullOrWhiteSpace(MaxPriceBox.Text) &&
                minPrice > maxPrice)
            {
                StatusText.Text =
                    "Минимальная цена больше максимальной.";

                ProductsItemsControl.ItemsSource =
                    new List<ProductCardModel>();
                return;
            }

            if (AvailableOnlyCheckBox.IsChecked == true)
            {
                result = result.Where(p =>
                    p.IsAvailable && p.StockQuantity > 0);
            }

            List<ProductCardModel> filtered = result.ToList();

            ProductsItemsControl.ItemsSource = filtered;
            StatusText.Text = "Найдено товаров: " + filtered.Count;
        }

        private bool TryParsePrice(string value, out decimal price)
        {
            price = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            string normalized = value.Trim()
                .Replace(" ", "")
                .Replace(',', '.');

            return decimal.TryParse(
                normalized,
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out price);
        }

        private void ClearFilters_Click(
            object sender,
            RoutedEventArgs e)
        {
            SearchBox.Clear();
            MinPriceBox.Clear();
            MaxPriceBox.Clear();

            CategoryFilter.SelectedIndex = 0;
            AvailableOnlyCheckBox.IsChecked = false;

            ApplyFilters();
        }

        private async void RetryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await LoadProductsAsync();
        }

        private void Details_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button button = sender as Button;

            ProductCardModel product = button == null
                ? null
                : button.DataContext as ProductCardModel;

            if (product != null)
            {
                NavigationService.Navigate(
                    new ProductDetailsPage(product));
            }
        }
    }
}
