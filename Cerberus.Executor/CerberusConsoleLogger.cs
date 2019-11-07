using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Logger.Console;
using Microsoft.Localization.LocSolutions.Logger;

namespace Microsoft.Localization.LocSolutions.Cerberus.Executor
{
    /// <summary>
    /// Custom implementation of <see cref="ConsoleLogger"/> but with filtering of uninteresting debug output coming from OSLEBot engine.
    /// </summary>
    public class CerberusConsoleLogger : ILogger
    {
        /// <summary>
        /// Writes the text entry to console.
        /// Filters uninteresting debug output coming from OSLEBot engine.
        /// </summary>
        /// <param name="logLevel">Severity of the incoming message.</param>
        /// <param name="entry">Log entry to process.</param>
        [CLSCompliant(false)]
        public void AcceptLogEntry(LogLevel logLevel, string entry)
        {
            if (!FilterEntry(entry)) WriteOrReformat(entry);
        }

        /// <summary>d
        /// Writes the entry directly to console, or - if it matches a specific regex - reformat it for more user-friendly look.
        /// </summary>
        /// <param name="entry">Filtered entry that we're interested in, but possibly it needs to be reformatted for more user-friendly look.</param>
        private void WriteOrReformat(string entry)
        {
            foreach (var replacer in osleBotOutputReplacers)
            {
                if (replacer.Pattern.IsMatch(entry))
                {
                    var reformattedEntry = replacer.Pattern.Replace(entry, replacer.StringEvaluator);
                    Console.WriteLine(reformattedEntry);
                    return;
                }
            }
            Console.WriteLine(entry);
        }

        /// <summary>
        /// Determines whether the entry is a debug output from OSLEBot engine and should be filtered out.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>True if the entry should not be shown to console as it is deemed 'uninteresting' to the user.</returns>
        private bool FilterEntry(string entry)
        {
            return osleBotFilters.Any(filter => filter.IsMatch(entry));
        }

        /// <summary>
        /// Filters that modify output lines matching the specified patterns to make them easier to read by humans.
        /// </summary>
        private readonly List<ConsoleStringReplacer> osleBotOutputReplacers;
        /// <summary>
        /// Filters that suppress lines of output with specified patterns.
        /// </summary>
        private readonly List<Regex> osleBotFilters;
        /// <summary>
        /// Default regex options for speed.
        /// </summary>
        private const RegexOptions DefaultOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture;
        /// <summary>
        /// Default constructor.
        /// Initializes list of filters.
        /// </summary>
        public CerberusConsoleLogger()
        {
            osleBotFilters = new List<Regex>
                                  {
                                      new Regex(@"Number of rules still running:", DefaultOptions),
                                      new Regex(@"Firing EngineCleanup event", DefaultOptions),
                                      new Regex(@"Stopping Asimo disk logger", DefaultOptions),
                                      new Regex(@"Loading and queueing objects finished", DefaultOptions),
                                      new Regex(@"Loading and queueing objects started", DefaultOptions),
                                      new Regex(@"Processing Classification Objects started. Number of objects:", DefaultOptions),
                                      new Regex(@"Compilation successful. Returning the assembly", DefaultOptions),
                                      new Regex(@"Creating output started", DefaultOptions),
                                      new Regex(@"Creating output finished", DefaultOptions),
                                      new Regex(@"finished. Time:", DefaultOptions),
                                      new Regex(@"\*\*\* Initializing Assembly", DefaultOptions),
                                      new Regex(@"\*\*\* Resolved assembly reference", DefaultOptions),
                                      new Regex(@"does not implement EngineCleanupHandler method", DefaultOptions),
                                      new Regex(@"Using alternate file name to generate the binary", DefaultOptions),
                                      new Regex(@"Access to the path .+ is denied", DefaultOptions),
                                      new Regex(@"Disabling rules based on config", DefaultOptions),
                                      new Regex(@"Starting engine on ", DefaultOptions),
                                      new Regex(@"Creating PropertyAdapter for ClassificationObject", DefaultOptions),
                                      new Regex(@"Engine configuration:", DefaultOptions),
                                      new Regex(@"Creating DataSourceProvider for DataSource type", DefaultOptions),
                                      new Regex(@"Loading rules from containers", DefaultOptions),
                                      new Regex(@"Found [\d]+ data source package", DefaultOptions),
                                      new Regex(@"Initializing Data Source Provider", DefaultOptions),
                                      new Regex(@"Creating DataAdapter for ClassificationObject type", DefaultOptions),
                                      new Regex(@"Adding rule to RuleManager: ", DefaultOptions),
                                      new Regex(@"No checks are enabled for \[", DefaultOptions)
                                  };

            osleBotOutputReplacers = new List<ConsoleStringReplacer>
                                          {
                                              new ConsoleStringReplacer
                                                  {
                                                      Pattern = new Regex(@"^.+Engine is processing package: Microsoft.Localization.LocDocument: (?<lclFile>[^;]+); Microsoft.Localization.OSLEBot.Core.Misc.ConfigDictionary.+$", DefaultOptions),
                                                      StringEvaluator = match => string.Format("Parsing: '{0}'", match.Groups["lclFile"].Value)
                                                  },
                                              new ConsoleStringReplacer
                                                  {
                                                      Pattern = new Regex(@"^\[Executor\] (?<meaningfulText>.*)$", DefaultOptions),
                                                      StringEvaluator = match => match.Groups["meaningfulText"].Value
                                                  },
                                          };
        }
    }

    /// <summary>
    /// A class that associates a regex with a match evaluator to provide a generic mechanism tu substitute strings in output logged by OSLEBot engine.
    /// </summary>
    public class ConsoleStringReplacer
    {
        /// <summary>
        /// Expression that replaces a match identified by <see cref="Pattern"/> with a user-friendly string.
        /// </summary>
        public MatchEvaluator StringEvaluator { get; set; }
        /// <summary>
        /// Pattern that identifies specific kinds of OSLEBot output to be replaced with more user-friendly output.
        /// </summary>
        public Regex Pattern { get; set; }
    }
}