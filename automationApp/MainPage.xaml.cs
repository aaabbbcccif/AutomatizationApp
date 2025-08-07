using System.Diagnostics;
using System.Data.SqlClient;
using Button = Microsoft.UI.Xaml.Controls.Button;


#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
#endif

namespace automationApp
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();


#if WINDOWS
            LoginButton.HandlerChanged += (s, e) =>
            {
                if (LoginButton.Handler?.PlatformView is Button platformButton)
                {
                    platformButton.PointerEntered += (sender, args) =>
                    {
                        LoginFrame.BackgroundColor = Color.FromArgb("#6C2B6F");
                    };

                    platformButton.PointerExited += (sender, args) =>
                    {
                        LoginFrame.BackgroundColor = Color.FromArgb("#4B164C");
                    };
                }
            };
#endif

#if WINDOWS
            this.HandlerChanged += MainPage_HandlerChanged;
#endif



            InitWindowsKeyHandling();



        }
#if WINDOWS
        private void MainPage_HandlerChanged(object sender, EventArgs e)
        {
            var nativeWindow = Microsoft.Maui.Controls.Application.Current.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

            if (nativeWindow != null && nativeWindow.Content is FrameworkElement rootElement)
            {
                rootElement.KeyDown += RootElement_KeyDown;
            }
        }

        private void RootElement_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.F1)
            {
                Debug.WriteLine("F1 pressed!");
                OpenHelpFile();
                e.Handled = true; // чтобы событие не пошло дальше
            }
        }

        private void OpenHelpFile()
        {
            string htmlFileName = "HelpSystem/HelpFile.html";
            string fullPath = Path.Combine(AppContext.BaseDirectory, htmlFileName);

            if (File.Exists(fullPath))
            {
                string fileUrl = $"file:///{fullPath.Replace("\\", "/")}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileUrl,
                    UseShellExecute = true
                });
            }
            else
            {
                Debug.WriteLine("Help file not found: " + fullPath);
            }
        }
#endif
        private void InitWindowsKeyHandling()
        {
#if WINDOWS
            // Получаем текущее MAUI окно и преобразуем его в native WinUI окно
            var nativeWindow = Microsoft.Maui.Controls.Application.Current.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;


            if (nativeWindow != null && nativeWindow.Content is FrameworkElement rootElement)
            {
                // Подписка на событие нажатия клавиши
                rootElement.KeyDown += (object sender, KeyRoutedEventArgs e) =>
                {
                    if (e.Key == Windows.System.VirtualKey.F1)
                    {
                        string htmlFileName = "HelpFile.html";
                        string fullPath = Path.Combine(AppContext.BaseDirectory, htmlFileName);

                        if (File.Exists(fullPath))
                        {
                            string fileUrl = $"file:///{fullPath.Replace("\\", "/")}";
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = fileUrl,
                                UseShellExecute = true
                            });
                        }
                    }
                };
            }
#endif
        }

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string login = LoginEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(login))
            {
                await DisplayAlert("Ошибка", "Логин не может быть пустым.", "ОК");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Пароль не может быть пустым.", "ОК");
                return;
            }

            // Проверка логина и пароля
            var user = ValidateLogin(login, password);

            if (user != null)
            {
                Preferences.Default.Set("CurrentUserId", user.UserId);

                // Получение роли пользователя
                string role = GetUserRole(user.UserId);

                // Добавление записи в таблицу UserActions
                await LogUserAction(user.UserId, "Вход пользователя", $"Вход пользователя с ID {user.UserId}", "Не прочитано");

                // Приветственное сообщение
                await DisplayAlert("Успешный вход", $"Добро пожаловать, {login}! Ваша роль: {role}", "ОК");

                // Переход на страницу в зависимости от роли
                switch (role)
                {
                    case "Админ":
                        await Navigation.PushAsync(new AdminPage(user.UserId));
                        break;
                    case "Исполнитель":
                        await Navigation.PushAsync(new IspolnitelPage(user.UserId));
                        break;
                    case "Кадровик":
                        await Navigation.PushAsync(new KadrovikPage(user.UserId));
                        break;
                    case "Руководитель подразделения":
                        await Navigation.PushAsync(new RukovoditelPodrazdelenia(user.UserId));
                        break;
                    default:
                        await DisplayAlert("Ошибка", "Неизвестная роль пользователя.", "ОК");
                        break;
                }
            }
            else
            {
                await DisplayAlert("Ошибка", "Неверный логин или пароль.", "ОК");
            }
        }

        private User ValidateLogin(string login, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, user_id FROM Auth WHERE login = @Login AND password = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = reader.GetInt32(0),
                                    UserId = reader.GetInt32(1)
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
            return null;
        }

        private string GetUserRole(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT role_name FROM UserRoles WHERE user_id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
            return null;
        }

        private async Task LogUserAction(int userId, string title, string action, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "INSERT INTO UserActions (user_id, title, action, action_date, status) VALUES (@UserId, @Title, @Action, @ActionDate, @Status)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@ActionDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Status", status);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
            }
        }

        bool isPasswordVisible = true;

        private void OnEyeTapped(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordEntry.IsPassword = !isPasswordVisible;
            EyeImage.Source = isPasswordVisible ? "eye_off.png" : "eye.png";
        }


    }

}