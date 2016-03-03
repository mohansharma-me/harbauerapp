using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Interaction logic for winImages.xaml
    /// </summary>
    public partial class winImages : Window
    {
        public winImages()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            reloadImages();
        }

        private void reloadImages()
        {
            if (Directory.Exists("images"))
            {
                string[] files = Directory.GetFiles("images");
                List<classes.Image> imgs = new List<classes.Image>();
                foreach (string img in files)
                    imgs.Add(new classes.Image() { ImagePath = AppDomain.CurrentDomain.BaseDirectory + "/" + img });
                lvImages.ItemsSource = imgs;
            }
        }

        private void lvImages_KeyDown(object sender, KeyEventArgs e)
        {
            if(lvImages.SelectedItems.Count>0 && e.Key==Key.Delete)
            {
                MessageBoxResult res = MessageBox.Show("Are you sure to delete all selected images ?", "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(res==MessageBoxResult.Yes)
                {
                    foreach(classes.Image img in lvImages.SelectedItems)
                    {
                        try
                        {
                            System.IO.File.Delete(img.ImagePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to delete image[" + img + "] due to '" + ex.Message + "'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    reloadImages();
                }
            }
        }
    }
}
