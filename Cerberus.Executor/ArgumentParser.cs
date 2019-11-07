using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Localization.LocSolutions.Cerberus.Executor
{
    /// <summary>
    /// Class that parses arguments from command line.
    /// </summary>
    public class ArgumentParser
    {
        private readonly List<string> _languages;
        private readonly List<string> _projects;

        /// <summary>
        /// Parses a command line for Cerberus executor and organizes input from command line into convenient data structures.
        /// </summary>
        /// <param name="args">Arguments from command line.</param>
        /// <returns>Parsed data structure with optional <see cref="Correct"/> set to false if there was a problem with one or more arguments</returns>
        public static ArgumentParser Parse(string[] args)
        {
            var retVal = new ArgumentParser {Correct = true, InvalidArgument = string.Empty};
            var machineState = MachineState.EntryState;

            foreach (var arg in args)
            {
                var lowercaseArg = arg.ToLowerInvariant();

                switch (machineState)
                {
                    case MachineState.EntryState:
                        switch (lowercaseArg)
                        {
                            case "-l":
                                machineState = MachineState.Language;
                                break;
                            case "-p":
                                machineState = MachineState.Project;
                                break;
                            case "-m":
                                machineState = MachineState.RunMode;
                                break;
                            case "-h":
                            case "/h":
                            case "?":
                            case "/?":
                                retVal.RunMode = ProgramRunMode.ShowHelpAndQuit;
                                machineState = MachineState.ShowHelpAndQuit;
                                break;
                            default:
                                retVal.Correct = false;
                                retVal.InvalidArgument = arg;
                                machineState = MachineState.InvalidArguments;
                                break;
                        }
                        break;
                    case MachineState.Language:
                        retVal._languages.Add(arg);
                        machineState = MachineState.EntryState;
                        break;
                    case MachineState.Project:
                        retVal._projects.Add(arg);
                        machineState = MachineState.EntryState;
                        break;
                    case MachineState.RunMode:
                        switch (lowercaseArg)
                        {
                            case "locgroups":
                                retVal.RunMode = ProgramRunMode.StandaloneLocGroups;
                                machineState = MachineState.EntryState;
                                break;
                            case "projects":
                                retVal.RunMode = ProgramRunMode.StandaloneProjects;
                                machineState = MachineState.EntryState;
                                break;
                            case "centralized":
                                retVal.RunMode = ProgramRunMode.DatabaseCentralized;
                                machineState = MachineState.EntryState;
                                break;
                            default:
                                retVal.Correct = false;
                                retVal.InvalidArgument = arg;
                                machineState = MachineState.InvalidArguments;
                                break;
                        }
                        break;
                    case MachineState.InvalidArguments: //In invalid state, just fall through to skip all arguments.
                        break;
                    case MachineState.ShowHelpAndQuit: //In this state, just fall through to skip all remaining arguments.
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("machineState", "Unexpected machine state of argument parser.");
                }
            }
            return retVal;
        }

        /// <summary>
        /// Private constructor to disalow creation outside of <see cref="Parse"/> method.
        /// </summary>
        private ArgumentParser()
        {
            _languages = new List<string>();
            _projects = new List<string>();
        }

        /// <summary>
        /// Indicates execution mode selected by the user.
        /// </summary>
        public ProgramRunMode RunMode { get; set; }

        /// <summary>
        /// Indicates whether the command line was parsed successfully.
        /// <para/>Set to false if any argument was unrecognized or unexpected.
        /// </summary>
        public bool Correct { get; private set; }
        /// <summary>
        /// Indicates which argument in the parsed command line was not recognized.
        /// <para/>This field is meaningful only when <see cref="Correct"/> flag is set to false.
        /// </summary>
        public string InvalidArgument { get; private set; }

        /// <summary>
        /// List of languages specified in command line using '-l' arguments
        /// </summary>
        public IEnumerable<string> Languages { get { return _languages; } }

        /// <summary>
        /// True if command line specified that filter by language should be performed.
        /// <para/>If true, <see cref="Languages"/> collection indicates which languages to process.
        /// </summary>
        public bool ShouldFilterByLanguage { get { return Languages.Count() > 0; } }

        /// <summary>
        /// List of projects specified in command line using '-p' arguments
        /// </summary>
        public IEnumerable<string> Projects { get { return _projects; } }

        /// <summary>
        /// True if command line specified that filter by project should be performed.
        /// <para/>If true, <see cref="Projects"/> collection indicates which projects to process.
        /// </summary>
        public bool ShouldFilterByProject { get { return Projects.Count() > 0; } }
    }

    /// <summary>
    /// All possible states of state machine of <see cref="ArgumentParser"/>.
    /// </summary>
    internal enum MachineState
    {
        /// <summary>
        /// Initial state of the state machine.
        /// </summary>
        EntryState,
        /// <summary>
        /// The '-l' argument was specified. The next argument must be a language name.
        /// </summary>
        Language,
        /// <summary>
        /// The '-p' argument was specified. The next argument must be a project name.
        /// </summary>
        Project,
        /// <summary>
        /// Indicates an invalid command line. The machine enters this state whenever it encounteres an unexpected argument.
        /// </summary>
        InvalidArguments,
        /// <summary>
        /// The '-m' argument was specified. The next argument must be a mode selector.
        /// </summary>
        RunMode,
        /// <summary>
        /// Either -h or /h or ? was specified. All other arguments should be ignored. The calling program should show usage info and quit.
        /// </summary>
        ShowHelpAndQuit
    }
}