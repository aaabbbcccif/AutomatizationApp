using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace automationApp
{
    public partial class KerioConnectPage : ContentPage
    {
        private readonly string apiUrl = "https://mail.kcep.kz:4040/admin/api/jsonrpc/";
        private string sessionId = "";
        private readonly HttpClient client = new HttpClient();

        public KerioConnectPage()
        {
            InitializeComponent();
            LoadUsersAsync();
        }

        private async Task<bool> LoginAsync()
        {
            var loginData = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "Session.login",
                @params = new
                {
                    userName = "admin@students.kcep.kz",
                    password = "37ecf1Bi5",
                    application = new
                    {
                        name = "Example: Get all domains",
                        vendor = "Kerio Technologies s.r.o.",
                        version = "BUILD_HASH"
                    }
                }
            };

            string json = JsonConvert.SerializeObject(loginData);
            var response = await client.PostAsync(apiUrl, new StringContent(json, Encoding.UTF8, "application/json-rpc"));

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
                string token = jsonResponse?.result?.token;

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Remove("X-Token");
                    client.DefaultRequestHeaders.Add("X-Token", token);
                    return true;
                }
            }

            await DisplayAlert("������", "������ �����������! ��������� ����� � ������.", "OK");
            return false;
        }

        private async Task<string> SendRequestAsync(string jsonRequest)
        {
            if (!client.DefaultRequestHeaders.Contains("X-Token"))
            {
                bool loggedIn = await LoginAsync();
                if (!loggedIn) return "������ �����������!";
            }

            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json-rpc");
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            return await response.Content.ReadAsStringAsync();
        }

        // ����� � �������� ������� (Task)
        private async Task LoadUsersAsync()
        {
            string requestJson = @"{
        ""jsonrpc"": ""2.0"",
        ""id"": 1,
        ""method"": ""Users.get"",
        ""params"": {
            ""domainId"": ""keriodb://domain/21424b88-213d-47a3-99ba-14bb4b6fd86e"",
            ""query"": {
                ""fields"": [""id"", ""loginName"", ""fullName"", ""description"", ""isEnabled""],
                ""limit"": 200
            }
        }
    }";

            string response = await SendRequestAsync(requestJson);
            dynamic result = JsonConvert.DeserializeObject(response);

            MyCollectionView.ItemsSource = null;
            if (result?.result?.list != null)
            {
                var users = new List<UserKerioConnect>();
                foreach (var user in result.result.list)
                {
                    users.Add(new UserKerioConnect
                    {
                        Id = user.id,
                        Loggin = user.loginName,
                        FullName = user.fullName,
                        Description = user.description,
                        Active = user.isEnabled
                    });
                }
                MyCollectionView.ItemsSource = users;
            }
            else
            {
                await DisplayAlert("������", "������ ��� ��������� ������ �������������.", "OK");
            }
        }

        // ���������� ������ (async void � ��� XAML)
        private async void LoadUserClick(object sender, EventArgs e)
        {
            await LoadUsersAsync();
        }

        private async void AddUserClick(object sender, EventArgs e)
        {
            string loginName = LogginEntry.Text?.Trim();
            string fullName = FullNameEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();
            string description = DescriptionEntry.Text?.Trim();
            string volumeText = VolumeEntry.Text?.Trim();

            // �������� ������
            if (string.IsNullOrWhiteSpace(loginName) || loginName.Length < 5)
            {
                await DisplayAlert("������", "����� ������ ��������� �� ����� 5 ��������.", "��");
                return;
            }

            // �������� �����
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 5)
            {
                await DisplayAlert("������", "��� ������ ��������� �� ����� 5 ��������.", "��");
                return;
            }

            // �������� ������
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 ||
                !password.Any(char.IsUpper) ||              // ���� �� ���� ���������
                !password.Any(char.IsLetter) ||             // ���� �� ���� �����
                !password.Any(char.IsDigit))                // ���� �� ���� �����
            {
                await DisplayAlert("������", "������ ������ ���� �� ����� 8 �������� � ��������� �����, ����� � ���� ��������� �����.", "��");
                return;
            }

            // �������� ��������� ������
            if (!int.TryParse(volumeText, out int diskLimit) || diskLimit < 1)
            {
                await DisplayAlert("������", "�������� ������������ ������ ���� ������ �� ����� 1.", "��");
                return;
            }

            string requestJson = $@"
            {{
                ""jsonrpc"": ""2.0"",
                ""id"": 1,
                ""method"": ""Users.create"",
                ""params"": {{
                    ""users"": [
                        {{
                            ""loginName"": ""{loginName}"",
                            ""fullName"": ""{fullName}"",
                            ""password"": ""{password}"",
                            ""description"": ""{description}"",
                            ""hasDefaultSpamRule"": true,
                            ""allowPasswordChange"": true,
                            ""authType"": ""UInternalAuth"",
                            ""domainId"": ""keriodb://domain/21424b88-213d-47a3-99ba-14bb4b6fd86e"",
                            ""isEnabled"": true,
                            ""diskSizeLimit"": {{
                                ""isActive"": true,
                                ""limit"": {{
                                    ""value"": {diskLimit},
                                    ""units"": ""GigaBytes""
                                }}
                            }},
                            ""role"": {{
                                ""userRole"": ""UserRole"",
                                ""publicFolderRight"": false
                            }},
                            ""userGroups"": []
                        }}
                    ]
                }}
            }}";

            string response = await SendRequestAsync(requestJson);
            dynamic jsonResponse = JsonConvert.DeserializeObject(response);

            if (jsonResponse?.result != null)
            {
                await DisplayAlert("�����", "������������ ��������!", "OK");
                await LoadUsersAsync();
            }
            else if (jsonResponse?.error != null)
            {
                string message = jsonResponse.error?.message ?? "����������� ������";
                await DisplayAlert("������", $"������ ��� ����������: {message}", "OK");
            }
            else
            {
                await DisplayAlert("������", "����������� ����� �� �������.", "OK");
            }

        }

        private async void DeleteUserClick(object sender, EventArgs e)
        {
            var selectedUser = MyCollectionView.SelectedItem as UserKerioConnect;
            if (selectedUser == null)
            {
                await DisplayAlert("������", "�������� ������������ ��� ��������.", "OK");
                return;
            }

            string userId = selectedUser.Id;

            string requestJson = $@"
            {{
                ""jsonrpc"": ""2.0"",
                ""id"": 1,
                ""method"": ""Users.remove"",
                ""params"": {{
                    ""requests"": [
                        {{
                            ""userId"": ""{userId}"",
                            ""method"": ""UDeleteFolder"",
                            ""mode"": ""DSModeDeactivate"",
                            ""removeReferences"": true,
                            ""targetUserId"": """"
                        }}
                    ]
                }}
            }}";

            string response = await SendRequestAsync(requestJson);
            await DisplayAlert("�����", "������������ �����!", "OK");
            await LoadUsersAsync();
        }

        private async void BlockAccessClick(object sender, EventArgs e)
        {
            var selectedUser = MyCollectionView.SelectedItem as UserKerioConnect;
            if (selectedUser == null)
            {
                await DisplayAlert("������", "�������� ������������.", "OK");
                return;
            }

            string userId = selectedUser.Id;
            bool currentStatus = selectedUser.Active;

            await SetUserEnabledStatus(userId, !currentStatus);
        }

        private async Task SetUserEnabledStatus(string userId, bool isEnabled)
        {
            string requestJson = $@"
    {{
        ""jsonrpc"": ""2.0"",
        ""id"": 1,
        ""method"": ""Batch.run"",
        ""params"": {{
            ""commandList"": [
                {{
                    ""method"": ""Users.set"",
                    ""params"": {{
                        ""pattern"": {{
                            ""isEnabled"": {isEnabled.ToString().ToLower()},
                            ""emailAddresses"": []
                        }},
                        ""userIds"": [""{userId}""]

                    }}
                }},
                {{
                    ""method"": ""Users.setPersonalContact"",
                    ""params"": {{
                        ""contact"": {{}},
                        ""userIds"": [""{userId}""]

                    }}
                }}
            ]
        }}
    }}";

            string response = await SendRequestAsync(requestJson);
            await DisplayAlert("�����", isEnabled ? "������������ �������������!" : "������������ ������������!", "OK");
            await LoadUsersAsync();
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = e.CurrentSelection.FirstOrDefault() as UserKerioConnect;
            if (selectedUser != null)
            {
                UpdateBlockAccessButton(selectedUser.Active);
            }
        }

        private void UpdateBlockAccessButton(bool isEnabled)
        {
            BlockAccessLabel.Text = isEnabled ? "�������������" : "��������������";
            BlockAccessImage.Source = isEnabled ? "block_access.png" : "assign_access.png";
        }
    }


    public class UserKerioConnect
    {
        public string Id { get; set; }
        public string Loggin { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
    }
}