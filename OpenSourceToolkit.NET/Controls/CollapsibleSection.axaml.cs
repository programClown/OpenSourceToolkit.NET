using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace OpenSourceToolkit.NET.Controls
{
    /// <summary>
    /// A collapsible section with a chevron toggle, header, and expandable content.
    /// </summary>
    public partial class CollapsibleSection : UserControl
    {
        public static readonly StyledProperty<bool> IsExpandedProperty =
            AvaloniaProperty.Register<CollapsibleSection, bool>(nameof(IsExpanded), defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

        public bool IsExpanded
        {
            get => GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<CollapsibleSection, object>(nameof(Header));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly StyledProperty<object> HeaderRightProperty =
            AvaloniaProperty.Register<CollapsibleSection, object>(nameof(HeaderRight));

        /// <summary>
        /// Content to display on the right side of the header (e.g., badges, buttons).
        /// </summary>
        public object HeaderRight
        {
            get => GetValue(HeaderRightProperty);
            set => SetValue(HeaderRightProperty, value);
        }

        public static new readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<CollapsibleSection, object>(nameof(Content));

        public new object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public RelayCommand ToggleExpandedCommand { get; }

        public CollapsibleSection()
        {
            ToggleExpandedCommand = new RelayCommand(() => IsExpanded = !IsExpanded);
            InitializeComponent();
        }

        protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            UpdateChevronRotation();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsExpandedProperty)
                UpdateChevronRotation();
        }

        private void UpdateChevronRotation()
        {
            ChevronIcon.RenderTransform = new RotateTransform(IsExpanded ? 0 : -90);
        }
    }
}
