using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Cerberus.Checks
{
    /// <summary>
    /// <list type="ol">
    /// <listheader>This check implements logic related to KeyTip characters..</listheader>
    /// Description:
    /// Several Keytip Disambiguation strings need to be validated in the following way: each of these strings must only consist of characters that belong to the
    /// target language alphabet. Preferrably they should also be the characters available natively (without the use of key modifiers) on the target language keyboard layout 
    /// 
    /// QUESTION:
    /// Should we accept any character accessible from keyboard, including punctuation, etc., or only letter and digits?
    /// Output message: 
    /// "This is a Keytip Disambiguation string and it must contain characters available in the default target keyboard layout. The following characters must not be used: "
    /// </list>
    /// </summary>
    public class KeytipCharactersCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public KeytipCharactersCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            mc = new StringAppendMessageCreator();
        }

        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly StringAppendMessageCreator mc;

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr =>
                IsKeytipDisambiguation(lr)
                &&
                mc.SetContext(GetIllegalChars(lr)).Any()
                , mc.SetInit("This is a Keytip Disambiguation string and it must contain characters available in the default target keyboard layout. The following characters must not be used: "));

        }
        /// <summary>
        /// Check if the resource is one of the Keytip disambiguation resources we want to check.
        /// </summary>
        /// <param name="lr"></param>
        /// <returns></returns>
        static private bool IsKeytipDisambiguation(LocResource lr)
        {
            var stringId = lr.LSResID.ItemID.StringId;
            return 
                stringId.Equals("msoidsChunkCharacter", StringComparison.OrdinalIgnoreCase) 
                ||
                stringId.Equals("msoidsDisambiguationCharacters", StringComparison.OrdinalIgnoreCase) 
                ||
                stringId.Equals("msoidsQATCharacters", StringComparison.OrdinalIgnoreCase) 
                ||
                stringId.Equals("msoidsSpecialCharacterForLowerRibbon", StringComparison.OrdinalIgnoreCase) 
                ||
                stringId.Equals("msoidsSpecialCharacterForUpperRibbon", StringComparison.OrdinalIgnoreCase);
        }
       
        /// <summary>
        /// Checks for characters that cannot be accessed from default target input locale identifier (keyboard layout) without the use
        /// of any modifiers.
        /// </summary>
        /// <returns>An collection of characters that cannot be accessed from the default keyboard layout.</returns>
        private IEnumerable<String> GetIllegalChars(LocResource lr)
        {
            // target string to be tested
            string tgtStr = lr.TargetString;
            if (String.IsNullOrEmpty(tgtStr))
            {
                return new string[0];
            }
            // target culture to be tested against
            var targetLanguageLCID = lr.TargetCulture.Value.LCID;

            IntPtr inputLocaleIdentifier;
            // check if the input locale identifier (keyboard layout) has been loaded for the target LCID and load it if it hasn't been.
            if (!lcidToInputLocaleIdentifier.TryGetValue(targetLanguageLCID, out inputLocaleIdentifier))
            {
                // create hex string for default input for the given LCID in form 0000<hex LCID>
                var hexStringForDefault = String.Format("0000{0}", targetLanguageLCID.ToString("X4"));
                inputLocaleIdentifier = LoadKeyboardLayout(hexStringForDefault, 0);
                if (inputLocaleIdentifier == null)
                {
                    throw new InvalidOperationException(String.Format("Could not obtain default input locale identifier for LCID ", targetLanguageLCID));
                }
                else
                {
                    lcidToInputLocaleIdentifier.Add(targetLanguageLCID, inputLocaleIdentifier);
                }
            }
            // check that each character in string is indeed accessible for the input locale identifier
            return ((IEnumerable<char>)tgtStr).Where(c => !IsCharacterAccessibleFromTargetKeyboard(c, inputLocaleIdentifier)).Select(c => c.ToString());
        }
        /// <summary>
        /// Given a character and an LCID of the target language, checks if this character is accessible from the given input locale identifier (keyboard layout).
        /// </summary>
        /// <param name="inputChar">Character to be checked.</param>
        /// <param name="inputLocaleIdentifier">Input locale identifier as accepted by Windows LoadKeyboardLayout API.</param>
        /// <returns>True if the character is accessible from the default target keyboard layout without the use of any modifier keys.</returns>
        private static bool IsCharacterAccessibleFromTargetKeyboard(char inputChar, IntPtr inputLocaleIdentifier)
        {
            // call Windows API to see if the character is available from the keyboard. 
            // if 0 or -1 is returned, character is not available
            // we are converting the key to lower first, otherwise the high byte would stay at 1 to indicate Shift.
            var virtualKeyCode = VkKeyScanEx(Char.ToLower(inputChar), inputLocaleIdentifier);
            if (virtualKeyCode == 0 || virtualKeyCode == -1)
            {
                return false;
            }
            
            var high = (byte)(virtualKeyCode >> 8);

            // we only consider characters with no modifier used as accessible
            return high == 0;
        }
        /// <summary>
        /// Caches input locale identifiers (keyboard layouts) loaded using LoadKeyboardLayout API.
        /// </summary>
        private Dictionary<int, IntPtr> lcidToInputLocaleIdentifier = new Dictionary<int, IntPtr>();


        #region Windows APIs used for looking up keyboard layout
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadKeyboardLayout(
                string pwszKLID,  // input locale identifier
                uint Flags         // input locale identifier options
            );
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScanEx(
                char ch,
                IntPtr dwhkl
            );
        #endregion
    }

}