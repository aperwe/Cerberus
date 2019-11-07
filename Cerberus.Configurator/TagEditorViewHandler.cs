using System;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Class that provides logic of the generic tag editor view.
    /// This view allows to add tags.
    /// </summary>
    internal class TagEditorViewHandler : ViewHandler
    {
        private StackPanel _tagEditorPanel;
        private ComboBox _comboBox;
        private Button _saveButton;
        private Button _applyToSelectionButton;

        /// <summary>
        /// Event that can be wired to, fired when after 'Apply to selection' button is pressed.
        /// <para/>Callers can hook up to this event to perform meaningful action when this button is pressed.
        /// </summary>
        internal Action<string> ApplyTagToSelectionEvent { get; set; }

        /// <summary>
        /// Event that can be wired to, fired when after 'Save' button is pressed and after tag has been saved to configuration database.
        /// <para/>Callers can hook up to this event to refresh their view after new tag has been created in configuration database.
        /// </summary>
        internal Action<string> SaveTagEvent { get; set; }

        /// <summary>
        /// When set before calling <see cref="Reveal()"/>, it will show the 'Apply to selection' button.
        /// </summary>
        internal bool AddApplyToSelectionButton { get; set; }

        public TagEditorViewHandler(Grid container, CheckConfiguration configuration) : base(container, configuration)
        {
            ApplyTagToSelectionEvent = new Action<string>(s => { });
            ApplyTagToSelectionEvent += SaveTag; //When tag is applied, we also want to save it and update the combo box.
            SaveTagEvent = new Action<string>(s => { });
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// The view will be visible immediately.
        /// </summary>
        internal override void Reveal()
        {
            _tagEditorPanel = new StackPanel {Orientation = Orientation.Horizontal};
            ViewContainer.Children.Add(_tagEditorPanel);

            _tagEditorPanel.Children.Add(new Label {Content = "Define new tag:"});
            _comboBox = new ComboBox
                            {
                                ItemsSource = Configuration.GetAllTags(),
                                IsEditable = true,
                                IsTextSearchEnabled = true,
                                MinWidth = 120
                            };
            _comboBox.KeyUp += ComboBoxKeyUp;
            _tagEditorPanel.Children.Add(_comboBox);

            _saveButton = UITheme.CreateButton("Save");
            _saveButton.Click += SaveButtonClick;
            _tagEditorPanel.Children.Add(_saveButton);

            if (AddApplyToSelectionButton)
            {
                _applyToSelectionButton = UITheme.CreateButton("Apply to selection");
                _applyToSelectionButton.Click += ApplyButtonClick;
                _tagEditorPanel.Children.Add(_applyToSelectionButton);
            }
        }

        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="ViewHandler.Configuration"/>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para/>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public override void UpdateViewToMatchData()
        {
            _comboBox.ItemsSource = Configuration.GetAllTags();
        }

        /// <summary>
        /// Responds to the user pressing 'Save' button that saves the newly defined tag.
        /// </summary>
        void SaveButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveTag(_comboBox.Text);
        }

        /// <summary>
        /// Responds to the user pressing 'Apply to selection' button that applies the selected tag to the selected items in an associated view.
        /// </summary>
        void ApplyButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var tagToApply = _comboBox.Text;
            if (string.IsNullOrEmpty(tagToApply)) return;

            ApplyTagToSelectionEvent(tagToApply);
        }

        /// <summary>
        /// Reacts to a key being pressed within the tag name combo box.
        /// </summary>
        void ComboBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.None:
                    break;
                case Key.Return:
                    SaveTag(_comboBox.Text);
                    break;
                case Key.Escape:
                    _comboBox.Text = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Saves the tag to the database. Trims any redundant whitespaces on left and right.
        /// If the tag is multiword, whitespaces in the middle of the name are preserved.
        /// </summary>
        /// <param name="tagName">Name of the tag to be added to the system.</param>
        private void SaveTag(string tagName)
        {
            Configuration.AddMissingTags(new[] { tagName.Trim() });
            _comboBox.Text = string.Empty;
            _comboBox.ItemsSource = Configuration.GetAllTags();
            SaveTagEvent(tagName);
        }
    }
}