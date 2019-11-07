using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SourceDepotClient;

namespace Microsoft.OffGlobe.SourceDepot
{
    /// <summary>
    /// Interface to source depot.
    /// </summary>
    public sealed class SourceDepot: SourceDepotBase
    {
        private string sdChangeListNumber;

        public SourceDepot(string sdIniPath): base(sdIniPath){}
        public SourceDepot(string port, string client, string root, string mapping): base(port, client) 
        {
            base.DefineMapping(root, mapping);
        }

        /// <summary>
        /// Synchronizes the specified file pattern in depot.
        /// </summary>
        /// <param name="filePattern">Pattern to synchronize.</param>
        public SourceDepotCommandResult Sync(string filePattern)
        {
            var commandArguments = new List<string>();
            commandArguments.Add(filePattern);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand() { Name = "sync", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        /// <summary>
        /// Synchronizes the specified file pattern in depot.
        /// </summary>
        /// <param name="filePattern">Pattern to synchronize.</param>
        /// <param name="branchName">Branch on which to synchronize.</param>
        public SourceDepotCommandResult Sync(string filePattern, string branchName)
        {
            var commandArguments = new List<string>();
            commandArguments.Add("-b");
            commandArguments.Add(branchName);
            commandArguments.Add(filePattern);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand() { Name = "sync", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        /// <summary>
        /// Forces synchronization of the specified file pattern in depot.
        /// </summary>
        /// <param name="filePattern">Pattern to synchronize.</param>
        public SourceDepotCommandResult ForceSync(string filePattern)
        {
            var commandArguments = new List<string>();
            commandArguments.Add("-f");
            commandArguments.Add(filePattern);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand() { Name = "sync", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        /// <summary>
        /// Forces synchronization of the specified file pattern in depot.
        /// </summary>
        /// <param name="filePattern">Pattern to synchronize.</param>
        /// <param name="branchName">Branch on which to synchronize.</param>
        public SourceDepotCommandResult ForceSync(string filePattern, string branchName)
        {
            var commandArguments = new List<string>();
            commandArguments.Add("-f");
            commandArguments.Add("-b");
            commandArguments.Add(branchName);
            commandArguments.Add(filePattern);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand() { Name = "sync", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult Opened(string filePattern)
        {
            var commandArguments = new List<string>();
            commandArguments.Add(filePattern);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand() { Name = "opened", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResultFiles(result);
        }

        public SourceDepotCommandResult Files(string filePattern)
        {
            var commandArguments = new List<string>();
            commandArguments.Add("-d");
            commandArguments.Add(filePattern);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand() { Name = "files", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResultFiles(result);
        }

        public string GetChangeList(string description)
        {
            // Get a change list number from core depot using a description
            this.sdChangeListNumber = base.CreateChangeList(description);
            return this.sdChangeListNumber;
        }

        public SourceDepotCommandResult Edit(string filePattern)
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            List<string> commandArguments = new List<string>();
            commandArguments.Add(string.Format("-c {0}", this.sdChangeListNumber));
            commandArguments.Add(filePattern);
            var command = new SourceDepotCommand() { Name = "edit", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult Add(string filePattern)
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            List<string> commandArguments = new List<string>();
            commandArguments.Add(string.Format("-c {0}", this.sdChangeListNumber));
            commandArguments.Add("-t text");
            commandArguments.Add(filePattern);
            var command = new SourceDepotCommand() { Name = "add", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult Delete(string filePattern)
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            List<string> commandArguments = new List<string>();
            commandArguments.Add(string.Format("-c {0}", this.sdChangeListNumber));
            commandArguments.Add(filePattern);
            var command = new SourceDepotCommand() { Name = "delete", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult DeleteChangeList()
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            List<string> commandArguments = new List<string>();
            commandArguments.Add("-d");
            commandArguments.Add(this.sdChangeListNumber);
            var command = new SourceDepotCommand() { Name = "change", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult RevertA()
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            List<string> commandArguments = new List<string>();
            commandArguments.Add("-a");
            commandArguments.Add(string.Format("-c {0}", this.sdChangeListNumber));
            var command = new SourceDepotCommand() { Name = "revert", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult Revert()
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            List<string> commandArguments = new List<string>();
            commandArguments.Add(string.Format("-c {0}", this.sdChangeListNumber));
            var command = new SourceDepotCommand() { Name = "revert", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public SourceDepotCommandResult Revert(string filePattern)
        {
            List<string> commandArguments = new List<string>();
            commandArguments.Add(filePattern);
            var command = new SourceDepotCommand() { Name = "revert", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }
        
        public SourceDepotCommandResult Describe()
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            var commandArguments = new List<string>();
            commandArguments.Add(this.sdChangeListNumber);
            var command = new SourceDepotCommand() { Name = "describe", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResultDescribe(result);
        }

        public SourceDepotCommandResult Submit()
        {
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            var commandArguments = new List<string>();
            commandArguments.Add(string.Format("-c {0}", this.sdChangeListNumber));
            var command = new SourceDepotCommand() { Name = "submit", Arguments = commandArguments };
            var result = base.Execute(command);
            return new SourceDepotCommandResult(result);
        }

        public int NumberOfFilesInChangeList(out Microsoft.OffGlobe.SourceDepot.SourceDepotCommandResult resultSD)
        {
            int numberOfFiles = 0;
            if (string.IsNullOrEmpty(this.sdChangeListNumber))
            {
                throw new SourceDepotException("You need to create first a change list before using this method");
            }
            var commandArguments = new List<string>();
            commandArguments.Add(this.sdChangeListNumber);
            var command = new SourceDepotCommand() { Name = "describe", Arguments = commandArguments };
            var result = base.Execute(command);
            resultSD = new SourceDepotCommandResultDescribe(result);
            for (int counter = 0; counter < result.StructuredOutput[0].Variables.Count; counter++)
            {
                if (result.StructuredOutput[0].Variables.SpecData == null)
                {
/*
                    Console.WriteLine("Variable name is: {0}", result.StructuredOutput[0].Variables[counter].Name);
                    Console.WriteLine("Variable value is: {0}", result.StructuredOutput[0].Variables[counter].Value);
*/ 
                    if (result.StructuredOutput[0].Variables[counter].Name.Contains("depotFile"))
                    {
                        numberOfFiles++;
                    }
                }
            }



           return numberOfFiles;
        }

        /// <summary>
        /// Gets desrciption of the specified label.
        /// </summary>
        /// <param name="labelName">Name of the label to find description for.</param>
        /// <returns>Result of sd label -o {0}, where {0} is <paramref name="labelName"/>.</returns>
        public SourceDepotCommandResult LabelDescription(string labelName)
        {
            var commandArguments = new List<string>();
            commandArguments.Add("-o");
            commandArguments.Add(labelName);
            // Sync to get the latest files from core depot
            var command = new SourceDepotCommand { Name = "label", Arguments = commandArguments };
            var result = base.Execute(command, false);
            return new SourceDepotCommandResultLabel(result);
        }

    }
}
