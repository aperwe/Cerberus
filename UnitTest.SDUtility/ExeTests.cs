using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core;
using Microsoft.Localization.LocSolutions.Cerberus.SDUtility;
using Microsoft.OffGlobe.SourceDepot;
using SourceDepotClient;

namespace UnitTest.SDUtility
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExeTests : CerberusTestClassBase
    {
        [TestMethod]
        [Description("Makes sure the show usage method works correctly with the format strings.")]
        public void TestShowUsage()
        {
            var context = new SDInterface(new[] { "-store", "7229", "fr-fr" });
            context.ShowUsage();
        }
    }
}
