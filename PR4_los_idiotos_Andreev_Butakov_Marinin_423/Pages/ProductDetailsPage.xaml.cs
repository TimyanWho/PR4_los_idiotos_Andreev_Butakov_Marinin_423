using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{
    public partial class ProductDetailsPage : Page
    {
        private readonly ProductCardModel _product;
        private readonly IProductDetailsApiClient _apiClient;
        private bool _isLoadingReviews;
        private bool _reviewsLoaded;

        public ProductDetailsPage(ProductCardModel product)
            : this(product, null)
        {
        }

        public ProductDetailsPage(
            ProductCardModel product,
            IProductDetailsApiClient apiClient)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            InitializeComponent();

            _product = product;
            _apiClient = apiClient;
            DataContext = product;

            bool canPurchase = product.IsAvailable &&
                               product.StockQuantity > 0;
            PurchaseActionsPanel.IsEnabled = canPurchase;

            RetryReviewsButton.Visibility = _apiClient == null
                ? Visibility.Collapsed
                : Visibility.Visible;

            UpdateReviewAccess();
            ActionStatusText.Text = _apiClient == null
                ? "Корзина, избранное и покупка подключаются через API."
                : "";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateReviewAccess();

            if (_reviewsLoaded || _isLoadingReviews)
            {
                return;
            }

            if (_apiClient == null)
            {
                ReviewsStatusText.Text =
                    "Отзывы будут загружаться из API; для их чтения вход не потребуется.";
                return;
            }

            await LoadReviewsAsync();
        }

        private void UpdateReviewAccess()
        {
            bool isAuthenticated = _apiClient != null &&
                                   _apiClient.IsAuthenticated;

            ReviewEditorPanel.Visibility = isAuthenticated
                ? Visibility.Visible
                : Visibility.Collapsed;

            ReviewAccessHint.Visibility = isAuthenticated
                ? Visibility.Collapsed
                : Visibility.Visible;

            ReviewAccessHint.Text = _apiClient == null
                ? "Форма отправки отзыва появится после подключения API. Отзывы будут доступны гостям."
                : "Войдите в аккаунт, чтобы поставить оценку и оставить отзыв.";
        }

        private async Task LoadReviewsAsync()
        {
            if (_apiClient == null || _isLoadingReviews)
            {
                return;
            }

            _isLoadingReviews = true;
            RetryReviewsButton.IsEnabled = false;
            ReviewsStatusText.Text = "Загружаем отзывы...";

            try
            {
                IReadOnlyList<ProductReviewModel> reviews =
                    await _apiClient.GetProductReviewsAsync(_product.Id);

                List<ProductReviewModel> reviewList =
                    (reviews ?? new List<ProductReviewModel>())
                    .Where(review => review != null)
                    .OrderByDescending(review => review.CreatedAt)
                    .ToList();

                ReviewsItemsControl.ItemsSource = reviewList;
                ReviewsStatusText.Text = reviewList.Count == 0
                    ? "Пока нет отзывов."
                    : "Отзывов: " + reviewList.Count;

                _reviewsLoaded = true;
            }
            catch
            {
                ReviewsStatusText.Text =
                    "Не удалось загрузить отзывы. Проверьте подключение к API.";
                ReviewsItemsControl.ItemsSource =
                    new List<ProductReviewModel>();
            }
            finally
            {
                _isLoadingReviews = false;
                RetryReviewsButton.IsEnabled = true;
            }
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (NavigationService == null)
            {
                return;
            }

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new CatalogPage());
            }
        }

        private async void AddToFavorites_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunAuthenticatedActionAsync(
                () => _apiClient.AddToFavoritesAsync(_product.Id),
                "Товар добавлен в избранное.");
        }

        private async void AddToCart_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunAuthenticatedActionAsync(
                () => _apiClient.AddToCartAsync(_product.Id, 1),
                "Товар добавлен в корзину.",
                requireAvailableProduct: true);
        }

        private void BuyNow_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!EnsureAuthenticated())
            {
                return;
            }

            if (!IsProductAvailable())
            {
                return;
            }

            ActionStatusText.Text =
                "Оформление покупки появится на странице заказа с выбором доставки или ПВЗ.";
        }

        private async Task RunAuthenticatedActionAsync(
            Func<Task> request,
            string successMessage,
            bool requireAvailableProduct = false)
        {
            if (!EnsureAuthenticated())
            {
                return;
            }

            if (requireAvailableProduct && !IsProductAvailable())
            {
                return;
            }

            try
            {
                await request();
                ActionStatusText.Text = successMessage;
            }
            catch (UnauthorizedAccessException)
            {
                ActionStatusText.Text =
                    "Сначала войдите в аккаунт, чтобы выполнить это действие.";
            }
            catch
            {
                ActionStatusText.Text =
                    "Не удалось выполнить действие через API. Попробуйте позже.";
            }
        }

        private bool EnsureAuthenticated()
        {
            if (_apiClient == null)
            {
                ActionStatusText.Text =
                    "Это действие станет доступно после подключения API.";
                return false;
            }

            if (!_apiClient.IsAuthenticated)
            {
                ActionStatusText.Text =
                    "Сначала войдите в аккаунт, чтобы выполнить это действие.";
                return false;
            }

            return true;
        }

        private bool IsProductAvailable()
        {
            if (_product.IsAvailable && _product.StockQuantity > 0)
            {
                return true;
            }

            ActionStatusText.Text =
                "Этот товар сейчас нельзя купить.";
            return false;
        }

        private async void RetryReviews_Click(
            object sender,
            RoutedEventArgs e)
        {
            await LoadReviewsAsync();
        }

        private async void SubmitReview_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!EnsureAuthenticated())
            {
                return;
            }

            ComboBoxItem selectedRating =
                ReviewRatingBox.SelectedItem as ComboBoxItem;

            int rating;
            if (selectedRating == null ||
                !int.TryParse(selectedRating.Tag as string, out rating))
            {
                ActionStatusText.Text = "Выберите оценку от 1 до 5.";
                return;
            }

            string text = ReviewTextBox.Text.Trim();
            if (text.Length == 0)
            {
                ActionStatusText.Text = "Напишите текст отзыва.";
                return;
            }

            try
            {
                await _apiClient.SubmitReviewAsync(
                    _product.Id,
                    rating,
                    text);

                ReviewTextBox.Clear();
                ActionStatusText.Text = "Отзыв отправлен.";
                _reviewsLoaded = false;
                await LoadReviewsAsync();
            }
            catch (UnauthorizedAccessException)
            {
                ActionStatusText.Text =
                    "Войдите в аккаунт, чтобы оставить отзыв.";
            }
            catch
            {
                ActionStatusText.Text =
                    "Не удалось отправить отзыв через API. Попробуйте позже.";
            }
        }
    }
}