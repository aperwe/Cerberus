using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// <list type="ol">
    /// <listheader>This check implements logic related to Outspace KeyHints.</listheader>
    /// Description:
    /// <item>Check 1</item>
    /// The character in resource id: "ResT", 0; "msoidsDynamicKeytipCharacter" cannot be one of the list of characters used in string with 
    /// resource id: "ResT", 0; "msoidsValidKeytipCharacters"
    /// <item>Check 2</item>
    /// The Outspace keyhint characters in 122 resources needs to match with an entry in resource id: "ResT", 0; "msoidsValidKeytipCharacters". 
    /// This resource contains a list of valid characters that can be used for the keyhints for each language, so entries in the resource id’s 
    /// listed must match with one of these entries in this resource. For the complete list of resource id’s to check 
    /// see \\eoc-data-02\public\denism\cerebus\keyhints.txt
    /// </list>
    /// </summary>
    public class OutspaceKeyHintCheck : LocResourceRule
    {
        public OutspaceKeyHintCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            
        }

        protected override void Run()
        {
            //Check that resource msoidsDynamicKeytipCharacter does not contain any of the ValidKeyTipCharacters
            Check(lr =>
                lr.FriendlyID.Equals(StringComparison.Ordinal, "msoidsDynamicKeytipCharacter")
                &&
                ValidKeyTipCharacters.Any(c => lr.TargetString.Value.Contains(c)),
                "This resource must not contain any of the characters in resource \"msoidsValidKeytipCharacters\"");

            //each of the keyhint resources must contain only valid keyhint characters defined in msoidsValidKeytipCharacters
            Check(lr =>
                keyHintResourceIDs.Contains(lr.FriendlyID)
                &&
                lr.TargetString.Value.Any(c => !ValidKeyTipCharacters.Contains(c)),
                "This keyhint resource must contain only characters defined in resource \"msoidsValidKeytipCharacters\"");
        }

        /// <summary>
        /// Gets a collection of valid keytip characters as defined in msoidsValidKeytipCharacters
        /// resource.
        /// Throws an exception if the msoidsValidKeytipCharacters cannot be retrieved when rule executes.
        /// This property should only be accessed if we know that this resource is accessible.
        /// </summary>
        private ICollection<Char> ValidKeyTipCharacters
        {
            get
            {
                var lr = (Microsoft.Localization.OSLEBot.ClassificationObjects.LocResource)this.CurrentCO;
                // try to get msoidsValidKeytipCharacters from the same document instance where the current resource resides
                // we are assuming here that all resources checked against msoidsValidKeytipCharacters live in the same document.
                var validKeyTipResource = lr.GetDimensionIntersection(msoidsValidKeytipCharactersID, lr.Document).SingleOrDefault();
                if (Object.ReferenceEquals(validKeyTipResource, null))
                {
                    throw new InvalidOperationException(String.Format(
                        "Could not find resource {0} required to perform the check.", msoidsValidKeytipCharactersID.Value.ToString())
                        );
                }
                return validKeyTipResource.TargetString.Value.ToString().ToCharArray();
            }
        }
        private readonly Microsoft.Localization.OSLEBot.Core.Engine.Properties.StringProperty msoidsValidKeytipCharactersID =
            new Microsoft.Localization.OSLEBot.Core.Engine.Properties.StringProperty("FriendlyID", "msoidsValidKeytipCharacters");

        /// <summary>
        /// A collection of all keyhint resources to be checked against msoidsValidKeytipCharacters
        /// </summary>
        private readonly HashSet<string> keyHintResourceIDs = new HashSet<string>{
            "msoidsOfficeButtonKeyHint",
            "msoidsPlaceDocInfoKeyHint",
            "msoidsSaveKeyHint",
            "msoidsSaveAsKeyHint",
            "msoidsPlacePrintKeyHint",
            "msoidsPlacePrintLegacyKeys",
            "msoidsPlaceSavingKeyHint",
            "msoidsPlaceNewKeyHint",
            "msoidsPlaceOpenKeyHint",
            "msoidsPlaceSendKeyHint",
            "msoidsPlaceSendLegacyKeys",
            "msoidsPlaceApplication",
            "msoidsPlaceSavingLegacyKeys",
            "msoidsCloseKeyHint",
            "msoidsExitKeyHint",
            "msoidsOptionsKeyHint",
            "msoidsCtlDocumentThumbnailKeyHint",
            "msoidsButtonCorruptPropertiesKeyHint",
            "msoidsMenuDocPropertiesKeyHint",
            "msoidsListDocPropertiesKeyHint",
            "msoidsMenuDocDatesKeyHint",
            "msoidsListDocDatesKeyHint",
            "msoidsMenuDocPeopleKeyHint",
            "msoidsListDocPeopleKeyHint",
            "msoidsMenuDocResourcesKeyHint",
            "msoidsListDocResourcesKeyHint",
            "msoidsButtonEditLinksKeyHint",
            "msoidsCtlDocLocationKeyHint",
            "msoidsHeroButtonPermissionsKeyHint",
            "msoidsHeroButtonPrepareForDistributionKeyHint",
            "msoidsHeroButtonViewSignaturesKeyHint",
            "msoidsHeroButtonVersionsKeyHint",
            "msoidsListVersionsKeyHint",
            "msoidsHyperlinkVersionsExpandCollapseKeyHint",
            "msoidsHeroButtonReadOnlyEditDocKeyHint",
            "msoidsHeroButtonReadOnlyReopenDocKeyHint",
            "msoidsHeroButtonReadOnlyEditOfflineKeyHint",
            "msoidsHeroButtonReadOnlyCheckOutKeyHint",
            "msoidsHeroButtonReadOnlyCheckinKeyHint",
            "msoidsHeroButtonDiscardCheckoutKeyHint",
            "msoidsHeroButtonReadOnlySaveAsKeyHint",
            "msoidsHeroButtonDisabledContentKeyHint",
            "msoidsHeroButtonSecureReaderKeyHint",
            "msoidsButtonSecureReaderSendReportKeyHint",
            "msoidsHeroButtonWorkflowKeyHint",
            "msoidsHeroButtonConvertKeyHint",
            "msoidsHeroButtonPolicyKeyHint",
            "msoidsHeroButtonUpdatesAvailableKeyHint",
            "msoidsButtonOpenKeyHint",
            "msoidsButtonOpenStartFromExistingKeyHint",
            "msoidsButtonSavePDFOrXPSKeyHint",
            "msoidsButtonSaveToOfficeLiveKeyHint",
            "msoidsButtonSaveToSharePointWorkspaceKeyHint",
            "msoidsButtonSaveToSharePointKeyHint",
            "msoidsHeroButtonShareKeyHint",
            "msoidsHeroButtonPrintKeyHint",
            "msoidsCtlPrintNumCopiesKeyHint",
            "msoidsButtonSaveAsKeyHint",
            "msoidsSaveAsGalleryOtherKeyHint",
            "msoidsCtlPrintWhichPrinterKeyHint",
            "msoidsCtlPrintPageRangeKeyHint",
            "msoidsCtlPrintSpecificPagesKeyHint",
            "msoidsCtlPrintDuplexKeyHint",
            "msoidsCtlPrintCollationKeyHint",
            "msoidsCtlPrintOrientationKeyHint",
            "msoidsCtlPrintPageSizeKeyHint",
            "msoidsCtlPrintPreviewWindowKeyHint",
            "msoidsCtlPrintMarginsKeyHint",
            "msoidsCtlPrintPreferencesKeyHint",
            "msoidsCtlPrintPageBackKeyHint",
            "msoidsCtlPrintPageForwardKeyHint",
            "msoidsCtlPrintGoToPageKeyHint",
            "msoidsCtlPrintZoomKeyHint",
            "msoidsButtonSendEmailAsAttachmentKeyHint",
            "msoidsButtonSendEmailAsLinkKeyHint",
            "msoidsButtonSendEmailAsPDFKeyHint",
            "msoidsButtonSendEmailAsXPSKeyHint",
            "msoidsButtonSendFaxOverIPKeyHint",
            "msoidsButtonSendShareNowKeyHint",
            "msoidsButtonSendByIMKeyHint",
            "msoidsHeroButtonSendKeyHint",
            "msoidsButtonSendUsingEmailKeyHint",
            "msoidsHeroButtonSendToMailRecipientKeyHint",
            "msoidsHeroButtonSendAsLinkKeyHint",
            "msoidsHeroButtonSendAsPdfKeyHint",
            "msoidsHeroButtonSendAsXpsKeyHint",
            "msoidsHyperlinkSharePointLearnMoreKeyHint",
            "msoidsHeroButtonNoteboardKeyHint",
            "msoidsHyperlinkExpandCollapseNotesKeyHint",
            "msoidsCtlPrintStaplingKeyHint",
            "msoidsCtlPrintPagesPerSheetKeyHint",
            "msoidsCtlPrintFitToWindowKeyHint",
            "msoidsCtlPrintZoomDialogKeyHint",
            "msoidsOutSpaceAddInCommandsKeyHint",
            "msoidsButtonNewPlaceBackKeyHint",
            "msoidsButtonNewPlaceForwardKeyHint",
            "msoidsButtonNewPlaceHomeKeyHint",
            "msoidsTxtNewPlaceSearchKeyHint",
            "msoidsButtonNewPlaceSearchKeyHint",
            "msoidsHeroButtonNewPlaceTemplateActionKeyHint",
            "msoidsLinkNewPlaceTemplateProviderKeyHint",
            "msoidsButtonNewPlaceOpenSampleDataKeyHint",
            "msoidsRadioButtonNewPlaceMetricKeyHint",
            "msoidsRadioButtonNewPlaceUSKeyHint",
            "msoidsTxtNewPlaceFilenameFieldKeyHint",
            "msoidsButtonUpdatesAvailableKeyHint",
            "msoidsHeroButtonResolveKeyHint",
            "msoidsHeroMenuDocCoauthorsActionsKeyHint",
            "msoidsHyperlinkExpandCollapseEditorsKeyHint",
            "msoidsListCoauthorsKeyHint",
            "msoidsButtonNewPlaceBrowseKeyHint",
            "msoidsItemNewPlaceBlankDocumentKeyHint",
            "msoidsItemNewPlaceRecentTemplatesKeyHint",
            "msoidsItemNewPlaceMyTemplatesKeyHint",
            "msoidsItemNewPlaceBlogPostKeyHint",
            "msoidsItemNewPlaceSampleTemplatesKeyHint",
            "msoidsItemNewPlaceNewFromExistingKeyHint",
            "msoidsItemNewPlaceOfficeOnlineTemplatesKeyHint",
            "msoidsItemNewPlaceInstalledThemesKeyHint",
            "msoidsItemNewPlaceBlankWebDatabaseKeyHint",
            "msoidsItemNewPlaceBlankCalligraphyKeyHint",
            "msoidsHyperlinkNewPlaceReportTemplateKeyHint",
        };
    }
}
