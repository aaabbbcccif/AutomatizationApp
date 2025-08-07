using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automationApp;

public class CustomDialogView : ContentView
{
    private TaskCompletionSource<Dictionary<string, bool>> _tcs;
    private Dictionary<string, CheckBox> _checkBoxes;

    public CustomDialogView(List<string> options, Dictionary<string, bool> initialValues = null)
    {
        initialValues ??= new();

        _checkBoxes = new();

        this.BackgroundColor = Color.FromArgb("#80000000"); // затемнение
        this.IsVisible = false;
        this.VerticalOptions = LayoutOptions.Fill;
        this.HorizontalOptions = LayoutOptions.Fill;

        var card = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 20,
            Padding = 20,
            WidthRequest = 350,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HasShadow = true
        };

        var stack = new VerticalStackLayout
        {
            Spacing = 14
        };

        stack.Children.Add(new Label
        {
            Text = "Выберите доступ к группам",
            FontAttributes = FontAttributes.Bold,
            FontSize = 20,
            TextColor = Color.FromArgb("#4B164C"),
            HorizontalOptions = LayoutOptions.Center
        });

        // Добавляем чекбоксы с подписями
        foreach (var option in options)
        {
            var checkBox = new CheckBox
            {
                IsChecked = initialValues.ContainsKey(option) && initialValues[option],
                Color = Color.FromArgb("#6C2B6F"),
                VerticalOptions = LayoutOptions.Center
            };

            var label = new Label
            {
                Text = option,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.Black
            };

            var row = new HorizontalStackLayout
            {
                Spacing = 10,
                Children = { checkBox, label }
            };

            _checkBoxes[option] = checkBox;
            stack.Children.Add(row);
        }

        // Кнопки
        var buttons = new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.End,
            Children =
            {
                new Button
                {
                    Text = "Отмена",
                    BackgroundColor = Colors.LightGray,
                    TextColor = Colors.Black,
                    CornerRadius = 10,
                    Command = new Command(() =>
                    {
                        Hide();
                        _tcs?.TrySetResult(null);
                    })
                },
                new Button
                {
                    Text = "ОК",
                    BackgroundColor = Color.FromArgb("#6C2B6F"),
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    Command = new Command(() =>
                    {
                        var result = new Dictionary<string, bool>();
                        foreach (var kvp in _checkBoxes)
                            result[kvp.Key] = kvp.Value.IsChecked;

                        Hide();
                        _tcs?.TrySetResult(result);
                    })
                }
            }
        };

        stack.Children.Add(buttons);
        card.Content = stack;
        Content = card;
    }


    public Task<Dictionary<string, bool>> ShowAsync()
    {
        this.IsVisible = true;
        _tcs = new TaskCompletionSource<Dictionary<string, bool>>();
        return _tcs.Task;
    }

    public void Hide() => this.IsVisible = false;
}
