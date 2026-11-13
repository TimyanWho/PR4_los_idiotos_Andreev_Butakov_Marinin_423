using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
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
        private readonly IOrdersApiClient _ordersApiClient;
        private readonly ObservableCollection<OrderSummaryDto> _orders = new ObservableCollection<OrderSummaryDto>();

        public OrdersPage()
        : this(ApiService.Instance)
        {
        }


        public OrdersPage(IOrdersApiClient ordersApiClient)
        {
            InitializeComponent();
            _ordersApiClient = ordersApiClient;
            OrdersList.ItemsSource = _orders;
            Loaded += OrdersPage_Loaded;
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

    // Контракт для подключения клиента API, который реализует участник команды.
    public interface IOrdersApiClient
    {
        Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(bool newestFirst);
    }

    // Модели ответа WPF-клиента; API может преобразовать свои DTO в эти типы.
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string DeliverySummary { get; set; }
        public decimal TotalAmount { get; set; }
        public IReadOnlyList<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}