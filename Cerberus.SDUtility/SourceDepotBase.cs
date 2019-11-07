using System;
using System.Collections.Generic; 
using System.Collections.ObjectModel;
using System.Text;
using SourceDepotClient;
using System.Runtime.InteropServices;

namespace Microsoft.OffGlobe.SourceDepot
{
    public class SourceDepotBase : IDisposable
    {
        private SDConnection connection;

        public SourceDepotBase(string sdIniPath)
        {
            try
            {
                this.connection = new SDConnection();
                this.connection.LoadIniFile(sdIniPath, true);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw new SourceDepotException(ex.Message); 
            }
        }

        public SourceDepotBase(string port, string client)
        {
            try
            {
                this.connection = new SDConnection();
                this.connection.Port = port;
                this.connection.Client = client;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw new SourceDepotException(ex.Message);
            }
        }

        public string Port
        {
            get { return this.connection.Port; }
        }

        public string Client
        {
            get { return this.connection.Client; }
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">Source depot command to be executed</param>
        /// <returns>Results of the source depot call.</returns>
        protected SDResults Execute(SourceDepotCommand command)
        {
            var results = ExecuteCommand(command);
            if (results.ErrorOutput.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (SDCommandOutput item in results.ErrorOutput)
                {
                    sb.AppendLine(item.Message);
                }
                throw new SourceDepotException(sb.ToString());
            }
            return results;
        }

        /// <summary>
        /// Executes the command and allows to specify whether structured output needs to be created.
        /// </summary>
        /// <param name="command">Source depot command to be executed</param>
        /// <param name="structuredOutput">Parameter passed into connection.Run() method.</param>
        /// <returns>Results of the source depot call.</returns>
        protected SDResults Execute(SourceDepotCommand command, bool structuredOutput)
        {
            var results = ExecuteCommand(command, structuredOutput);
            if (results.ErrorOutput.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (SDCommandOutput item in results.ErrorOutput)
                {
                    sb.AppendLine(item.Message);
                }
                throw new SourceDepotException(sb.ToString());
            }
            return results;
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">Source depot command to be executed</param>
        /// <returns>Results of the source depot call.</returns>
        private SDResults ExecuteCommand(SourceDepotCommand command)
        {
            if (command.Arguments != null)
            {
                foreach (string argument in command.Arguments)
                {
                    this.connection.AddArg(argument);
                }
            }
            SDResults results = this.connection.Run(command.Name, true, false);
            results.WaitUntilFinished();
            return results;
        }

        /// <summary>
        /// Executes the command and allows to specify whether structured output needs to be created.
        /// </summary>
        /// <param name="command">Source depot command to be executed</param>
        /// <param name="structuredOutput">Parameter passed into connection.Run() method.</param>
        /// <returns>Results of the source depot call.</returns>
        private SDResults ExecuteCommand(SourceDepotCommand command, bool structuredOutput)
        {
            if (command.Arguments != null)
            {
                foreach (string argument in command.Arguments)
                {
                    this.connection.AddArg(argument);
                }
            }
            SDResults results = this.connection.Run(command.Name, structuredOutput, false);
            results.WaitUntilFinished();
            return results;
        }

        protected void DefineMapping(string root, object views)
        {
            SDResults sdresults = this.connection.Run("client -o", true, true);
            sdresults.WaitUntilFinished();
            if (sdresults.ErrorOutput.Count != 0)
            {
                throw new SourceDepotException("Error: can't get a client specification.");
            }

            SDCommandOutput sdoutput = sdresults.StructuredOutput[0];
            SDSpecData sdspecForm = sdoutput.Variables.SpecData;

            // set up view
            sdspecForm["Root"] = root;
            sdspecForm["View"] = views;
            connection.SpecData = sdspecForm.FormattedSpec;
            sdresults = connection.Run("client -i", true, true);
            sdresults.WaitUntilFinished();
            if (sdresults.ErrorOutput.Count != 0)
            {
                throw new SourceDepotException("Error: didn't get a client mapping...");
            }
        }

        protected string CreateChangeList(string description)
        {
            SDResults sdresults = this.connection.Run("change -o", true, true);
            sdresults.WaitUntilFinished();
            if (sdresults.ErrorOutput.Count != 0)
            {
                throw new SourceDepotException("Error: can't get a change specification.");
            }

            SDCommandOutput sdoutput = sdresults.StructuredOutput[0];
            SDSpecData sdspecForm = sdoutput.Variables.SpecData;

            // set up view
            sdspecForm["Description"] = description;
            connection.SpecData = sdspecForm.FormattedSpec;
            sdresults = connection.Run("change -i", true, true);
            sdresults.WaitUntilFinished();
            if (sdresults.ErrorOutput.Count != 0)
            {
                throw new SourceDepotException("Error: didn't get a change list number...");
            }
            
            // Change XXXXXX created
            SourceDepotCommandResult result = new SourceDepotCommandResult(sdresults);
            var resultMessages = result.GetMessages();
            string[] separator = new string[1] { " " };
            if (resultMessages.Count == 1)
            {
                string[] splitResult = resultMessages[0].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                return splitResult[1];
            }
            else
            {
                throw new SourceDepotException("Error: didn't get a change list number...");
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.connection.Connected)
            {
                this.connection.Disconnect();
                while (Marshal.ReleaseComObject(this.connection) != 0) { }
                this.connection = null;
            }
        }

        #endregion
    }
}
