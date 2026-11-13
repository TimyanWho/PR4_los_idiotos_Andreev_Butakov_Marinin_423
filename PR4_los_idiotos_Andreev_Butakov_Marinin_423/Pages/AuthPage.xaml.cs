using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;


namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages
{

    public partial class AuthPage : Page
    {
        private readonly IAuthApiClient _apiClient;
        private bool _isRegisterMode;

        public event EventHandler AuthenticationCompleted;

        public AuthPage()
        : this(ApiService.Instance)
        {
        }


        public AuthPage(IAuthApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
        }

        private void LoginMode_Click(object sender, RoutedEventArgs e)
        {
            SetMode(false);
        }

        private void RegisterMode_Click(object sender, RoutedEventArgs e)
        {
            SetMode(true);
        }

        private void SetMode(bool isRegisterMode)
        {
            _isRegisterMode = isRegisterMode;
            RepeatPasswordPanel.Visibility = isRegisterMode ? Visibility.Visible : Visibility.Collapsed;
            SubmitButton.Content = isRegisterMode ? "Создать аккаунт" : "Войти";
            SubtitleText.Text = isRegisterMode
                ? "Создайте аккаунт, чтобы пользоваться корзиной и заказами."
                : "Войдите в аккаунт, чтобы продолжить.";
            LoginModeButton.Background = isRegisterMode
                ? Brushes.White
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAF0FF"));
            LoginModeButton.Foreground = isRegisterMode
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#667085"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#244A9B"));
            RegisterModeButton.Background = isRegisterMode
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAF0FF"))
                : Brushes.White;
            RegisterModeButton.Foreground = isRegisterMode
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#244A9B"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#667085"));
            StatusText.Text = string.Empty;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                StatusText.Text = "Введите логин.";
                LoginBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                StatusText.Text = "Введите пароль.";
                PasswordBox.Focus();
                return;
            }

            if (_isRegisterMode && password != RepeatPasswordBox.Password)
            {
                StatusText.Text = "Пароли не совпадают.";
                RepeatPasswordBox.Focus();
                return;
            }

            if (_apiClient == null)
            {
                StatusText.Text = "Форма готова. Подключение к API авторизации добавим после согласования маршрутов.";
                return;
            }

            SubmitButton.IsEnabled = false;
            StatusText.Text = string.Empty;
            try
            {
                var succeeded = _isRegisterMode
                    ? await _apiClient.RegisterAsync(login, password)
                    : await _apiClient.LoginAsync(login, password);

                if (!succeeded)
                {
                    StatusText.Text = _isRegisterMode
                        ? "Не удалось создать аккаунт. Проверьте данные и повторите попытку."
                        : "Не удалось войти. Проверьте логин и пароль.";
                    return;
                }

                StatusText.Text = _isRegisterMode ? "Аккаунт создан." : "Вход выполнен.";
                AuthenticationCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                StatusText.Text = "Не удалось связаться с API. Проверьте подключение и повторите попытку.";
            }
            finally
            {
                SubmitButton.IsEnabled = true;
            }
        }

        private void BackToCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}