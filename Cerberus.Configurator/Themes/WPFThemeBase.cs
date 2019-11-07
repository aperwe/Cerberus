using System.Windows.Controls;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator.Themes
{
    /// <summary>
    /// Class that provides 'theming' of UI controls of the configurator.
    /// Implementations of this will provide the UI with customized look.
    /// </summary>
    public abstract class WPFThemeBase
    {
        #region Abstract interfaces
        /// <summary>
        /// Creates a button that matches the theme's parameters.
        /// </summary>
        public abstract Button CreateButton();

        /// <summary>
        /// Creates a text block that matches theme's parameters.
        /// </summary>
        public abstract TextBlock CreateTextBlock();

        /// <summary>
        /// Creates a default grid that matches theme's parameters.
        /// </summary>
        public abstract Grid CreateGrid();

        /// <summary>
        /// Creates a radio button that matches the theme's parameters.
        /// </summary>
        public abstract RadioButton CreateRadioButton();

        /// <summary>
        /// Creates a group box that matches the theme's parameters.
        /// </summary>
        public abstract GroupBox CreateGroupBox();
        #endregion

        /// <summary>
        /// Creates a button that matches the theme's parameters and has the specified content.
        /// </summary>
        public Button CreateButton(object content)
        {
            var button = CreateButton();
            button.Content = content;
            return button;
        }

        /// <summary>
        /// Creates a radio button that matches the theme's parameters and has the specified content.
        /// </summary>
        public RadioButton CreateRadioButton(object content)
        {
            var button = CreateRadioButton();
            button.Content = content;
            return button;
        }

        /// <summary>
        /// Creates a group box that matches the theme's parameters and has the specified header.
        /// </summary>
        public GroupBox CreateGroupBox(object header)
        {
            var groupBox = CreateGroupBox();
            groupBox.Header = header;
            return groupBox;
        }

        /// <summary>
        /// Creates a text block that matches theme's parameters.
        /// </summary>
        /// <param name="text">Text that will be displayed inside the text block.</param>
        public TextBlock CreateTextBlock(string text)
        {
            var textBlock = CreateTextBlock();
            textBlock.Text = text;
            return textBlock;
        }
    }
}
