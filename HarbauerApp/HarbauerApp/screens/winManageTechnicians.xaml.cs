using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HarbauerApp.screens
{
    /// <summary>
    /// Interaction logic for winManageTechnicians.xaml
    /// </summary>
    public partial class winManageTechnicians : Window
    {
        public winManageTechnicians()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshUsers();
        }

        private void refreshUsers()
        {
            lvUsers.ItemsSource = classes.Job.Database.getAllUsers();
        }

        private void btnNewUser_Click(object sender, RoutedEventArgs e)
        {
            lvUsers.Visibility = navListView.Visibility = Visibility.Collapsed;
            spNewUser.Visibility = navAddUser.Visibility = Visibility.Visible;
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteUsers_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure to delete all selected users ?", "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(res==MessageBoxResult.Yes)
            {
                System.Collections.IList items = lvUsers.SelectedItems;
                new Thread((list) => {
                    try
                    {
                        #region MyRegion
                        List<long> userIds = new List<long>();
                        foreach (classes.User user in (System.Collections.IList)list)
                        {
                            userIds.Add(user.userId);
                        }

                        if(!classes.Job.Database.deleteUsers(userIds))
                        {
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                                MessageBox.Show("Unable to delete users, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }));
                        }

                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { refreshUsers(); }));

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                    }
                }) { ApartmentState = ApartmentState.STA }.Start(items);
            }
        }

        private void btnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            refreshUsers();
        }

        private void btnSubmitNewUserForm_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtUsername.Text.Trim();
            string userPass = txtPassword.Password.Trim();
            bool isAdmin = cmbUserKind.SelectedIndex == 0;

            if(userName.Length==0)
            {
                MessageBox.Show("Invalid username, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return;
            }

            new System.Threading.Thread(()=> {
                try
                {
                    #region MyRegion
                    bool added = classes.Job.Database.addUser(userName, userPass, isAdmin);
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,new Action(()=> {
                        if(added)
                        {
                            MessageBox.Show("User successfully added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        } else
                        {
                            MessageBox.Show("Cant added user, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }));
                    #endregion
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            }) { ApartmentState=System.Threading.ApartmentState.STA }.Start();
        }

        private void btnBackToListView_Click(object sender, RoutedEventArgs e)
        {
            lvUsers.Visibility = navListView.Visibility = Visibility.Visible;
            spNewUser.Visibility = navAddUser.Visibility = Visibility.Collapsed;
            refreshUsers();
        }

        private void lvUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //btnEditUser.IsEnabled = lvUsers.SelectedItems.Count == 1;
            btnDeleteUsers.IsEnabled = lvUsers.SelectedItems.Count > 0;
        }
    }
}
