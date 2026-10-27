using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class CatalogPage : Page
    {
        private List<ProductCardModel> _allProducts =
            new List<ProductCardModel>();

        private bool _isReady;
        private bool _loaded;

        public CatalogPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;

            try
            {
                LoadProducts();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка загрузки каталога.";

                MessageBox.Show(
                    "Не удалось загрузить товары из базы данных:\n" +
                    ex.Message,
                    "GMWOG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            // Получаем товары, категории и отзывы из существующей БД.
            var products = Core.Context.products.ToList();
            var categories = Core.Context.categories.ToList();
            var reviews = Core.Context.reviews.ToList();

            _allProducts = products.Select(p =>
            {
                var category = categories.FirstOrDefault(
                    c => c.id == p.category_id);

                var productReviews = reviews.Where(
                    r => r.product_id == p.id).ToList();

                double rating = productReviews.Count > 0
                    ? productReviews.Average(r => (double)r.rating)
                    : 0;

                return new ProductCardModel
                {
                    Id = p.id,
                    Name = p.name,
                    Price = p.price,
                    Description = p.description,
                    Category = category == null
                        ? "Без категории"
                        : category.name,
                    ImagePath = GetImagePath(p.image_path),
                    StockQuantity = p.stock_quantity,
                    IsAvailable = p.is_available,
                    Rating = rating,
                    RatingText = productReviews.Count > 0
                        ? "★ " + rating.ToString("0.0") +
                          " (" + productReviews.Count + " отзывов)"
                        : "Пока нет оценок",
                    AvailableText =
                        p.is_available && p.stock_quantity > 0
                            ? "В наличии"
                            : "Нет в наличии",
                    PackageText = p.items_per_package.HasValue &&
                                  p.items_per_package.Value > 1
                        ? "В упаковке: " + p.items_per_package.Value + " шт."
                        : "Обычный товар"
                };
            }).ToList();

            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add("Все категории");

            foreach (var category in categories.OrderBy(c => c.name))
            {
                CategoryFilter.Items.Add(category.name);
            }

            CategoryFilter.SelectedIndex = 0;

            _isReady = true;
            ApplyFilters();
        }

        private string GetImagePath(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                return null;
            }

            // Из БД получаем, например:
            // /images/products/xphone15.jpg
            // Ищем файл в Assets/Products внутри WPF-проекта.
            string fileName = Path.GetFileName(
                databasePath.Replace('\\', '/'));

            string fullPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Products",
                fileName);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            return fullPath;
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

            var filtered = result.ToList();

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