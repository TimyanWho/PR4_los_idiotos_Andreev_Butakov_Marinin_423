using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public class CartItemModel : INotifyPropertyChanged
    {
        private int _quantity;
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int Quantity
        {
            get { return _quantity; }
            set { if (_quantity == value) return; _quantity = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Quantity")); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LineTotal")); }
        }
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class CartPage : Page
    {
        private readonly ICartApiClient _client;
        private readonly ObservableCollection<CartItemModel> _items = new ObservableCollection<CartItemModel>();
        public event EventHandler CheckoutRequested;

        public CartPage()
        : this(ApiService.Instance.Cart)
        {
        }

        public CartPage(ICartApiClient client)
        {
            InitializeComponent();
            _client = client;
            CartItemsList.ItemsSource = _items;
            Loaded += async (s, e) => await LoadAsync();
            UpdateSummary();
        }

        private async Task LoadAsync()
        {
            if (_client == null) return;

            if (!ApiService.Instance.IsAuthenticated)
            {
                ApiStatusPanel.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                CartItemsList.Visibility = Visibility.Collapsed;
                ApiStatusText.Text = "Войдите в аккаунт, чтобы просматривать корзину.";
                return;
            }

            try
            {
                var result = await _client.GetItemsAsync();
                _items.Clear();
                if (result != null) foreach (var item in result) _items.Add(item);
                ApiStatusPanel.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                CartItemsList.Visibility = _items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                UpdateSummary();
            }
            catch
            {
                ApiStatusPanel.Visibility = Visibility.Visible;
                ApiStatusText.Text = "Не удалось загрузить корзину. Проверьте подключение к API.";
            }
        }


        private async void QuantityButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button == null ? null : button.DataContext as CartItemModel;
            if (item == null || _client == null) return;
            var quantity = item.Quantity + ((string)button.Tag == "increase" ? 1 : -1);
            if (quantity < 1 || (item.StockQuantity > 0 && quantity > item.StockQuantity)) return;
            if (await _client.UpdateQuantityAsync(item.ProductId, quantity)) { item.Quantity = quantity; UpdateSummary(); }
            else StatusText.Text = "Не удалось изменить количество.";
        }

        private async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button == null ? null : button.DataContext as CartItemModel;
            if (item == null || _client == null) return;
            if (await _client.RemoveItemAsync(item.ProductId))
            {
                _items.Remove(item);
                EmptyStatePanel.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                CartItemsList.Visibility = _items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                UpdateSummary();
            }
            else StatusText.Text = "Не удалось удалить товар.";
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0) return;
            CheckoutRequested?.Invoke(this, EventArgs.Empty);
            StatusText.Text = "Экран оформления заказа будет следующим этапом.";
        }

        private void BackToCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void UpdateSummary()
        {
            CartCountText.Text = _items.Sum(item => item.Quantity) + " товаров";
            TotalText.Text = _items.Sum(item => item.LineTotal).ToString("N2") + " ₽";
            CheckoutButton.IsEnabled = _client != null && _items.Count > 0;
        }
    }
}