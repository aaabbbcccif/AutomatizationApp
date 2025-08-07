using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace automationApp;

public partial class GoogleDriveDiskPage : ContentPage
{
    private DriveService _driveService;
    private string _selectedServiceAccountFile;
    private string _selectedFolderId;

    private readonly Dictionary<string, string> _accountFileMap = new()
    {
        { "kcepemail@gmail.com", "service_account_kcepemail.json" },
        { "kcepkazcenterelectroprovod@gmail.com", "service_account_kcepkazcenterelectroprovod.json" }
    };

    public GoogleDriveDiskPage()
    {
        InitializeComponent();

        AccountPicker.ItemsSource = _accountFileMap.Keys.ToList();
        AccountPicker.SelectedIndex = 0;
        _selectedServiceAccountFile = _accountFileMap.Values.First();

        _ = InitializeDriveServiceAsync();

        RolePicker.ItemsSource = new List<string>
    {
        "�������� ����",
        "writer",
        "reader",
        "commenter"
    };
        RolePicker.SelectedIndex = 0;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        RolePicker.ItemsSource = new List<string>
    {
        "�������� ����",
        "writer",
        "reader",
        "commenter"
    };
        RolePicker.SelectedIndex = 0;
    }
    private async void AccountPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (AccountPicker.SelectedItem is string selectedAccount && _accountFileMap.ContainsKey(selectedAccount))
        {
            _selectedServiceAccountFile = _accountFileMap[selectedAccount];
            await InitializeDriveServiceAsync();
        }
    }

    private async Task<string> ExtractJsonFileFromResources(string filename)
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, filename);

        if (!System.IO.File.Exists(filePath))
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
            using var outputStream = System.IO.File.Create(filePath);
            await stream.CopyToAsync(outputStream);
        }

        return filePath;
    }

    private async Task InitializeDriveServiceAsync()
    {
        try
        {
            string jsonPath = await ExtractJsonFileFromResources(_selectedServiceAccountFile);

            using var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.Drive);

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Drive API Access"
            });

            await LoadFoldersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� ���������������� ������: {ex.Message}", "OK");
            Console.WriteLine($"������ ������������� �������: {ex.Message}");
        }
    }

    private async Task LoadFoldersAsync()
    {
        try
        {
            var request = _driveService.Files.List();
            request.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            request.Fields = "files(id, name)";

            var folders = await request.ExecuteAsync();

            if (folders.Files.Any())
            {
                IDFolderPicker.ItemsSource = folders.Files.Select(f => new FolderItem { Id = f.Id, Name = f.Name }).ToList();
                IDFolderPicker.ItemDisplayBinding = new Binding("Name");
                IDFolderPicker.SelectedIndex = 0;

                _selectedFolderId = folders.Files.FirstOrDefault()?.Id;
                await LoadFolderUsersAsync();
            }
            else
            {
                await DisplayAlert("������", "�� ������� �� ����� ����� � ��������� ��������.", "OK");
                Console.WriteLine("�� ������� �� ����� ����� � ��������� ��������.");
            }

            // ��������� ����������� ��� �������
            Console.WriteLine("�����, ��������� � Google Drive:");
            foreach (var folder in folders.Files)
            {
                Console.WriteLine($"ID: {folder.Id}, ��������: {folder.Name}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� ��������� �����: {ex.Message}", "OK");
            Console.WriteLine($"������ ��� �������� �����: {ex.Message}");
        }
    }

    private async Task LoadFolderUsersAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_selectedFolderId)) return;

            var request = _driveService.Permissions.List(_selectedFolderId);
            request.Fields = "permissions(id, emailAddress)";

            var permissions = await request.ExecuteAsync();
            var users = permissions.Permissions
                .Where(p => !string.IsNullOrEmpty(p.EmailAddress))
                .Select(p => p.EmailAddress)
                .ToList();

            UsersFolders.ItemsSource = users;

            Console.WriteLine("������������ � �������� � �����:");
            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� ��������� �������������: {ex.Message}", "OK");
            Console.WriteLine($"������ ��� �������� �������������: {ex.Message}");
        }
    }


    private async void AddUserClick(object sender, TappedEventArgs e)
    {
        if (_driveService == null || string.IsNullOrEmpty(_selectedFolderId)) return;

        string email = EmailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("������", "������� Email!", "OK");
            return;
        }

        try
        {
            var permissions = await _driveService.Permissions.List(_selectedFolderId).ExecuteAsync();
            if (permissions.Permissions.Any(p => p.EmailAddress == email))
            {
                await DisplayAlert("������", "������������ ��� ����� ������ � �����", "OK");
                return;
            }

            string selectedRole = RolePicker.SelectedItem?.ToString() ?? "reader";

            var permission = new Permission()
            {
                Type = "user",
                Role = selectedRole,
                EmailAddress = email
            };

            var request = _driveService.Permissions.Create(permission, _selectedFolderId);
            request.SendNotificationEmail = false;
            await request.ExecuteAsync();

            await DisplayAlert("�����", $"������ �������� ��� {email} ��� {selectedRole}", "OK");
            await LoadFolderUsersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� �������� ������: {ex.Message}", "OK");
            Console.WriteLine($"������ ��� ���������� �������: {ex.Message}");
        }
    }


    private async void DeleteUserClick(object sender, TappedEventArgs e)
    {
        if (_driveService == null || string.IsNullOrEmpty(_selectedFolderId) || UsersFolders.SelectedItem == null)
        {
            await DisplayAlert("������", "�������� ������������ � �����.", "OK");
            return;
        }

        string email = UsersFolders.SelectedItem.ToString();

        try
        {
            var request = _driveService.Permissions.List(_selectedFolderId);
            request.Fields = "permissions(id, emailAddress, role, type)";
            var permissions = await request.ExecuteAsync();

            foreach (var perm in permissions.Permissions)
            {
                Console.WriteLine($"ID: {perm.Id}, Email: {perm.EmailAddress}, Role: {perm.Role}, Type: {perm.Type}");
            }

            var userPermission = permissions.Permissions.FirstOrDefault(p => p.EmailAddress == email);

            if (userPermission != null)
            {
                await _driveService.Permissions.Delete(_selectedFolderId, userPermission.Id).ExecuteAsync();
                await DisplayAlert("�����", $"������ ������ ��� {email}", "OK");
                await LoadFolderUsersAsync();
            }
            else
            {
                await DisplayAlert("������", "������������ �� ������", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� ������� ������: {ex.Message}", "OK");
            Console.WriteLine($"������ ��� �������� �������: {ex.Message}");
        }
    }


    private void UsersFoldersSelected(object sender, SelectionChangedEventArgs e)
    {
        if (UsersFolders.SelectedItem != null)
        {
            string selectedEmail = UsersFolders.SelectedItem.ToString();
            EmailEntry.Text = selectedEmail;
        }
    }


    private async void IDFolderPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (IDFolderPicker.SelectedItem is FolderItem selectedFolder && selectedFolder.Id != _selectedFolderId)
        {
            _selectedFolderId = selectedFolder.Id;
            await LoadFolderUsersAsync();
        }
    }

    
}

public class FolderItem
{
    public string Id { get; set; }
    public string Name { get; set; }
}