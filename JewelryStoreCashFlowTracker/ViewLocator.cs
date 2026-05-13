using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using JewelryStoreCashFlowTracker.ViewModels;

namespace JewelryStoreCashFlowTracker;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var viewModelName = param.GetType().FullName!;
        var viewName = viewModelName.Replace("ViewModel", "View", StringComparison.Ordinal);

        // Try to get type from the same assembly as the ViewModel
        var assembly = param.GetType().Assembly;
        var type = assembly.GetType(viewName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        // Fallback: try with full assembly name
        var assemblyName = assembly.GetName().Name;
        var fullTypeName = $"{viewName}, {assemblyName}";
        type = Type.GetType(fullTypeName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + viewName, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red) };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}