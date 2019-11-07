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
using System.IO;
using System.Xml.Linq;
using System.Collections;
using Microsoft.Localization.Lcx;
using System.Xml;

namespace Microsoft.Localization.LocSolutions.Tools.LcxOmMemoryChecker
{
    /// <summary>
    /// LCX OM facade class to allow loading LCX OM files.
    /// </summary>
    public class Facade
    {
        /// <summary>
        /// Path to the response file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Constructor. After constructing call <see cref="LoadAll"/> to process the list.
        /// </summary>
        /// <param name="path">Path to OSLEBot response file.</param>
        public Facade(string path)
        {
            Path = path;
            if (!File.Exists(Path))
            {
                throw new FileNotFoundException("File not found.", Path);
            }

            #region This is purely to avoid null reference exceptions when accessing events.
            LoadingStarted += (x, y) => { };
            ItemLoaded += (x, y) => { };
            LoadingFinished += (x, y) => { };
            #endregion
        }

        /// <summary>
        /// Loads all LCX files specified in the supplied response file.
        /// </summary>
        public void LoadAll()
        {
            LoadingStarted(this, new LcxLoaderEventData { });
            var files = File.ReadAllLines(Path);
            foreach (var file in files)
            {
                try
                {
                    var xElement = XElement.Parse(file);
                    var collection = LoadLcxFile(xElement.Value);
                    ItemLoaded(this, new LcxLoaderEventData { Data = collection, FileName = xElement.Value });
                }
                catch (XmlException ex)
                {
                    ItemLoaded(this, new LcxLoaderEventData { FileName = ex.Message });
                }
                catch (FileNotFoundException ex)
                {
                    ItemLoaded(this, new LcxLoaderEventData { FileName = string.Format("{0} - {1}", ex.Message, ex.FileName) });
                }
            }
            LoadingFinished(this, new LcxLoaderEventData { });
        }

        /// <summary>
        /// Loads a single LCX file and returns a flat enumeration of LocItems contained in that document.
        /// </summary>
        private IEnumerable LoadLcxFile(string pathToLcx)
        {
            if (!File.Exists(pathToLcx))
            {
                throw new FileNotFoundException("File not found.", pathToLcx);
            }
            LcxReaderWriter reader = new LcxReaderWriter(pathToLcx, FileMode.Open, FileAccess.Read);
            LcxPolicy policy = new LcxPolicy();
            var document = reader.Load(policy);

            return CreateEnumeration(document);
        }

        /// <summary>
        /// Creates a flat enumeration of all LocItems contained within the specified document.
        /// </summary>
        /// <param name="document">LCX document to process.</param>
        private IEnumerable CreateEnumeration(LocDocument document)
        {
            List<LocItem> collection = new List<LocItem>();
            AddRecursively(collection, document.Items);
            return collection;
        }

        /// <summary>
        /// Recursively adds all items and their children to the collection.
        /// </summary>
        private void AddRecursively(List<LocItem> collection, LocItemList items)
        {
            collection.AddRange(items);
            foreach (var item in items)
            {
                AddRecursively(collection, item.Children);
            }
        }

        /// <summary>
        /// Event raised when loading of the whole response file has started.
        /// </summary>
        public event EventHandler<LcxLoaderEventData> LoadingStarted;

        /// <summary>
        /// Event raised when loading of a single LCX file has been completed.
        /// </summary>
        public event EventHandler<LcxLoaderEventData> ItemLoaded;

        /// <summary>
        /// Event raised after loading of the whole response file has been completed.
        /// </summary>
        public event EventHandler<LcxLoaderEventData> LoadingFinished;
    }

}
