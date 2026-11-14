using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class OrdersPage : Page
    {
        private readonly ObservableCollection<OrderSummaryDto> _orders = new ObservableCollection<OrderSummaryDto>();
        private readonly IOrdersApiClient _ordersApiClient;

        public OrdersPage() : this(ApiService.Instance.Orders)
        {
        }

        public OrdersPage(IOrdersApiClient ordersApiClient)
        {
            InitializeComponent();
            _ordersApiClient = ordersApiClient;
            OrdersList.ItemsSource = _orders;
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync(IsNewestFirst());
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync(IsNewestFirst());
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && _ordersApiClient != null)
            {
                await LoadOrdersAsync(IsNewestFirst());
            }
        }

        private void OrdersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OrderSummaryDto selectedOrder = OrdersList.SelectedItem as OrderSummaryDto;
            if (selectedOrder != null)
            {
                SelectedOrderPanel.DataContext = selectedOrder;
            }
        }

        private bool IsNewestFirst()
        {
            ComboBoxItem selectedItem = SortComboBox.SelectedItem as ComboBoxItem;
            return selectedItem == null || string.Equals(selectedItem.Tag as string, "newest", StringComparison.OrdinalIgnoreCase);
        }

        private async Task LoadOrdersAsync(bool newestFirst)
        {
            if (!ApiService.Instance.IsAuthenticated)
            {
                ShowEmptyState(
                    "Требуется авторизация",
                    "Войдите в аккаунт, чтобы увидеть историю ваших заказов.");
                return;
            }

            if (_ordersApiClient == null)
            {
                ShowEmptyState(
                    "История заказов появится после подключения API",
                    "Здесь будут отображаться ваши покупки, статусы, способы получения и состав заказов.");
                return;
            }

            EmptyStatePanel.Visibility = Visibility.Visible;
            OrdersContentPanel.Visibility = Visibility.Collapsed;
            EmptyTitleText.Text = "Загружаем заказы…";
            EmptyDescriptionText.Text = "Получаем историю покупок.";

            try
            {
                IReadOnlyList<OrderSummaryDto> result = await _ordersApiClient.GetMyOrdersAsync(newestFirst);
                _orders.Clear();

                if (result != null)
                {
                    foreach (OrderSummaryDto order in result)
                    {
                        _orders.Add(order);
                    }
                }

                if (_orders.Count == 0)
                {
                    ShowEmptyState("Заказов пока нет", "Оформленные покупки появятся здесь.");
                    return;
                }

                OrdersContentPanel.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                OrdersList.SelectedIndex = 0;
            }
            catch (Exception)
            {
                _orders.Clear();
                ShowEmptyState("Не удалось загрузить заказы", "Проверьте подключение и повторите попытку.");
            }
        }

        private void ShowEmptyState(string title, string description)
        {
            OrdersContentPanel.Visibility = Visibility.Collapsed;
            EmptyStatePanel.Visibility = Visibility.Visible;
            EmptyTitleText.Text = title;
            EmptyDescriptionText.Text = description;
        }
    }
}