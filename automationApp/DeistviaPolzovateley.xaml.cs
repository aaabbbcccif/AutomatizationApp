using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace automationApp
{
    public partial class DeistviaPolzovateley : ContentPage
    {
        private string _currentSortOption = "По дате";
        private string _currentFilterOption = "Все";

        public DeistviaPolzovateley()
        {
            InitializeComponent();

            // Инициализация Picker'ов
            SortPicker.ItemsSource = new List<string> { "По дате", "По статусу", "По ФИО" };
            SortPicker.SelectedIndex = 0;

            FilterPicker.ItemsSource = new List<string> { "Все", "Прочитано", "Не прочитано" };
            FilterPicker.SelectedIndex = 0;

            ActionsPicker.ItemsSource = new List<string> { "Выберите действие", "Пометить как прочитанное", "Пометить как непрочитанное", "Экспортировать", "Очистить" };
            ActionsPicker.SelectedIndex = 0;
        }

       
        private async void ActionsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is UserAction selectedAction)
            {
                var dialog = new UserActionDialogView(selectedAction);

                DialogLayer.Content = dialog;
                DialogLayer.IsVisible = true;

                await dialog.ShowAsync();

                DialogLayer.IsVisible = false;
                DialogLayer.Content = null;

                ActionsCollectionView.SelectedItem = null;
            }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUserActions();
        }

        private async void SortPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentSortOption = SortPicker.SelectedItem?.ToString();
            await LoadUserActions();
        }

        private async void FilterPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentFilterOption = FilterPicker.SelectedItem?.ToString();
            await LoadUserActions();
        }

        private async void ActionsPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedAction = ActionsPicker.SelectedItem?.ToString();
            if (selectedAction == "Пометить как прочитанное")
                await UpdateAllStatuses("Прочитано");
            else if (selectedAction == "Пометить как непрочитанное")
                await UpdateAllStatuses("Не прочитано");
            else if (selectedAction == "Экспортировать")
                await ExportActions();
            else if (selectedAction == "Очистить")
                await ClearAllActions();

            ActionsPicker.SelectedIndex = 0; // Сброс выбора
            await LoadUserActions();
        }

        private async Task LoadUserActions()
        {
            var actions = new List<UserAction>();
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = @"
                SELECT ua.id, ua.title, ua.action, ua.action_date, ua.status,
                       u.last_name, u.first_name, u.middle_name
                FROM UserActions ua
                JOIN Users u ON ua.user_id = u.id";

            if (_currentFilterOption != "Все")
                query += " WHERE ua.status = @Status";

            switch (_currentSortOption)
            {
                case "По дате": query += " ORDER BY ua.action_date DESC"; break;
                case "По статусу": query += " ORDER BY ua.status"; break;
                case "По ФИО": query += " ORDER BY u.last_name, u.first_name, u.middle_name"; break;
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (_currentFilterOption != "Все")
                        {
                            string statusParam = _currentFilterOption.Trim().ToLower() == "прочитано" ? "Прочитано" : "Не прочитано";
                            command.Parameters.AddWithValue("@Status", statusParam);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var statusText = reader.GetString(4);
                                actions.Add(new UserAction
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Action = reader.GetString(2),
                                    ActionDate = reader.GetDateTime(3),
                                    Status = statusText,
                                    StatusColor = statusText == "Прочитано" ? Colors.Green : Colors.Red,
                                    FullName = $"{reader.GetString(5)} {reader.GetString(6)} {reader.GetString(7)}"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка загрузки", ex.Message, "OK");
            }

            ActionsCollectionView.ItemsSource = actions;
        }

        private async Task UpdateAllStatuses(string newStatus)
        {
            try
            {
                string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
                string query = "UPDATE UserActions SET status = @NewStatus";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        string statusValue = newStatus.Trim().ToLower() == "прочитано" ? "Прочитано" : "Не прочитано";
                        command.Parameters.AddWithValue("@NewStatus", statusValue);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка обновления статусов", ex.Message, "OK");
            }
        }

        private async Task ExportActions()
        {
            try
            {
                string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
                string query = @"
            SELECT ua.title, ua.action_date, ua.status,
                   u.last_name, u.first_name, u.middle_name
            FROM UserActions ua
            JOIN Users u ON ua.user_id = u.id";

                var rows = new List<string>();
                rows.Add("FullName,Title,ActionDate,Status"); // Заголовки

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string fullName = $"{reader.GetString(3)} {reader.GetString(4)} {reader.GetString(5)}";
                            string title = reader.GetString(0);
                            string actionDate = reader.GetDateTime(1).ToString("yyyy-MM-dd");
                            string status = reader.GetString(2);

                            rows.Add($"{fullName},{title},{actionDate},{status}");
                        }
                    }
                }

                // Путь к рабочему столу
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "UserActionsExport.csv");

                await File.WriteAllLinesAsync(filePath, rows);
                await DisplayAlert("Успех", $"Файл экспортирован:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка экспорта", ex.Message, "OK");
            }
        }


        private async Task ClearAllActions()
        {
            try
            {
                string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
                string query = "DELETE FROM UserActions";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка очистки данных", ex.Message, "OK");
            }
        }
    }


}
