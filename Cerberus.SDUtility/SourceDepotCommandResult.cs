using System;
using System.Collections.Generic;
using System.Text;
using SourceDepotClient;

namespace Microsoft.OffGlobe.SourceDepot
{
    public enum SourceDepotCommandResultType
    {
        Infos,
        Warnings
    }

    public class SourceDepotCommandResult
    {
        public SourceDepotCommandResultType ResultType { get; private set; }

        public SDCommandOutputs Outputs { get; private set; }

        public SourceDepotCommandResult(SDResults sdResults)
        {
            if (sdResults.WarningOutput.Count > 0)
            {
                this.ResultType = SourceDepotCommandResultType.Warnings;
                this.Outputs = sdResults.WarningOutput;
            }
            if (sdResults.InfoOutput.Count > 0)
            {
                this.ResultType = SourceDepotCommandResultType.Infos;
                this.Outputs = sdResults.InfoOutput;
            }

            if (sdResults.StructuredOutput.Count > 0)
            {
                this.ResultType = SourceDepotCommandResultType.Infos;
                this.Outputs = sdResults.StructuredOutput;
            }
        }

        public virtual IList<string> GetMessages()
        { 
            var outputList = new List<string>();
// Check for existence of this.Outputs
            if (this.Outputs != null)
            {
                foreach (SDCommandOutput item in this.Outputs)
                {
                    outputList.Add(item.Message.ToLower());
                }
            }
            return outputList;   
        }
    }

    public class SourceDepotCommandResultFiles : SourceDepotCommandResult
    {
        public SourceDepotCommandResultFiles(SDResults sdResults) : base(sdResults) { }

        public override IList<string> GetMessages()
        {
            var outputList = new List<string>();
            foreach (SDCommandOutput item in this.Outputs)
            {
                for (int i = 0; i < item.Variables.Count; i++)
                {
                    if (item.Variables[i].Name == "depotFile")
                    {
                        outputList.Add(item.Variables[i].Value.ToLower());
                    }
                }
            }
            return outputList;   
        }
    }

    public class SourceDepotCommandResultDescribe : SourceDepotCommandResult
    {
        public SourceDepotCommandResultDescribe(SDResults sdResults) : base(sdResults) { }

        public override IList<string> GetMessages()
        {
            var outputList = new List<string>();
            var dicOutputs = new Dictionary<string, string>();
            var item = this.Outputs[0];
            for (int i = 0; i < item.Variables.Count; i++)
            {
                dicOutputs.Add(item.Variables[i].Name, item.Variables[i].Value);
                //Console.WriteLine(string.Format("{0}:{1}", item.Variables[i].Name, item.Variables[i].Value));
            }
            for (int i = 0; i < dicOutputs.Count; i++)
            {
                if (dicOutputs.ContainsKey("depotFile" + i))
                {
                    string message = string.Format("{0}:{1}", dicOutputs["depotFile" + i].ToLower(), dicOutputs["action" + i].ToLower());
                    outputList.Add(message);
                }
            }
            return outputList;
        }
    }

    /// <summary>
    /// Command results from sd label -o {labelName} call.
    /// </summary>
    public class SourceDepotCommandResultLabel : SourceDepotCommandResult
    {
        public SourceDepotCommandResultLabel(SDResults sdResults) : base(sdResults) { }

        /// <summary>
        /// Gets an array of lines of output from the call.
        /// </summary>
        public override IList<string> GetMessages()
        {
            var outputList = new List<string>(); //Debug info.
            var dicOutputs = new Dictionary<string, string>();
            var item = this.Outputs[0];
            for (int i = 0; i < item.Variables.Count; i++)
            {
                dicOutputs.Add(item.Variables[i].Name, item.Variables[i].Value);
                //Console.WriteLine(string.Format("{0}:{1}", item.Variables[i].Name, item.Variables[i].Value));
            }
            var x = item.Message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            outputList.AddRange(x);
            return outputList;
        }
    }
}
