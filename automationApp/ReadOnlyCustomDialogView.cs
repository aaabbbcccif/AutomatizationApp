using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automationApp;

public class ReadOnlyCustomDialogView : ContentView
{
    private TaskCompletionSource<bool> _tcs;

    public ReadOnlyCustomDialogView(Dictionary<string, bool> values, List<string> groupNames)
    {
        values ??= new();
        this.IsVisible = false;
        this.BackgroundColor = Color.FromArgb("#80000000"); // затемнённый фон
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
            Text = "Назначенные доступы",
            FontAttributes = FontAttributes.Bold,
            FontSize = 20,
            TextColor = Color.FromArgb("#4B164C"),
            HorizontalOptions = LayoutOptions.Center
        });

        foreach (var group in groupNames)
        {
            var row = new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new CheckBox
                    {
                        IsChecked = values.ContainsKey(group) && values[group],
                        IsEnabled = false, // Только для чтения
                        Color = Color.FromArgb("#6C2B6F"),
                        VerticalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = group,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 16,
                        TextColor = Colors.Black
                    }
                }
            };

            stack.Children.Add(row);
        }

        var backButton = new Button
        {
            Text = "Назад",
            BackgroundColor = Colors.LightGray,
            TextColor = Colors.Black,
            CornerRadius = 10,
            HorizontalOptions = LayoutOptions.End,
            Command = new Command(() =>
            {
                Hide();
                _tcs?.TrySetResult(true);
            })
        };

        stack.Children.Add(backButton);
        card.Content = stack;
        Content = card;
    }

    public Task<bool> ShowAsync()
    {
        this.IsVisible = true;
        _tcs = new TaskCompletionSource<bool>();
        return _tcs.Task;
    }

    public void Hide() => this.IsVisible = false;
}
