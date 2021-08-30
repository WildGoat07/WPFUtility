using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wildgoat.WPFUtility.Controls
{
    public partial class ActionIcon : UserControl
    {
        public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
            nameof(Action),
            typeof(Icon?),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnActionPropertyChanged));

        public static readonly DependencyProperty ActionSourceProperty = DependencyProperty.Register(
            nameof(ActionSource),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ActionTemplateProperty = DependencyProperty.Register(
            nameof(ActionTemplate),
            typeof(DataTemplate),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BaseSourceProperty = DependencyProperty.Register(
            nameof(BaseSource),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BaseTemplateProperty = DependencyProperty.Register(
            nameof(BaseTemplate),
            typeof(DataTemplate),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ModifierProperty = DependencyProperty.Register(
            nameof(Modifier),
            typeof(Icon?),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnModifierPropertyChanged));

        public static readonly DependencyProperty ModifierSourceProperty = DependencyProperty.Register(
            nameof(ModifierSource),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ModifierTemplateProperty = DependencyProperty.Register(
            nameof(ModifierTemplate),
            typeof(DataTemplate),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status),
            typeof(Icon?),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnStatusPropertyChanged));

        public static readonly DependencyProperty StatusSourceProperty = DependencyProperty.Register(
            nameof(StatusSource),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StatusTemplateProperty = DependencyProperty.Register(
                                                                    nameof(StatusTemplate),
            typeof(DataTemplate),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActionIcon()
        {
            InitializeComponent();
        }

        [Description("Triggers after the Action property has changed.")]
        public event EventHandler<IconChangedEventArgs>? ActionChanged;

        [Description("Triggers after the Modifier property has changed.")]
        public event EventHandler<IconChangedEventArgs>? ModifierChanged;

        [Description("Triggers after the Status property has changed.")]
        public event EventHandler<IconChangedEventArgs>? StatusChanged;

        /// <summary>
        /// Icon used in the top left area
        /// </summary>
        [Description("Icon in the top left."), Category("Appearance")]
        public Icon? Action
        {
            get => GetValue(ActionProperty) as Icon?;
            set => SetValue(ActionProperty, value);
        }

        /// <summary>
        /// Custom source image for the top left area.
        /// </summary>
        [Description("Custom source for the icon in the top left."), Category("Appearance")]
        public ImageSource? ActionSource
        {
            get => GetValue(ActionSourceProperty) as ImageSource;
            set => SetValue(ActionSourceProperty, value);
        }

        /// <summary>
        /// Custom template for the top left area.
        /// </summary>
        [Description("Custom template icon in the top left.")]
        public DataTemplate? ActionTemplate
        {
            get => GetValue(ActionTemplateProperty) as DataTemplate;
            set => SetValue(ActionTemplateProperty, value);
        }

        /// <summary>
        /// Source image of the base reference icon.
        /// </summary>
        [Description("Source of the main icon."), Category("Appearance")]
        public ImageSource? BaseSource
        {
            get => GetValue(BaseSourceProperty) as ImageSource;
            set => SetValue(BaseSourceProperty, value);
        }

        /// <summary>
        /// Custom template for the main icon.
        /// </summary>
        [Description("Custom template of the main icon.")]
        public DataTemplate? BaseTemplate
        {
            get => GetValue(BaseTemplateProperty) as DataTemplate;
            set => SetValue(BaseTemplateProperty, value);
        }

        /// <summary>
        /// Icon used in the bottom left area
        /// </summary>
        [Description("Icon in the bottom left."), Category("Appearance")]
        public Icon? Modifier
        {
            get => GetValue(ModifierProperty) as Icon?;
            set => SetValue(ModifierProperty, value);
        }

        /// <summary>
        /// Custom source image for the bottom left area.
        /// </summary>
        [Description("Custom source for the icon in the bottom left."), Category("Appearance")]
        public ImageSource? ModifierSource
        {
            get => GetValue(ModifierSourceProperty) as ImageSource;
            set => SetValue(ModifierSourceProperty, value);
        }

        /// <summary>
        /// Custom template for the bottom left area.
        /// </summary>
        [Description("Custom template icon in the bottom left.")]
        public DataTemplate? ModifierTemplate
        {
            get => GetValue(ModifierTemplateProperty) as DataTemplate;
            set => SetValue(ModifierTemplateProperty, value);
        }

        /// <summary>
        /// Icon used in the bottom right area
        /// </summary>
        [Description("Icon in the bottom right."), Category("Appearance")]
        public Icon? Status
        {
            get => GetValue(StatusProperty) as Icon?;
            set => SetValue(StatusProperty, value);
        }

        /// <summary>
        /// Custom source image for the bottom right area.
        /// </summary>
        [Description("Custom source for the icon in the bottom right."), Category("Appearance")]
        public ImageSource? StatusSource
        {
            get => GetValue(StatusSourceProperty) as ImageSource;
            set => SetValue(StatusSourceProperty, value);
        }

        /// <summary>
        /// Custom template for the bottom right area.
        /// </summary>
        [Description("Custom template icon in the bottom left.")]
        public DataTemplate? StatusTemplate
        {
            get => GetValue(StatusTemplateProperty) as DataTemplate;
            set => SetValue(StatusTemplateProperty, value);
        }

        private static string? GetIconKey(Icon? icon) => icon switch
        {
            Icon.ADD => "Add",
            Icon.ALERT => "Alert",
            Icon.DELETE => "Delete",
            Icon.DOWNLOAD => "Download",
            Icon.EDIT => "Edit",
            Icon.EDIT_REVERSE => "EditReverse",
            Icon.ERROR => "Error",
            Icon.FIND => "Find",
            Icon.HELP => "Help",
            Icon.IMPORT => "Import",
            Icon.INFO => "Info",
            Icon.LOCK => "Lock",
            Icon.NEW => "New",
            Icon.NEXT => "Next",
            Icon.NO => "No",
            Icon.OK => "Ok",
            Icon.OPEN => "Open",
            Icon.PAUSE => "Pause",
            Icon.PREVIOUS => "Previous",
            Icon.REDO => "Redo",
            Icon.REFRESH => "Refresh",
            Icon.REMOVE => "Remove",
            Icon.RUN => "Run",
            Icon.SAVE => "Save",
            Icon.SAVE_REVERSE => "SaveReverse",
            Icon.STAR => "Star",
            Icon.STOP => "Stop",
            Icon.SYNC => "Sync",
            Icon.UNDO => "Undo",
            Icon.UPLOAD => "Upload",
            Icon.VALIDATE => "Validate",
            Icon.WARNING => "Warning",
            _ => null,
        };

        private static void OnActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actionIcon = (ActionIcon)d;
            var key = GetIconKey(e.NewValue as Icon?);
            actionIcon.ActionSource = key is null
                ? null
                : actionIcon.TryFindResource(key) as ImageSource;
            actionIcon.ActionChanged?.Invoke(actionIcon, new IconChangedEventArgs(e.OldValue as Icon?, e.NewValue as Icon?));
        }

        private static void OnModifierPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actionIcon = (ActionIcon)d;
            var key = GetIconKey(e.NewValue as Icon?);
            actionIcon.ModifierSource = key is null
                ? null
                : actionIcon.TryFindResource(key) as ImageSource;
            actionIcon.ModifierChanged?.Invoke(actionIcon, new IconChangedEventArgs(e.OldValue as Icon?, e.NewValue as Icon?));
        }

        private static void OnStatusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actionIcon = (ActionIcon)d;
            var key = GetIconKey(e.NewValue as Icon?);
            actionIcon.StatusSource = key is null
                ? null
                : actionIcon.TryFindResource(key) as ImageSource;
            actionIcon.StatusChanged?.Invoke(actionIcon, new IconChangedEventArgs(e.OldValue as Icon?, e.NewValue as Icon?));
        }
    }
}