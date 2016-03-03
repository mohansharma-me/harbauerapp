using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarbauerApp.classes
{
    class Image
    {
        private System.Windows.Media.Imaging.BitmapImage cacheImage = null;
        public string ImagePath
        {
            get; set;
        }

        public string ImageName
        {
            get
            {
                return new System.IO.FileInfo(ImagePath).Name;
            }
        }

        public System.Windows.Media.Imaging.BitmapImage ImageObject
        {
            get
            {
                if(cacheImage==null)
                {
                    try
                    {
                        System.IO.MemoryStream ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(ImagePath));
                        cacheImage = new System.Windows.Media.Imaging.BitmapImage();
                        cacheImage.BeginInit();
                        cacheImage.StreamSource = ms;
                        cacheImage.EndInit();
                    }
                    catch (Exception) { }
                }
                return cacheImage;
            }
        }
    }
}
