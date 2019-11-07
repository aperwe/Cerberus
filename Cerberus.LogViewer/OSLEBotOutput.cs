using System.Linq;
using System.Collections;
using System.Data;
using System;
using System.Collections.Generic;

namespace Microsoft.Localization.LocSolutions.Cerberus.LogViewer
{
    public partial class OSLEBotOutputDataSet : DataSet
    {
        /// <summary>
        /// Gets a flat report from the data source. The returned report can be displayed in a data grid.
        /// </summary>
        public IList<ReportItem> GetReport()
        {
            var items = from CO in co
                        join PROP in props on CO.co_Id equals PROP.co_Id
                        join RULE in rules on CO.co_Id equals RULE.co_Id
                        let PROPERTIES = GetProperties(PROP)
                        join CHECKS in rule on RULE.rules_Id equals CHECKS.rules_Id
                        join ITEM in item on CHECKS.rule_Id equals ITEM.rule_Id
                        where ITEM.result == "True"
                        select new ReportItem
                        {
                            FileName = PROPERTIES.Single(p => p.name == "LcxFileName").value,
                            ResourceID = PROPERTIES.Single(p => p.name == "LSResID").value,
                            CheckName = CHECKS.name,
                            Language = PROPERTIES.Single(p => p.name == "TargetCulture").value,
                            Project = PROPERTIES.Single(p => p.name == "Project").value,
                            Locgroup = PROPERTIES.Single(p => p.name == "Locgroup").value,
                            Comments = PROPERTIES.Single(p => p.name == "Comments").value,
                            SourceString = PROPERTIES.Single(p => p.name == "SourceString").value,
                            TargetString = PROPERTIES.Single(p => p.name == "TargetString").value,
                            Message = ITEM.message,
                            Severity = ITEM.severity
                        };

            return items.ToList();
        }

        /// <summary>
        /// Gets an array of properties belonging to the specified.
        /// </summary>
        /// <param name="PROP">Row that contains the list of properties of a CO</param>
        /// <returns>Value an array of properties. Access them by name property.</returns>
        private propertyRow[] GetProperties(propsRow PROP)
        {
            var x = from y in property
                    where y.props_Id == PROP.props_Id
                    select y;
            return x.ToArray();
        }

        private ruleRow[] GetChecks(rulesRow RULE)
        {
            var x = from y in rule
                    where y.rules_Id == RULE.rules_Id
                    select y;
            return x.ToArray();
        }
    }
}