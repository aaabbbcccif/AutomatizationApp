using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace automationApp
{
    public partial class MicrosoftActiveDirectoryPage : ContentPage
    {
        public MicrosoftActiveDirectoryPage()
        {
            InitializeComponent();
            LoadFirstLevelAsync();
            LoadAllUsers();
        }

        private async Task LoadFirstLevelAsync()
        {
            try
            {
                var organizationalUnits = await Task.Run(() =>
                    ActiveDirectoryHelper.GetOrganizationalUnits("LDAP://10.10.8.4/OU=STORE2,OU=SERVERS,OU=KCEP,DC=kcep,DC=local"));

                if (organizationalUnits == null || !organizationalUnits.Any())
                {
                    await DisplayAlert("Информация", "Не найдено организационных единиц.", "OK");
                    CVRootFolders.ItemsSource = null;
                }
                else
                {
                    CVRootFolders.ItemsSource = organizationalUnits;
                    await DisplayAlert("Успех", $"Загружено {organizationalUnits.Count} организационных единиц.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить организационные единицы: {ex.Message}", "OK");
            }
        }


        private async void CVRootFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CVSubFolders.ItemsSource = null;
            if (e.CurrentSelection.FirstOrDefault() is string selectedOU)
            {
                string ldapPath = $"LDAP://10.10.8.4/OU={selectedOU},OU=STORE2,OU=SERVERS,OU=KCEP,DC=kcep,DC=local";
                var userGroups = await Task.Run(() => ActiveDirectoryHelper.GetUserGroups(ldapPath));
                if (userGroups == null || userGroups.Count == 0)
                {
                    await DisplayAlert("Информация", "Группы не найдены в выбранной организационной единице.", "OK");
                    return;
                }
                CVSubFolders.ItemsSource = userGroups;
            }
        }

        private async void CVSubFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CVGroupUsers.ItemsSource = null;
            if (e.CurrentSelection.FirstOrDefault() is string selectedGroup)
            {
                string domainLdapPath = "LDAP://10.10.8.4/DC=kcep,DC=local";
                var groupUsers = await Task.Run(() => ActiveDirectoryHelper.GetGroupMembers(domainLdapPath, selectedGroup));
                if (groupUsers == null || groupUsers.Count == 0)
                {
                    await DisplayAlert("Информация", "Пользователи в группе не найдены.", "OK");
                    return;
                }
                CVGroupUsers.ItemsSource = groupUsers;
            }
        }


        private void OnAllUsersSelected(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора пользователя из списка всех пользователей
        }

        private void AddUserClick(object sender, TappedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserNameEntry.Text) || string.IsNullOrWhiteSpace(FirstNameEntry.Text) || string.IsNullOrWhiteSpace(LastNameEntry.Text) || string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            {
                DisplayAlert("Ошибка", "Пожалуйста, заполните все поля!", "OK");
                return;
            }

            string userName = UserNameEntry.Text.Trim();
            string firstName = FirstNameEntry.Text.Trim();
            string lastName = LastNameEntry.Text.Trim();
            string description = DescriptionEntry.Text.Trim();

            try
            {
                ActiveDirectoryHelper.CreateUser("LDAP://10.10.8.4/OU=STORE2,OU=SERVERS,OU=KCEP,DC=kcep,DC=local", userName, firstName, lastName, description);
                LoadAllUsers();
                DisplayAlert("Успех", $"Пользователь {userName} успешно создан!", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private void DeleteUserClick(object sender, TappedEventArgs e)
        {
            if (CVAllUsers.SelectedItem == null)
            {
                DisplayAlert("Ошибка", "Выберите пользователя для удаления!", "OK");
                return;
            }

            string userName = CVAllUsers.SelectedItem.ToString().Trim();
            string adminUsername = "kcep\\StudentIT1";
            string adminPassword = "123";

            try
            {
                RemoveUserFromAllGroups(userName, adminUsername, adminPassword);

                ActiveDirectoryHelper.DeleteUser("LDAP://10.10.8.4/OU=STORE2,OU=SERVERS,OU=KCEP,DC=kcep,DC=local", userName);

                LoadAllUsers();
                DisplayAlert("Успех", $"Пользователь {userName} успешно удален!", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка при удалении пользователя: {ex.Message}", "OK");
            }
        }

        private void RemoveUserFromAllGroups(string userName, string adminUsername, string adminPassword)
        {
            try
            {
                string ldapPath = "LDAP://10.10.8.4/DC=kcep,DC=local";

                using (DirectoryEntry domainEntry = new DirectoryEntry(ldapPath, adminUsername, adminPassword))
                {
                    DirectorySearcher userSearcher = new DirectorySearcher(domainEntry)
                    {
                        Filter = $"(&(objectClass=user)(sAMAccountName={userName}))",
                        SearchScope = SearchScope.Subtree
                    };
                    SearchResult userResult = userSearcher.FindOne();

                    if (userResult == null) return;

                    string userDN = userResult.Properties["distinguishedName"][0].ToString();

                    DirectorySearcher groupSearcher = new DirectorySearcher(domainEntry)
                    {
                        Filter = $"(&(objectClass=group)(member={userDN}))",
                        SearchScope = SearchScope.Subtree
                    };

                    foreach (SearchResult groupResult in groupSearcher.FindAll())
                    {
                        string groupDN = groupResult.Properties["distinguishedName"][0].ToString();

                        using (DirectoryEntry groupEntry = new DirectoryEntry($"LDAP://10.10.8.4/{groupDN}", adminUsername, adminPassword))
                        {
                            groupEntry.Properties["member"].Remove(userDN);
                            groupEntry.CommitChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении пользователя из групп: {ex.Message}");
            }
        }

        private void AssignAccessClick(object sender, TappedEventArgs e)
        {
            if (CVSubFolders.SelectedItem == null || CVAllUsers.SelectedItem == null)
            {
                DisplayAlert("Ошибка", "Выберите группу и пользователя!", "OK");
                return;
            }

            string selectedOU = CVRootFolders.SelectedItem.ToString().Trim();
            string groupName = CVSubFolders.SelectedItem.ToString().Trim();
            string userName = CVAllUsers.SelectedItem.ToString().Trim();

            string adminUsername = "kcep\\StudentIT1";
            string adminPassword = "123";

            try
            {
                ActiveDirectoryHelper.AddUserToGroup(userName, groupName, adminUsername, adminPassword);
                LoadAllUsers();
                DisplayAlert("Успех", $"Пользователь {userName} добавлен в группу {groupName}!", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка при добавлении пользователя в группу: {ex.Message}", "OK");
            }
        }

        private void BlockAccessClick(object sender, TappedEventArgs e)
        {
            if (CVSubFolders.SelectedItem == null || CVGroupUsers.SelectedItem == null)
            {
                DisplayAlert("Ошибка", "Выберите группу и пользователя!", "OK");
                return;
            }

            string groupName = CVSubFolders.SelectedItem.ToString().Trim();
            string userName = CVGroupUsers.SelectedItem.ToString().Trim();

            string adminUsername = "kcep\\StudentIT1";
            string adminPassword = "123";

            try
            {
                ActiveDirectoryHelper.RemoveUserFromGroup(userName, groupName, adminUsername, adminPassword);

                LoadAllUsers();
                DisplayAlert("Успех", $"Пользователь {userName} удален из группы {groupName}!", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка при удалении пользователя из группы: {ex.Message}", "OK");
            }
        }

        private void LoadAllUsers()
        {
            try
            {
                CVAllUsers.ItemsSource = ActiveDirectoryHelper.GetAllUsers("LDAP://10.10.8.4/OU=STORE2,OU=SERVERS,OU=KCEP,DC=kcep,DC=local");
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Не удалось загрузить список пользователей: {ex.Message}", "OK");
            }
        }

        private void OnGroupUsersSelected(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора пользователя из списка пользователей группы
        }
    }

    public static class ActiveDirectoryHelper
    {
        public static List<string> GetOrganizationalUnits(string ldapPath)
        {
            List<string> ouList = new List<string>();
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(objectCategory=organizationalUnit)";
                    searcher.PropertiesToLoad.Add("name");
                    SearchResultCollection results = searcher.FindAll();
                    foreach (SearchResult result in results)
                    {
                        if (result.Properties.Contains("name"))
                        {
                            string ouName = result.Properties["name"][0].ToString();
                            ouList.Add(ouName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка AD: " + ex.Message);
            }
            return ouList;
        }


        public static List<string> GetUserGroups(string ldapPath)
        {
            List<string> groupList = new List<string>();
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(objectClass=group)";
                    searcher.PropertiesToLoad.Add("cn");
                    foreach (SearchResult result in searcher.FindAll())
                    {
                        if (result.Properties.Contains("cn"))
                            groupList.Add(result.Properties["cn"][0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка AD: " + ex.Message);
            }
            return groupList;
        }

        public static List<string> GetGroupMembers(string ldapPath, string groupName)
        {
            List<string> members = new List<string>();
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(&(objectClass=group)(cn={groupName}))";
                    searcher.PropertiesToLoad.Add("member");

                    SearchResult result = searcher.FindOne();
                    if (result != null && result.Properties.Contains("member"))
                    {
                        foreach (var dn in result.Properties["member"])
                        {
                            members.Add(GetCommonNameFromDistinguishedName(dn.ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка AD: " + ex.Message);
            }
            return members;
        }

        private static string GetCommonNameFromDistinguishedName(string distinguishedName)
        {
            if (distinguishedName.StartsWith("CN="))
            {
                return distinguishedName.Split(',')[0].Substring(3);
            }
            return distinguishedName;
        }

        public static List<string> GetAllUsers(string ldapPath)
        {
            List<string> users = new List<string>();
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(objectClass=user)";
                    searcher.PropertiesToLoad.Add("cn");

                    foreach (SearchResult result in searcher.FindAll())
                    {
                        if (result.Properties.Contains("cn"))
                            users.Add(result.Properties["cn"][0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при загрузке пользователей: " + ex.Message);
            }
            return users;
        }

        public static void CreateUser(string ldapPath, string userName, string firstName, string lastName, string description)
        {
            string adminUsername = "kcep\\StudentIT1";
            string adminPassword = "123";

            if (!TestConnection(ldapPath, adminUsername, adminPassword))
            {
                throw new Exception("Не удалось подключиться к Active Directory. Проверьте параметры подключения.");
            }

            try
            {
                using (DirectoryEntry dirEntry = new DirectoryEntry(ldapPath, adminUsername, adminPassword))
                {
                    DirectoryEntry newUser = dirEntry.Children.Add($"CN={userName}", "user");
                    newUser.Properties["sAMAccountName"].Value = userName;
                    newUser.Properties["givenName"].Value = firstName;
                    newUser.Properties["sn"].Value = lastName;
                    newUser.Properties["description"].Value = description;
                    newUser.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании пользователя: {ex.Message}");
                throw;
            }
        }

        public static void DeleteUser(string ldapPath, string userName)
        {
            string adminUsername = "kcep\\StudentIT1";
            string adminPassword = "123";

            if (!TestConnection(ldapPath, adminUsername, adminPassword))
            {
                throw new Exception("Не удалось подключиться к Active Directory. Проверьте параметры подключения.");
            }

            try
            {
                using (DirectoryEntry dirEntry = new DirectoryEntry(ldapPath, adminUsername, adminPassword))
                {
                    DirectoryEntry user = dirEntry.Children.Find($"CN={userName}", "user");
                    dirEntry.Children.Remove(user);
                    dirEntry.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка при удалении пользователя: " + ex.ToString());
                throw;
            }
        }

        public static void AddUserToGroup(string userName, string groupName, string adminUsername, string adminPassword)
        {
            string ldapPath = "LDAP://10.10.8.4/DC=kcep,DC=local";

            if (!TestConnection(ldapPath, adminUsername, adminPassword))
            {
                throw new Exception("Не удалось подключиться к Active Directory. Проверьте параметры подключения.");
            }

            try
            {
                using (DirectoryEntry domainEntry = new DirectoryEntry(ldapPath, adminUsername, adminPassword))
                {
                    DirectorySearcher userSearcher = new DirectorySearcher(domainEntry)
                    {
                        Filter = $"(&(objectClass=user)(sAMAccountName={userName}))",
                        SearchScope = SearchScope.Subtree
                    };
                    SearchResult userResult = userSearcher.FindOne();

                    if (userResult == null)
                        throw new Exception($"Пользователь '{userName}' не найден!");

                    string userDN = userResult.Properties["distinguishedName"][0].ToString();

                    DirectorySearcher groupSearcher = new DirectorySearcher(domainEntry)
                    {
                        Filter = $"(&(objectClass=group)(cn={groupName}))",
                        SearchScope = SearchScope.Subtree
                    };
                    SearchResult groupResult = groupSearcher.FindOne();

                    if (groupResult == null)
                        throw new Exception($"Группа '{groupName}' не найдена!");

                    string groupDN = groupResult.Properties["distinguishedName"][0].ToString();

                    using (DirectoryEntry groupEntry = new DirectoryEntry($"LDAP://10.10.8.4/{groupDN}", adminUsername, adminPassword))
                    {
                        groupEntry.Properties["member"].Add(userDN);
                        groupEntry.CommitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при добавлении пользователя в группу: {ex.Message}");
                throw;
            }
        }

        public static void RemoveUserFromGroup(string userName, string groupName, string adminUsername, string adminPassword)
        {
            string ldapPath = "LDAP://10.10.8.4/DC=kcep,DC=local";

            if (!TestConnection(ldapPath, adminUsername, adminPassword))
            {
                throw new Exception("Не удалось подключиться к Active Directory. Проверьте параметры подключения.");
            }

            try
            {
                using (DirectoryEntry domainEntry = new DirectoryEntry(ldapPath, adminUsername, adminPassword))
                {
                    DirectorySearcher userSearcher = new DirectorySearcher(domainEntry)
                    {
                        Filter = $"(&(objectClass=user)(sAMAccountName={userName}))",
                        SearchScope = SearchScope.Subtree
                    };
                    SearchResult userResult = userSearcher.FindOne();

                    if (userResult == null)
                        throw new Exception($"Пользователь '{userName}' не найден!");

                    string userDN = userResult.Properties["distinguishedName"][0].ToString();

                    DirectorySearcher groupSearcher = new DirectorySearcher(domainEntry)
                    {
                        Filter = $"(&(objectClass=group)(cn={groupName}))",
                        SearchScope = SearchScope.Subtree
                    };
                    SearchResult groupResult = groupSearcher.FindOne();

                    if (groupResult == null)
                        throw new Exception($"Группа '{groupName}' не найдена!");

                    string groupDN = groupResult.Properties["distinguishedName"][0].ToString();

                    using (DirectoryEntry groupEntry = new DirectoryEntry($"LDAP://10.10.8.4/{groupDN}", adminUsername, adminPassword))
                    {
                        groupEntry.Properties["member"].Remove(userDN);
                        groupEntry.CommitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении пользователя из группы: {ex.Message}");
                throw;
            }
        }

        public static bool TestConnection(string ldapPath, string adminUsername, string adminPassword)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(ldapPath, adminUsername, adminPassword))
                {
                    // Попытка получить NativeObject для проверки подключения
                    object nativeObject = entry.NativeObject;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке подключения: {ex.Message}");
                return false;
            }
        }
    }
}