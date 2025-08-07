using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace automationApp;

public class EditUserDialog : ReadOnlyCustomDialogView
{
    private Entry entryLogin;
    private Entry entryPassword;
    private Button buttonSave;
    private readonly int userId;

    public string Login { get; set; }
    public string Password { get; set; }
    public TaskCompletionSource<bool> CompletionSource { get; private set; } = new();

    public EditUserDialog(int userId, string initialLogin, string initialPassword) : base(new Dictionary<string, bool>(), new List<string>())
    {
        this.userId = userId;

        // Создаем поля для ввода логина и пароля
        entryLogin = new Entry
        {
            Placeholder = "Логин",
            Text = initialLogin,
            Margin = new Thickness(0, 10, 0, 0),
            FontSize = 16,
            FontFamily = "Inter"
        };
        entryPassword = new Entry
        {
            Placeholder = "Пароль",
            IsPassword = true,
            Text = initialPassword,
            Margin = new Thickness(0, 10, 0, 0),
            FontSize = 16,
            FontFamily = "Inter"
        };

        // Создаем кнопку сохранения
        buttonSave = new Button
        {
            Text = "Сохранить",
            BackgroundColor = Color.FromArgb("#4B164C"),
            TextColor = Colors.White,
            CornerRadius = 10,
            Margin = new Thickness(0, 20, 0, 0),
            Command = new Command(async () => await SaveAsync())
        };

        // Добавляем элементы в стек
        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Children.Add(new Label { Text = "Редактирование пользователя", FontSize = 20, TextColor = Color.FromArgb("#4B164C"), HorizontalOptions = LayoutOptions.Center });
        stack.Children.Add(entryLogin);
        stack.Children.Add(entryPassword);
        stack.Children.Add(buttonSave);

        // Устанавливаем содержимое карточки
        var card = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 20,
            Padding = 20,
            WidthRequest = 350,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HasShadow = true,
            Content = stack
        };

        Content = card;
    }

    private async Task SaveAsync()
    {
        // Получаем значения полей
        Login = entryLogin.Text;
        Password = entryPassword.Text;

        // Обновляем данные пользователя в базе данных
        await SaveUserChangesToDatabase(userId, Login, Password);


        // Скрываем диалоговое окно
        Hide();
        CompletionSource.TrySetResult(true);

    }
    public Task ShowAsync()
    {
        this.IsVisible = true;
        this.Opacity = 1;
        return CompletionSource.Task; // ждать, пока пользователь сохранит
    }


    public void Hide()
    {
        this.IsVisible = false;
        this.Opacity = 0;

        if (this.Parent is Layout parentLayout)
        {
            parentLayout.Children.Remove(this); // удаляет диалог из MainLayout
        }
    }


    private async Task SaveUserChangesToDatabase(int userId, string login, string password)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "UPDATE Auth SET login = @Login, password = @Password WHERE user_id = @UserId";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Password", password);
                command.Parameters.AddWithValue("@UserId", userId);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}