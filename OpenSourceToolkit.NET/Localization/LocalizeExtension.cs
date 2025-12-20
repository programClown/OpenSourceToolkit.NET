#nullable enable

using Flowery.Localization;
using System;
using System.Globalization;

namespace OpenSourceToolkit.NET.Localization
{
    /// <summary>
    /// Markup extension for localizing strings in XAML.
    /// Returns the localized string directly and updates when culture changes.
    /// Usage: {loc:Localize Button_Generate}
    /// </summary>
    public class LocalizeExtension : LocalizeExtensionBase
    {
        /// <summary>
        /// Initializes a new instance of the LocalizeExtension class.
        /// </summary>
        public LocalizeExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the LocalizeExtension class with the specified key.
        /// </summary>
        /// <param name="key">The resource key to localize.</param>
        public LocalizeExtension(string key) : base(key)
        {
        }

        /// <inheritdoc/>
        protected override string GetLocalizedString(string key)
            => ToolkitLocalization.GetString(key);

        /// <inheritdoc/>
        protected override void SubscribeToCultureChanged(EventHandler<CultureInfo> handler)
            => ToolkitLocalization.CultureChanged += handler;

        /// <inheritdoc/>
        protected override object? GetBindingSource()
            => ToolkitLocalization.Instance;
    }
}
