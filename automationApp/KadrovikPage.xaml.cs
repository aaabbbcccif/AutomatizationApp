using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Google.Apis.Drive.v3.Data;

namespace automationApp
{
    public partial class KadrovikPage : ContentPage
    {
        private readonly int currentUserId;

        private Dictionary<string, List<string>> departments = new Dictionary<string, List<string>>
{
    { "Финансовый департамент", new List<string> { "Выберите отдел", "Бухгалтерия", "Отдел контроллинга" } },
    { "HR-департамент", new List<string> { "Выберите отдел",  "Отдел управления персоналом", "Отдел кадров", "Секретариат" } },
    { "Административный департамент", new List<string> { "Выберите отдел",  "Отдел безопасности и охраны труда", "Медицинская служба", "Служба безопасности", "Служба хозяйственного обеспечения" } },
    { "Департамент качества", new List<string> { "Выберите отдел",  "Испытательная лаборатория" } },
    { "Департамент логистики и закупок", new List<string> { "Выберите отдел", "Закупки", "Логистика" } },
    { "Департамент технический", new List<string> { "Выберите отдел",  "Отдел исследования и разработок", "Производственно-технический отдел", "Отдел информационных технологий", "Служба по ремонту оборудования", "Участок технического обслуживания и ремонта оборудования" } },
    { "Департамент продаж", new List<string> { "Выберите отдел", "Операционный отдел", "Техническая поддержка направлений" } },
    { "Производственный департамент", new List<string> { "Выберите отдел", "Отдел планирования производства", "Цех производственный", "Участок металлообработки" } }
};


        private Dictionary<string, List<string>> positions = new Dictionary<string, List<string>>
{
    { "Бухгалтерия", new List<string> { "Бухгалтер", "Главный бухгалтер", "Кадровик", "Финансовый аналитик", "Экономист", "Начальник отдела" } },

    { "Испытательная лаборатория", new List<string> { "Инженер-исследователь", "Лаборант", "Специалист по качеству", "Техник-лаборант", "Начальник лаборатории" } },

    { "Закупки", new List<string> { "Менеджер по закупкам", "Старший менеджер", "Специалист по тендерам", "Аналитик закупок", "Начальник отдела" } },

    { "Логистика", new List<string> { "Менеджер", "Логист", "Специалист по транспортной логистике", "Начальник отдела", "Координатор поставок" } },

    { "Отдел исследования и разработок", new List<string> { "Исследователь", "Разработчик", "Инженер-разработчик", "Проектировщик", "Начальник отдела" } },

    { "Производственно-технический отдел", new List<string> { "Инженер", "Техник", "Начальник отдела", "Технолог", "Мастер смены" } },

    { "Операционный отдел", new List<string> { "Операционист", "Менеджер", "Специалист по документообороту", "Координатор операций", "Начальник отдела" } },

    { "Техническая поддержка направлений", new List<string> { "Инженер", "Менеджер по поддержке", "Сервис-инженер", "Техник поддержки", "Начальник направления" } },

    { "Отдел безопасности и охраны труда", new List<string> { "Инженер по охране труда", "Специалист по технике безопасности", "Инспектор по охране труда", "Начальник отдела" } },

    { "Медицинская служба", new List<string> { "Фельдшер", "Врач", "Медицинская сестра", "Санитарный врач", "Заведующий медицинской службой" } },

    { "Отдел информационных технологий", new List<string> { "Системный администратор", "Разработчик", "Руководитель IT-отдела", "Технический специалист", "Инженер-программист" } },

    { "Отдел контроллинга", new List<string> { "Финансовый контролер", "Аналитик", "Экономист", "Специалист по бюджету", "Начальник отдела" } },

    { "Отдел управления персоналом", new List<string> { "HR-менеджер", "Рекрутер", "Специалист по персоналу", "Бизнес-партнёр по HR", "Начальник отдела" } },

    { "Отдел кадров", new List<string> { "Инспектор отдела кадров", "Кадровик", "Специалист по кадрам", "Менеджер по персоналу" } },

    { "Секретариат", new List<string> { "Секретарь", "Офис-менеджер", "Помощник руководителя", "Делопроизводитель" } },

    { "Служба безопасности", new List<string> { "Охранник", "Начальник службы безопасности", "Инспектор", "Специалист по видеонаблюдению" } },

    { "Служба хозяйственного обеспечения", new List<string> { "Завхоз", "Рабочий по зданию", "Кладовщик", "Уборщик", "Техник по обслуживанию" } },

    { "Служба по ремонту оборудования", new List<string> { "Инженер по ремонту", "Техник по обслуживанию", "Начальник службы ремонта" } },

    { "Участок технического обслуживания и ремонта оборудования", new List<string> {
        "Слесарь по ремонту оборудования",
        "Слесарь-электрик по ремонту электрооборудования",
        "Техник по обслуживанию",
        "Механик",
        "Электромеханик",
        "Начальник участка"
    } },

    { "Отдел планирования производства", new List<string> { "Планировщик", "Инженер", "Координатор производства", "Начальник отдела" } },

    { "Цех производственный", new List<string> {
        "Рабочий",
        "Мастер",
        "Контролер линии",
        "Оператор станка",
        "Начальник цеха",
        "Электромонтер",
        "Наладчик оборудования"
    } },

    { "Участок металлообработки", new List<string> {
        "Токарь",
        "Фрезеровщик",
        "Механик",
        "Оператор станков с ЧПУ",
        "Контролер качества",
        "Слесарь",
        "Начальник участка"
    } }
};



        private string currentHrOperation; // Переменная для хранения текущей кадровой операции

        public KadrovikPage(int currentUserId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;


            DepartmentPicker.ItemsSource = new List<string>
{
    "Выберите департамент",
    "Финансовый департамент",
    "HR-департамент",
    "Административный департамент",
    "Департамент качества",
    "Департамент логистики и закупок",
    "Департамент технический",
    "Департамент продаж",
    "Производственный департамент"
};

            OtdelPicker.ItemsSource = new List<string> { "Выберите отдел" };
            DolgnostPicker.ItemsSource = new List<string> { "Выберите должность" };

            DepartmentPicker.SelectedIndex = 0;
            OtdelPicker.SelectedIndex = 0;
            DolgnostPicker.SelectedIndex = 0;
            OtdelPicker.IsEnabled = false;
            DolgnostPicker.IsEnabled = false;

            // Сделать кнопку "Новый сотрудник" активной изначально
            OnEmployeeTypeButtonClicked(NewEmployeeButton, EventArgs.Empty);
                                    
        }

        string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";

        

        private async Task<List<string>> LoadUserNames()
        {
            List<string> userNames = new List<string>();
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = "SELECT last_name + ' ' + first_name + ' ' + middle_name AS FullName FROM Users";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        userNames.Add(reader["FullName"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading user names: {ex.Message}");
                    await DisplayAlert("Ошибка", $"Не удалось загрузить имена пользователей: {ex.Message}", "ОК");
                }
            }

            return userNames;
        }

        private async Task<User> LoadUserInfo(string fullName)
        {
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = @"
        SELECT id, first_name, last_name, middle_name, phone_number, department, division, position, birth_date
        FROM Users
        WHERE last_name + ' ' + first_name + ' ' + middle_name = @FullName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FullName", fullName);

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            First_name = reader["first_name"].ToString(),
                            Last_name = reader["last_name"].ToString(),
                            Middle_name = reader["middle_name"].ToString(),
                            Phone_number = reader["phone_number"].ToString(),
                            Department = reader["department"].ToString(),
                            Division = reader["division"].ToString(),
                            Position = reader["position"].ToString(),
                            Birth_date = reader["birth_date"].ToString()
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading user info: {ex.Message}");
                    await DisplayAlert("Ошибка", $"Не удалось загрузить данные пользователя: {ex.Message}", "ОК");
                }
            }

            return null;
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            List<string> userNames = await LoadUserNames();
            if (userNames.Count > 0)
            {
                string selectedUser = await DisplayActionSheet("Выберите пользователя", "Отмена", null, userNames.ToArray());
                if (selectedUser != null && selectedUser != "Отмена")
                {
                    // Загрузить данные выбранного пользователя из базы данных
                    var userInfo = await LoadUserInfo(selectedUser);
                    if (userInfo != null)
                    {
                        // Заполнить поля формы данными пользователя
                        UserIdEntry.Text = userInfo.Id.ToString(); // Используем Id вместо UserId
                        FirstNameEntry.Text = userInfo.First_name;
                        LastNameEntry.Text = userInfo.Last_name;
                        MiddleNameEntry.Text = userInfo.Middle_name;
                        PhoneNumberEntry.Text = userInfo.Phone_number;

                        // Форматируем дату рождения в формат "dd.MM.yyyy"
                        DateTime birthDate;
                        if (DateTime.TryParse(userInfo.Birth_date, out birthDate))
                        {
                            BirthdayEntry.Text = birthDate.ToString("dd.MM.yyyy");
                        }
                        else
                        {
                            BirthdayEntry.Text = ""; // Если дата некорректная, оставляем поле пустым
                        }

                        // Заполнить пикеры данными пользователя
                        DepartmentPicker.SelectedItem = userInfo.Department;
                        OtdelPicker.SelectedItem = userInfo.Division;
                        DolgnostPicker.SelectedItem = userInfo.Position;
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Не удалось загрузить данные пользователя", "ОК");
                    }
                }
            }
            else
            {
                await DisplayAlert("Ошибка", "Не найдено пользователей", "ОК");
            }
        }
        private async Task AddUserToDatabase(string firstName, string lastName, string middleName, string phoneNumber, string department, string division, string position, string birthDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Users (first_name, last_name, middle_name, phone_number, department, division, position, birth_date, hr_operation)
                                     VALUES (@FirstName, @LastName, @MiddleName, @PhoneNumber, @Department, @Division, @Position, @BirthDate, @HrOperation)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@MiddleName", middleName);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@Department", department);
                        cmd.Parameters.AddWithValue("@Division", division);
                        cmd.Parameters.AddWithValue("@Position", position);
                        cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                        cmd.Parameters.AddWithValue("@HrOperation", currentHrOperation);

                        cmd.ExecuteNonQuery();
                        DisplayAlert("Успех", "Сотрудник добавлен в базу данных", "ОК");

                        // Получаем последний добавленный user_id
                        string getUserIdQuery = "SELECT TOP 1 id FROM Users ORDER BY id DESC";
                        using (SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, conn))
                        {
                            int userId = (int)getUserIdCmd.ExecuteScalar();

                            // Добавляем записи в дополнительные таблицы
                            AddRightsRequestToDatabase(userId);
                            AddRightsExecutionToDatabase(userId);
                            AddAdditionalFileServerRequestToDatabase(userId);
                            AddAdditionalFileServerExecutionToDatabase(userId);
                            AddAdditionalDomainRequestToDatabase(userId);
                            AddAdditionalDomainExecutionToDatabase(userId);
                            AddAdditionalBitrixRequestToDatabase(userId);
                            AddAdditionalBitrixExecutionToDatabase(userId);
                            AddAdditional1CRequestToDatabase(userId);
                            AddAdditional1CExecutionToDatabase(userId);
                            // Добавляем роль пользователя в таблицу UserRoles
                            AddRoleToDatabase(userId, position);


                            // Добавляем запись в таблицу Auth
                            AddAuthToDatabase(firstName, lastName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Произошла ошибка при добавлении: {ex.Message}", "ОК");
            }
        }

        private void AddAuthToDatabase(string firstName, string lastName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Генерация логина и пароля
                    string login = GenerateLogin(firstName, lastName);
                    string password = GeneratePassword();

                    // Получение последнего добавленного user_id
                    string getUserIdQuery = "SELECT TOP 1 id FROM Users ORDER BY id DESC";
                    using (SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, conn))
                    {
                        int userId = (int)getUserIdCmd.ExecuteScalar();

                        // Вставка данных в таблицу Auth
                        string insertAuthQuery = @"INSERT INTO Auth (user_id, login, password) VALUES (@UserId, @Login, @Password)";
                        using (SqlCommand insertAuthCmd = new SqlCommand(insertAuthQuery, conn))
                        {
                            insertAuthCmd.Parameters.AddWithValue("@UserId", userId);
                            insertAuthCmd.Parameters.AddWithValue("@Login", login);
                            insertAuthCmd.Parameters.AddWithValue("@Password", password);

                            insertAuthCmd.ExecuteNonQuery();
                            DisplayAlert("Успех", $"Логин: {login}, Пароль: {password}", "ОК");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Произошла ошибка при добавлении в таблицу Auth: {ex.Message}", "ОК");
            }
        }

        private string GenerateLogin(string firstName, string lastName)
        {
            return $"{firstName[0].ToString().ToLower()}{lastName.ToLower()}";
        }

        private string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }



        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            string firstName = FirstNameEntry.Text;
            string lastName = LastNameEntry.Text;
            string middleName = MiddleNameEntry.Text;
            string phoneNumber = PhoneNumberEntry.Text;
            string department = DepartmentPicker.SelectedItem?.ToString();
            string division = OtdelPicker.SelectedItem?.ToString();
            string position = DolgnostPicker.SelectedItem?.ToString();
            string birthDate = BirthdayEntry.Text;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(middleName) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(birthDate))
            {
                await DisplayAlert("Ошибка", "Все поля должны быть заполнены.", "ОК");
                return;
            }

            // Проверка имени и фамилии на латиницу
            if (!IsLatin(firstName) || !IsLatin(lastName))
            {
                await DisplayAlert("Ошибка", "Имя и фамилия должны быть на латинице.", "ОК");
                return;
            }

            // Проверка отчества на кириллицу
            if (!IsCyrillic(middleName))
            {
                await DisplayAlert("Ошибка", "Отчество должно быть на кириллице.", "ОК");
                return;
            }

            // Проверка номера телефона на 11 цифр
            if (!IsPhoneNumberValid(phoneNumber))
            {
                await DisplayAlert("Ошибка", "Номер телефона должен состоять из 11 цифр.", "ОК");
                return;
            }

            // Проверка корректности даты рождения
            DateTime parsedDate;
            if (!DateTime.TryParseExact(birthDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                await DisplayAlert("Ошибка", "Введите корректную дату рождения (дд.мм.гггг).", "ОК");
                return;
            }

            if (AddUserLabel.Text == "Удалить")
            {
                if (string.IsNullOrWhiteSpace(UserIdEntry.Text))
                {
                    await DisplayAlert("Ошибка", "Введите ID сотрудника для удаления.", "ОК");
                    return;
                }

                int userId = int.Parse(UserIdEntry.Text);
                await DeleteUserFromDatabase(userId);
            }
            else if (AddUserLabel.Text == "Обновить")
            {
                if (string.IsNullOrWhiteSpace(UserIdEntry.Text))
                {
                    await DisplayAlert("Ошибка", "Введите ID сотрудника для обновления.", "ОК");
                    return;
                }

                int userId = int.Parse(UserIdEntry.Text);
                await UpdateUser(userId, firstName, lastName, middleName, phoneNumber, department, division, position, parsedDate.ToString("yyyy-MM-dd"));
                await LogUserAction(currentUserId, "Перевод пользователя", $"Пользователь с ID {userId} переведен в отдел {division} на должность {position}", "Не прочитано");
            }
            else if (AddUserLabel.Text == "Добавить")
            {
                await AddUserToDatabase(firstName, lastName, middleName, phoneNumber, department, division, position, parsedDate.ToString("yyyy-MM-dd"));
                await LogUserAction(currentUserId, "Добавление пользователя", $"Добавлен пользователь с ID {UserIdEntry.Text} в отдел {division} на должность {position}", "Не прочитано");
            }
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

        private async Task UpdateUser(int userId, string firstName, string lastName, string middleName, string phoneNumber, string department, string division, string position, string birthDate)
        {
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";
            string query = @"
        UPDATE Users
        SET first_name = @FirstName,
            last_name = @LastName,
            middle_name = @MiddleName,
            phone_number = @PhoneNumber,
            department = @Department,
            division = @Division,
            position = @Position,
            birth_date = @BirthDate,
            hr_operation = @HrOperation
        WHERE id = @UserId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@MiddleName", middleName);
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@Department", department);
                        command.Parameters.AddWithValue("@Division", division);
                        command.Parameters.AddWithValue("@Position", position);
                        command.Parameters.AddWithValue("@BirthDate", birthDate);
                        command.Parameters.AddWithValue("@HrOperation", currentHrOperation);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            await DisplayAlert("Успех", "Данные пользователя обновлены", "ОК");
                        }
                        else
                        {
                            await DisplayAlert("Ошибка", "Пользователь с указанным ID не найден", "ОК");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating user: {ex.Message}");
                    await DisplayAlert("Ошибка", $"Не удалось обновить данные пользователя: {ex.Message}", "ОК");
                }
            }
        }



        private void AddRightsRequestToDatabase(int userId)
        {
            string query = @"INSERT INTO RightsRequest (user_id, email, bitrix, domain, c1, vpn, teflex, jira, radius, google, phonebook, atc, fileserver)
                             VALUES (@UserId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddRightsExecutionToDatabase(int userId)
        {
            string query = @"INSERT INTO RightsExecution (user_id, email, bitrix, domain, c1, vpn, teflex, jira, radius, google, phonebook, atc, fileserver)
                             VALUES (@UserId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditionalFileServerRequestToDatabase(int userId)
        {
            string query = @"INSERT INTO AdditionalFileServerRequest (user_id, folder_name1, folder_name2, folder_name3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditionalFileServerExecutionToDatabase(int userId)
        {
            string query = @"INSERT INTO AdditionalFileServerExecution (user_id, folder_name1, folder_name2, folder_name3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditionalDomainRequestToDatabase(int userId)
        {
            string query = @"INSERT INTO AdditionalDomainRequest (user_id, group1, group2, group3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditionalDomainExecutionToDatabase(int userId)
        {
            string query = @"INSERT INTO AdditionalDomainExecution (user_id, group1, group2, group3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditionalBitrixRequestToDatabase(int userId)
        {
            string query = @"INSERT INTO AdditionalBitrixRequest (user_id, crm1, crm2, crm3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditionalBitrixExecutionToDatabase(int userId)
        {
            string query = @"INSERT INTO AdditionalBitrixExecution (user_id, crm1, crm2, crm3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditional1CRequestToDatabase(int userId)
        {
            string query = @"INSERT INTO Additional1CRequest (user_id, department1, department2, department3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void AddAdditional1CExecutionToDatabase(int userId)
        {
            string query = @"INSERT INTO Additional1CExecution (user_id, department1, department2, department3)
                             VALUES (@UserId, 0, 0, 0)";
            ExecuteNonQuery(query, userId);
        }

        private void ExecuteNonQuery(string query, int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Произошла ошибка при добавлении записи: {ex.Message}", "ОК");
            }
        }



        private void OnEmployeeTypeButtonClicked(object sender, EventArgs e)
        {
            // Сбросить все кнопки в неактивное состояние
            ResetEmployeeTypeButtons();

            // Активировать нажатую кнопку
            if (sender is Button clickedButton)
            {
                clickedButton.BackgroundColor = Color.FromArgb("#4B164C"); // Фиолетовый фон
                clickedButton.TextColor = Colors.White; // Белый текст
                clickedButton.BorderColor = Colors.Transparent; // Без рамки

                // Сохраняем текущую кадровую операцию
                currentHrOperation = clickedButton.Text; // Устанавливаем currentHrOperation в соответствии с текстом кнопки

                // Логика для отображения/скрытия поля UserID
                if (clickedButton == NewEmployeeButton)
                {
                    UserIdFrame.IsVisible = false; // Новый сотрудник – поле скрываем
                    AddUserLabel.Text = "Добавить";
                }
                else if (clickedButton == TransferredEmployeeButton)
                {
                    UserIdFrame.IsVisible = true; // Переведённый сотрудник – поле показываем
                    AddUserLabel.Text = "Обновить";
                }
                else if (clickedButton == DeletedEmployeeButton)
                {
                    UserIdFrame.IsVisible = true; // Удалённый сотрудник – поле показываем
                    AddUserLabel.Text = "Удалить";
                }
            }
        }

        private void ResetEmployeeTypeButtons()
        {
            // Новый сотрудник
            NewEmployeeButton.BackgroundColor = Colors.White;
            NewEmployeeButton.TextColor = Color.FromArgb("#4B164C");
            NewEmployeeButton.BorderColor = Color.FromArgb("#4B164C");

            // Переведенный сотрудник
            TransferredEmployeeButton.BackgroundColor = Colors.White;
            TransferredEmployeeButton.TextColor = Color.FromArgb("#4B164C");
            TransferredEmployeeButton.BorderColor = Color.FromArgb("#4B164C");

            // Удаленный сотрудник
            DeletedEmployeeButton.BackgroundColor = Colors.White;
            DeletedEmployeeButton.TextColor = Color.FromArgb("#4B164C");
            DeletedEmployeeButton.BorderColor = Color.FromArgb("#4B164C");
        }

        // Проверка на латиницу
        private bool IsLatin(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z]+$");
        }

        // Проверка на кириллицу
        private bool IsCyrillic(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[а-яА-Я]+$");
        }

        // Проверка номера телефона на 11 цифр
        private bool IsPhoneNumberValid(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d{11}$");
        }

        // Метод для сброса значений и блокировки элементов управления
        private void ResetPickerValues()
        {
            OtdelPicker.ItemsSource = new List<string> { "Выберите отдел" };
            OtdelPicker.SelectedIndex = 0;
            OtdelPicker.IsEnabled = false;

            DolgnostPicker.ItemsSource = new List<string> { "Выберите должность" };
            DolgnostPicker.SelectedIndex = 0;
            DolgnostPicker.IsEnabled = false;
        }

        // Обработчик выбора департамента
        private void DepartmentPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            OtdelPicker.SelectedIndex = 0;
            if (DepartmentPicker.SelectedIndex == 0)
            {
                ResetPickerValues(); // Сбрасываем значения при выборе "Выберите департамент"
                return;
            }

            // Если департамент выбран
            if (DepartmentPicker.SelectedIndex != -1)
            {
                // Разблокируем выбор отдела
                OtdelPicker.IsEnabled = true;

                // Получаем выбранный департамент
                string selectedDepartment = DepartmentPicker.SelectedItem.ToString();

                // Загружаем отделы для выбранного департамента
                if (departments.ContainsKey(selectedDepartment))
                {
                    OtdelPicker.ItemsSource = departments[selectedDepartment];
                    OtdelPicker.SelectedIndex = 0; // Сбрасываем выбранный отдел
                }
                else
                {
                    ResetPickerValues(); // Если департамент не найден, сбрасываем все
                }
            }
        }

        // Обработчик выбора отдела
        private void OtdelPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            DolgnostPicker.SelectedIndex = 0;
            if (OtdelPicker.SelectedIndex == 0)
            {
                DolgnostPicker.ItemsSource = new List<string> { "Выберите должность" };
                DolgnostPicker.SelectedIndex = 0;
                DolgnostPicker.IsEnabled = false;
                return;
            }

            // Если отдел выбран
            if (OtdelPicker.SelectedIndex != -1)
            {
                // Разблокируем выбор должности
                DolgnostPicker.IsEnabled = true;

                // Получаем выбранный отдел
                string selectedDepartmentDetail = OtdelPicker.SelectedItem.ToString();

                // Проверка, существует ли отдел в словаре
                if (positions.ContainsKey(selectedDepartmentDetail))
                {
                    // Загружаем должности для выбранного отдела
                    DolgnostPicker.ItemsSource = positions[selectedDepartmentDetail];
                    DolgnostPicker.SelectedIndex = 0; // Сбрасываем выбранную должность
                }
                else
                {
                    DolgnostPicker.ItemsSource = new List<string> { "Выберите должность" };
                    DolgnostPicker.IsEnabled = false;
                }
            }
        }

        
        private void AddRoleToDatabase(int userId, string position)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Определение роли на основе должности
                    string role = DetermineRole(position);

                    // Вставка данных в таблицу UserRoles
                    string insertRoleQuery = @"INSERT INTO UserRoles (user_id, role_name) VALUES (@UserId, @RoleName)";
                    using (SqlCommand insertRoleCmd = new SqlCommand(insertRoleQuery, conn))
                    {
                        insertRoleCmd.Parameters.AddWithValue("@UserId", userId);
                        insertRoleCmd.Parameters.AddWithValue("@RoleName", role);

                        insertRoleCmd.ExecuteNonQuery();
                        DisplayAlert("Успех", $"Роль '{role}' добавлена для пользователя с ID {userId}", "ОК");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Произошла ошибка при добавлении роли: {ex.Message}", "ОК");
            }
        }

        private string DetermineRole(string position)
        {
            if (position == "Кадровик" || position == "Специалист по кадрам" || position == "Менеджер по персоналу")
            {
                return "Кадровик";
            }
            else if (position == "Руководитель it отдела")
            {
                return "Админ";
            }
            else if (YesOrNoButton.Text == "Да")
            {
                return "Руководитель подразделения";
            }
            else if (position == "Системный администратор")
            {
                return "Исполнитель";
            }
            else
            {
                return "Сотрудник";
            }
        }



        // Обработчик изменения текста в Picker
        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is Picker picker)
                {
                    string selectedItem = picker.SelectedItem?.ToString() ?? "";

                    // Проверяем: выбрал ли пользователь "Выберите ..."
                    if (selectedItem.StartsWith("Выберите"))
                    {
                        picker.TextColor = Color.FromArgb("#4A4A4A");
                    }
                    else
                    {
                        picker.TextColor = Colors.Black;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                // Обрабатываем ошибку, если она возникла
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void YesOrNoButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            if (button.Text == "Нет")
            {
                button.Text = "Да";
                button.BackgroundColor = Color.FromArgb("#4B164C");
                button.TextColor = Colors.White;
                button.BorderColor = Color.FromArgb("#4B164C");
            }
            else
            {
                button.Text = "Нет";
                button.BackgroundColor = Colors.White;
                button.TextColor = Colors.Black;
                button.BorderColor = Color.FromArgb("#4B164C");
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
                        await LogUserAction(currentUserId, "Удаление пользователя", $"Пользователь {LastNameEntry.Text} {FirstNameEntry.Text} {MiddleNameEntry.Text} с ID {UserIdEntry.Text} удален из всех таблиц", "Не прочитано");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", $"Произошла ошибка при удалении пользователя: {ex.Message}", "ОК");
                }
            }
        }
    }
}