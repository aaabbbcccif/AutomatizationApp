using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Linq;
using Azure;

namespace automationApp
{
    public partial class RadiusPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();
        private const string usersPageUrl = "http://radius.kcep.local:8008/logpass";
        private const string addUserUrl = "http://radius.kcep.local:8008/logpass_add_submit";
        private const string deleteUserUrl = "http://radius.kcep.local:8008/logpass_edit";
        private const string devicesPageUrl = "http://radius.kcep.local:8008/macs";

        public ObservableCollection<UsersRadius> Users { get; set; } = new ObservableCollection<UsersRadius>();
        public ObservableCollection<DeviceRadius> Devices { get; set; } = new ObservableCollection<DeviceRadius>();

        public RadiusPage()
        {
            InitializeComponent();
            MyCollectionView.ItemsSource = Users;
            MyCollectionView2.ItemsSource = Devices;
            LoadUsers();
            LoadDevices();
        }

        private async void LoadUsers()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(usersPageUrl);
                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    var rows = doc.DocumentNode.SelectNodes("//table//tr");
                    if (rows != null)
                    {
                        Users.Clear();
                        foreach (var row in rows.Skip(1)) // Пропускаем заголовок таблицы
                        {
                            var cells = row.SelectNodes("td");
                            if (cells != null && cells.Count >= 3)
                            {
                                string userId = cells[0].InnerText.Trim();
                                string username = cells[1].InnerText.Trim();
                                string status = cells[2].InnerText.Trim();

                                Users.Add(new UsersRadius { UserId = userId, Username = username, Status = status });
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", "Ошибка загрузки списка пользователей!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", "Ошибка сети!", "OK");
            }
        }

        private async void AddUserClick(object sender, EventArgs e)
        {
            string username = LogginEntry.Text.Trim();
            string password = PasswordEntry.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Ошибка", "Введите имя пользователя и пароль!", "OK");
                return;
            }

            var addRequest = new
            {
                add = new[]
                {
                    new { username = username, value = password }
                }
            };

            string json = JsonConvert.SerializeObject(addRequest);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("sendData", json)
            });

            try
            {
                HttpResponseMessage response = await client.PostAsync(addUserUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Уведомление", "Пользователь успешно добавлен!", "OK");
                    LogginEntry.Text = string.Empty;
                    PasswordEntry.Text = string.Empty;
                    LoadUsers();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Ошибка при добавлении пользователя!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", "Ошибка подключения к серверу!", "OK");
            }
        }

        private async void DeleteUserClick(object sender, EventArgs e)
        {
            if (MyCollectionView.SelectedItem is UsersRadius selectedUser)
            {
                var data = new
                {
                    delete = new[]
                    {
                        new { id = selectedUser.UserId, username = selectedUser.Username, value = selectedUser.Status }
                    }
                };

                string json = JsonConvert.SerializeObject(data);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("sendData", json)
                });

                try
                {
                    HttpResponseMessage response = await client.PostAsync(deleteUserUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Уведомление", $"Пользователь {selectedUser.Username} успешно удалён!", "OK");
                        LoadUsers();
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", $"Ошибка при удалении пользователя {selectedUser.Username}: {response}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", $" Ошибка соединения: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Ошибка", $"Выберите пользователя для удаления!", "OK");
            }
        }

        private async void LoadDevices()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(devicesPageUrl);
                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    var rows = doc.DocumentNode.SelectNodes("//table//tr");
                    if (rows != null)
                    {
                        Devices.Clear();
                        foreach (var row in rows.Skip(1)) // Пропускаем заголовок таблицы
                        {
                            var cells = row.SelectNodes("td");
                            if (cells != null && cells.Count >= 4)
                            {
                                string deviceId = cells[0].InnerText.Trim();
                                string macAddress = cells[1].InnerText.Trim();
                                string owner = cells[2].InnerText.Trim();
                                string model = cells[3].InnerText.Trim();

                                Devices.Add(new DeviceRadius { DeviceId = deviceId, MacAddress = macAddress, Owner = owner, Model = model });
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Ошибка загрузки списка устройств!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка сети!", "OK");
            }
        }

        private async void AddDeviceClick(object sender, EventArgs e)
        {
            string mac = MacAddressEntry2.Text.Trim();
            string owner = OwnerPlaceEntry2.Text.Trim();
            string model = ModelEntry2.Text.Trim();

            if (string.IsNullOrEmpty(mac) || string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(model))
            {
                // Показать сообщение об ошибке
                return;
            }

            var request = new
            {
                add = new[]
                {
                    new { mac = mac, name = owner, model = model }
                }
            };

            string json = JsonConvert.SerializeObject(request);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("sendData", json)
            });

            try
            {
                HttpResponseMessage response = await client.PostAsync("http://radius.kcep.local:8008/macs_add_submit", content);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Уведомление", $"Устройство добавлено!", "OK");
                    MacAddressEntry2.Text = string.Empty;
                    OwnerPlaceEntry2.Text = string.Empty;
                    ModelEntry2.Text = string.Empty;
                    LoadDevices();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Ошибка при добавлении устройства!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }

        private async void DeleteDeviceClick(object sender, EventArgs e)
        {
            if (MyCollectionView2.SelectedItem is DeviceRadius selectedDevice)
            {
                var request = new
                {
                    delete = new[]
                    {
                        new { id = selectedDevice.DeviceId, mac = selectedDevice.MacAddress, name = selectedDevice.Owner, model = selectedDevice.Model }
                    }
                };

                string json = JsonConvert.SerializeObject(request);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("sendData", json)
                });

                try
                {
                    HttpResponseMessage response = await client.PostAsync("http://radius.kcep.local:8008/macs_edit", content);
                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Уведомление", $"Устройство {selectedDevice.Owner} удалено!", "OK");
                        LoadDevices();
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", $"Ошибка: {response}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", $"Сетевая ошибка: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Ошибка", $"Выберите устройство для удаления!", "OK");
            }
        }

        private void LoadUserClick(object sender, TappedEventArgs e)
        {
            LoadUsers();
        }
        private void LoadDeviceClick(object sender, TappedEventArgs e)
        {
            LoadDevices();
        }
        private void GeneratePasswordClick(object sender, TappedEventArgs e)
        {
            PasswordEntry.Text = GeneratePassword();
        }
        private string GeneratePassword(int length = 16)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@$%?&*+-_=~";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class UsersRadius
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
    }

    public class DeviceRadius
    {
        public string DeviceId { get; set; }
        public string MacAddress { get; set; }
        public string Owner { get; set; }
        public string Model { get; set; }
    }
}