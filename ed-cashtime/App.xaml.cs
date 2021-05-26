using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation.Collections;

namespace EdCashtime {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            ToastNotificationManagerCompat.OnActivated += toastArgs => {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                ValueSet userInput = toastArgs.UserInput;
                Current.Dispatcher.Invoke(delegate {
                    MessageBox.Show("Toast activated. Args: " + toastArgs.Argument);
                });
            };
        }
    }
}
