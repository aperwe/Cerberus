using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Themes;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;
using System.Windows;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Handler that shows a view that illustrates a non-implemented view.
    /// </summary>
    internal class NotImplementedViewHandler : ViewHandler
    {
        internal NotImplementedViewHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
        {

        }

        /// <summary>
        /// This is a view description provided by the view implementation to the container (calling code).
        /// The calling code places the view description in an appropriate location, so that the user can read it.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                return "The selected view is not implemented yet. Please report it to arturp@microsoft.com.";
            }
        }
        /// <summary>
        /// Called by the container (calling code) in order to show the view to the user.
        /// The base implementation of this method contains code to initialize common elements of each view
        /// and then it calls view-specific control construction.
        /// </summary>
        public override void Show()
        {
            base.Show();
        }
        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// <para></para>The view will be visible immediately.
        /// <para></para>This internal method is called from the publicly available Show() method.
        /// <para></para>Client code (container that uses this view), should not call this method directly, but it should call <see cref="Show"></see> instead.
        /// </summary>
        internal override void Reveal()
        {
            this.ViewContainer.Children.Add(ResourceHelper.GetImage(IconType.ConstructionCone, 128, 128));
        }
        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="ViewHandler.Configuration"></see>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para></para>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public override void UpdateViewToMatchData()
        {
            throw new NotImplementedException();
        }
    }
}
