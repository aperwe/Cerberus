using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Diagnostics;
using System.ComponentModel;
using System.Data.Linq;
using System.Collections;
using System.Security.AccessControl;
using System.Data.SqlClient;

namespace Cerberus.LogAnalyser
{
    public class Program
    {

        private static ArgumentParser ParsedArguments;

        static void Main(string[] args)
        {
            string htmlResult = "";
            try
            {
                ParsedArguments = new ArgumentParser(args);
                IntroduceYourself(ParsedArguments);

                switch (ParsedArguments.Upload)
                {
                    case UploadLocation.BackUp:
                        BackupFiles(ParsedArguments);
                        break;
                    case UploadLocation.CleanUp:
                        CleanUpFiles(ParsedArguments);
                        break;
                    case UploadLocation.FastTrack:
                        UploadToFastTrack(ParsedArguments);
                        break;
                    case UploadLocation.Local:
                        UploadLocal(ParsedArguments, ref htmlResult);
                        break;
                    case UploadLocation.FastTrackSingle:
                        uploadFastTrackSingle(ParsedArguments);
                        break;
                    case UploadLocation.Help:
                        ShowHelp();
                        break;
                    case UploadLocation.ShowHelpOnBadArguments:
                        Console.WriteLine(Cerberus.LogAnalyser.Properties.Resources.OneArgumentNotHelp);
                        ShowHelp();
                        break;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = -6;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = -1;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("No arguments passed in");
                Console.WriteLine(ParsedArguments.Help());
                Environment.ExitCode = -2;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ParsedArguments.Help());
                Environment.ExitCode = -3;
            }

            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = -4;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(string.Format("Error openeing IE with the result file. please open {0} to view the results", htmlResult));
                Environment.ExitCode = -5;
            }
            catch (NotImplementedException ex)
            {
                Console.WriteLine("The Method you have typed is not implimented yet. Sorry");
                Console.WriteLine(ex.Message);
                Environment.ExitCode = -6;
            }
        }


        /// <summary>
        /// Where is exception handling Tiernan? It This method should report an error on failure.
        /// </summary>
        private static void uploadFastTrackSingle(ArgumentParser p)
        {
            //try
            //{

            OSLEBot ob = GetSingleOLSEBotObject(p.OfficeDir);

            UploadOsleBotStuffToFastTrack(ob);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Something went wrong while uploading to FastTrack: {0}", ex.Message);
            //}
        }

        private static void ShowHelp()
        {
            Console.WriteLine(ParsedArguments.Help());
        }

        /// <summary>
        /// Displays general introduction of the tool.
        /// </summary>
        private static void IntroduceYourself(ArgumentParser parser)
        {
            Console.WriteLine("{0} ({1})", parser.ExeName, parser.ExeID); //Introduce yourself
            Console.WriteLine("Product created in August 2009. Support alias: LocSolutions Support (lssup@microsoft.com).");
            Console.WriteLine();
        }

        //private static Nullable<bool> StringToBool(string result)
        //{
        //    if (result.ToLower() == "true")
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        private static void UploadToFastTrack(ArgumentParser p)
        {
            try
            {
                
                OSLEBot ob = GetOSLEBotObject(new LogAnalyser(p.OfficeDir));

                UploadOsleBotStuffToFastTrack(ob);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong while uploading to FastTrack: {0}", ex.Message);
            }
        }

        private static void UploadOsleBotStuffToFastTrack(OSLEBot ob)
        {
            using (FastTrackDataContext dc = new FastTrackDataContext())
            {

                int? newRun = 0;
                dc.NewRun(Environment.UserName, ref newRun);

                Console.WriteLine("RunID : {0}", newRun);
                foreach (var coo in ob.co)
                {
                    string lsResID = null;
                    string sourceString = null;
                    string comments = null;
                    string targetCulture = null;
                    string locGroup = null;
                    string project = null;
                    string lcxFileName = null;

                    foreach (var prop in coo.props)
                    {
                        switch (prop.name.ToLower())
                        {
                            case "lsresid":
                                lsResID = prop.value;
                                break;
                            case "sourcestring":
                                sourceString = prop.value;
                                break;
                            case "comments":
                                comments = prop.value;
                                break;
                            case "targetculture":
                                targetCulture = prop.value;
                                break;
                            case "locgroup":
                                locGroup = prop.value;
                                break;
                            case "project":
                                project = prop.value;
                                break;
                            case "lcxfilename":
                                lcxFileName = prop.value;
                                break;
                            default:
                                Debug.WriteLine(string.Format("Not known property: {0}", prop.value));
                                break;
                        }
                    }

                    if (!String.IsNullOrEmpty(lsResID) && !String.IsNullOrEmpty(sourceString) && !String.IsNullOrEmpty(comments) && !String.IsNullOrEmpty(targetCulture) && !String.IsNullOrEmpty(locGroup) && !String.IsNullOrEmpty(project) && !String.IsNullOrEmpty(lcxFileName))
                    {
                        int? strID = 0;
                        dc.NewStrInfo(newRun.Value.ToString(),
                                        project,
                                        locGroup,
                                        lcxFileName,
                                        sourceString,
                                        comments,
                                        lsResID,
                                        targetCulture,
                                        newRun, ref strID);
                        Array.ForEach(coo.rules, rule =>
                        {
                            if (strID != 0)
                            {
                                int? strResultID = 0;
                                dc.NewStrResult(strID, rule.item[0].message, rule.item[0].result, rule.name, ref strResultID);
                            }
                        });
                    }
                }
            }
        }

        private static void CleanUpFiles(ArgumentParser p)
        {
            Console.WriteLine("This will remove all files in the directory {0} with the name *_OSLEBotOutput.xml.", p.OfficeDir);
            Console.WriteLine("Are you sure you want to continue? type yes or no");
            string response = Console.ReadLine();
            if (response.ToLower() == "yes")
            {

                new LogAnalyser(p.OfficeDir).GetResults().ForEach(getResult =>
                {
                    try
                    {
                        File.Delete(getResult.LogLocation);
                        Console.WriteLine("Deleted {0}", getResult.LogLocation);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot delete {0}. please manually remove this... {1}", getResult.LogLocation, ex.Message);
                    }
                });
                return;
            }
            if (response.ToLower() == "no")
            {
                Console.WriteLine("Grand job... Bye now!");
                return;
            }
            Console.WriteLine("Sorry, i did not quite here you there.... try again...");
            CleanUpFiles(p);
        }

        private static void BackupFiles(ArgumentParser p)
        {
            string append = string.Format("_back_{0}", DateTime.Now.Ticks);
            Console.WriteLine("This command will append {0} to all files currently ending in OSLEBotOutput.xml so as they wont get processed again", append);
            Console.WriteLine("Are you sure you want to continue? type yes or no");
            string response = Console.ReadLine();
            if (response.ToLower() == "yes")
            {
                var l = new LogAnalyser(p.OfficeDir);
                l.GetResults().ForEach(getResult =>
                {
                    string newFileName = getResult.LogLocation + append;
                    try
                    {
                        File.Move(getResult.LogLocation, newFileName);
                        Console.WriteLine("Backed up {0} to {1}", getResult.LogLocation, newFileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot move {0} to {1}. please manually move this... {2}", getResult.LogLocation, newFileName, ex.Message);
                    }
                });
                return;
            }
            if (response.ToLower() == "no")
            {
                Console.WriteLine("Grand job... Bye now!");
                return;
            }
            Console.WriteLine("Sorry, i did not quite hear you there.... try again...");
            BackupFiles(p);
        }

        private static void UploadLocal(ArgumentParser p, ref string htmlResult)
        {
            LogAnalyser la = new LogAnalyser(p.OfficeDir);

            string outputFile = Path.Combine(p.OfficeDir, "oslebot_combined_Output.xml");
            using (FileStream fs = new FileStream(outputFile, FileMode.OpenOrCreate))
            {
                XmlSerializer xs = new XmlSerializer(typeof(OSLEBot));
                xs.Serialize(fs, GetOSLEBotObject(la));
                
                fs.Flush();
                fs.Close();
            }
            XslCompiledTransform xslTrans = new XslCompiledTransform();

            xslTrans.Load(@"OSLEBot.V1.Output.1.xslt");
            htmlResult = Path.Combine(p.OfficeDir, string.Format("OlseBotOutput_{0}.html", DateTime.Now.Ticks));
            xslTrans.Transform(outputFile, htmlResult);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = htmlResult;

                proc.Start();
            }
        }

        private static OSLEBot GetOSLEBotObject(LogAnalyser la)
        {
            

            OSLEBot ob = new OSLEBot { start = "Now", version = "1.0" };
            List<OSLEBotCO> listOCos = new List<OSLEBotCO>();
            XmlSerializer xs = new XmlSerializer(typeof(OSLEBot));
            la.GetResults().ForEach(x =>
            {
                Console.WriteLine("LocGroup: {0} \n Project: {1} \n Language: {2} \n LogFile: {3}", x.LocGroup, x.Project, x.Language, x.LogLocation);
                OSLEBot o = (OSLEBot)xs.Deserialize(new FileStream(x.LogLocation, FileMode.Open));
                Array.ForEach(o.co, listOCos.Add);
            });

            ob.co = listOCos.ToArray();
            return ob;
        }

        private static OSLEBot GetSingleOLSEBotObject(string singleFile)
        {
            XmlSerializer xs = new XmlSerializer(typeof(OSLEBot));
            OSLEBot ob = (OSLEBot)xs.Deserialize(new FileStream(singleFile, FileMode.Open));
            return ob;
        }
    }
}