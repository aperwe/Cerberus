using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cerberus.LogAnalyser
{
    class LogAnalyser
    {
        public string LogDirectory { get; private set; }
        /// <summary>
        /// Initializes a new instance of the LogAnalyser class.
        /// </summary>
        public LogAnalyser(string logDirectory)
        {
            LogDirectory = logDirectory;
        }
        public List<FileSearchResults> GetResults()
        {
            List<FileSearchResults> results = new List<FileSearchResults>();

            string[] files = Directory.GetFiles(LogDirectory, "*_OSLEBotOutput.xml", SearchOption.AllDirectories);
            Array.ForEach(files, s =>
            {
                //"C:\Users\tiernano\Desktop\Office\dev14\intl\accwiz\de-de\wizards\acwizrc.rest.lcl_OSLEBotOutput.xml"
                var sp = s.Split('\\');
                results.Add(new FileSearchResults { LocGroup = sp[sp.Length - 1], Language = sp[sp.Length - 2], LogLocation = s, Project = sp[sp.Length - 3] });
            });
            return results;
        }

      


    }
}
