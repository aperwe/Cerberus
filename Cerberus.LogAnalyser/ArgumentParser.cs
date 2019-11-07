using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace Cerberus.LogAnalyser
{
    /// <summary>
    /// What's this Tiernan? Why don't you put comments in your code?
    /// </summary>
    class ArgumentParser
    {
        /// <summary>
        /// Current assembly's full name identification.
        /// </summary>
        public string ExeID { get; private set; }
        /// <summary>
        /// Name of the exectuable. To be used in user-friendly test (help).
        /// </summary>
        public string ExeName { get; private set; }
        /// <summary>
        /// Path to directory where this exe is located.
        /// </summary>
        public string ExeDir { get; private set; }
        /// <summary>
        /// Reference to the assembly object containing this code (client exe).
        /// </summary>
        public Assembly ThisAssembly { get; private set; }

        /// <summary>
        /// What's this Tiernan? Why don't you put comments in your code?
        /// </summary>
        public string OfficeDir { get; private set; }
        /// <summary>
        /// What's this Tiernan? Why don't you put comments in your code?
        /// </summary>
        public UploadLocation Upload { get; private set; }
        /// <summary>
        /// What's this Tiernan? Why don't you put comments in your code?
        /// </summary>
        public bool isDirectory { get; private set; }
        /// <summary>
        /// What's this Tiernan? Why don't you put comments in your code?
        /// </summary>
        public bool isFile { get; private set; }


        /// <summary>
        /// What's this Tiernan? Why don't you put comments in your code?
        /// </summary>
        public ArgumentParser(string[] args)
        {
            Initialize();

            if (args != null)
            {
                if (args.Length == 1)
                {
                    if (args[0].ToLower() == "help")
                    {
                        Upload = UploadLocation.Help;
                    }
                    else
                    {
                        Upload = UploadLocation.ShowHelpOnBadArguments;
                    }
                }
                if (args.Length == 2)
                {
                    OfficeDir = args[0];
                    switch (args[1].ToLower())
                    {
                        case "local":
                            {
                                Upload = UploadLocation.Local;
                                
                                if (!Directory.Exists(OfficeDir))
                                {
                                    throw new DirectoryNotFoundException();
                                }
                                break;
                            }
                        case "fasttrack":
                            {
                                Upload = UploadLocation.FastTrack;

                                if (!Directory.Exists(OfficeDir))
                                {
                                    throw new DirectoryNotFoundException();
                                }
                                break;
                            }
                        case "backup":
                            {
                                Upload = UploadLocation.BackUp;

                                if (!Directory.Exists(OfficeDir))
                                {
                                    throw new DirectoryNotFoundException();
                                }
                                break;
                            }
                        case "cleanup":
                            {
                                Upload = UploadLocation.CleanUp;

                                if (!Directory.Exists(OfficeDir))
                                {
                                    throw new DirectoryNotFoundException();
                                }
                                break;
                            }
                        case "fasttracksingle":
                            {
                                Upload = UploadLocation.FastTrackSingle;
                                if (!File.Exists(OfficeDir))
                                {
                                    throw new FileNotFoundException();
                                }
                                break;
                            }
                        default:
                            {
                                throw new ArgumentException(Cerberus.LogAnalyser.Properties.Resources.InvalidUploadLocation);
                            }

                    }
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// What's this Tiernan? Why don't you put comments in your code?
        /// </summary>
        public string Help()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("Parameters for {0}:", ExeName);
            sb.AppendLine();
            sb.AppendLine("Type 'help' or specify no parameters to display this help.");
            sb.AppendLine("Examples of usage:");
            sb.AppendLine(@"C:\office local                          - shows the results in your C:\Office directory locally");
            sb.AppendLine(@"C:\office fasttrack                      - uploads the results in your C:\Office directory to fasttrack");
            sb.AppendLine(@"C:\office backup                         - backs up the results in your C:\Office directory");
            sb.AppendLine(@"C:\office cleanup                        - cleans up the results in your C:\Office directory");
            sb.AppendLine(@"C:\CerberusReport.xml fasttracksingle    - uploads the contents of the XML file to FastTrack");
            return sb.ToString();
        }
        /// <summary>
        /// Initializes variables of LogAnalyser instance on program startup.
        /// </summary>
        private void Initialize()
        {
            ThisAssembly = Assembly.GetExecutingAssembly();
            ExeID = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}", ThisAssembly.GetName().Name, ThisAssembly.GetName().Version);
            ExeName = ThisAssembly.ManifestModule.Name;
            ExeDir = Path.GetDirectoryName(ThisAssembly.Location);
        }
    }
}
