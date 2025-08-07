        using Microsoft.Maui.Controls;
        using System;
        using System.Collections.Generic;
        using System.Data.SqlClient;
        using System.Diagnostics;
        using System.Threading.Tasks;


namespace automationApp
{
    public partial class IspolnitelPage : ContentPage
    {
        private ApplicationStatus _selectedItem;
        private string _currentSortOption = "По дате";
        private string _currentFilterOption = "Все";

        private readonly int currentUserId;


        private ReadOnlyCustomDialogView readOnlyDialog;
        public IspolnitelPage(int currentUserId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;
            LoadApplications();

            
            SortPicker.ItemsSource = new List<string>
                    {
                        "Сортировка", 
                        "По дате",
                        "По статусу",
                        "По ФИО"
                    };
            SortPicker.SelectedIndex = 0; 

            
            FilterPicker.ItemsSource = new List<string>
                    {
                        "Фильтрация", 
                        "Все",
            "Не выполнено",
            "Выполняется",
            "Выполнено"
                    };
            FilterPicker.SelectedIndex = 0; 
        }

        private async Task ShowReadOnlyDialog(Dictionary<string, bool> values, List<string> groupNames)
        {
            readOnlyDialog = new ReadOnlyCustomDialogView(values, groupNames);
            MainLayout.Children.Add(readOnlyDialog); 
            await readOnlyDialog.ShowAsync();
        }
        private void SortPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = SortPicker.SelectedIndex;
            if (selectedIndex == 0)
            {
                SortPicker.SelectedIndex = 0; 
                return;
            }

            
            _currentSortOption = SortPicker.SelectedItem?.ToString();
            Debug.WriteLine($"Выбран метод сортировки: {_currentSortOption}");

            
            LoadApplications();
        }

        private void FilterPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = FilterPicker.SelectedIndex;
            if (selectedIndex == 0)
            {
                FilterPicker.SelectedIndex = 0; 
                return;
            }

            
            _currentFilterOption = FilterPicker.SelectedItem?.ToString();
            Debug.WriteLine($"Выбран метод фильтрации: {_currentFilterOption}");

            
            LoadApplications();
        }

        private async Task LoadApplications()
        {
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = @"
                SELECT a.id, a.user_id, a.comment, a.application_date, a.status, 
                       u.last_name + ' ' + u.first_name + ' ' + u.middle_name AS FIO
                FROM ApplicationStatus a
                JOIN Users u ON a.user_id = u.id";

            // Добавляем фильтрацию
            if (_currentFilterOption != "Все")
            {
                query += " WHERE a.status = @Status";
            }

            // Добавляем сортировку
            switch (_currentSortOption)
            {
                case "По дате":
                    query += " ORDER BY a.application_date DESC";
                    break;
                case "По статусу":
                    query += " ORDER BY a.status";
                    break;
                case "По ФИО":
                    query += " ORDER BY u.last_name, u.first_name, u.middle_name";
                    break;
            }

            List<ApplicationStatus> applications = new List<ApplicationStatus>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);

                    
                    if (_currentFilterOption != "Все")
                    {
                        command.Parameters.AddWithValue("@Status", _currentFilterOption);
                    }

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        string status = reader["status"].ToString();
                        Color statusTextColor;

                        switch (status)
                        {
                            case "Не выполнено":
                                statusTextColor = Color.FromArgb("#E12629"); // Красный
                                break;
                            case "Выполняется":
                                statusTextColor = Color.FromArgb("#D1D400"); // Желтый
                                break;
                            case "Выполнено":
                                statusTextColor = Color.FromArgb("#11CC0B"); // Зеленый
                                break;
                            default:
                                statusTextColor = Color.FromArgb("#CC0B0E"); // Стандартный цвет
                                break;
                        }

                        applications.Add(new ApplicationStatus
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            UserId = Convert.ToInt32(reader["user_id"]),
                            Comment = reader["comment"].ToString(),
                            ApplicationDate = Convert.ToDateTime(reader["application_date"]),
                            Status = status,
                            FIO = reader["FIO"].ToString(),
                            BackgroundColor = Colors.White,
                            BorderColor = Color.FromArgb("#4B164C"),
                            FioTextColor = Color.FromArgb("000000"),
                            DateTextColor = Color.FromArgb("#4A4A4A"),
                            StatusTextColor = statusTextColor
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading applications: {ex.Message}");
                }
            }

            ApplicationsListView.ItemsSource = applications;
        }

        private async void ApplicationsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is ApplicationStatus selectedItem)
            {

                ResetListViewItemStyles();

                selectedItem.BackgroundColor = Color.FromArgb("#4B164C");
                selectedItem.BorderColor = Color.FromArgb("#4B164C");
                selectedItem.FioTextColor = Colors.White;
                selectedItem.DateTextColor = Color.FromArgb("#D0D0D0");

                _selectedItem = selectedItem;

                ApplicationsListView.SelectedItem = null;

                await LoadUserRights(selectedItem.UserId);
            }
            else if (e.SelectedItem == null)
            {
                Debug.WriteLine("ItemSelected triggered with null item. Ignoring.");
            }
            else
            {
                DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
            }
        }

        private async Task LoadUserRights(int userId)
        {
            var rightsRequest = GetRightsRequest(userId);
            var rightsExecution = GetRightsExecution(userId);

            Device.BeginInvokeOnMainThread(() =>
            {
                // Email
                label_right_email.BackgroundColor = rightsRequest["Email"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_email.TextColor = rightsRequest["Email"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_email.BorderColor = rightsRequest["Email"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Bitrix
                label_right_bitrix.BackgroundColor = rightsRequest["Bitrix"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_bitrix.TextColor = rightsRequest["Bitrix"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_bitrix.BorderColor = rightsRequest["Bitrix"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Domain
                label_right_domain.BackgroundColor = rightsRequest["Domain"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_domain.TextColor = rightsRequest["Domain"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_domain.BorderColor = rightsRequest["Domain"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // 1C
                label_right_1c.BackgroundColor = rightsRequest["OneC"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_1c.TextColor = rightsRequest["OneC"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_1c.BorderColor = rightsRequest["OneC"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Elastix (Asterisk)
                label_right_elastix.BackgroundColor = rightsRequest["Asterisk"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_elastix.TextColor = rightsRequest["Asterisk"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_elastix.BorderColor = rightsRequest["Asterisk"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // VPN
                label_right_vpn.BackgroundColor = rightsRequest["VPN"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_vpn.TextColor = rightsRequest["VPN"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_vpn.BorderColor = rightsRequest["VPN"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Teflex
                label_right_teflex.BackgroundColor = rightsRequest["Telex"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_teflex.TextColor = rightsRequest["Telex"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_teflex.BorderColor = rightsRequest["Telex"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Jira
                label_right_jira.BackgroundColor = rightsRequest["Jira"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_jira.TextColor = rightsRequest["Jira"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_jira.BorderColor = rightsRequest["Jira"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Radius
                label_right_radius.BackgroundColor = rightsRequest["Radius"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_radius.TextColor = rightsRequest["Radius"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_radius.BorderColor = rightsRequest["Radius"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Google
                label_right_google.BackgroundColor = rightsRequest["Google"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_google.TextColor = rightsRequest["Google"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_google.BorderColor = rightsRequest["Google"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // PhoneBook
                label_right_phonebook.BackgroundColor = rightsRequest["PhoneBook"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_phonebook.TextColor = rightsRequest["PhoneBook"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_phonebook.BorderColor = rightsRequest["PhoneBook"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // FileServer
                label_right_fileserver.BackgroundColor = rightsRequest["FileServer"] ? Color.FromArgb("#4B164C") : Colors.White;
                label_right_fileserver.TextColor = rightsRequest["FileServer"] ? Colors.White : Color.FromArgb("#4B164C");
                label_right_fileserver.BorderColor = rightsRequest["FileServer"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Email
                checkbox_right_email.BackgroundColor = rightsExecution["Email"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_email.TextColor = rightsExecution["Email"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_email.BorderColor = rightsExecution["Email"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Bitrix
                checkbox_right_bitrix.BackgroundColor = rightsExecution["Bitrix"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_bitrix.TextColor = rightsExecution["Bitrix"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_bitrix.BorderColor = rightsExecution["Bitrix"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Domain
                checkbox_right_domain.BackgroundColor = rightsExecution["Domain"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_domain.TextColor = rightsExecution["Domain"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_domain.BorderColor = rightsExecution["Domain"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // 1C
                checkbox_right_1c.BackgroundColor = rightsExecution["OneC"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_1c.TextColor = rightsExecution["OneC"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_1c.BorderColor = rightsExecution["OneC"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Elastix (Asterisk)
                checkbox_right_elastix.BackgroundColor = rightsExecution["Asterisk"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_elastix.TextColor = rightsExecution["Asterisk"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_elastix.BorderColor = rightsExecution["Asterisk"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // VPN
                checkbox_right_vpn.BackgroundColor = rightsExecution["VPN"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_vpn.TextColor = rightsExecution["VPN"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_vpn.BorderColor = rightsExecution["VPN"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Teflex
                checkbox_right_teflex.BackgroundColor = rightsExecution["Telex"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_teflex.TextColor = rightsExecution["Telex"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_teflex.BorderColor = rightsExecution["Telex"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Jira
                checkbox_right_jira.BackgroundColor = rightsExecution["Jira"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_jira.TextColor = rightsExecution["Jira"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_jira.BorderColor = rightsExecution["Jira"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Radius
                checkbox_right_radius.BackgroundColor = rightsExecution["Radius"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_radius.TextColor = rightsExecution["Radius"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_radius.BorderColor = rightsExecution["Radius"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // Google
                checkbox_right_google.BackgroundColor = rightsExecution["Google"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_google.TextColor = rightsExecution["Google"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_google.BorderColor = rightsExecution["Google"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // PhoneBook
                checkbox_right_phonebook.BackgroundColor = rightsExecution["PhoneBook"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_phonebook.TextColor = rightsExecution["PhoneBook"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_phonebook.BorderColor = rightsExecution["PhoneBook"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");

                // FileServer
                checkbox_right_fileserver.BackgroundColor = rightsExecution["FileServer"] ? Color.FromArgb("#4B164C") : Colors.White;
                checkbox_right_fileserver.TextColor = rightsExecution["FileServer"] ? Colors.White : Color.FromArgb("#4B164C");
                checkbox_right_fileserver.BorderColor = rightsExecution["FileServer"] ? Color.FromArgb("#4B164C") : Color.FromArgb("#4B164C");
            });
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

        private Dictionary<string, bool> GetRightsExecution(int userId)
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
            string query = "SELECT email, bitrix, domain, c1, vpn, teflex, jira, radius, google, phonebook, atc, fileserver FROM RightsExecution WHERE user_id = @UserID";

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


        private void ResetListViewItemStyles()
        {
            if (_selectedItem != null)
            {
                _selectedItem.BackgroundColor = Colors.White;
                _selectedItem.BorderColor = Color.FromArgb("#4B164C");
                _selectedItem.FioTextColor = Color.FromArgb("000000");
                _selectedItem.DateTextColor = Color.FromArgb("#4A4A4A");

            }
        }

        private void OnServiceButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string currentBgHex = clickedButton.BackgroundColor.ToHex().ToUpper();

                string activeHex = Color.FromArgb("#4B164C").ToHex().ToUpper();

                bool isActive = currentBgHex == activeHex;

                if (isActive)
                {
                    // Сделать НЕактивной
                    clickedButton.BackgroundColor = Colors.White;
                    clickedButton.TextColor = Color.FromArgb("#4B164C");
                    clickedButton.BorderColor = Color.FromArgb("#4B164C");
                }
                else
                {
                    // Сделать активной
                    clickedButton.BackgroundColor = Color.FromArgb("#4B164C");
                    clickedButton.TextColor = Colors.White;
                    clickedButton.BorderColor = Colors.Transparent;
                }

                if (_selectedItem != null)
                {
                    int userId = _selectedItem.UserId;

                    Dictionary<string, bool> rights = new Dictionary<string, bool>
                    {
                        { "Email", checkbox_right_email.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Bitrix", checkbox_right_bitrix.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Domain", checkbox_right_domain.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "OneC", checkbox_right_1c.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "VPN", checkbox_right_vpn.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Telex", checkbox_right_teflex.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Jira", checkbox_right_jira.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Radius", checkbox_right_radius.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Google", checkbox_right_google.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "PhoneBook", checkbox_right_phonebook.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "Asterisk", checkbox_right_elastix.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() },
                        { "FileServer", checkbox_right_fileserver.BackgroundColor.ToHex().ToUpper() == Color.FromArgb("#4B164C").ToHex().ToUpper() }
                    };

                    SaveRightsExecution(userId, rights);
                    SaveCurrentRightsExecution(userId);
                }
                else
                {
                    DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
                }
            }
        }
        private void SaveRightsExecution(int userId, Dictionary<string, bool> rights)
        {
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string checkQuery = "SELECT COUNT(*) FROM RightsExecution WHERE user_id = @UserID";
            string insertQuery = @"
                INSERT INTO RightsExecution (user_id, email, bitrix, domain, c1, vpn, teflex, jira, radius, google, phonebook, atc, fileserver)
                VALUES (@UserID, @Email, @Bitrix, @Domain, @OneC, @VPN, @Telex, @Jira, @Radius, @Google, @PhoneBook, @Asterisk, @FileServer)";
            string updateQuery = @"
                UPDATE RightsExecution 
                SET email = @Email, bitrix = @Bitrix, domain = @Domain, c1 = @OneC, vpn = @VPN, teflex = @Telex, 
                    jira = @Jira, radius = @Radius, google = @Google, phonebook = @PhoneBook, atc = @Asterisk, fileserver = @FileServer 
                WHERE user_id = @UserID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserID", userId);
                        int userExists = (int)checkCommand.ExecuteScalar();

                        if (userExists > 0)
                        {
                            // Обновляем существующего пользователя
                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@UserID", userId);
                                updateCommand.Parameters.AddWithValue("@Email", rights["Email"]);
                                updateCommand.Parameters.AddWithValue("@Bitrix", rights["Bitrix"]);
                                updateCommand.Parameters.AddWithValue("@Domain", rights["Domain"]);
                                updateCommand.Parameters.AddWithValue("@OneC", rights["OneC"]);
                                updateCommand.Parameters.AddWithValue("@VPN", rights["VPN"]);
                                updateCommand.Parameters.AddWithValue("@Telex", rights["Telex"]);
                                updateCommand.Parameters.AddWithValue("@Jira", rights["Jira"]);
                                updateCommand.Parameters.AddWithValue("@Radius", rights["Radius"]);
                                updateCommand.Parameters.AddWithValue("@Google", rights["Google"]);
                                updateCommand.Parameters.AddWithValue("@PhoneBook", rights["PhoneBook"]);
                                updateCommand.Parameters.AddWithValue("@Asterisk", rights["Asterisk"]);
                                updateCommand.Parameters.AddWithValue("@FileServer", rights["FileServer"]);

                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@UserID", userId);
                                insertCommand.Parameters.AddWithValue("@Email", rights["Email"]);
                                insertCommand.Parameters.AddWithValue("@Bitrix", rights["Bitrix"]);
                                insertCommand.Parameters.AddWithValue("@Domain", rights["Domain"]);
                                insertCommand.Parameters.AddWithValue("@OneC", rights["OneC"]);
                                insertCommand.Parameters.AddWithValue("@VPN", rights["VPN"]);
                                insertCommand.Parameters.AddWithValue("@Telex", rights["Telex"]);
                                insertCommand.Parameters.AddWithValue("@Jira", rights["Jira"]);
                                insertCommand.Parameters.AddWithValue("@Radius", rights["Radius"]);
                                insertCommand.Parameters.AddWithValue("@Google", rights["Google"]);
                                insertCommand.Parameters.AddWithValue("@PhoneBook", rights["PhoneBook"]);
                                insertCommand.Parameters.AddWithValue("@Asterisk", rights["Asterisk"]);
                                insertCommand.Parameters.AddWithValue("@FileServer", rights["FileServer"]);

                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Ошибка при сохранении прав: " + ex.Message);
                }
            }
        }

        private async void OnStatusLabelTapped(object sender, EventArgs e)
        {
            if (sender is Label label && _selectedItem != null)
            {
                string currentStatus = _selectedItem.Status;

                string newStatus;
                Color newStatusTextColor;

                switch (currentStatus)
                {
                    case "Не выполнено":
                        newStatus = "Выполняется";
                        newStatusTextColor = Color.FromArgb("#D1D400"); // Желтый
                        break;
                    case "Выполняется":
                        newStatus = "Выполнено";
                        newStatusTextColor = Color.FromArgb("#11CC0B"); // Зеленый
                        break;
                    case "Выполнено":
                        newStatus = "Не выполнено";
                        newStatusTextColor = Color.FromArgb("#E12629"); // Красный
                        break;
                    default:
                        newStatus = "Не выполнено";
                        newStatusTextColor = Color.FromArgb("#E12629"); // Красный
                        break;
                }

                _selectedItem.Status = newStatus;
                _selectedItem.StatusTextColor = newStatusTextColor;

                await UpdateApplicationStatus(_selectedItem.Id, newStatus);

                label.Text = newStatus;
                label.TextColor = newStatusTextColor;
            }
        }
        private async Task UpdateApplicationStatus(int applicationId, string newStatus)
        {
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = "UPDATE ApplicationStatus SET status = @Status WHERE id = @ApplicationId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", newStatus);
                        command.Parameters.AddWithValue("@ApplicationId", applicationId);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Ошибка при обновлении статуса заявки: " + ex.Message);
                }
            }
        }

        private async void OpenMicrosoftActiveDirectory(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MicrosoftActiveDirectoryPage());
        }
        private async void OpenKerioConnect(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KerioConnectPage());
        }
        private async void OpenRadius(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RadiusPage());
        }

        private async void OpenPhoneBook(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PhoneBookPage());
        }
        private async void OpenGoogleDriveDisk(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GoogleDriveDiskPage());
        }
        private async void OpenBitrix(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BitrixPage());
        }



        private async void OnRightDomainButtonClicked(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {
                Debug.WriteLine("Пользователь выбран");

                var initialGroupValues = GetSelectedGroupsFromDatabase(_selectedItem.UserId);
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
            if (_selectedItem != null)
            {
                Debug.WriteLine("Пользователь выбран");

                var initialGroupValues = GetSelectedGroupsFrom1CDatabase(_selectedItem.UserId);
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
            if (_selectedItem != null)
            {
                Debug.WriteLine("Пользователь выбран");

                var initialGroupValues = GetSelectedGroupsFromBitrixDatabase(_selectedItem.UserId);
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
            if (_selectedItem != null)
            {
                int userId = _selectedItem.UserId;

                Debug.WriteLine($"Пользователь выбран: ID = {userId}");

                var initialGroupValues = GetSelectedGroupsFromFileServerDatabase(userId);
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











        private async void OnRightExecutionDomainButtonClicked(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {

                int userId = _selectedItem.UserId;
                Debug.WriteLine("Пользователь выбран");

                var initialGroupValues = GetSelectedGroupsFromDatabaseExecution(userId);

                var groupNames = new List<string> { "Группа 1", "Группа 2", "Группа 3" };

                var dialog = new CustomDialogView(groupNames, initialGroupValues);

                MainLayout.Children.Add(dialog);

                var selectedGroups = await dialog.ShowAsync();

                MainLayout.Children.Remove(dialog);

                if (selectedGroups != null)
                {
                    bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                    checkbox_right_domain.BackgroundColor = hasAnyGroup ? Color.FromArgb("#4B164C") : Colors.White;
                    checkbox_right_domain.TextColor = hasAnyGroup ? Colors.White : Color.FromArgb("#4B164C");
                    checkbox_right_domain.BorderColor = hasAnyGroup ? Colors.Transparent : Color.FromArgb("#4B164C");

                    SaveAdditionalDomainRequest(userId, selectedGroups);
                }
                SaveCurrentRightsExecution(userId);
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

                    string checkQuery = "SELECT COUNT(*) FROM AdditionalDomainExecution WHERE user_id = @UserID";
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
                            string updateQuery = @"
                                UPDATE AdditionalDomainExecution
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
                            string insertQuery = @"
                                INSERT INTO AdditionalDomainExecution(user_id, group1, group2, group3)
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

                        string updateRightsQuery = @"
                            UPDATE RightsExecution
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
                                    INSERT INTO RightsExecution (user_id, domain)
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

        private Dictionary<string, bool> GetSelectedGroupsFromDatabaseExecution(int userId)
        {
            var result = new Dictionary<string, bool>
            {
                { "Группа 1", false },
                { "Группа 2", false },
                { "Группа 3", false }
            };

            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = "SELECT group1, group2, group3 FROM AdditionalDomainExecution WHERE user_id = @UserID";

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
            var initialGroupValues = getInitialValues(userId);
            var dialog = new CustomDialogView(fieldNames.ToList(), initialGroupValues);
            MainLayout.Children.Add(dialog);
            var selectedGroups = await dialog.ShowAsync();
            MainLayout.Children.Remove(dialog);

            if (selectedGroups != null)
            {
                Debug.WriteLine("Группы выбраны");

                bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                button.BackgroundColor = hasAnyGroup ? Color.FromArgb("#4B164C") : Colors.White;
                button.TextColor = hasAnyGroup ? Colors.White : Color.FromArgb("#4B164C");
                button.BorderColor = hasAnyGroup ? Colors.Transparent : Color.FromArgb("#4B164C");

                saveRequest(userId, selectedGroups);
                await LoadUserRights(userId);

            }
        }


        private async void OnRightExecution1CButtonClicked(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {

                int userId = _selectedItem.UserId;
                Debug.WriteLine("Пользователь выбран");

                var initialGroupValues = GetSelectedGroupsFrom1CDatabaseExecution(userId);

                var groupNames = new List<string> { "department1", "department2", "department3" };

                var dialog = new CustomDialogView(groupNames, initialGroupValues);

                MainLayout.Children.Add(dialog);

                var selectedGroups = await dialog.ShowAsync();

                MainLayout.Children.Remove(dialog);

                if (selectedGroups != null)
                {
                    bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                    checkbox_right_1c.BackgroundColor = hasAnyGroup ? Color.FromArgb("#4B164C") : Colors.White;
                    checkbox_right_1c.TextColor = hasAnyGroup ? Colors.White : Color.FromArgb("#4B164C");
                    checkbox_right_1c.BorderColor = hasAnyGroup ? Colors.Transparent : Color.FromArgb("#4B164C");

                    SaveAdditional1CRequest(userId, selectedGroups);
                }
                SaveCurrentRightsExecution(userId);
            }
            else
            {
                await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
            }
        }

        private Dictionary<string, bool> GetSelectedGroupsFrom1CDatabaseExecution(int userId)
        {
            var result = new Dictionary<string, bool>
            {
                { "department1", false },
                { "department2", false },
                { "department3", false }
            };

            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = "SELECT department1, department2, department3 FROM Additional1CExecution WHERE user_id = @UserID";

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

                    string checkQuery = "SELECT COUNT(*) FROM Additional1CExecution WHERE user_id = @UserID";
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
                                UPDATE Additional1CExecution
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
                            string insertQuery = @"
                                INSERT INTO Additional1CExecution (user_id, department1, department2, department3)
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

                        string updateRightsQuery = @"
                            UPDATE RightsExecution
                            SET c1 = @C1
                            WHERE user_id = @UserID";
                        using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                        {
                            rightsCmd.Parameters.AddWithValue("@UserID", userId);
                            rightsCmd.Parameters.AddWithValue("@C1", hasAnyGroup);
                            int rowsAffected = rightsCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                string insertRightsQuery = @"
                                    INSERT INTO RightsExecution (user_id, c1)
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

        private async void OnRightExecutionBitrixButtonClicked(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {
                int userId = _selectedItem.UserId;
                Debug.WriteLine("Пользователь выбран");

                await HandleAdditionalRequestAsync(
                    userId,
                    "AdditionalBitrixExecution",
                    new[] { "crm1", "crm2", "crm3" },
                    checkbox_right_bitrix,
                    GetSelectedGroupsFromBitrixDatabaseExecution,
                    SaveAdditionalBitrixRequest
                );
                SaveCurrentRightsExecution(userId);
            }
            else
            {
                await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
            }
        }



        private async void OnRightExecutionFileServerButtonClicked(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {
                int userId = _selectedItem.UserId;

                Debug.WriteLine("Пользователь выбран");

                var initialGroupValues = GetSelectedGroupsFromFileServerDatabaseExecution(userId);

                var groupNames = new List<string> { "folder_name1", "folder_name2", "folder_name3" };

                var dialog = new CustomDialogView(groupNames, initialGroupValues);

                MainLayout.Children.Add(dialog);

                // Показываем диалоговое окно
                var selectedGroups = await dialog.ShowAsync();

                MainLayout.Children.Remove(dialog);

                if (selectedGroups != null)
                {
                    bool hasAnyGroup = selectedGroups.Values.Any(v => v);

                    checkbox_right_fileserver.BackgroundColor = hasAnyGroup ? Color.FromArgb("#4B164C") : Colors.White;
                    checkbox_right_fileserver.TextColor = hasAnyGroup ? Colors.White : Color.FromArgb("#4B164C");
                    checkbox_right_fileserver.BorderColor = hasAnyGroup ? Colors.Transparent : Color.FromArgb("#4B164C");

                    SaveAdditionalFileServerRequest(userId, selectedGroups);
                }
                SaveCurrentRightsExecution(userId);
            }
            else
            {
                await DisplayAlert("Ошибка", "Выберите пользователя", "ОК");
            }
        }

        private Dictionary<string, bool> GetSelectedGroupsFromFileServerDatabaseExecution(int userId)
        {
            var result = new Dictionary<string, bool>
            {
                { "folder_name1", false },
                { "folder_name2", false },
                { "folder_name3", false }
            };

            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = "SELECT folder_name1, folder_name2, folder_name3 FROM AdditionalFileServerExecution WHERE user_id = @UserID";

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

                    string checkQuery = "SELECT COUNT(*) FROM AdditionalFileServerExecution WHERE user_id = @UserID";
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
                            string updateQuery = @"
                                UPDATE AdditionalFileServerExecution
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
                            string insertQuery = @"
                                INSERT INTO AdditionalFileServerExecution (user_id, folder_name1, folder_name2, folder_name3)
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

                        string updateRightsQuery = @"
                            UPDATE RightsExecution
                            SET fileserver = @FileServer
                            WHERE user_id = @UserID";
                        using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                        {
                            rightsCmd.Parameters.AddWithValue("@UserID", userId);
                            rightsCmd.Parameters.AddWithValue("@FileServer", hasAnyGroup);
                            int rowsAffected = rightsCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                string insertRightsQuery = @"
                                    INSERT INTO RightsExecution (user_id, fileserver)
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




        private Dictionary<string, bool> GetSelectedGroupsFromBitrixDatabaseExecution(int userId)
        {
            var result = new Dictionary<string, bool>
            {
                { "crm1", false },
                { "crm2", false },
                { "crm3", false }
            };

            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = "SELECT crm1, crm2, crm3 FROM AdditionalBitrixExecution WHERE user_id = @UserID";

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

                    string checkQuery = "SELECT COUNT(*) FROM AdditionalBitrixExecution WHERE user_id = @UserID";
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
                            UPDATE AdditionalBitrixExecution
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
                            string insertQuery = @"
                            INSERT INTO AdditionalBitrixExecution (user_id, crm1, crm2, crm3)
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

                        string updateRightsQuery = @"
                        UPDATE RightsExecution
                        SET bitrix = @Bitrix
                        WHERE user_id = @UserID";
                        using (SqlCommand rightsCmd = new SqlCommand(updateRightsQuery, connection))
                        {
                            rightsCmd.Parameters.AddWithValue("@UserID", userId);
                            rightsCmd.Parameters.AddWithValue("@Bitrix", hasAnyGroup);
                            int rowsAffected = rightsCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                string insertRightsQuery = @"
                                INSERT INTO RightsExecution (user_id, bitrix)
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

        private readonly Color ActiveColor = Color.Parse("#4B164C");
        private readonly Color InactiveColor = Colors.White;

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
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при записи лога: {ex.Message}");
            }
        }

        private async Task SaveCurrentRightsExecution(int userId)
        {
            Dictionary<string, bool> currentRights = new()
    {
        { "Email", checkbox_right_email.BackgroundColor == ActiveColor },
        { "Bitrix", checkbox_right_bitrix.BackgroundColor == ActiveColor },
        { "Domain", checkbox_right_domain.BackgroundColor == ActiveColor },
        { "OneC", checkbox_right_1c.BackgroundColor == ActiveColor },
        { "VPN", checkbox_right_vpn.BackgroundColor == ActiveColor },
        { "Telex", checkbox_right_teflex.BackgroundColor == ActiveColor },
        { "Jira", checkbox_right_jira.BackgroundColor == ActiveColor },
        { "Radius", checkbox_right_radius.BackgroundColor == ActiveColor },
        { "Google", checkbox_right_google.BackgroundColor == ActiveColor },
        { "PhoneBook", checkbox_right_phonebook.BackgroundColor == ActiveColor },
        { "Asterisk", checkbox_right_elastix.BackgroundColor == ActiveColor },
        { "FileServer", checkbox_right_fileserver.BackgroundColor == ActiveColor }
    };

            Debug.WriteLine("Текущие значения прав:");
            foreach (var right in currentRights)
                Debug.WriteLine($"{right.Key} = {right.Value}");

            var previousRights = GetPreviousRightsExecution(userId);
            List<string> changedFlags = new();

            foreach (var right in currentRights)
            {
                if (!previousRights.ContainsKey(right.Key) || previousRights[right.Key] != right.Value)
                    changedFlags.Add($"{right.Key}: {(right.Value ? "Деактивирован" : "Активирован")}");
            }

            if (changedFlags.Count > 0)
            {
                Debug.WriteLine("Изменения прав:");
                foreach (var change in changedFlags)
                    Debug.WriteLine(change);
            }
            else
            {
                Debug.WriteLine("Нет изменений в правах.");
            }

            SaveRightsExecution(userId, currentRights);

            string actionDescription = changedFlags.Count > 0 ? string.Join(", ", changedFlags) : "Нет изменений";
            await LogUserAction(userId, "Изменение прав (кнопки)", $"Изменены флажки: {actionDescription}", "Не прочитано");
        }

        private Dictionary<string, bool> GetPreviousRightsExecution(int userId)
        {
            Dictionary<string, bool> rights = new()
    {
        { "Email", false }, { "Bitrix", false }, { "Domain", false }, { "OneC", false },
        { "VPN", false }, { "Telex", false }, { "Jira", false }, { "Radius", false },
        { "Google", false }, { "PhoneBook", false }, { "Asterisk", false }, { "FileServer", false }
    };

            string query = "SELECT email, bitrix, domain, c1, vpn, teflex, jira, radius, google, phonebook, atc, fileserver FROM RightsExecution WHERE user_id = @UserID";

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

            foreach (var right in rights)
                Debug.WriteLine($"Previous Right: {right.Key} = {right.Value}");

            return rights;
        }

        private async void OnCheckboxClicked(object sender, EventArgs e)
        {
            if (_selectedItem != null)
            {
                await SaveCurrentRightsExecution(_selectedItem.UserId);
            }
        }

    }
}