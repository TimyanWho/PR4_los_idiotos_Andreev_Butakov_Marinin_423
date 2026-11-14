using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class CheckoutPage : Page
    {
        private readonly List<CartItemModel> _items;
        private readonly bool _isDirectPurchase;
        private readonly int _directProductId;
        private readonly int _directQuantity;

        private decimal _total;
        private bool _isSubmitting;
        private bool _pickupPointsLoaded;

        public CheckoutPage(IEnumerable<CartItemModel> cartItems)
            : this(cartItems, null, 0)
        {
        }

        public CheckoutPage(ProductCardModel product, int quantity = 1)
            : this(CreateDirectPurchaseItems(product, quantity), product, quantity)
        {
        }

        private CheckoutPage(
            IEnumerable<CartItemModel> cartItems,
            ProductCardModel directProduct,
            int directQuantity)
        {
            InitializeComponent();

            _items = (cartItems ?? Enumerable.Empty<CartItemModel>())
                .Where(item => item != null && item.Quantity > 0)
                .ToList();

            _isDirectPurchase = directProduct != null;
            _directProductId = directProduct == null ? 0 : directProduct.Id;
            _directQuantity = directQuantity;

            CheckoutItemsList.ItemsSource = _items;

            _total = _items.Sum(item => item.LineTotal);

            ItemsCountText.Text =
                "Товаров: " + _items.Sum(item => item.Quantity);

            TotalText.Text = _total.ToString("N2") + " ₽";

            EmptyItemsText.Visibility = _items.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            DeliveryRadioButton.IsEnabled = _total > 600m;

            if (_total <= 600m)
            {
                PickupRadioButton.IsChecked = true;

                StatusText.Text =
                    "При заказе на сумму 600 ₽ или меньше доступен только самовывоз.";
            }

            if (_items.Count == 0)
            {
                PlaceOrderButton.IsEnabled = false;
                StatusText.Text = "Нельзя оформить пустой заказ.";
            }

            Loaded += Page_Loaded;
        }

        private static IEnumerable<CartItemModel> CreateDirectPurchaseItems(
            ProductCardModel product,
            int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            return new List<CartItemModel>
            {
                new CartItemModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    UnitPrice = product.Price,
                    StockQuantity = product.StockQuantity,
                    Quantity = quantity
                }
            };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_pickupPointsLoaded)
            {
                return;
            }

            if (!ApiService.Instance.IsAuthenticated)
            {
                StatusText.Text =
                    "Войдите в аккаунт, чтобы оформить заказ.";

                PlaceOrderButton.IsEnabled = false;
                return;
            }

            _pickupPointsLoaded = true;
            PickupStatusText.Text = "Загружаем список пунктов выдачи...";

            try
            {
                IReadOnlyList<PickupPointOptionDto> points =
                    await ApiService.Instance.Orders.GetPickupPointsAsync();

                PickupPointComboBox.ItemsSource = points;
                PickupPointComboBox.SelectedIndex =
                    points.Count > 0 ? 0 : -1;

                PickupStatusText.Text = points.Count > 0
                    ? "Выберите удобный пункт выдачи."
                    : "Пункты выдачи не найдены. Оформление самовывоза пока невозможно.";
            }
            catch (Exception ex)
            {
                PickupStatusText.Text =
                    "Не удалось загрузить ПВЗ: " + ex.Message;
            }
        }

        private void DeliveryMethod_Changed(
            object sender,
            RoutedEventArgs e)
        {
            bool useDelivery = DeliveryRadioButton.IsChecked == true;

            PickupPanel.Visibility = useDelivery
                ? Visibility.Collapsed
                : Visibility.Visible;

            DeliveryPanel.Visibility = useDelivery
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void PlaceOrder_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_isSubmitting || _items.Count == 0)
            {
                return;
            }

            if (!ApiService.Instance.IsAuthenticated)
            {
                StatusText.Text = "Сначала войдите в аккаунт.";
                return;
            }

            string deliveryType;
            string deliveryAddress = null;
            int? pickupPointId = null;

            if (DeliveryRadioButton.IsChecked == true)
            {
                if (_total <= 600m)
                {
                    StatusText.Text =
                        "Доставка доступна только при сумме заказа больше 600 ₽.";
                    return;
                }

                deliveryAddress = DeliveryAddressBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    StatusText.Text = "Укажите адрес доставки.";
                    DeliveryAddressBox.Focus();
                    return;
                }

                deliveryType = "Доставка";
            }
            else
            {
                PickupPointOptionDto point =
                    PickupPointComboBox.SelectedItem as PickupPointOptionDto;

                if (point == null)
                {
                    StatusText.Text = "Выберите пункт выдачи заказов.";
                    return;
                }

                deliveryType = "Самовывоз";
                pickupPointId = point.Id;
            }

            _isSubmitting = true;
            PlaceOrderButton.IsEnabled = false;
            PlaceOrderButton.Content = "Оформляем заказ...";
            StatusText.Text = "Отправляем заказ на сервер...";

            try
            {
                OrderCreatedResultDto result;

                if (_isDirectPurchase)
                {
                    result = await ApiService.Instance.Orders.CreateDirectOrderAsync(
                        _directProductId,
                        _directQuantity,
                        deliveryType,
                        deliveryAddress,
                        pickupPointId);
                }
                else
                {
                    result = await ApiService.Instance.Orders.CreateOrderFromCartAsync(
                        deliveryType,
                        deliveryAddress,
                        pickupPointId);
                }

                MessageBox.Show(
                    "Заказ №" + result.Id +
                    " успешно оформлен.\nСумма: " +
                    result.Total.ToString("N2") + " ₽",
                    "Заказ оформлен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NavigationService?.Navigate(new OrdersPage());
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    "Не удалось оформить заказ: " + ex.Message;
            }
            finally
            {
                _isSubmitting = false;
                PlaceOrderButton.IsEnabled = _items.Count > 0;
                PlaceOrderButton.Content = "Подтвердить заказ";
            }
        }

        private void BackToCart_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService?.Navigate(new CartPage());
            }
        }
    } 
}