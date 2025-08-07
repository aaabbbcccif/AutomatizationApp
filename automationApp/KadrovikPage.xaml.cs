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
    { "���������� �����������", new List<string> { "�������� �����", "�����������", "����� ������������" } },
    { "HR-�����������", new List<string> { "�������� �����",  "����� ���������� ����������", "����� ������", "�����������" } },
    { "���������������� �����������", new List<string> { "�������� �����",  "����� ������������ � ������ �����", "����������� ������", "������ ������������", "������ �������������� �����������" } },
    { "����������� ��������", new List<string> { "�������� �����",  "������������� �����������" } },
    { "����������� ��������� � �������", new List<string> { "�������� �����", "�������", "���������" } },
    { "����������� �����������", new List<string> { "�������� �����",  "����� ������������ � ����������", "���������������-����������� �����", "����� �������������� ����������", "������ �� ������� ������������", "������� ������������ ������������ � ������� ������������" } },
    { "����������� ������", new List<string> { "�������� �����", "������������ �����", "����������� ��������� �����������" } },
    { "���������������� �����������", new List<string> { "�������� �����", "����� ������������ ������������", "��� ����������������", "������� ����������������" } }
};


        private Dictionary<string, List<string>> positions = new Dictionary<string, List<string>>
{
    { "�����������", new List<string> { "���������", "������� ���������", "��������", "���������� ��������", "���������", "��������� ������" } },

    { "������������� �����������", new List<string> { "�������-�������������", "��������", "���������� �� ��������", "������-��������", "��������� �����������" } },

    { "�������", new List<string> { "�������� �� ��������", "������� ��������", "���������� �� ��������", "�������� �������", "��������� ������" } },

    { "���������", new List<string> { "��������", "������", "���������� �� ������������ ���������", "��������� ������", "����������� ��������" } },

    { "����� ������������ � ����������", new List<string> { "�������������", "�����������", "�������-�����������", "�������������", "��������� ������" } },

    { "���������������-����������� �����", new List<string> { "�������", "������", "��������� ������", "��������", "������ �����" } },

    { "������������ �����", new List<string> { "������������", "��������", "���������� �� ����������������", "����������� ��������", "��������� ������" } },

    { "����������� ��������� �����������", new List<string> { "�������", "�������� �� ���������", "������-�������", "������ ���������", "��������� �����������" } },

    { "����� ������������ � ������ �����", new List<string> { "������� �� ������ �����", "���������� �� ������� ������������", "��������� �� ������ �����", "��������� ������" } },

    { "����������� ������", new List<string> { "��������", "����", "����������� ������", "���������� ����", "���������� ����������� �������" } },

    { "����� �������������� ����������", new List<string> { "��������� �������������", "�����������", "������������ IT-������", "����������� ����������", "�������-�����������" } },

    { "����� ������������", new List<string> { "���������� ���������", "��������", "���������", "���������� �� �������", "��������� ������" } },

    { "����� ���������� ����������", new List<string> { "HR-��������", "��������", "���������� �� ���������", "������-������ �� HR", "��������� ������" } },

    { "����� ������", new List<string> { "��������� ������ ������", "��������", "���������� �� ������", "�������� �� ���������" } },

    { "�����������", new List<string> { "���������", "����-��������", "�������� ������������", "�����������������" } },

    { "������ ������������", new List<string> { "��������", "��������� ������ ������������", "���������", "���������� �� ���������������" } },

    { "������ �������������� �����������", new List<string> { "������", "������� �� ������", "���������", "�������", "������ �� ������������" } },

    { "������ �� ������� ������������", new List<string> { "������� �� �������", "������ �� ������������", "��������� ������ �������" } },

    { "������� ������������ ������������ � ������� ������������", new List<string> {
        "������� �� ������� ������������",
        "�������-�������� �� ������� �������������������",
        "������ �� ������������",
        "�������",
        "��������������",
        "��������� �������"
    } },

    { "����� ������������ ������������", new List<string> { "�����������", "�������", "����������� ������������", "��������� ������" } },

    { "��� ����������������", new List<string> {
        "�������",
        "������",
        "��������� �����",
        "�������� ������",
        "��������� ����",
        "�������������",
        "�������� ������������"
    } },

    { "������� ����������������", new List<string> {
        "������",
        "�����������",
        "�������",
        "�������� ������� � ���",
        "��������� ��������",
        "�������",
        "��������� �������"
    } }
};



        private string currentHrOperation; // ���������� ��� �������� ������� �������� ��������

        public KadrovikPage(int currentUserId)
        {
            InitializeComponent();
            this.currentUserId = currentUserId;


            DepartmentPicker.ItemsSource = new List<string>
{
    "�������� �����������",
    "���������� �����������",
    "HR-�����������",
    "���������������� �����������",
    "����������� ��������",
    "����������� ��������� � �������",
    "����������� �����������",
    "����������� ������",
    "���������������� �����������"
};

            OtdelPicker.ItemsSource = new List<string> { "�������� �����" };
            DolgnostPicker.ItemsSource = new List<string> { "�������� ���������" };

            DepartmentPicker.SelectedIndex = 0;
            OtdelPicker.SelectedIndex = 0;
            DolgnostPicker.SelectedIndex = 0;
            OtdelPicker.IsEnabled = false;
            DolgnostPicker.IsEnabled = false;

            // ������� ������ "����� ���������" �������� ����������
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
                    await DisplayAlert("������", $"�� ������� ��������� ����� �������������: {ex.Message}", "��");
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
                    await DisplayAlert("������", $"�� ������� ��������� ������ ������������: {ex.Message}", "��");
                }
            }

            return null;
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            List<string> userNames = await LoadUserNames();
            if (userNames.Count > 0)
            {
                string selectedUser = await DisplayActionSheet("�������� ������������", "������", null, userNames.ToArray());
                if (selectedUser != null && selectedUser != "������")
                {
                    // ��������� ������ ���������� ������������ �� ���� ������
                    var userInfo = await LoadUserInfo(selectedUser);
                    if (userInfo != null)
                    {
                        // ��������� ���� ����� ������� ������������
                        UserIdEntry.Text = userInfo.Id.ToString(); // ���������� Id ������ UserId
                        FirstNameEntry.Text = userInfo.First_name;
                        LastNameEntry.Text = userInfo.Last_name;
                        MiddleNameEntry.Text = userInfo.Middle_name;
                        PhoneNumberEntry.Text = userInfo.Phone_number;

                        // ����������� ���� �������� � ������ "dd.MM.yyyy"
                        DateTime birthDate;
                        if (DateTime.TryParse(userInfo.Birth_date, out birthDate))
                        {
                            BirthdayEntry.Text = birthDate.ToString("dd.MM.yyyy");
                        }
                        else
                        {
                            BirthdayEntry.Text = ""; // ���� ���� ������������, ��������� ���� ������
                        }

                        // ��������� ������ ������� ������������
                        DepartmentPicker.SelectedItem = userInfo.Department;
                        OtdelPicker.SelectedItem = userInfo.Division;
                        DolgnostPicker.SelectedItem = userInfo.Position;
                    }
                    else
                    {
                        await DisplayAlert("������", "�� ������� ��������� ������ ������������", "��");
                    }
                }
            }
            else
            {
                await DisplayAlert("������", "�� ������� �������������", "��");
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
                        DisplayAlert("�����", "��������� �������� � ���� ������", "��");

                        // �������� ��������� ����������� user_id
                        string getUserIdQuery = "SELECT TOP 1 id FROM Users ORDER BY id DESC";
                        using (SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, conn))
                        {
                            int userId = (int)getUserIdCmd.ExecuteScalar();

                            // ��������� ������ � �������������� �������
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
                            // ��������� ���� ������������ � ������� UserRoles
                            AddRoleToDatabase(userId, position);


                            // ��������� ������ � ������� Auth
                            AddAuthToDatabase(firstName, lastName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("������", $"��������� ������ ��� ����������: {ex.Message}", "��");
            }
        }

        private void AddAuthToDatabase(string firstName, string lastName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ��������� ������ � ������
                    string login = GenerateLogin(firstName, lastName);
                    string password = GeneratePassword();

                    // ��������� ���������� ������������ user_id
                    string getUserIdQuery = "SELECT TOP 1 id FROM Users ORDER BY id DESC";
                    using (SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, conn))
                    {
                        int userId = (int)getUserIdCmd.ExecuteScalar();

                        // ������� ������ � ������� Auth
                        string insertAuthQuery = @"INSERT INTO Auth (user_id, login, password) VALUES (@UserId, @Login, @Password)";
                        using (SqlCommand insertAuthCmd = new SqlCommand(insertAuthQuery, conn))
                        {
                            insertAuthCmd.Parameters.AddWithValue("@UserId", userId);
                            insertAuthCmd.Parameters.AddWithValue("@Login", login);
                            insertAuthCmd.Parameters.AddWithValue("@Password", password);

                            insertAuthCmd.ExecuteNonQuery();
                            DisplayAlert("�����", $"�����: {login}, ������: {password}", "��");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("������", $"��������� ������ ��� ���������� � ������� Auth: {ex.Message}", "��");
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

            // �������� �� ������ ����
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(middleName) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(birthDate))
            {
                await DisplayAlert("������", "��� ���� ������ ���� ���������.", "��");
                return;
            }

            // �������� ����� � ������� �� ��������
            if (!IsLatin(firstName) || !IsLatin(lastName))
            {
                await DisplayAlert("������", "��� � ������� ������ ���� �� ��������.", "��");
                return;
            }

            // �������� �������� �� ���������
            if (!IsCyrillic(middleName))
            {
                await DisplayAlert("������", "�������� ������ ���� �� ���������.", "��");
                return;
            }

            // �������� ������ �������� �� 11 ����
            if (!IsPhoneNumberValid(phoneNumber))
            {
                await DisplayAlert("������", "����� �������� ������ �������� �� 11 ����.", "��");
                return;
            }

            // �������� ������������ ���� ��������
            DateTime parsedDate;
            if (!DateTime.TryParseExact(birthDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                await DisplayAlert("������", "������� ���������� ���� �������� (��.��.����).", "��");
                return;
            }

            if (AddUserLabel.Text == "�������")
            {
                if (string.IsNullOrWhiteSpace(UserIdEntry.Text))
                {
                    await DisplayAlert("������", "������� ID ���������� ��� ��������.", "��");
                    return;
                }

                int userId = int.Parse(UserIdEntry.Text);
                await DeleteUserFromDatabase(userId);
            }
            else if (AddUserLabel.Text == "��������")
            {
                if (string.IsNullOrWhiteSpace(UserIdEntry.Text))
                {
                    await DisplayAlert("������", "������� ID ���������� ��� ����������.", "��");
                    return;
                }

                int userId = int.Parse(UserIdEntry.Text);
                await UpdateUser(userId, firstName, lastName, middleName, phoneNumber, department, division, position, parsedDate.ToString("yyyy-MM-dd"));
                await LogUserAction(currentUserId, "������� ������������", $"������������ � ID {userId} ��������� � ����� {division} �� ��������� {position}", "�� ���������");
            }
            else if (AddUserLabel.Text == "��������")
            {
                await AddUserToDatabase(firstName, lastName, middleName, phoneNumber, department, division, position, parsedDate.ToString("yyyy-MM-dd"));
                await LogUserAction(currentUserId, "���������� ������������", $"�������� ������������ � ID {UserIdEntry.Text} � ����� {division} �� ��������� {position}", "�� ���������");
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
                            await DisplayAlert("�����", "������ ������������ ���������", "��");
                        }
                        else
                        {
                            await DisplayAlert("������", "������������ � ��������� ID �� ������", "��");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating user: {ex.Message}");
                    await DisplayAlert("������", $"�� ������� �������� ������ ������������: {ex.Message}", "��");
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
                DisplayAlert("������", $"��������� ������ ��� ���������� ������: {ex.Message}", "��");
            }
        }



        private void OnEmployeeTypeButtonClicked(object sender, EventArgs e)
        {
            // �������� ��� ������ � ���������� ���������
            ResetEmployeeTypeButtons();

            // ������������ ������� ������
            if (sender is Button clickedButton)
            {
                clickedButton.BackgroundColor = Color.FromArgb("#4B164C"); // ���������� ���
                clickedButton.TextColor = Colors.White; // ����� �����
                clickedButton.BorderColor = Colors.Transparent; // ��� �����

                // ��������� ������� �������� ��������
                currentHrOperation = clickedButton.Text; // ������������� currentHrOperation � ������������ � ������� ������

                // ������ ��� �����������/������� ���� UserID
                if (clickedButton == NewEmployeeButton)
                {
                    UserIdFrame.IsVisible = false; // ����� ��������� � ���� ��������
                    AddUserLabel.Text = "��������";
                }
                else if (clickedButton == TransferredEmployeeButton)
                {
                    UserIdFrame.IsVisible = true; // ����������� ��������� � ���� ����������
                    AddUserLabel.Text = "��������";
                }
                else if (clickedButton == DeletedEmployeeButton)
                {
                    UserIdFrame.IsVisible = true; // �������� ��������� � ���� ����������
                    AddUserLabel.Text = "�������";
                }
            }
        }

        private void ResetEmployeeTypeButtons()
        {
            // ����� ���������
            NewEmployeeButton.BackgroundColor = Colors.White;
            NewEmployeeButton.TextColor = Color.FromArgb("#4B164C");
            NewEmployeeButton.BorderColor = Color.FromArgb("#4B164C");

            // ������������ ���������
            TransferredEmployeeButton.BackgroundColor = Colors.White;
            TransferredEmployeeButton.TextColor = Color.FromArgb("#4B164C");
            TransferredEmployeeButton.BorderColor = Color.FromArgb("#4B164C");

            // ��������� ���������
            DeletedEmployeeButton.BackgroundColor = Colors.White;
            DeletedEmployeeButton.TextColor = Color.FromArgb("#4B164C");
            DeletedEmployeeButton.BorderColor = Color.FromArgb("#4B164C");
        }

        // �������� �� ��������
        private bool IsLatin(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z]+$");
        }

        // �������� �� ���������
        private bool IsCyrillic(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[�-��-�]+$");
        }

        // �������� ������ �������� �� 11 ����
        private bool IsPhoneNumberValid(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d{11}$");
        }

        // ����� ��� ������ �������� � ���������� ��������� ����������
        private void ResetPickerValues()
        {
            OtdelPicker.ItemsSource = new List<string> { "�������� �����" };
            OtdelPicker.SelectedIndex = 0;
            OtdelPicker.IsEnabled = false;

            DolgnostPicker.ItemsSource = new List<string> { "�������� ���������" };
            DolgnostPicker.SelectedIndex = 0;
            DolgnostPicker.IsEnabled = false;
        }

        // ���������� ������ ������������
        private void DepartmentPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            OtdelPicker.SelectedIndex = 0;
            if (DepartmentPicker.SelectedIndex == 0)
            {
                ResetPickerValues(); // ���������� �������� ��� ������ "�������� �����������"
                return;
            }

            // ���� ����������� ������
            if (DepartmentPicker.SelectedIndex != -1)
            {
                // ������������ ����� ������
                OtdelPicker.IsEnabled = true;

                // �������� ��������� �����������
                string selectedDepartment = DepartmentPicker.SelectedItem.ToString();

                // ��������� ������ ��� ���������� ������������
                if (departments.ContainsKey(selectedDepartment))
                {
                    OtdelPicker.ItemsSource = departments[selectedDepartment];
                    OtdelPicker.SelectedIndex = 0; // ���������� ��������� �����
                }
                else
                {
                    ResetPickerValues(); // ���� ����������� �� ������, ���������� ���
                }
            }
        }

        // ���������� ������ ������
        private void OtdelPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            DolgnostPicker.SelectedIndex = 0;
            if (OtdelPicker.SelectedIndex == 0)
            {
                DolgnostPicker.ItemsSource = new List<string> { "�������� ���������" };
                DolgnostPicker.SelectedIndex = 0;
                DolgnostPicker.IsEnabled = false;
                return;
            }

            // ���� ����� ������
            if (OtdelPicker.SelectedIndex != -1)
            {
                // ������������ ����� ���������
                DolgnostPicker.IsEnabled = true;

                // �������� ��������� �����
                string selectedDepartmentDetail = OtdelPicker.SelectedItem.ToString();

                // ��������, ���������� �� ����� � �������
                if (positions.ContainsKey(selectedDepartmentDetail))
                {
                    // ��������� ��������� ��� ���������� ������
                    DolgnostPicker.ItemsSource = positions[selectedDepartmentDetail];
                    DolgnostPicker.SelectedIndex = 0; // ���������� ��������� ���������
                }
                else
                {
                    DolgnostPicker.ItemsSource = new List<string> { "�������� ���������" };
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

                    // ����������� ���� �� ������ ���������
                    string role = DetermineRole(position);

                    // ������� ������ � ������� UserRoles
                    string insertRoleQuery = @"INSERT INTO UserRoles (user_id, role_name) VALUES (@UserId, @RoleName)";
                    using (SqlCommand insertRoleCmd = new SqlCommand(insertRoleQuery, conn))
                    {
                        insertRoleCmd.Parameters.AddWithValue("@UserId", userId);
                        insertRoleCmd.Parameters.AddWithValue("@RoleName", role);

                        insertRoleCmd.ExecuteNonQuery();
                        DisplayAlert("�����", $"���� '{role}' ��������� ��� ������������ � ID {userId}", "��");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("������", $"��������� ������ ��� ���������� ����: {ex.Message}", "��");
            }
        }

        private string DetermineRole(string position)
        {
            if (position == "��������" || position == "���������� �� ������" || position == "�������� �� ���������")
            {
                return "��������";
            }
            else if (position == "������������ it ������")
            {
                return "�����";
            }
            else if (YesOrNoButton.Text == "��")
            {
                return "������������ �������������";
            }
            else if (position == "��������� �������������")
            {
                return "�����������";
            }
            else
            {
                return "���������";
            }
        }



        // ���������� ��������� ������ � Picker
        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is Picker picker)
                {
                    string selectedItem = picker.SelectedItem?.ToString() ?? "";

                    // ���������: ������ �� ������������ "�������� ..."
                    if (selectedItem.StartsWith("��������"))
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
                // ������������ ������, ���� ��� ��������
                Debug.WriteLine($"������: {ex.Message}");
            }
        }

        private void YesOrNoButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            if (button.Text == "���")
            {
                button.Text = "��";
                button.BackgroundColor = Color.FromArgb("#4B164C");
                button.TextColor = Colors.White;
                button.BorderColor = Color.FromArgb("#4B164C");
            }
            else
            {
                button.Text = "���";
                button.BackgroundColor = Colors.White;
                button.TextColor = Colors.Black;
                button.BorderColor = Color.FromArgb("#4B164C");
            }
        }
        private async Task DeleteUserFromDatabase(int userId)
        {
            string connectionString = "Server=Vivi; Database=automationDB; Integrated Security=True;";

            // ������ ������, �� ������� ����� ������� ������
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
                        await DisplayAlert("�����", $"������������ � ID {userId} ������ �� ���� ������", "��");
                        await LogUserAction(currentUserId, "�������� ������������", $"������������ {LastNameEntry.Text} {FirstNameEntry.Text} {MiddleNameEntry.Text} � ID {UserIdEntry.Text} ������ �� ���� ������", "�� ���������");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("������", $"��������� ������ ��� �������� ������������: {ex.Message}", "��");
                }
            }
        }
    }
}