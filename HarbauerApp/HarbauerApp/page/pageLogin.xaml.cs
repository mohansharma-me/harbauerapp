using HarbauerApp.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HarbauerApp.page
{
    /// <summary>
    /// Interaction logic for pageLogin.xaml
    /// </summary>
    public partial class pageLogin : UserControl
    {
        page.pageControlPanel cpanelPage = null;
        public pageLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Password.Trim();
            bool isAdministrator = cmbUserKind.SelectedIndex == 0;

            if(user.Length==0)
            {
                MessageBox.Show("Please enter valid username.", "Invalid Username!", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }

            
            new System.Threading.Thread(() =>
            {
                try
                {
                    #region MyRegion
                    if (Job.Database.Login(user, pass, isAdministrator))
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            startControlPanel();
                        }));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            MessageBox.Show("Username/Password mismatched, please try again.", "Wrong combination.", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));

                }
            })
            { ApartmentState = System.Threading.ApartmentState.STA }.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UserTokenOwner: " + Properties.Settings.Default.appLoginName);
            Console.WriteLine("UserToken: " + Properties.Settings.Default.appLogin);
            if(Properties.Settings.Default.appLogin.Trim().Length>0 && Properties.Settings.Default.appLoginName.Trim().Length>0)
            {
                startControlPanel();
            }
        }

        private void startControlPanel()
        {
            scrollViewer4Login.Visibility = Visibility.Collapsed;
            scrollViewer4CP.Visibility = Visibility.Visible;

            new System.Threading.Thread(() =>
            {
                try
                {
                    #region MyRegion
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        scrollViewer4CP.Content = new page.pageControlPanel(Properties.Settings.Default.appLogin, () =>
                        {
                            scrollViewer4CP.Visibility = Visibility.Collapsed;
                            scrollViewer4Login.Visibility = Visibility.Visible;
                            scrollViewer4CP.Content = null;
                            Job.Database.Logout();
                        });
                    }));
                    #endregion
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(()=> {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            })
            { ApartmentState = System.Threading.ApartmentState.STA }.Start();

        }
    }
}
