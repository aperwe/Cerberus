using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Microsoft.Localization.LocSolutions.Cerberus.Resources
{
    /// <summary>
    /// Class that enables easy access to cached icons.
    /// </summary>
    public static class ResourceHelper
    {
        #region Private members
        static ImageSourceConverter IconLoader { get; set; }
        static Dictionary<string, ImageSource> ImageCache { get; set; }
        #endregion

        #region Public API
        /// <summary>
        /// Gets an image with the specified icon file.
        /// </summary>
        /// <param name="icon">Type of icon correpsonding to the icon file stored in Resources folder.</param>
        public static ImageSource GetCachedImageSource(IconType icon)
        {
            string resUriString = GetResourceUri(icon);
            if (ImageCache.ContainsKey(resUriString)) return ImageCache[resUriString];

            //Cache hit failed, construct new image source
            var resUri = new Uri(resUriString);
            var resInfo = Application.GetResourceStream(resUri);
            var retVal = IconLoader.ConvertFrom(resInfo.Stream) as ImageSource;
            ImageCache.Add(resUriString, retVal);
            return retVal;
        }
        /// <summary>
        /// Gets a cached Image object using the specified image source (cached on first use).
        /// </summary>
        /// <param name="icon">Icon from which to derive the icon.</param>
        /// <param name="width">Intended width of the image. Scaling will happen as required.</param>
        /// <param name="height">Intended height of the image. Scaling will happen as required.</param>
        public static Image GetImage(IconType icon, double width, double height)
        {
            var retVal = new Image { Width = width, Height = height, Source = GetCachedImageSource(icon) };
            return retVal;
        }
        #endregion

        static ResourceHelper()
        {
            IconLoader = new ImageSourceConverter();
            ImageCache = new Dictionary<string, ImageSource>();
        }
        /// <summary>
        /// Retrieve Uri string for the resource file corresponding to the icon type.
        /// </summary>
        /// <param name="icon">Icon, whose Uri is to be retrieved</param>
        private static string GetResourceUri(IconType icon)
        {
            //var retVal = new StringBuilder("pack://application:,,,/ResourceFiles/");
            var retVal = new StringBuilder("pack://application:,,,/Cerberus.Resources;component/ResourceFiles/");
            switch (icon)
            {
                case IconType.Add: retVal.Append("Add.ico"); break;
                case IconType.Basketball: retVal.Append("Basketball.ico"); break;
                case IconType.Calendar: retVal.Append("Calendar.ico"); break;
                case IconType.Check: retVal.Append("Check.ico"); break;
                case IconType.ConstructionCone: retVal.Append("construction_cone.ico"); break;
                case IconType.Database: retVal.Append("Database-3.ico"); break;
                case IconType.DogDachshund: retVal.Append("Dachshund.ico"); break;
                case IconType.DogWalking: retVal.Append("dog.ico"); break;
                case IconType.DogBarking: retVal.Append("dog2.ico"); break;
                case IconType.Document: retVal.Append("Document.ico"); break;
                case IconType.Edit: retVal.Append("Edit.ico"); break;
                case IconType.EditCutScissors: retVal.Append("editcut.ico"); break;
                case IconType.EditAdd: retVal.Append("edit_add.ico"); break;
                case IconType.EditNo: retVal.Append("Edit_No.ico"); break;
                case IconType.EditRemove: retVal.Append("edit_remove.ico"); break;
                case IconType.EditYes: retVal.Append("Edit_Yes.ico"); break;
                case IconType.Favorites: retVal.Append("Favorites.ico"); break;
                case IconType.FolderPale: retVal.Append("Folder.ico"); break;
                case IconType.FolderBlue: retVal.Append("Folder2.ico"); break;
                case IconType.Info: retVal.Append("Info.ico"); break;
                case IconType.EarthGlobe: retVal.Append("Internet.ico"); break;
                case IconType.PadLock: retVal.Append("Lock.ico"); break;
                case IconType.MailWhite: retVal.Append("Mail 1.ico"); break;
                case IconType.MailGreen: retVal.Append("Mail.ico"); break;
                case IconType.ManBrown: retVal.Append("Man_Brown.ico"); break;
                case IconType.Network: retVal.Append("Network.ico"); break;
                case IconType.PlayButton: retVal.Append("Play1Normal.ico"); break;
                case IconType.Pug: retVal.Append("Pug.ico"); break;
                case IconType.RecycleBinEmpty: retVal.Append("RB-Empty.ico"); break;
                case IconType.RecycleBinFull: retVal.Append("RB-Full.ico"); break;
                case IconType.Refresh: retVal.Append("Refresh.ico"); break;
                case IconType.Search: retVal.Append("Search.ico"); break;
                case IconType.Settings: retVal.Append("Settings 2.ico"); break;
                case IconType.TagAdd: retVal.Append("Tag Add.ico"); break;
                case IconType.TagRemove: retVal.Append("Tag Remove.ico"); break;
                case IconType.Tag: retVal.Append("Tag.ico"); break;
                default: throw new ArgumentOutOfRangeException("icon", "Unsupported icon requested.");
            }
            return retVal.ToString();
        }
    }
}
