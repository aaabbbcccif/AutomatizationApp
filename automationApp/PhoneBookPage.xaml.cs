using ClosedXML.Excel;

namespace automationApp;

public partial class PhoneBookPage : ContentPage
{
	public PhoneBookPage()
	{
		InitializeComponent();
        LoadDataFromExcel();
    }
    private void LoadDataFromExcel()
    {
        var filePath = "C:\\Users\\ssrah\\OneDrive\\Desktop\\PhoneBook.xlsx"; // Укажите путь к вашему Excel-файлу
        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1); 
            var rows = worksheet.RowsUsed().Skip(1); 

            var contacts = new List<PhoneBookClass>(); // Создаем список для контактов
            foreach (var row in rows)
            {
                var contact = new PhoneBookClass
                {
                    LastName = row.Cell(1).Value.ToString(),
                    FirstName = row.Cell(2).Value.ToString(),
                    Patronymic = row.Cell(3).Value.ToString(),
                    Phone = row.Cell(4).Value.ToString(),
                    Email = row.Cell(5).Value.ToString(),
                    Description = row.Cell(6).Value.ToString()
                };
                contacts.Add(contact);
            }

            MyCollectionView.ItemsSource = contacts; // Обновляем источник данных для CollectionView
        }
    }
    private void AddContactToExcel(PhoneBookClass contact)
    {
        var filePath = "C:\\Users\\ssrah\\OneDrive\\Desktop\\PhoneBook.xlsx"; // Укажите путь к вашему Excel-файлу
        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1); // Берем первый лист
            var newRow = worksheet.Row(worksheet.RowsUsed().Count() + 1); // Создаем новую строку
            newRow.Cell(1).Value = contact.LastName;
            newRow.Cell(2).Value = contact.FirstName;
            newRow.Cell(3).Value = contact.Patronymic;
            newRow.Cell(4).Value = contact.Phone;
            newRow.Cell(5).Value = contact.Email;
            newRow.Cell(6).Value = contact.Description;

            workbook.Save(); // Сохраняем изменения
        }
    }
    private void AddUserClick(object sender, TappedEventArgs e)
    {
        var contact = new PhoneBookClass
        {
            LastName = LastNameEntry.Text,
            FirstName = FirstNameEntry.Text,
            Patronymic = PatronymicEntry.Text,
            Phone = PhoneEntry.Text,
            Email = EmailEntry.Text,
            Description = DescriptionEntry.Text
        };

        AddContactToExcel(contact); // Добавляем контакт в Excel
        LoadDataFromExcel(); // Обновляем таблицу
    }
    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void LoadUserClick(object sender, TappedEventArgs e)
    {
        LoadDataFromExcel(); // Загружаем данные из Excel и обновляем таблицу
    }
    private void DeleteUserClick(object sender, TappedEventArgs e)
    {
        var selectedContact = MyCollectionView.SelectedItem as PhoneBookClass;
        if (selectedContact == null) return;

        var filePath = "C:\\Users\\ssrah\\OneDrive\\Desktop\\PhoneBook.xlsx"; // Укажите путь к вашему Excel-файлу
        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1); // Берем первый лист
            var rows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок

            foreach (var row in rows)
            {
                if (row.Cell(1).Value.ToString() == selectedContact.LastName &&
                    row.Cell(2).Value.ToString() == selectedContact.FirstName &&
                    row.Cell(3).Value.ToString() == selectedContact.Patronymic &&
                    row.Cell(4).Value.ToString() == selectedContact.Phone &&
                    row.Cell(5).Value.ToString() == selectedContact.Email &&
                    row.Cell(6).Value.ToString() == selectedContact.Description)
                {
                    row.Delete(); // Удаляем строку
                    break;
                }
            }

            workbook.Save(); // Сохраняем изменения
        }

        LoadDataFromExcel(); // Обновляем таблицу
    }

}
