using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace automationApp
{
    public class BasePage : ContentPage
    {
        public BasePage()
        {
#if WINDOWS
            InitWindowsKeyHandling();
#endif
        }

#if WINDOWS
        private void InitWindowsKeyHandling()
        {
            var nativeWindow = Microsoft.Maui.Controls.Application.Current.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;


            if (nativeWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.KeyDown += OnKeyDown;
            }
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.F1)
            {
                OpenHelpFile();
            }
        }

        private void OpenHelpFile()
        {
            string htmlFileName = "HelpFile.html";
            string fullPath = Path.Combine(AppContext.BaseDirectory, htmlFileName);

            if (File.Exists(fullPath))
            {
                string fileUrl = $"file:///{fullPath.Replace("\\", "/")}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileUrl,
                    UseShellExecute = true
                });
            }
        }
#endif
    }
}