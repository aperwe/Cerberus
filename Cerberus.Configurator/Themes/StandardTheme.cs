using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator.Themes
{
    /// <summary>
    /// Standard theme of Cerberus.Configurator.
    /// </summary>
    class StandardTheme : WPFThemeBase
    {
        /// <summary>
        /// Creates a button that matches the theme's parameters.
        /// </summary>
        public override Button CreateButton()
        {
            return new Button { Margin = new Thickness(2), Height = 23, Padding = new Thickness(4, 1, 4, 1) };
        }

        /// <summary>
        /// Creates a text block that matches theme's parameters.
        /// </summary>
        public override TextBlock CreateTextBlock()
        {
            return new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 2, 4, 2) };
        }

        /// <summary>
        /// Creates a default grid that matches theme's parameters.
        /// </summary>
        public override Grid CreateGrid()
        {
            return new Grid { Margin = new Thickness(1) };
        }

        /// <summary>
        /// Creates a radio button that matches the theme's parameters.
        /// </summary>
        public override RadioButton CreateRadioButton()
        {
            return new RadioButton { Margin = new Thickness(2, 1, 2, 1), Padding = new Thickness(1, 1, 1, 1) };
        }

        /// <summary>
        /// Creates a group box that matches the theme's parameters.
        /// </summary>
        public override GroupBox CreateGroupBox()
        {
            return new GroupBox { Margin = new Thickness(2, 2, 2, 1), Padding = new Thickness(2, 2, 2, 1) };
        }
    }
}