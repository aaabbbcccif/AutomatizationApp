using System.Data.SqlClient;
using System.Diagnostics;

namespace automationApp;

public partial class AdminPage : ContentPage
{
    private ReadOnlyCustomDialogView readOnlyDialog;
    private readonly int currentUserId;

    public AdminPage(int currentUserId)
    {
        InitializeComponent();
        LoadUsers();
        this.currentUserId = currentUserId;
    }
    string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
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
    private async Task ShowReadOnlyDialog(Dictionary<string, bool> values, List<string> groupNames)
    {
        readOnlyDialog = new ReadOnlyCustomDialogView(values, groupNames);
        MainLayout.Children.Add(readOnlyDialog); // MainLayout — это Grid или StackLayout твоей страницы
        await readOnlyDialog.ShowAsync();
    }

    private async Task LoadUsers()
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = @"
            SELECT u.id, u.last_name, u.first_name, u.middle_name, u.phone_number, u.birth_date, u.department, u.division, u.position,
                   a.login, a.password, r.role_name
            FROM Users u
            LEFT JOIN Auth a ON u.id = a.user_id
            LEFT JOIN UserRoles r ON u.id = r.user_id";

        List<User> users = new List<User>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Last_name = reader["last_name"].ToString(),
                        First_name = reader["first_name"].ToString(),
                        Middle_name = reader["middle_name"].ToString(),
                        Phone_number = reader["phone_number"].ToString(),
                        Birth_date = reader["birth_date"].ToString(),
                        Department = reader["department"].ToString(),
                        Division = reader["division"].ToString(),
                        Position = reader["position"].ToString(),
                        Login = reader.IsDBNull(reader.GetOrdinal("login")) ? "" : reader["login"].ToString(),
                        Password = reader.IsDBNull(reader.GetOrdinal("password")) ? "" : reader["password"].ToString(),
                        Role_name = reader.IsDBNull(reader.GetOrdinal("role_name")) ? "" : reader["role_name"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                users.Add(new User
                {
                    Id = -1,
                    Last_name = "Ошибка",
                    First_name = ex.Message,
                    Middle_name = "",
                    Phone_number = "",
                    Birth_date = "",
                    Department = "",
                    Division = "",
                    Position = "",
                    Login = "",
                    Password = "",
                    Role_name = ""
                });
            }
        }

        MyCollectionView.ItemsSource = users;
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Получаем предыдущий и текущий выбранный элементы
        var previousItem = e.PreviousSelection.FirstOrDefault() as Frame;
        var currentItem = e.CurrentSelection.FirstOrDefault() as Frame;

        // Сбрасываем состояние предыдущего выбранного элемента
        if (previousItem != null)
        {
            VisualStateManager.GoToState(previousItem, "Normal");
        }

        // Применяем состояние к текущему выбранному элементу
        if (currentItem != null)
        {
            VisualStateManager.GoToState(currentItem, "Selected");
        }
    }


    private void OnServiceButtonClicked(object sender, EventArgs e)
    {

    }

    private async void OnSaveTapped(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            var dialog = new EditUserDialog(selectedUser.Id, selectedUser.Login, selectedUser.Password);
            dialog.Opacity = 0;
            dialog.IsVisible = false;
            MainLayout.Children.Add(dialog);

            await dialog.ShowAsync(); // ждём пока пользователь сохранит

            await DisplayAlert("Сохранено", "Данные сохранены", "ОК");
            await LoadUsers(); // теперь вызывается после закрытия диалога

        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }



    private async Task SaveUserChangesToDatabase(User user)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "UPDATE Auth SET login = @Login, password = @Password WHERE user_id = @UserId";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Login", user.Login);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@UserId", user.Id);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось обновить данные пользователя: {ex.Message}", "ОК");
            }
        }
    }
    private async void OnDeleteTapped(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            string message = "Вы уверенны, что хотите удалить этого пользователя? Это действие необратертурно.";
            bool result = await DisplayAlert("Подтверждение", message, "Да", "Нет");
            if (result)
            {
                await DeleteUserFromDatabase(selectedUser.Id);
                await LoadUsers();
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Пожалуйста, выберите пользователя для удаления", "ОК");
        }
    }

    private async Task DeleteUserFromDatabase(int userId)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";

        // Список таблиц, из которых нужно удалить записи
        string[] tablesToDeleteFrom = { "RightsRequest", "RightsExecution", "AdditionalFileServerRequest", "AdditionalFileServerExecution", "AdditionalDomainRequest", "AdditionalDomainExecution", "AdditionalBitrixRequest", "AdditionalBitrixExecution", "Additional1CRequest", "Additional1CExecution", "UserRoles", "Auth", "ApplicationStatus" };

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            try
            {
                await conn.OpenAsync();
                foreach (var table in tablesToDeleteFrom)
                {
                    string deleteQuery = $"DELETE FROM {table} WHERE user_id = @UserId";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@UserId", userId);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }
                }

                string deleteUserQuery = "DELETE FROM Users WHERE id = @UserId";
                using (SqlCommand deleteUserCmd = new SqlCommand(deleteUserQuery, conn))
                {
                    deleteUserCmd.Parameters.AddWithValue("@UserId", userId);
                    await deleteUserCmd.ExecuteNonQueryAsync();
                    await DisplayAlert("Успех", $"Пользователь с ID {userId} удален из всех таблиц", "ОК");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка при удалении пользователя: {ex.Message}", "ОК");
            }
        }
    }
    
    private void OnLoadTapped(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            int userId = selectedUser.Id;
            var rights = GetRightsRequest(userId);

            Device.BeginInvokeOnMainThread(() =>
            {
                right_email.BackgroundColor = rights["Email"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_email.TextColor = rights["Email"] ? Colors.White : Color.FromArgb("#4B164C");
                right_email.BorderColor = rights["Email"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_bitrix.BackgroundColor = rights["Bitrix"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_bitrix.TextColor = rights["Bitrix"] ? Colors.White : Color.FromArgb("#4B164C");
                right_bitrix.BorderColor = rights["Bitrix"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_domain.BackgroundColor = rights["Domain"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_domain.TextColor = rights["Domain"] ? Colors.White : Color.FromArgb("#4B164C");
                right_domain.BorderColor = rights["Domain"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_1c.BackgroundColor = rights["OneC"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_1c.TextColor = rights["OneC"] ? Colors.White : Color.FromArgb("#4B164C");
                right_1c.BorderColor = rights["OneC"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_elastix.BackgroundColor = rights["Asterisk"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_elastix.TextColor = rights["Asterisk"] ? Colors.White : Color.FromArgb("#4B164C");
                right_elastix.BorderColor = rights["Asterisk"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_vpn.BackgroundColor = rights["VPN"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_vpn.TextColor = rights["VPN"] ? Colors.White : Color.FromArgb("#4B164C");
                right_vpn.BorderColor = rights["VPN"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_teflex.BackgroundColor = rights["Telex"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_teflex.TextColor = rights["Telex"] ? Colors.White : Color.FromArgb("#4B164C");
                right_teflex.BorderColor = rights["Telex"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_jira.BackgroundColor = rights["Jira"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_jira.TextColor = rights["Jira"] ? Colors.White : Color.FromArgb("#4B164C");
                right_jira.BorderColor = rights["Jira"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_radius.BackgroundColor = rights["Radius"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_radius.TextColor = rights["Radius"] ? Colors.White : Color.FromArgb("#4B164C");
                right_radius.BorderColor = rights["Radius"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_google.BackgroundColor = rights["Google"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_google.TextColor = rights["Google"] ? Colors.White : Color.FromArgb("#4B164C");
                right_google.BorderColor = rights["Google"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_phonebook.BackgroundColor = rights["PhoneBook"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_phonebook.TextColor = rights["PhoneBook"] ? Colors.White : Color.FromArgb("#4B164C");
                right_phonebook.BorderColor = rights["PhoneBook"] ? Colors.Transparent : Color.FromArgb("#4B164C");

                right_fileserver.BackgroundColor = rights["FileServer"] ? Color.FromArgb("#4B164C") : Colors.White;
                right_fileserver.TextColor = rights["FileServer"] ? Colors.White : Color.FromArgb("#4B164C");
                right_fileserver.BorderColor = rights["FileServer"] ? Colors.Transparent : Color.FromArgb("#4B164C");
            });
        }
        else
        {
            DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }

    private Dictionary<string, bool> GetRightsRequest(int userId)
    {
        Dictionary<string, bool> rights = new Dictionary<string, bool>
    {
        { "Email", false },
        { "Bitrix", false },
        { "Domain", false },
        { "OneC", false },
        { "VPN", false },
        { "Telex", false },
        { "Jira", false },
        { "Radius", false },
        { "Google", false },
        { "PhoneBook", false },
        { "Asterisk", false },
        { "FileServer", false }
    };

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "SELECT email, bitrix, domain, c1, vpn, teflex, jira, radius, google, phonebook, atc, fileserver FROM RightsRequest WHERE user_id = @UserID";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rights["Email"] = Convert.ToBoolean(reader["email"]);
                        rights["Bitrix"] = Convert.ToBoolean(reader["bitrix"]);
                        rights["Domain"] = Convert.ToBoolean(reader["domain"]);
                        rights["OneC"] = Convert.ToBoolean(reader["c1"]);
                        rights["VPN"] = Convert.ToBoolean(reader["vpn"]);
                        rights["Telex"] = Convert.ToBoolean(reader["teflex"]);
                        rights["Jira"] = Convert.ToBoolean(reader["jira"]);
                        rights["Radius"] = Convert.ToBoolean(reader["radius"]);
                        rights["Google"] = Convert.ToBoolean(reader["google"]);
                        rights["PhoneBook"] = Convert.ToBoolean(reader["phonebook"]);
                        rights["Asterisk"] = Convert.ToBoolean(reader["atc"]);
                        rights["FileServer"] = Convert.ToBoolean(reader["fileserver"]);
                    }
                }
            }
        }

        return rights;
    }

    private void OnActionsTapped(object sender, TappedEventArgs e)
    {
        Navigation.PushAsync(new DeistviaPolzovateley());
    }



    private async void OnRightDomainButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            var initialGroupValues = GetSelectedGroupsFromDatabase(selectedUser.Id);
            var groupNames = new List<string> { "Группа 1", "Группа 2", "Группа 3" };

            await ShowReadOnlyDialog(initialGroupValues, groupNames);
        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }


    private Dictionary<string, bool> GetSelectedGroupsFromDatabase(int userId)
    {
        var result = new Dictionary<string, bool>
            {
                { "Группа 1", false },
                { "Группа 2", false },
                { "Группа 3", false }
            };

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "SELECT group1, group2, group3 FROM AdditionalDomainRequest WHERE user_id = @UserID";

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result["Группа 1"] = reader.GetBoolean(0);
                            result["Группа 2"] = reader.GetBoolean(1);
                            result["Группа 3"] = reader.GetBoolean(2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при получении групп из БД: " + ex.Message);
            }
        }

        return result;
    }

    private async void OnRight1CButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            var initialGroupValues = GetSelectedGroupsFrom1CDatabase(selectedUser.Id);
            var groupNames = new List<string> { "department1", "department2", "department3" };

            await ShowReadOnlyDialog(initialGroupValues, groupNames);
        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }


    private Dictionary<string, bool> GetSelectedGroupsFrom1CDatabase(int userId)
    {
        var result = new Dictionary<string, bool>
            {
                { "department1", false },
                { "department2", false },
                { "department3", false }
            };

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "SELECT department1, department2, department3 FROM Additional1CRequest WHERE user_id = @UserID";

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result["department1"] = reader.GetBoolean(0);
                            result["department2"] = reader.GetBoolean(1);
                            result["department3"] = reader.GetBoolean(2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при получении групп из БД: " + ex.Message);
            }
        }

        return result;
    }

    private async void OnRightBitrixButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            var initialGroupValues = GetSelectedGroupsFromBitrixDatabase(selectedUser.Id);
            var groupNames = new List<string> { "crm1", "crm2", "crm3" };

            await ShowReadOnlyDialog(initialGroupValues, groupNames);
        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }


    private Dictionary<string, bool> GetSelectedGroupsFromBitrixDatabase(int userId)
    {
        var result = new Dictionary<string, bool>
            {
                { "crm1", false },
                { "crm2", false },
                { "crm3", false }
            };

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "SELECT crm1, crm2, crm3 FROM AdditionalBitrixRequest WHERE user_id = @UserID";

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result["crm1"] = reader.GetBoolean(0);
                            result["crm2"] = reader.GetBoolean(1);
                            result["crm3"] = reader.GetBoolean(2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при получении групп из БД: " + ex.Message);
            }
        }

        return result;
    }

    private async void OnRightFileServerButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            var initialGroupValues = GetSelectedGroupsFromFileServerDatabase(selectedUser.Id);
            var groupNames = new List<string> { "folder_name1", "folder_name2", "folder_name3" };

            await ShowReadOnlyDialog(initialGroupValues, groupNames);
        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }

    private Dictionary<string, bool> GetSelectedGroupsFromFileServerDatabase(int userId)
    {
        var result = new Dictionary<string, bool>
            {
                { "folder_name1", false },
                { "folder_name2", false },
                { "folder_name3", false }
            };

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = "SELECT folder_name1, folder_name2, folder_name3 FROM AdditionalFileServerRequest WHERE user_id = @UserID";

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result["folder_name1"] = reader.GetBoolean(0);
                            result["folder_name2"] = reader.GetBoolean(1);
                            result["folder_name3"] = reader.GetBoolean(2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при получении групп из БД: " + ex.Message);
            }
        }

        return result;
    }
}