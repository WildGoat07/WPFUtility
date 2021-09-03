using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wildgoat.WPFUtility.Controls
{
    /// <summary>
    /// The behaviour of the small icons
    /// </summary>
    public enum IconBehaviour
    {
        /// <summary>
        /// The icons will try to fit as much as possible in the space taken by the Base, they may overlap
        /// </summary>
        FIT,

        /// <summary>
        /// The icons will avoid being overlaping each other, but the resulting icon may look bigger than the Base
        /// </summary>
        AVOID_OVERLAP
    }

    [ContentProperty(nameof(BaseContent))]
    public partial class ActionIcon : UserControl
    {
        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            nameof(ActionContent),
            typeof(UIElement),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ActionImageProperty = DependencyProperty.Register(
                    nameof(ActionImage),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
                    nameof(Action),
            typeof(Icon?),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BaseContentProperty = DependencyProperty.Register(
            nameof(BaseContent),
            typeof(UIElement),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BaseImageProperty = DependencyProperty.Register(
                    nameof(BaseImage),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IconBehaviourProperty = DependencyProperty.Register(
                nameof(IconBehaviour),
            typeof(IconBehaviour),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(IconBehaviour.FIT, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IconGapProperty = DependencyProperty.Register(
                    nameof(IconGap),
            typeof(Thickness),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ModifierContentProperty = DependencyProperty.Register(
                            nameof(ModifierContent),
            typeof(UIElement),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ModifierImageProperty = DependencyProperty.Register(
                    nameof(ModifierImage),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ModifierProperty = DependencyProperty.Register(
                    nameof(Modifier),
            typeof(Icon?),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StatusContentProperty = DependencyProperty.Register(
            nameof(StatusContent),
            typeof(UIElement),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StatusImageProperty = DependencyProperty.Register(
                    nameof(StatusImage),
            typeof(ImageSource),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
                    nameof(Status),
            typeof(Icon?),
            typeof(ActionIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActionIcon()
        {
            InitializeComponent();
        }

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
        /// Custom template for the top left area.
        /// </summary>
        [Description("Custom template icon in the top left.")]
        public UIElement? ActionContent
        {
            get => GetValue(ActionContentProperty) as UIElement;
            set => SetValue(ActionContentProperty, value);
        }

        /// <summary>
        /// Custom source image for the top left area.
        /// </summary>
        [Description("Custom source for the icon in the top left."), Category("Appearance")]
        public ImageSource? ActionImage
        {
            get => GetValue(ActionImageProperty) as ImageSource;
            set => SetValue(ActionImageProperty, value);
        }

        /// <summary>
        /// Custom template for the main icon.
        /// </summary>
        [Description("Custom template of the main icon.")]
        public UIElement? BaseContent
        {
            get => GetValue(BaseContentProperty) as UIElement;
            set => SetValue(BaseContentProperty, value);
        }

        /// <summary>
        /// Source image of the base reference icon.
        /// </summary>
        [Description("Source of the main icon."), Category("Appearance")]
        public ImageSource? BaseImage
        {
            get => GetValue(BaseImageProperty) as ImageSource;
            set => SetValue(BaseImageProperty, value);
        }

        /// <summary>
        /// The way the icons are layed
        /// </summary>
        [Description("The way the icons are layed."), Category("Appearance")]
        public IconBehaviour IconBehaviour
        {
            get => (IconBehaviour)GetValue(IconBehaviourProperty);
            set => SetValue(IconBehaviourProperty, value);
        }

        /// <summary>
        /// Gap between icons, for better visibility
        /// </summary>
        [Description("Gap between icons, for better visibility."), Category("Layout")]
        public Thickness IconGap
        {
            get => (Thickness)GetValue(IconGapProperty);
            set => SetValue(IconGapProperty, value);
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
        /// Custom template for the bottom left area.
        /// </summary>
        [Description("Custom template icon in the bottom left.")]
        public UIElement? ModifierContent
        {
            get => GetValue(ModifierContentProperty) as UIElement;
            set => SetValue(ModifierContentProperty, value);
        }

        /// <summary>
        /// Custom source image for the bottom left area.
        /// </summary>
        [Description("Custom source for the icon in the bottom left."), Category("Appearance")]
        public ImageSource? ModifierImage
        {
            get => GetValue(ModifierImageProperty) as ImageSource;
            set => SetValue(ModifierImageProperty, value);
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
        /// Custom template for the bottom right area.
        /// </summary>
        [Description("Custom template icon in the bottom left.")]
        public UIElement? StatusContent
        {
            get => GetValue(StatusContentProperty) as UIElement;
            set => SetValue(StatusContentProperty, value);
        }

        /// <summary>
        /// Custom source image for the bottom right area.
        /// </summary>
        [Description("Custom source for the icon in the bottom right."), Category("Appearance")]
        public ImageSource? StatusImage
        {
            get => GetValue(StatusImageProperty) as ImageSource;
            set => SetValue(StatusImageProperty, value);
        }

        internal static string? GetIconKey(Icon? icon) => icon switch
        {
            Icon.ADD => "Add",
            Icon.ALERT => "Alert",
            Icon.DELETE => "Delete",
            Icon.DOWNLOAD => "Download",
            Icon.EDIT => "Edit",
            Icon.EDIT_REVERSE => "EditReverse",
            Icon.ERROR => "Error",
            Icon.EXT_LINK => "ExtLink",
            Icon.FIND => "Find",
            Icon.HELP => "Help",
            Icon.IMPORT => "Import",
            Icon.INFO => "Info",
            Icon.LINK => "Link",
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
    }

    internal class GetIconConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = values[0] as Icon?;
            var sender = values[1] as ActionIcon;
            if (icon != null && sender != null)
                return sender.TryFindResource(ActionIcon.GetIconKey(icon));
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    internal class IconGapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thickness = (Thickness)value;
            return int.Parse(parameter.ToString() ?? "-1") switch
            {
                0 => new Thickness(0, 0, thickness.Right, thickness.Bottom),
                1 => new Thickness(thickness.Left, thickness.Top, 0, 0),
                2 => new Thickness(0, thickness.Top, thickness.Right, 0),
                _ => new Thickness(0)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}