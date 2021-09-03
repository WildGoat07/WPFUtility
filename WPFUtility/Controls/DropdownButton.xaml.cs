using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wildgoat.WPFUtility.Controls
{
    /// <summary>
    /// Button that has a dropdown for more options
    /// </summary>
    public partial class DropdownButton : Button
    {
        /// <summary>
        /// Style of the inner buttons. TargetType must be Button
        /// </summary>
        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(DropdownButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Content of the dropdown. Supports handling ContextMenu or Popup objects.
        /// </summary>
        public static readonly DependencyProperty DropdownProperty = DependencyProperty.Register(
            nameof(Dropdown),
            typeof(UIElement),
            typeof(DropdownButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// True if the the default button must be shown. If false, there is only one whole button that shows the contextmenu.
        /// </summary>
        public static readonly DependencyProperty ShowDefaultButtonProperty = DependencyProperty.Register(
            nameof(ShowDefaultButton),
            typeof(bool),
            typeof(DropdownButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Constructor
        /// </summary>
        public DropdownButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Click on the default button, if enabled
        /// </summary>
        public event RoutedEventHandler? DefaultClick;

        /// <summary>
        /// Style of the inner buttons. TargetType must be Button
        /// </summary>
        public Style? ButtonStyle { get => (Style)GetValue(DropdownProperty); set => SetValue(DropdownProperty, value); }

        /// <summary>
        /// Content of the dropdown. Supports handling ContextMenu or Popup objects.
        /// </summary>
        public UIElement? Dropdown { get => (UIElement)GetValue(DropdownProperty); set => SetValue(DropdownProperty, value); }

        /// <summary>
        /// True if the the default button must be shown. If false, there is only one whole button that shows the contextmenu.
        /// </summary>
        public bool ShowDefaultButton { get => (bool)GetValue(ShowDefaultButtonProperty); set => SetValue(ShowDefaultButtonProperty, value); }

        /// <summary>
        /// Forces the dropdown to close
        /// </summary>
        public void CloseDrown()
        {
            if (Dropdown != null)
            {
                if (Dropdown is ContextMenu contextMenu)
                {
                    contextMenu.IsOpen = false;
                }
                else if (Dropdown is Popup popup)
                {
                    popup.IsOpen = false;
                }
                else
                {
                    popup = (Popup)Template.FindName("popup", this);
                    popup.IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Forces the dropdown to open
        /// </summary>
        public void OpenDropdown()
        {
            if (Dropdown != null)
            {
                if (Dropdown is ContextMenu contextMenu)
                {
                    contextMenu.Placement = PlacementMode.Bottom;
                    contextMenu.PlacementTarget = this;
                    contextMenu.IsOpen = true;
                }
                else if (Dropdown is Popup popup)
                {
                    popup.Placement = PlacementMode.Bottom;
                    popup.PlacementTarget = this;
                    popup.IsOpen = true;
                }
                else
                {
                    popup = (Popup)Template.FindName("popup", this);
                    popup.Child = Dropdown;
                    popup.IsOpen = true;
                }
            }
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            e.Source = this;
            DefaultClick?.Invoke(this, e);
        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            OpenDropdown();
        }
    }
}