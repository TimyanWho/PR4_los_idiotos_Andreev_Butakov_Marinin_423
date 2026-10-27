using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ProductRow> _allProducts =
            new ObservableCollection<ProductRow>();

        private ICollectionView _productsView;

        public MainWindow()
        {
            InitializeComponent();

            ProductsGrid.ItemsSource = _allProducts;

            _productsView =
                CollectionViewSource.GetDefaultView(ProductsGrid.ItemsSource);

            LoadDemoProducts();
            LoadCategories();
            ApplyFilters();

            ResetProductDetails();
        }

        // ВРЕМЕННЫЕ ДАННЫЕ.
        // На втором этапе заменим этот метод загрузкой из БД.
        private void LoadDemoProducts()
        {
            _allProducts.Add(new ProductRow
            {
                Id = 1,
                Name = "Смартфон X-Phone 15",
                Category = "Электроника",
                Price = 79999m,
                Description =
                    "Флагманский смартфон с отличной камерой.",
                ImagePath = "/images/products/xphone15.jpg",
                StockQuantity = 50,
                ItemsPerPackage = 1,
                IsAvailable = true,
                Rating = 4.8m
            });

            _allProducts.Add(new ProductRow
            {
                Id = 2,
                Name = "Беспроводные наушники AirSound",
                Category = "Электроника",
                Price = 4990m,
                Description =
                    "Беспроводные наушники с активным шумоподавлением.",
                ImagePath = "/images/products/airsound.jpg",
                StockQuantity = 120,
                ItemsPerPackage = 1,
                IsAvailable = true,
                Rating = 5.0m
            });

            _allProducts.Add(new ProductRow
            {
                Id = 3,
                Name = "Футболка хлопковая Black",
                Category = "Одежда и обувь",
                Price = 1200m,
                Description =
                    "Базовая чёрная футболка из 100% хлопка.",
                ImagePath = "/images/products/tshirt_black.jpg",
                StockQuantity = 300,
                ItemsPerPackage = 1,
                IsAvailable = true,
                Rating = 4.0m
            });

            _allProducts.Add(new ProductRow
            {
                Id = 4,
                Name = "Книга «Изучаем SQL»",
                Category = "Книги",
                Price = 1500m,
                Description =
                    "Руководство по SQL для начинающих разработчиков.",
                ImagePath = "/images/products/sql_book.jpg",
                StockQuantity = 15,
                ItemsPerPackage = 1,
                IsAvailable = true,
                Rating = 5.0m
            });

            _allProducts.Add(new ProductRow
            {
                Id = 5,
                Name = "Набор чашек для чая",
                Category = "Дом и сад",
                Price = 2400m,
                Description =
                    "Фарфоровый набор чашек на шесть персон.",
                ImagePath = "/images/products/tea_set.jpg",
                StockQuantity = 40,
                ItemsPerPackage = 6,
                IsAvailable = true,
                Rating = 0m
            });

            _allProducts.Add(new ProductRow
            {
                Id = 6,
                Name = "Коврик для йоги нескользящий",
                Category = "Спорт и отдых",
                Price = 1800m,
                Description =
                    "Нескользящий коврик для фитнеса и занятий йогой.",
                ImagePath = "/images/products/yoga_mat.jpg",
                StockQuantity = 0,
                ItemsPerPackage = 1,
                IsAvailable = false,
                Rating = 0m
            });
        }

        private void LoadCategories()
        {
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add("Все категории");
            CategoryFilter.SelectedIndex = 0;

            var categories = _allProducts
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(category => category);

            foreach (string category in categories)
            {
                CategoryFilter.Items.Add(category);
            }
        }

        private void TextFilters_Changed(
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
            if (_productsView == null)
            {
                return;
            }

            string search = SearchBox.Text.Trim();

            string selectedCategory =
                CategoryFilter.SelectedItem == null
                    ? "Все категории"
                    : CategoryFilter.SelectedItem.ToString();

            decimal minPrice;
            decimal maxPrice;

            bool hasMinPrice = decimal.TryParse(
                MinPriceBox.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out minPrice);

            bool hasMaxPrice = decimal.TryParse(
                MaxPriceBox.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out maxPrice);

            bool onlyAvailable =
                AvailableOnlyCheckBox.IsChecked == true;

            _productsView.Filter = item =>
            {
                ProductRow product = item as ProductRow;

                if (product == null)
                {
                    return false;
                }

                // Поиск по названию.
                if (!string.IsNullOrWhiteSpace(search) &&
                    product.Name.IndexOf(
                        search,
                        StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }

                // Фильтр по категории.
                if (selectedCategory != "Все категории" &&
                    product.Category != selectedCategory)
                {
                    return false;
                }

                // Минимальная цена.
                if (hasMinPrice && product.Price < minPrice)
                {
                    return false;
                }

                // Максимальная цена.
                if (hasMaxPrice && product.Price > maxPrice)
                {
                    return false;
                }

                // В наличии и разрешён к покупке.
                if (onlyAvailable &&
                    (!product.IsAvailable || product.StockQuantity <= 0))
                {
                    return false;
                }

                return true;
            };

            _productsView.Refresh();

            ResultCountText.Text =
                "Найдено товаров: " +
                _productsView.Cast<ProductRow>().Count();
        }

        private void ClearFilters_Click(
            object sender,
            RoutedEventArgs e)
        {
            SearchBox.Clear();
            MinPriceBox.Clear();
            MaxPriceBox.Clear();

            AvailableOnlyCheckBox.IsChecked = false;
            CategoryFilter.SelectedIndex = 0;

            ApplyFilters();
        }

        private void ProductsGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ProductRow product = ProductsGrid.SelectedItem as ProductRow;

            if (product == null)
            {
                ResetProductDetails();
                return;
            }

            SelectedProductName.Text = product.Name;

            SelectedProductCategory.Text =
                "Категория: " + product.Category;

            SelectedProductPrice.Text =
                product.Price.ToString("N2") + " ₽";

            SelectedProductDescription.Text =
                string.IsNullOrWhiteSpace(product.Description)
                    ? "Описание отсутствует."
                    : product.Description;

            SelectedProductRating.Text =
                product.Rating > 0
                    ? "Средняя оценка: " +
                      product.Rating.ToString("0.0")
                    : "Средняя оценка: пока нет оценок";

            SelectedProductStock.Text =
                "Остаток на складе: " + product.StockQuantity;

            SelectedProductAvailability.Text =
                "Статус: " + product.AvailableText;

            SelectedProductAvailability.Foreground =
                product.IsAvailable && product.StockQuantity > 0
                    ? System.Windows.Media.Brushes.ForestGreen
                    : System.Windows.Media.Brushes.Firebrick;

            SelectedProductPackage.Text =
                "Количество в упаковке: " +
                (product.ItemsPerPackage.HasValue
                    ? product.ItemsPerPackage.Value.ToString()
                    : "не указано");

            // Пока отображаем путь к файлу.
            // Настоящие изображения подключим при интеграции каталога.
            SelectedProductImage.Text =
                string.IsNullOrWhiteSpace(product.ImagePath)
                    ? "Изображение отсутствует"
                    : product.ImagePath;
        }

        private void ResetProductDetails()
        {
            SelectedProductImage.Text =
                "Выберите товар в каталоге";

            SelectedProductName.Text = "Товар не выбран";
            SelectedProductCategory.Text = "Категория: не выбрана";
            SelectedProductPrice.Text = "0,00 ₽";

            SelectedProductDescription.Text =
                "Выберите товар, чтобы посмотреть его описание.";

            SelectedProductRating.Text =
                "Средняя оценка: нет данных";

            SelectedProductStock.Text = "Остаток: не указан";
            SelectedProductAvailability.Text = "Статус: не указан";

            SelectedProductPackage.Text =
                "Количество в упаковке: не указано";
        }

        private void Login_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "Окно авторизации добавим на следующем этапе.",
                "GMWOG",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Временная модель для отображения товаров.
        // Отдельный ProductRow.cs создавать не требуется.
        public class ProductRow
        {
            public int Id { get; set; }

            public string Name { get; set; } = "";

            public string Category { get; set; } = "";

            public decimal Price { get; set; }

            public string Description { get; set; } = "";

            public string ImagePath { get; set; } = "";

            public int StockQuantity { get; set; }

            public int? ItemsPerPackage { get; set; }

            public bool IsAvailable { get; set; }

            public decimal Rating { get; set; }

            public string AvailableText
            {
                get
                {
                    return IsAvailable && StockQuantity > 0
                        ? "В наличии"
                        : "Нет в наличии";
                }
            }
        }
    }
}