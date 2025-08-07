using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace automationApp
{
    public partial class BitrixPage : ContentPage
    {
        private string bitrixUrl = "https://b24-zib4mj.bitrix24.kz/rest/1/u1dp157blsp9wapj/";
        private ObservableCollection<UserBitrix> users = new ObservableCollection<UserBitrix>();

        public BitrixPage()
        {
            InitializeComponent();
            UsersFolders.ItemsSource = users;
            LoadUsers();
        }

        private async void AddUserClick(object sender, EventArgs e)
        {
            try
            {
                string name = FirstNameEntry.Text;
                string lastName = SecondNameEntry.Text;
                string email = EmailEntry.Text;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Ошибка", "Пожалуйста, заполните все поля: имя, фамилия и электронная почта.", "ОК");
                    return;
                }

                var existingUser = await GetUserByEmail(email);
                if (existingUser != null)
                {
                    // Повторно активируем уволенного пользователя
                    var response = await SendPostRequest("user.update", new { ID = existingUser.ID, ACTIVE = "Y" });
                    var responseObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                    if (responseObj.ContainsKey("result") && (bool)responseObj["result"] == true)
                    {
                        await DisplayAlert("Успех", "Пользователь повторно принят на работу.", "ОК");
                        await LoadUsers();
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Ошибка при приёме на работу: " + response, "ОК");
                    }
                    return;
                }

                var parameters = new Dictionary<string, string>
                {
                    { "NAME", name },
                    { "LAST_NAME", lastName },
                    { "EMAIL", email },
                    { "UF_DEPARTMENT", "2" }
                };

                var addResponse = await SendPostRequest("user.add", parameters);
                await DisplayAlert("Успех", "Пользователь добпален", "ОК");
                await LoadUsers();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при добавлении пользователя: {ex.Message}", "ОК");
            }
        }

        private async void DeleteUserClick(object sender, EventArgs e)
        {
            try
            {
                var selectedUser = UsersFolders.SelectedItem as UserBitrix;
                if (selectedUser == null)
                {
                    await DisplayAlert("Ошибка", "Пожалуйста, выберите пользователя для удаления.", "ОК");
                    return;
                }

                var updateData = new { ID = selectedUser.ID, ACTIVE = "N" };
                var responseJson = await SendPostRequest("user.update", updateData);

                var responseObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson);

                if (responseObj.ContainsKey("result") && (bool)responseObj["result"] == true)
                {
                    await DisplayAlert("Успех", "Пользователь уволен.", "ОК");
                    users.Remove(selectedUser);
                    await LoadUsers();
                }
                else if (responseObj.ContainsKey("error_description"))
                {
                    await DisplayAlert("Ошибка", responseObj["error_description"].ToString(), "ОК");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Произошла неизвестная ошибка.", "ОК");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при увольнении пользователя: {ex.Message}", "ОК");
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                var usersList = await GetUsers();
                users.Clear();
                foreach (var user in usersList)
                {
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при загрузке пользователей: {ex.Message}", "ОК");
            }
        }

        private async Task<List<UserBitrix>> GetUsers()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{bitrixUrl}user.get.json?FILTER[ACTIVE]=Y");
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BitrixResponse>(responseString);
                return result.result;
            }
        }

        private async Task<UserBitrix> GetUserByEmail(string email)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{bitrixUrl}user.get.json?FILTER[EMAIL]={Uri.EscapeDataString(email)}&SELECT=ID,NAME,LAST_NAME");
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BitrixResponse>(responseString);

                if (result.result.Count > 0)
                {
                    return result.result[0];
                }
                return null;
            }
        }

        private async Task<string> SendPostRequest(string method, object data)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{bitrixUrl}{method}.json";
                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void UsersFoldersSelected(object sender, SelectionChangedEventArgs e)
        {
            // Заполнить, если нужно
        }

        private void AssignCRMClick(object sender, TappedEventArgs e)
        {

        }
    }

    public class UserBitrix
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string LAST_NAME { get; set; }
    }

    public class BitrixResponse
    {
        public List<UserBitrix> result { get; set; }
    }
}
