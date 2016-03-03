using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HarbauerApp.classes
{
    public class User
    {
        public long userId;

        public string userName
        {
            get; set;
        }

        public string userKind
        {
            get; set;
        }
        
        public System.Windows.Media.Imaging.BitmapImage userImage
        {
            get
            {
                string filename = userKind.Equals("administrator") ? "admin.png" : "technicien.png";
                return Job.getImageFromMedia(filename);
            }
        }
    }
}
