using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace automationApp;

public class UserActionDialogView : ContentView
{
    private TaskCompletionSource<bool> _tcs;

    public UserActionDialogView(UserAction action)
    {
        this.IsVisible = false;
        this.BackgroundColor = Color.FromArgb("#80000000"); // полупрозрачный фон
        this.VerticalOptions = LayoutOptions.Fill;
        this.HorizontalOptions = LayoutOptions.Fill;

        var card = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 20,
            Padding = 20,
            WidthRequest = 400,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HasShadow = true
        };

        var stack = new VerticalStackLayout { Spacing = 12 };

        stack.Children.Add(new Label
        {
            Text = "Информация о действии",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#4B164C"),
            HorizontalOptions = LayoutOptions.Center
        });

        stack.Children.Add(CreateInfoLabel("Название", action.Title));
        stack.Children.Add(CreateInfoLabel("Действие", action.Action));
        stack.Children.Add(CreateInfoLabel("Дата", action.ActionDate.ToString("dd.MM.yyyy HH:mm")));
        stack.Children.Add(CreateInfoLabel("Статус", action.Status));

        var closeButton = new Button
        {
            Text = "Закрыть",
            BackgroundColor = Color.FromArgb("#4B164C"),
            TextColor = Colors.White,
            CornerRadius = 10,
            HorizontalOptions = LayoutOptions.End,
            Command = new Command(() =>
            {
                Hide();
                _tcs?.TrySetResult(true);
            })
        };

        stack.Children.Add(closeButton);
        card.Content = stack;
        Content = card;
    }

    private View CreateInfoLabel(string title, string value)
    {
        return new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = title, FontSize = 14, TextColor = Colors.Gray },
                new Label { Text = value, FontSize = 16, TextColor = Colors.Black, FontAttributes = FontAttributes.Bold }
            }
        };
    }

    public Task<bool> ShowAsync()
    {
        this.IsVisible = true;
        _tcs = new TaskCompletionSource<bool>();
        return _tcs.Task;
    }

    public void Hide() => this.IsVisible = false;
}
