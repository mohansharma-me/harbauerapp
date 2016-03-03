using HarbauerApp.classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace HarbauerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                App.Current.MainWindow = new MainWindow();
                Job.Database.openDatabaseConnection();
                App.Current.MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Job.Database.closeDatabaseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.MainWindow.Close();
            }
        }
    }
}
