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
    /// Base class for WPF views that provide various perspectives on data to the user.
    /// </summary>
    internal abstract class ViewHandler
    {
        #region Private members


        /// <summary>
        /// This is the element read from resources as indicated by ResourceName property.
        /// </summary>
        private UIElement implementorView;

        private readonly Grid viewHandlerPrivateView;
        /// <summary>
        /// Configuration data that governs this view.
        /// </summary>
        private readonly CheckConfiguration configurationReference;

        /// <summary>
        /// This is the container passed in the constructor. In it, we still create a separate container for the inherited view.
        /// The inherited view accesses its own container via <see cref="ViewContainer"/> property.
        /// </summary>
        private readonly Grid realParentContainer;
        /// <summary>
        /// This is the view that implementors can access through <see cref="ViewContainer"/>.
        /// May or may not be the same as the real container passed from the client code.
        /// </summary>
        private readonly Grid viewContainerForImplementors;
        #endregion

        #region Public members
        /// <summary>
        /// This is the name of the XAML resource to load for this view.
        /// By default it shows 'notImplementedUI', unless the implementor overrides it
        /// and provides a working UI resource name.
        /// <para/>It has to derive from UIElement.
        /// </summary>
        virtual public string ResourceName { get { return "notImplementedUI"; } }


        /// <summary>
        /// UI element that contains this view.
        /// </summary>
        public Grid ViewContainer
        {
            get { return viewContainerForImplementors; }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Database that feeds this view.
        /// </summary>
        public CheckConfiguration Configuration
        {
            get { return configurationReference; }
        }


        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// <para/>The view will be visible immediately.
        /// <para/>This internal method is called from the publicly available Show() method.
        /// <para/>Client code (container that uses this view), should not call this method directly, but it should call <see cref="Show"/> instead.
        /// </summary>
        internal abstract void Reveal();

        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="Configuration"/>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para/>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public abstract void UpdateViewToMatchData();
        /// <summary>
        /// This is a view description provided by the view implementation to the container (calling code).
        /// The calling code places the view description in an appropriate location, so that the user can read it.
        /// </summary>
        virtual public string ViewDescription { get { return string.Empty; } }
        #endregion

        protected ViewHandler(Grid viewContainer, CheckConfiguration configuration)
        {
            realParentContainer = viewContainer;

            viewHandlerPrivateView = realParentContainer.FindResource("ViewHandlerPrivateView") as Grid;
            viewHandlerPrivateView.DisconnectLogicalParent();
            realParentContainer.Children.Add(viewHandlerPrivateView);
            DynamicResourceExtension dre = new DynamicResourceExtension("ViewHandlerPrivateView");
            viewContainerForImplementors = GetViewElement<Grid>(viewHandlerPrivateView, "ViewHandlerViewForImplementors");
            configurationReference = configuration;
        }

        /// <summary>
        /// Called by the container (calling code) in order to show the view to the user.
        /// The base implementation of this method contains code to initialize common elements of each view
        /// and then it calls view-specific control construction.
        /// </summary>
        public virtual void Show()
        {
            #region Common elements of the view
            if (!string.IsNullOrEmpty(ViewDescription))
            {
                var infoPanel = GetViewElement<Grid>(viewHandlerPrivateView, "InfoPanel");

                //var infoIcon = infoPanel.Children.AddAndReference(ResourceHelper.GetImage(IconType.Info, 32, 32));

                var viewDescriptionPlaceholder = infoPanel.Children.AddAndReference(UITheme.CreateTextBlock());
                viewDescriptionPlaceholder.Text = ViewDescription;
                Grid.SetColumn(viewDescriptionPlaceholder, 1);

                //If the description appears above, the implementor's view should occupy row number 1 (in 0-based index numbering)
                Grid.SetRow(ViewContainer, 1);
            }
            #endregion

            //Load the resource definition provided by the implementor.
            //If the implementor has not overridden the ResourceName, property, it will load
            //the generic 'Not implemented' UI.
            LoadResource(ResourceName);

            //View-specific layout.
            Reveal();
        }

        /// <summary>
        /// Loads the specified resource key. It has to derive from UIElement.
        /// </summary>
        /// <param name="resourceName">Name of XAML resource to load into the view. Provided by implementers.</param>
        private void LoadResource(string resourceName)
        {
            implementorView = ViewContainer.FindResource(resourceName) as UIElement;
            implementorView.DisconnectLogicalParent();
            ViewContainer.Children.Add(implementorView);
        }

        /// <summary>
        /// Info icon displayed in the info panel on top of the implementer's view.
        /// </summary>
        static Image InfoIcon { get { return ResourceHelper.GetImage(IconType.Info, 32, 32); } }

        /// <summary>
        /// Attempts to return the element of the view loaded from XAML resources
        /// by the element's name.
        /// </summary>
        /// <typeparam name="T">Type to convert the element to.</typeparam>
        /// <param name="elementName">Name of the element. To be able to access elements of the view, you have to name them first.</param>
        /// <returns>The element or null.</returns>
        protected T GetViewElement<T>(string elementName) where T : DependencyObject
        {
            return GetViewElement<T>(implementorView, elementName);
        }

        /// <summary>
        /// Attempts to return the child element of the specified parent by the element's name.
        /// </summary>
        /// <typeparam name="T">Type to convert the element to.</typeparam>
        /// <param name="elementName">Name of the element. To be able to access elements of the view, you have to name them first.</param>
        /// <param name="parent">Parent UI element from which the search should start.</param>
        /// <returns>The element or null.</returns>
        private T GetViewElement<T>(DependencyObject parent, string elementName) where T : DependencyObject
        {
            return LogicalTreeHelper.FindLogicalNode(parent, elementName) as T;
        }

        /// <summary>
        /// If the tags collection contains more than 0 elements, appends a 'tag' icon to the stack panel
        /// and decorates the icon with tooltip: "Tags: {0}, {1}...", which shows all the tags in collection.
        /// </summary>
        /// <param name="panel">Panel to receive the optional tag icon.</param>
        /// <param name="tags">Collection of tag values that determine the content of the tooltip. If this collection has 0 elements, the tag icon is not added.</param>
        protected void AddOptionalTagIconWithTooltip(Panel panel, IEnumerable<string> tags)
        {
            if (tags.Count() > 0)
            {
                var tagImage = panel.Children.AddAndReference(ResourceHelper.GetImage(IconType.Tag, 16, 16));
                tagImage.ToolTip = string.Format("Tags: {0}.", string.Join(", ", tags.ToArray()));

                var removeTagImage = panel.Children.AddAndReference(ResourceHelper.GetImage(IconType.TagRemove, 16, 16));
                removeTagImage.ToolTip = "Click here to remove tags.";
                removeTagImage.Tag = "RemoveTag";
                var tagRemoveMenu = new ContextMenu();
                foreach (var tag in tags)
                {
                    var tagRemoverItem = new MenuItem {Header = string.Format("Remove '{0}' tag", tag), Tag = tag};
                    tagRemoveMenu.Items.Add(tagRemoverItem);
                }
                removeTagImage.ContextMenu = tagRemoveMenu;
            }
        }

        /// <summary>
        /// Returns a reference to the current UI theme that is used to draw UI elements.
        /// </summary>
        protected WPFThemeBase UITheme
        {
            get { return ConfiguratorApplication.Current.UITheme; }
        }

        /// <summary>
        /// Shows a generic 'not implemented' message box.
        /// </summary>
        protected static void ShowNotImplementedUI()
        {
            MessageBox.Show(
                //Properties.Resources.FeatureNotImplementedString,
                "This feature is not yet implemented.",
                //Properties.Resources.MessageBoxHeader,
                "Cerberus",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
    public static class WpfExtensions
    {
        public static T ReuseResource<T>(this FrameworkElement me, object resourceKey) where T : class
        {
            var retVal = me.FindResource(resourceKey);
            return retVal as T;
        }
        /// <summary>
        /// For resources loaded from XAML, call this before attaching them to another parent control.
        /// This is because XAML loader caches the resources and returns the only instance of them.
        /// </summary>
        public static void DisconnectLogicalParent(this UIElement me)
        {
            if (me == null) return;

            var parent = LogicalTreeHelper.GetParent(me);
            if (parent == null) return;

            #region If parent derives from Panel
            var panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Remove(me);
            }
            #endregion

        }
    }
}