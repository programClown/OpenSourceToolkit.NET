using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using OpenSourceToolkit.NET.ViewModels;

namespace OpenSourceToolkit.NET
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object param)
        {
            if (param == null)
                return null;

            var name = param.GetType().FullName.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                try
                {
                    var control = (Control)Activator.CreateInstance(type);
                    System.Diagnostics.Debug.WriteLine($"[ViewLocator] Successfully created: {name}");
                    return control;
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error creating view '{name}':\n{ex.GetType().Name}: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMessage += $"\n\nInner2: {ex.InnerException.InnerException.GetType().Name}: {ex.InnerException.InnerException.Message}";
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[ViewLocator] ERROR: {errorMessage}");
                    Console.WriteLine($"[ViewLocator] ERROR: {errorMessage}");

                    return new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(80, 0, 0)),
                        Padding = new global::Avalonia.Thickness(20),
                        Child = new ScrollViewer
                        {
                            Content = new TextBlock
                            {
                                Text = errorMessage,
                                TextWrapping = TextWrapping.Wrap,
                                Foreground = Brushes.White,
                                FontFamily = new FontFamily("Consolas, Courier New, monospace")
                            }
                        }
                    };
                }
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
