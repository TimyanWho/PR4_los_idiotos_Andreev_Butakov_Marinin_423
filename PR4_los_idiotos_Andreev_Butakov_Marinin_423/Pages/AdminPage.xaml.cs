using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class AdminPage : Page
    {
        private List<AdminProductModel> _products =
            new List<AdminProductModel>();

        private List<Categories> _categories =
            new List<Categories>();

        private List<PickupPointOptionDto> _pickupPoints =
            new List<PickupPointOptionDto>();

        private List<AdminOrderModel> _orders =
            new List<AdminOrderModel>();

        private List<OrderStatusOption> _orderStatuses =
            new List<OrderStatusOption>();

        private List<AdminReviewModel> _reviews =
            new List<AdminReviewModel>();

        private AdminProductModel? _selectedProduct;
        private Categories? _selectedCategory;
        private PickupPointOptionDto? _selectedPickupPoint;
        private AdminOrderModel? _selectedOrder;
        private AdminReviewModel? _selectedReview;

        private bool _isLoading;

        public AdminPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ApiService.Instance.IsAuthenticated ||
                !ApiService.Instance.IsAdministrator)
            {
                StatusText.Text =
                    "Доступ запрещён. Войдите под учётной записью администратора.";

                AdminTabs.IsEnabled = false;
                return;
            }

            await LoadAllAsync();
        }

        // Общая загрузка всех данных панели.
        private async Task LoadAllAsync()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;
            StatusText.Text = "Загружаем данные...";

            try
            {
                await LoadCategoriesAsync();
                await LoadProductsAsync();
                await LoadPickupPointsAsync();
                await LoadOrderStatusesAsync();
                await LoadOrdersAsync();

                if (ReviewProductComboBox.Items.Count > 0 &&
                    ReviewProductComboBox.SelectedIndex < 0)
                {
                    ReviewProductComboBox.SelectedIndex = 0;
                }

                await LoadReviewsAsync();

                StatusText.Text =
                    "Данные успешно загружены. Изменения сохраняются в базе данных через API.";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    "Не удалось загрузить данные: " + ex.Message;
            }
            finally
            {
                _isLoading = false;
            }
        }

        // -------------------- ЗАГРУЗКА ДАННЫХ --------------------

        private async Task LoadCategoriesAsync()
        {
            IReadOnlyList<Categories> result =
                await ApiService.Instance.Catalog.GetCategoriesAsync();

            _categories = result == null
                ? new List<Categories>()
                : result.ToList();

            CategoriesList.ItemsSource = null;
            CategoriesList.ItemsSource = _categories;

            ProductCategoryComboBox.ItemsSource = null;
            ProductCategoryComboBox.ItemsSource = _categories;
        }

        private async Task LoadProductsAsync()
        {
            _products =
                await ApiService.Instance.Catalog.GetAdminProductsAsync();

            ProductsGrid.ItemsSource = null;
            ProductsGrid.ItemsSource = _products;

            ReviewProductComboBox.ItemsSource = null;
            ReviewProductComboBox.ItemsSource = _products;
        }

        private async Task LoadPickupPointsAsync()
        {
            _pickupPoints =
                await ApiService.Instance.Orders.GetAdminPickupPointsAsync();

            PickupPointsList.ItemsSource = null;
            PickupPointsList.ItemsSource = _pickupPoints;
        }

        private async Task LoadOrderStatusesAsync()
        {
            _orderStatuses =
                await ApiService.Instance.Orders.GetOrderStatusesAsync();

            OrderStatusComboBox.ItemsSource = null;
            OrderStatusComboBox.ItemsSource = _orderStatuses;
        }

        private async Task LoadOrdersAsync()
        {
            _orders =
                await ApiService.Instance.Orders.GetAdminOrdersAsync();

            OrdersGrid.ItemsSource = null;
            OrdersGrid.ItemsSource = _orders;
        }

        private async Task LoadReviewsAsync()
        {
            AdminProductModel? product =
                ReviewProductComboBox.SelectedItem as AdminProductModel;

            _selectedReview = null;

            if (product == null)
            {
                _reviews.Clear();
                ReviewsGrid.ItemsSource = null;
                return;
            }

            _reviews =
                await ApiService.Instance.ProductDetails.GetAdminReviewsAsync(product.Id);

            ReviewsGrid.ItemsSource = null;
            ReviewsGrid.ItemsSource = _reviews;
        }

        private async void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            await LoadAllAsync();
        }

        // -------------------- ТОВАРЫ --------------------

        // Выбор товара в таблице.
        private void ProductsGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            _selectedProduct =
                ProductsGrid.SelectedItem as AdminProductModel;

            if (_selectedProduct == null)
            {
                return;
            }

            ProductNameBox.Text = _selectedProduct.Name;

            ProductPriceBox.Text =
                _selectedProduct.Price.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture);

            ProductDescriptionBox.Text =
                _selectedProduct.Description;

            ProductImageBox.Text =
                _selectedProduct.ImagePath;

            ProductStockBox.Text =
                _selectedProduct.StockQuantity.ToString();

            ProductPackageBox.Text =
                _selectedProduct.ItemsPerPackage.HasValue
                    ? _selectedProduct.ItemsPerPackage.Value.ToString()
                    : "";

            ProductAvailableCheckBox.IsChecked =
                _selectedProduct.IsAvailable;

            ProductCategoryComboBox.SelectedValue =
                _selectedProduct.CategoryId;
        }

        // Добавление нового товара или сохранение изменений.
        private async void SaveProduct_Click(
            object sender,
            RoutedEventArgs e)
        {
            string name = ProductNameBox.Text.Trim();
            string description = ProductDescriptionBox.Text.Trim();
            string imagePath = ProductImageBox.Text.Trim();

            Categories? category =
                ProductCategoryComboBox.SelectedItem as Categories;

            decimal price;
            int stock;
            int? package = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                StatusText.Text = "Укажите название товара.";
                ProductNameBox.Focus();
                return;
            }

            if (!TryParsePrice(ProductPriceBox.Text, out price) ||
                price < 0)
            {
                StatusText.Text = "Укажите корректную цену товара.";
                ProductPriceBox.Focus();
                return;
            }

            if (category == null)
            {
                StatusText.Text =
                    "Сначала выберите категорию товара.";
                return;
            }

            if (!int.TryParse(ProductStockBox.Text.Trim(), out stock) ||
                stock < 0)
            {
                StatusText.Text =
                    "Количество на складе должно быть целым числом не меньше нуля.";

                ProductStockBox.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(ProductPackageBox.Text))
            {
                int packageValue;

                if (!int.TryParse(
                        ProductPackageBox.Text.Trim(),
                        out packageValue) ||
                    packageValue < 1)
                {
                    StatusText.Text =
                        "Количество в упаковке должно быть целым числом больше нуля.";

                    ProductPackageBox.Focus();
                    return;
                }

                package = packageValue;
            }

            AdminProductModel model = new AdminProductModel
            {
                Name = name,
                Price = price,
                Description = description,
                CategoryId = category.Id,
                Category = category.Name,
                ImagePath = imagePath,
                StockQuantity = stock,
                ItemsPerPackage = package,
                IsAvailable = ProductAvailableCheckBox.IsChecked == true
            };

            try
            {
                if (_selectedProduct == null)
                {
                    await ApiService.Instance.Catalog.CreateProductAsync(model);
                    StatusText.Text = "Товар успешно добавлен.";
                }
                else
                {
                    model.Id = _selectedProduct.Id;

                    await ApiService.Instance.Catalog.UpdateProductAsync(
                        _selectedProduct.Id,
                        model);
                    StatusText.Text = "Изменения товара сохранены.";
                }
                await LoadProductsAsync();
                ClearProductForm();
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось сохранить товар: " + ex.Message;
            }
        }
        private void ClearProductForm_Click(
        object sender,
        RoutedEventArgs e)
        {
            ClearProductForm();
        }
        private void ClearProductForm()
        {
            _selectedProduct = null;
            ProductsGrid.SelectedItem = null;
            ProductNameBox.Clear();
            ProductPriceBox.Clear();
            ProductDescriptionBox.Clear();
            ProductImageBox.Clear();
            ProductStockBox.Text = "0";
            ProductPackageBox.Clear();
            ProductAvailableCheckBox.IsChecked = true;
            ProductCategoryComboBox.SelectedIndex =
            _categories.Count > 0 ? 0 : -1;
        }
        // -------------------- КАТЕГОРИИ --------------------
        private void CategoriesList_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
        {
            _selectedCategory =
            CategoriesList.SelectedItem as Categories;
            CategoryNameBox.Text =
            _selectedCategory == null
            ? ""
            : _selectedCategory.Name;
        }
        private async void SaveCategory_Click(
        object sender,
        RoutedEventArgs e)
        {
            string name = CategoryNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                StatusText.Text =
                "Введите название категории.";
                CategoryNameBox.Focus();
                return;
            }
            try
            {
                if (_selectedCategory == null)
                {
                    await ApiService.Instance.Catalog.CreateCategoryAsync(name);
                    StatusText.Text =
                    "Категория добавлена.";
                }
                else
                {
                    await ApiService.Instance.Catalog.UpdateCategoryAsync(
                    _selectedCategory.Id,
                    name);
                    StatusText.Text =
                    "Категория переименована.";
                }
                await LoadCategoriesAsync();
                ClearCategoryForm();
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось сохранить категорию: " + ex.Message;
            }
        }
        private void ClearCategory_Click(
        object sender,
        RoutedEventArgs e)
        {
            ClearCategoryForm();
        }
        private void ClearCategoryForm()
        {
            _selectedCategory = null;
            CategoriesList.SelectedItem = null;
            CategoryNameBox.Clear();
        }
        // -------------------- ПВЗ --------------------
        private void PickupPointsList_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
        {
            _selectedPickupPoint =
            PickupPointsList.SelectedItem as PickupPointOptionDto;
            PickupNameBox.Text =
            _selectedPickupPoint == null
            ? ""
            : _selectedPickupPoint.Name;
            PickupAddressBox.Text =
            _selectedPickupPoint == null
            ? ""
            : _selectedPickupPoint.Address;
        }
        private async void SavePickupPoint_Click(
        object sender,
        RoutedEventArgs e)
        {
            string name = PickupNameBox.Text.Trim();
            string address = PickupAddressBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(address))
            {
                StatusText.Text = "Укажите адрес ПВЗ.";
                PickupAddressBox.Focus();
                return;
            }
            try
            {
                if (_selectedPickupPoint == null)
                {
                    await ApiService.Instance.Orders.CreatePickupPointAsync(
                    name,
                    address);
                    StatusText.Text = "Пункт выдачи добавлен.";
                }
                else
                {
                    await ApiService.Instance.Orders.UpdatePickupPointAsync(
                    _selectedPickupPoint.Id,
                    name,
                    address);
                    StatusText.Text =
                    "Данные пункта выдачи обновлены.";
                }
                await LoadPickupPointsAsync();
                ClearPickupPointForm();
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось сохранить ПВЗ: " + ex.Message;
            }
        }
        private async void DeletePickupPoint_Click(
        object sender,
        RoutedEventArgs e)
        {
            if (_selectedPickupPoint == null)
            {
                StatusText.Text =
                "Сначала выберите пункт выдачи.";
                return;
            }
            MessageBoxResult answer = MessageBox.Show(
            "Удалить выбранный пункт выдачи?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                await ApiService.Instance.Orders.DeletePickupPointAsync(
                _selectedPickupPoint.Id);
                await LoadPickupPointsAsync();
                ClearPickupPointForm();
                StatusText.Text = "Пункт выдачи удалён.";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось удалить ПВЗ: " + ex.Message;
            }
        }
        private void ClearPickupPoint_Click(
        object sender,
        RoutedEventArgs e)
        {
            ClearPickupPointForm();
        }
        private void ClearPickupPointForm()
        {
            _selectedPickupPoint = null;
            PickupPointsList.SelectedItem = null;
            PickupNameBox.Clear();
            PickupAddressBox.Clear();
        }
        // -------------------- ЗАКАЗЫ --------------------
        private void OrdersGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
        {
            _selectedOrder =
            OrdersGrid.SelectedItem as AdminOrderModel;
            if (_selectedOrder == null)
            {
                OrderStatusComboBox.SelectedIndex = -1;
                return;
            }
            OrderStatusComboBox.SelectedItem =
            _orderStatuses.FirstOrDefault(status =>
            string.Equals(
            status.Name,
            _selectedOrder.Status,
            StringComparison.OrdinalIgnoreCase));
        }
        private async void UpdateOrderStatus_Click(
        object sender,
        RoutedEventArgs e)
        {
            OrderStatusOption? status =
            OrderStatusComboBox.SelectedItem as OrderStatusOption;
            if (_selectedOrder == null || status == null)
            {
                StatusText.Text =
                "Выберите заказ и новый статус.";
                return;
            }
            int orderId = _selectedOrder.Id;
            try
            {
                await ApiService.Instance.Orders.UpdateOrderStatusAsync(
                orderId,
                status.Id);
                await LoadOrdersAsync();
                StatusText.Text =
                "Статус заказа №" + orderId + " обновлён.";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось изменить статус заказа: " + ex.Message;
            }
        }
        // -------------------- ОТЗЫВЫ --------------------
        private async void ReviewProductComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _isLoading)
            {
                return;
            }
            try
            {
                await LoadReviewsAsync();
                StatusText.Text = _reviews.Count == 0
                ? "Для выбранного товара отзывов пока нет."
                : "Загружено отзывов: " + _reviews.Count;
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось загрузить отзывы: " + ex.Message;
            }
        }
        private void ReviewsGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
        {
            _selectedReview =
            ReviewsGrid.SelectedItem as AdminReviewModel;
        }
        private async void DeleteReview_Click(
        object sender,
        RoutedEventArgs e)
        {
            if (_selectedReview == null)
            {
                StatusText.Text =
                "Выберите отзыв для удаления.";
                return;
            }
            MessageBoxResult answer = MessageBox.Show(
            "Удалить выбранный отзыв?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
            int reviewId = _selectedReview.Id;
            try
            {
                await ApiService.Instance.ProductDetails.DeleteReviewAsync(reviewId);
                await LoadReviewsAsync();
                StatusText.Text = "Отзыв удалён.";
            }
            catch (Exception ex)
            {
                StatusText.Text =
                "Не удалось удалить отзыв: " + ex.Message;
            }
        }
        // Разбор цены с поддержкой запятой и точки.
        private bool TryParsePrice(string text, out decimal price)
        {
            string value = (text ?? "").Trim().Replace(" ", "");
            if (decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out price))
            {
                return true;
            }
            value = value.Replace(',', '.');
            return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out price);
        }
    }
}
