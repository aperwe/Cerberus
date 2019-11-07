using System;
using System.IO;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Resources
{
    /// <summary>
    /// Summary description for Tests for functionality of Cerberus.Resources project.
    /// </summary>
    [TestClass]
    public class ResourcesTests : CerberusTestClassBase
    {
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), Description("Tests that access to any icon through ResourceHelper is successful"), Owner("arturp"), TestMethod]
        public void TestAccessToAllIcons()
        {
            const double iconWidth = 24;
            const double iconHeight = 24;
            foreach (IconType iconType in Enum.GetValues(typeof(IconType)))
            {
                try
                {
                    TestContext.WriteLine("Testing access to '{0}' icon.", iconType);
                    var image = ResourceHelper.GetImage(iconType, iconWidth, iconHeight);
                    Assert.IsNotNull(image);
                    Assert.AreEqual(iconWidth, image.Width);
                    Assert.AreEqual(iconHeight, image.Height);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Assert.Fail("Failed to get icon of type: {0}", iconType);
                }
                catch (IOException)
                {
                    Assert.Fail("Mapping of icon file for icon type: {0} is invalid.", iconType);
                }
            }
            TestContext.WriteLine("All icons retrieved successfully.");
        }
    }
}