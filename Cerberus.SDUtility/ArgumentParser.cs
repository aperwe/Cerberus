using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Indicates what run mode the user has specified on command line.
    /// </summary>
    public enum ProgramRunMode
    {
        /// <summary>
        /// Default state. The program cannot continue in this state.
        /// </summary>
        Undetermined,
        /// <summary>
        /// The user requested that usage help be displayed and the program should quit.
        /// </summary>
        ShowHelpAndQuit,
        /// <summary>
        /// The user specified that store should be synced.
        /// </summary>
        SyncStore,
        /// <summary>
        /// The user specified that core should be synced.
        /// </summary>
        SyncCore
    }

    /// <summary>
    /// Class that parses arguments from command line.
    /// </summary>
    public class ArgumentParser
    {
        private readonly List<string> _languages;

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
        /// This is valid when -store mode is specified.
        /// It is the checkpoint to which files in store should be synced.
        /// For -core this value is ignored.
        /// </summary>
        public string CheckpointNumber { get; set; }

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
                            case "-store":
                                retVal.RunMode = ProgramRunMode.SyncStore;
                                machineState = MachineState.CheckpointNumberExpected; //After -store we expect checkpoint number
                                break;
                            case "-core":
                                retVal.RunMode = ProgramRunMode.SyncCore;
                                machineState = MachineState.LanguageExpected;
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
                    case MachineState.LanguageExpected: //ASSUMPTION: The parameter cannot be numeric
                        int possibleNumericParam;
                        if (int.TryParse(arg, out possibleNumericParam))
                        {
                            retVal.Correct = false;
                            retVal.InvalidArgument = arg;
                            machineState = MachineState.InvalidArguments;
                            break;
                        }
                        retVal._languages.Add(arg);
                        machineState = MachineState.MoreLanguages;
                        break;
                    case MachineState.MoreLanguages:
                        retVal._languages.Add(arg);
                        break;
                    case MachineState.CheckpointNumberExpected:
                        int checkpointNumber;
                        if (!int.TryParse(arg, out checkpointNumber))
                        {
                            retVal.Correct = false;
                            retVal.InvalidArgument = arg;
                            machineState = MachineState.InvalidArguments;
                            break;
                        }
                        retVal.CheckpointNumber = arg;
                        machineState = MachineState.LanguageExpected;
                        break;
                    case MachineState.InvalidArguments: //In invalid state, just fall through to skip all arguments.
                        break;
                    case MachineState.ShowHelpAndQuit: //In this state, just fall through to skip all remaining arguments.
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("machineState", "Unexpected machine state of argument parser.");
                }
            }
            if (retVal.RunMode == ProgramRunMode.Undetermined)
            {
                retVal.Correct = false;
            }
            if (retVal.Languages.Count() == 0)
            {
                retVal.Correct = false;
            }
            return retVal;
        }

        /// <summary>
        /// Private constructor to disalow creation outside of <see cref="Parse"/> method.
        /// </summary>
        private ArgumentParser()
        {
            _languages = new List<string>();
        }

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
        ShowHelpAndQuit,
        /// <summary>
        /// The user has specified either store or core sync, and the following parameter should be language.
        /// </summary>
        LanguageExpected,
        /// <summary>
        /// If there are more languages specified, we expect them as the last arguments on the command line.
        /// </summary>
        MoreLanguages,
        /// <summary>
        /// When -store argument is used, we expect checkpoint to be specified as second argument. Then, at least one language must follow.
        /// </summary>
        CheckpointNumberExpected
    }
}