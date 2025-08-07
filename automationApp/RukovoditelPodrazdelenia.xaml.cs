using DocumentFormat.OpenXml.Spreadsheet;
using Google.Apis.Drive.v3.Data;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;


namespace automationApp;

public partial class RukovoditelPodrazdelenia : ContentPage
{
    private List<Button> allServiceButtons;
    private Dictionary<string, bool> copiedRights = null;
    private readonly int currentUserId;

    private CustomDialogView dialog;
    public RukovoditelPodrazdelenia(int currentUserId)
    {
        InitializeComponent();
        this.currentUserId = currentUserId;
        LoadUsers();
        // Сохраняем все кнопки в список
        allServiceButtons = new List<Button>
    {
        right_email, right_bitrix, right_domain, right_1c,
        right_elastix, right_google, right_vpn, right_teflex,
        right_jira, right_radius, right_phonebook, right_fileserver
    };

        // Создаем и настраиваем диалоговое окно
        dialog = new CustomDialogView(
            new List<string> { "Сделки", "Лиды", "Контакты" },
            new Dictionary<string, bool> { { "Сделки", true } }
        );

        // Добавляем диалоговое окно в MainLayout
        MainLayout.Children.Add(dialog);

    }


    private async void OnOpenDialogClicked(object sender, EventArgs e)
    {
        // Показываем диалоговое окно
        var result = await dialog.ShowAsync();
        if (result != null)
        {
            // используй результат
            foreach (var kv in result)
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
        }
    }

    private async void LoadUsers()
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        int currentUserId = Preferences.Default.Get("CurrentUserId", -1);
        string division = "";

        // Получаем division текущего пользователя
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                string divisionQuery = "SELECT division FROM Users WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(divisionQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@id", currentUserId);
                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        division = result.ToString();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось найти отдел текущего пользователя.", "ОК");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка при получении отдела: {ex.Message}", "ОК");
                return;
            }
        }

        // Загружаем только подчинённых из того же division
        string query = "SELECT id, last_name, first_name, middle_name, position FROM Users WHERE division = @division";
        List<User> users = new();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@division", division);

                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Last_name = reader["last_name"].ToString(),
                        First_name = reader["first_name"].ToString(),
                        Middle_name = reader["middle_name"].ToString(),
                        Position = reader["position"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка при загрузке пользователей: {ex.Message}", "ОК");
            }
        }

        MyCollectionView.ItemsSource = users;
    }

    private void OnServiceButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            // Получаем текущий цвет кнопки в HEX
            string currentBgHex = clickedButton.BackgroundColor.ToHex().ToUpper();

            // Наш цвет активной кнопки в HEX
            string activeHex = Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper();

            bool isActive = currentBgHex == activeHex;

            if (isActive)
            {
                // Сделать НЕактивной
                clickedButton.BackgroundColor = Microsoft.Maui.Graphics.Colors.White;
                clickedButton.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                clickedButton.BorderColor = Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            }
            else
            {
                // Сделать активной
                clickedButton.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                clickedButton.TextColor = Microsoft.Maui.Graphics.Colors.White;
                clickedButton.BorderColor = Microsoft.Maui.Graphics.Colors.Transparent;
            }
        }


    }
    private void OnCopyRightsClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            int userId = selectedUser.Id;

            // Собираем текущие состояния кнопок в словарь
            copiedRights = new Dictionary<string, bool>
        {
            { "Email", right_email.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Bitrix", right_bitrix.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Domain", right_domain.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "OneC", right_1c.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "VPN", right_vpn.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Telex", right_teflex.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Jira", right_jira.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Radius", right_radius.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Google", right_google.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "PhoneBook", right_phonebook.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Asterisk", right_elastix.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "FileServer", right_fileserver.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() }
        };

            DisplayAlert("Успех", "Права успешно скопированы", "ОК");
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

    private void OnPasteRightsClicked(object sender, EventArgs e)
    {
        if (copiedRights != null && MyCollectionView.SelectedItem is User selectedUser)
        {
            int userId = selectedUser.Id;

            // Применяем сохраненные права к кнопкам
            right_email.BackgroundColor = copiedRights["Email"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_email.TextColor = copiedRights["Email"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_email.BorderColor = copiedRights["Email"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_bitrix.BackgroundColor = copiedRights["Bitrix"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_bitrix.TextColor = copiedRights["Bitrix"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_bitrix.BorderColor = copiedRights["Bitrix"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_domain.BackgroundColor = copiedRights["Domain"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_domain.TextColor = copiedRights["Domain"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_domain.BorderColor = copiedRights["Domain"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_1c.BackgroundColor = copiedRights["OneC"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_1c.TextColor = copiedRights["OneC"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_1c.BorderColor = copiedRights["OneC"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_elastix.BackgroundColor = copiedRights["Asterisk"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_elastix.TextColor = copiedRights["Asterisk"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_elastix.BorderColor = copiedRights["Asterisk"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_vpn.BackgroundColor = copiedRights["VPN"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_vpn.TextColor = copiedRights["VPN"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_vpn.BorderColor = copiedRights["VPN"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_teflex.BackgroundColor = copiedRights["Telex"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_teflex.TextColor = copiedRights["Telex"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_teflex.BorderColor = copiedRights["Telex"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_jira.BackgroundColor = copiedRights["Jira"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_jira.TextColor = copiedRights["Jira"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_jira.BorderColor = copiedRights["Jira"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_radius.BackgroundColor = copiedRights["Radius"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_radius.TextColor = copiedRights["Radius"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_radius.BorderColor = copiedRights["Radius"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_google.BackgroundColor = copiedRights["Google"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_google.TextColor = copiedRights["Google"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_google.BorderColor = copiedRights["Google"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_phonebook.BackgroundColor = copiedRights["PhoneBook"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_phonebook.TextColor = copiedRights["PhoneBook"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_phonebook.BorderColor = copiedRights["PhoneBook"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            right_fileserver.BackgroundColor = copiedRights["FileServer"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            right_fileserver.TextColor = copiedRights["FileServer"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            right_fileserver.BorderColor = copiedRights["FileServer"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            // Сохраняем изменения в базе данных
            UpdateRightsRequest(userId, copiedRights);

            DisplayAlert("Успех", "Права успешно вставлены и сохранены", "ОК");
        }
        else
        {
            DisplayAlert("Ошибка", "Сначала скопируйте права другого пользователя или выберите текущего пользователя", "ОК");
        }
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
    private string GetEnabledRightsDescription(Dictionary<string, bool> rights)
    {
        List<string> enabledRights = new List<string>();

        if (rights["Email"]) enabledRights.Add("Email");
        if (rights["Bitrix"]) enabledRights.Add("Bitrix");
        if (rights["Domain"]) enabledRights.Add("Domain");
        if (rights["OneC"]) enabledRights.Add("1C");
        if (rights["VPN"]) enabledRights.Add("VPN");
        if (rights["Telex"]) enabledRights.Add("Telex");
        if (rights["Jira"]) enabledRights.Add("Jira");
        if (rights["Radius"]) enabledRights.Add("Radius");
        if (rights["Google"]) enabledRights.Add("Google");
        if (rights["PhoneBook"]) enabledRights.Add("PhoneBook");
        if (rights["Asterisk"]) enabledRights.Add("Asterisk");
        if (rights["FileServer"]) enabledRights.Add("FileServer");

        return string.Join(", ", enabledRights);
    }
    private async void OnSaveTapped(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            int userId = selectedUser.Id;

            // Собираем текущие состояния кнопок
            Dictionary<string, bool> rights = new Dictionary<string, bool>
        {
            { "Email", right_email.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Bitrix", right_bitrix.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Domain", right_domain.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "OneC", right_1c.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "VPN", right_vpn.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Telex", right_teflex.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Jira", right_jira.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Radius", right_radius.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Google", right_google.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "PhoneBook", right_phonebook.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "Asterisk", right_elastix.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() },
            { "FileServer", right_fileserver.BackgroundColor.ToHex().ToUpper() == Microsoft.Maui.Graphics.Color.FromArgb("#4B164C").ToHex().ToUpper() }
        };

            // Сохраняем изменения в базе данных
            UpdateRightsRequest(userId, rights);

            // Получаем описание прав, которые включены
            string rightsDescription = GetEnabledRightsDescription(rights);

            // Показываем диалог для ввода комментария
            string comment = await DisplayPromptAsync("Комментарий", "Введите комментарий к заявке", "Сохранить", "Отмена", maxLength: 255);

            // Если пользователь нажал "Отмена", комментарий будет null
            if (string.IsNullOrEmpty(comment))
            {
                await DisplayAlert("Отмена", "Действие сохранения отменено", "ОК");
                return; // Выходим из метода, не сохраняя данные
            }
            // Сохраняем изменения в базе данных
            UpdateRightsRequest(userId, rights);

         
                
            // Сохраняем заявку в таблицу ApplicationStatus
            SaveApplicationStatus(userId, comment);

            // Сохраняем лог действия с описанием прав
            await LogUserAction(currentUserId, "Создание заявки на права", $"Создана заявка на права для пользователя с ID {userId}. Права включают: {rightsDescription}", "Не прочитано");

            await DisplayAlert("Успех", "Права успешно сохранены и заявка создана", "ОК");
        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }

    private void SaveApplicationStatus(int userId, string comment)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = @"
        INSERT INTO ApplicationStatus (user_id, comment, application_date, status)
        VALUES (@UserID, @Comment, @ApplicationDate, @Status)";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@Comment", comment);
                    command.Parameters.AddWithValue("@ApplicationDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Status", "Не выполнено");

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                Debug.WriteLine("Ошибка при сохранении заявки: " + ex.Message);
            }
        }
    }

    private void UpdateRightsRequest(int userId, Dictionary<string, bool> rights)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        string query = @"
        UPDATE RightsRequest 
        SET email = @Email, bitrix = @Bitrix, domain = @Domain, c1 = @OneC, vpn = @VPN, teflex = @Telex, 
            jira = @Jira, radius = @Radius, google = @Google, phonebook = @PhoneBook, atc = @Asterisk, fileserver = @FileServer 
        WHERE user_id = @UserID";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@Email", rights["Email"]);
                    command.Parameters.AddWithValue("@Bitrix", rights["Bitrix"]);
                    command.Parameters.AddWithValue("@Domain", rights["Domain"]);
                    command.Parameters.AddWithValue("@OneC", rights["OneC"]);
                    command.Parameters.AddWithValue("@VPN", rights["VPN"]);
                    command.Parameters.AddWithValue("@Telex", rights["Telex"]);
                    command.Parameters.AddWithValue("@Jira", rights["Jira"]);
                    command.Parameters.AddWithValue("@Radius", rights["Radius"]);
                    command.Parameters.AddWithValue("@Google", rights["Google"]);
                    command.Parameters.AddWithValue("@PhoneBook", rights["PhoneBook"]);
                    command.Parameters.AddWithValue("@Asterisk", rights["Asterisk"]);
                    command.Parameters.AddWithValue("@FileServer", rights["FileServer"]);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", "Ошибка при обновлении прав: " + ex.Message, "ОК");
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
                right_email.BackgroundColor = rights["Email"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_email.TextColor = rights["Email"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_email.BorderColor = rights["Email"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_bitrix.BackgroundColor = rights["Bitrix"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_bitrix.TextColor = rights["Bitrix"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_bitrix.BorderColor = rights["Bitrix"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_domain.BackgroundColor = rights["Domain"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_domain.TextColor = rights["Domain"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_domain.BorderColor = rights["Domain"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_1c.BackgroundColor = rights["OneC"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_1c.TextColor = rights["OneC"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_1c.BorderColor = rights["OneC"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_elastix.BackgroundColor = rights["Asterisk"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_elastix.TextColor = rights["Asterisk"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_elastix.BorderColor = rights["Asterisk"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_vpn.BackgroundColor = rights["VPN"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_vpn.TextColor = rights["VPN"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_vpn.BorderColor = rights["VPN"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_teflex.BackgroundColor = rights["Telex"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_teflex.TextColor = rights["Telex"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_teflex.BorderColor = rights["Telex"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_jira.BackgroundColor = rights["Jira"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_jira.TextColor = rights["Jira"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_jira.BorderColor = rights["Jira"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_radius.BackgroundColor = rights["Radius"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_radius.TextColor = rights["Radius"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_radius.BorderColor = rights["Radius"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_google.BackgroundColor = rights["Google"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_google.TextColor = rights["Google"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_google.BorderColor = rights["Google"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_phonebook.BackgroundColor = rights["PhoneBook"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_phonebook.TextColor = rights["PhoneBook"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_phonebook.BorderColor = rights["PhoneBook"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                right_fileserver.BackgroundColor = rights["FileServer"] ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_fileserver.TextColor = rights["FileServer"] ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_fileserver.BorderColor = rights["FileServer"] ? Microsoft.Maui.Graphics.Colors.Transparent : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            });
        }
        else
        {
            DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }

    private async void OnRightDomainButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            // Получаем текущие значения групп из базы
            var initialGroupValues = GetSelectedGroupsFromDatabase(selectedUser.Id);

            // Список названий групп
            var groupNames = new List<string> { "Группа 1", "Группа 2", "Группа 3" };

            // Создаем и настраиваем диалоговое окно
            var dialog = new CustomDialogView(groupNames, initialGroupValues);

            // Добавляем диалоговое окно в текущий layout
            MainLayout.Children.Add(dialog);

            // Показываем диалоговое окно
            var selectedGroups = await dialog.ShowAsync();

            // Удаляем диалоговое окно из layout после закрытия
            MainLayout.Children.Remove(dialog);

            if (selectedGroups != null)
            {
                bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                // Меняем внешний вид кнопки
                right_domain.BackgroundColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_domain.TextColor = hasAnyGroup ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_domain.BorderColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                // Сохраняем в базу
                SaveAdditionalDomainRequest(selectedUser.Id, selectedGroups);
            }

        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }


    private void SaveAdditionalDomainRequest(int userId, Dictionary<string, bool> selectedGroups)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                // Проверяем, существует ли запись для user_id
                string checkQuery = "SELECT COUNT(*) FROM AdditionalDomainRequest WHERE user_id = @UserID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    int count = (int)checkCmd.ExecuteScalar();

                    bool group1 = selectedGroups["Группа 1"];
                    bool group2 = selectedGroups["Группа 2"];
                    bool group3 = selectedGroups["Группа 3"];
                    bool hasAnyGroup = group1 || group2 || group3;

                    if (count > 0)
                    {
                        // Обновляем запись
                        string updateQuery = @"
                        UPDATE AdditionalDomainRequest
                        SET group1 = @Group1, group2 = @Group2, group3 = @Group3
                        WHERE user_id = @UserID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@UserID", userId);
                            updateCmd.Parameters.AddWithValue("@Group1", group1);
                            updateCmd.Parameters.AddWithValue("@Group2", group2);
                            updateCmd.Parameters.AddWithValue("@Group3", group3);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Вставляем новую запись
                        string insertQuery = @"
                        INSERT INTO AdditionalDomainRequest (user_id, group1, group2, group3)
                        VALUES (@UserID, @Group1, @Group2, @Group3)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@UserID", userId);
                            insertCmd.Parameters.AddWithValue("@Group1", group1);
                            insertCmd.Parameters.AddWithValue("@Group2", group2);
                            insertCmd.Parameters.AddWithValue("@Group3", group3);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    // Обновляем поле domain в таблице RightsRequest
                    string updateRightsQuery = @"
                    UPDATE RightsRequest
                    SET domain = @Domain
                    WHERE user_id = @UserID";
                    using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                    {
                        rightsCmd.Parameters.AddWithValue("@UserID", userId);
                        rightsCmd.Parameters.AddWithValue("@Domain", hasAnyGroup);
                        int rowsAffected = rightsCmd.ExecuteNonQuery();

                        // Если запись не существует — создаем
                        if (rowsAffected == 0)
                        {
                            string insertRightsQuery = @"
                            INSERT INTO RightsRequest (user_id, domain)
                            VALUES (@UserID, @Domain)";
                            using (SqlCommand insertRightsCmd = new SqlCommand(insertRightsQuery, connection))
                            {
                                insertRightsCmd.Parameters.AddWithValue("@UserID", userId);
                                insertRightsCmd.Parameters.AddWithValue("@Domain", hasAnyGroup);
                                insertRightsCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при сохранении дополнительных прав домена: " + ex.Message);
            }
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

    private async Task HandleAdditionalRequestAsync(
    int userId,
    string tableName,
    string[] fieldNames,
    Button button,
    Func<int, Dictionary<string, bool>> getInitialValues,
    Action<int, Dictionary<string, bool>> saveRequest)
    {
        // Получаем текущие значения групп из базы
        var initialGroupValues = getInitialValues(userId);

        // Создаем и настраиваем диалоговое окно
        var dialog = new CustomDialogView(fieldNames.ToList(), initialGroupValues);

        // Добавляем диалоговое окно в текущий layout
        MainLayout.Children.Add(dialog);

        // Показываем диалоговое окно
        var selectedGroups = await dialog.ShowAsync();

        // Удаляем диалоговое окно из layout после закрытия
        MainLayout.Children.Remove(dialog);

        if (selectedGroups != null)
        {
            bool hasAnyGroup = selectedGroups.Values.Any(v => v);

            // Меняем внешний вид кнопки
            button.BackgroundColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
            button.TextColor = hasAnyGroup ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
            button.BorderColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

            // Сохраняем в базу
            saveRequest(userId, selectedGroups);
        }

    }

    private async void OnRight1CButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            // Получаем текущие значения групп из базы
            var initialGroupValues = GetSelectedGroupsFrom1CDatabase(selectedUser.Id);

            // Список названий групп
            var groupNames = new List<string> { "department1", "department2", "department3" };

            // Создаем и настраиваем диалоговое окно
            var dialog = new CustomDialogView(groupNames, initialGroupValues);

            // Добавляем диалоговое окно в текущий layout
            MainLayout.Children.Add(dialog);

            // Показываем диалоговое окно
            var selectedGroups = await dialog.ShowAsync();

            // Удаляем диалоговое окно из layout после закрытия
            MainLayout.Children.Remove(dialog);

            if (selectedGroups != null)
            {
                bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                // Меняем внешний вид кнопки
                right_1c.BackgroundColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_1c.TextColor = hasAnyGroup ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_1c.BorderColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                // Сохраняем в базу
                SaveAdditional1CRequest(selectedUser.Id, selectedGroups);
            }

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

    private void SaveAdditional1CRequest(int userId, Dictionary<string, bool> selectedGroups)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                // Проверяем, существует ли запись для user_id
                string checkQuery = "SELECT COUNT(*) FROM Additional1CRequest WHERE user_id = @UserID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    int count = (int)checkCmd.ExecuteScalar();

                    bool department1 = selectedGroups["department1"];
                    bool department2 = selectedGroups["department2"];
                    bool department3 = selectedGroups["department3"];
                    bool hasAnyGroup = department1 || department2 || department3;

                    if (count > 0)
                    {
                        // Обновляем запись
                        string updateQuery = @"
                        UPDATE Additional1CRequest
                        SET department1 = @Department1, department2 = @Department2, department3 = @Department3
                        WHERE user_id = @UserID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@UserID", userId);
                            updateCmd.Parameters.AddWithValue("@Department1", department1);
                            updateCmd.Parameters.AddWithValue("@Department2", department2);
                            updateCmd.Parameters.AddWithValue("@Department3", department3);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Вставляем новую запись
                        string insertQuery = @"
                        INSERT INTO Additional1CRequest (user_id, department1, department2, department3)
                        VALUES (@UserID, @Department1, @Department2, @Department3)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@UserID", userId);
                            insertCmd.Parameters.AddWithValue("@Department1", department1);
                            insertCmd.Parameters.AddWithValue("@Department2", department2);
                            insertCmd.Parameters.AddWithValue("@Department3", department3);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    // Обновляем поле 1c в таблице RightsRequest
                    string updateRightsQuery = @"
                    UPDATE RightsRequest
                    SET c1 = @C1
                    WHERE user_id = @UserID";
                    using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                    {
                        rightsCmd.Parameters.AddWithValue("@UserID", userId);
                        rightsCmd.Parameters.AddWithValue("@C1", hasAnyGroup);
                        int rowsAffected = rightsCmd.ExecuteNonQuery();

                        // Если запись не существует — создаем
                        if (rowsAffected == 0)
                        {
                            string insertRightsQuery = @"
                            INSERT INTO RightsRequest (user_id, c1)
                            VALUES (@UserID, @C1)";
                            using (SqlCommand insertRightsCmd = new SqlCommand(insertRightsQuery, connection))
                            {
                                insertRightsCmd.Parameters.AddWithValue("@UserID", userId);
                                insertRightsCmd.Parameters.AddWithValue("@C1", hasAnyGroup);
                                insertRightsCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при сохранении дополнительных прав 1C: " + ex.Message);
            }
        }
    }

    private async void OnRightBitrixButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            await HandleAdditionalRequestAsync(
                selectedUser.Id,
                "AdditionalBitrixRequest",
                new[] { "crm1", "crm2", "crm3" },
                right_bitrix,
                GetSelectedGroupsFromBitrixDatabase,
                SaveAdditionalBitrixRequest
            );
        }
        else
        {
            await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
        }
    }



    private async void OnRightFileServerButtonClicked(object sender, EventArgs e)
    {
        if (MyCollectionView.SelectedItem is User selectedUser)
        {
            Debug.WriteLine("Пользователь выбран");

            // Получаем текущие значения групп из базы
            var initialGroupValues = GetSelectedGroupsFromFileServerDatabase(selectedUser.Id);

            // Список названий групп
            var groupNames = new List<string> { "folder_name1", "folder_name2", "folder_name3" };

            // Создаем и настраиваем диалоговое окно
            var dialog = new CustomDialogView(groupNames, initialGroupValues);

            // Добавляем диалоговое окно в текущий layout
            MainLayout.Children.Add(dialog);

            // Показываем диалоговое окно
            var selectedGroups = await dialog.ShowAsync();

            // Удаляем диалоговое окно из layout после закрытия
            MainLayout.Children.Remove(dialog);

            if (selectedGroups != null)
            {
                bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                // Меняем внешний вид кнопки
                right_fileserver.BackgroundColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Colors.White;
                right_fileserver.TextColor = hasAnyGroup ? Microsoft.Maui.Graphics.Colors.White : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");
                right_fileserver.BorderColor = hasAnyGroup ? Microsoft.Maui.Graphics.Color.FromArgb("#4B164C") : Microsoft.Maui.Graphics.Color.FromArgb("#4B164C");

                // Сохраняем в базу
                SaveAdditionalFileServerRequest(selectedUser.Id, selectedGroups);
            }

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

    private void SaveAdditionalFileServerRequest(int userId, Dictionary<string, bool> selectedGroups)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                // Проверяем, существует ли запись для user_id
                string checkQuery = "SELECT COUNT(*) FROM AdditionalFileServerRequest WHERE user_id = @UserID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    int count = (int)checkCmd.ExecuteScalar();

                    bool folder_name1 = selectedGroups["folder_name1"];
                    bool folder_name2 = selectedGroups["folder_name2"];
                    bool folder_name3 = selectedGroups["folder_name3"];
                    bool hasAnyGroup = folder_name1 || folder_name2 || folder_name3;

                    if (count > 0)
                    {
                        // Обновляем запись
                        string updateQuery = @"
                        UPDATE AdditionalFileServerRequest
                        SET folder_name1 = @FolderName1, folder_name2 = @FolderName2, folder_name3 = @FolderName3
                        WHERE user_id = @UserID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@UserID", userId);
                            updateCmd.Parameters.AddWithValue("@FolderName1", folder_name1);
                            updateCmd.Parameters.AddWithValue("@FolderName2", folder_name2);
                            updateCmd.Parameters.AddWithValue("@FolderName3", folder_name3);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Вставляем новую запись
                        string insertQuery = @"
                        INSERT INTO AdditionalFileServerRequest (user_id, folder_name1, folder_name2, folder_name3)
                        VALUES (@UserID, @FolderName1, @FolderName2, @FolderName3)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@UserID", userId);
                            insertCmd.Parameters.AddWithValue("@FolderName1", folder_name1);
                            insertCmd.Parameters.AddWithValue("@FolderName2", folder_name2);
                            insertCmd.Parameters.AddWithValue("@FolderName3", folder_name3);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    // Обновляем поле fileserver в таблице RightsRequest
                    string updateRightsQuery = @"
                    UPDATE RightsRequest
                    SET fileserver = @FileServer
                    WHERE user_id = @UserID";
                    using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                    {
                        rightsCmd.Parameters.AddWithValue("@UserID", userId);
                        rightsCmd.Parameters.AddWithValue("@FileServer", hasAnyGroup);
                        int rowsAffected = rightsCmd.ExecuteNonQuery();

                        // Если запись не существует — создаем
                        if (rowsAffected == 0)
                        {
                            string insertRightsQuery = @"
                            INSERT INTO RightsRequest (user_id, fileserver)
                            VALUES (@UserID, @FileServer)";
                            using (SqlCommand insertRightsCmd = new SqlCommand(insertRightsQuery, connection))
                            {
                                insertRightsCmd.Parameters.AddWithValue("@UserID", userId);
                                insertRightsCmd.Parameters.AddWithValue("@FileServer", hasAnyGroup);
                                insertRightsCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при сохранении дополнительных прав FileServer: " + ex.Message);
            }
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

    private void SaveAdditionalBitrixRequest(int userId, Dictionary<string, bool> selectedGroups)
    {
        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                // Проверяем, существует ли запись для user_id
                string checkQuery = "SELECT COUNT(*) FROM AdditionalBitrixRequest WHERE user_id = @UserID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    int count = (int)checkCmd.ExecuteScalar();

                    bool crm1 = selectedGroups["crm1"];
                    bool crm2 = selectedGroups["crm2"];
                    bool crm3 = selectedGroups["crm3"];
                    bool hasAnyGroup = crm1 || crm2 || crm3;

                    if (count > 0)
                    {
                        // Обновляем запись
                        string updateQuery = @"
                    UPDATE AdditionalBitrixRequest
                    SET crm1 = @Crm1, crm2 = @Crm2, crm3 = @Crm3
                    WHERE user_id = @UserID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@UserID", userId);
                            updateCmd.Parameters.AddWithValue("@Crm1", crm1);
                            updateCmd.Parameters.AddWithValue("@Crm2", crm2);
                            updateCmd.Parameters.AddWithValue("@Crm3", crm3);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Вставляем новую запись
                        string insertQuery = @"
                    INSERT INTO AdditionalBitrixRequest (user_id, crm1, crm2, crm3)
                    VALUES (@UserID, @Crm1, @Crm2, @Crm3)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@UserID", userId);
                            insertCmd.Parameters.AddWithValue("@Crm1", crm1);
                            insertCmd.Parameters.AddWithValue("@Crm2", crm2);
                            insertCmd.Parameters.AddWithValue("@Crm3", crm3);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    // Обновляем поле bitrix в таблице RightsRequest
                    string updateRightsQuery = @"
                UPDATE RightsRequest
                SET bitrix = @Bitrix
                WHERE user_id = @UserID";
                    using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                    {
                        rightsCmd.Parameters.AddWithValue("@UserID", userId);
                        rightsCmd.Parameters.AddWithValue("@Bitrix", hasAnyGroup);
                        int rowsAffected = rightsCmd.ExecuteNonQuery();

                        // Если запись не существует — создаем
                        if (rowsAffected == 0)
                        {
                            string insertRightsQuery = @"
                        INSERT INTO RightsRequest (user_id, bitrix)
                        VALUES (@UserID, @Bitrix)";
                            using (SqlCommand insertRightsCmd = new SqlCommand(insertRightsQuery, connection))
                            {
                                insertRightsCmd.Parameters.AddWithValue("@UserID", userId);
                                insertRightsCmd.Parameters.AddWithValue("@Bitrix", hasAnyGroup);
                                insertRightsCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при сохранении дополнительных прав Bitrix: " + ex.Message);
            }
        }
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


}

