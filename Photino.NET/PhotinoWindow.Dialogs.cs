using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Photino.NET.Utils;

namespace Photino.NET;

using static NativeMethods;

public partial class PhotinoWindow
{
    /// <summary>
    /// Show an open file dialog native to the OS.
    /// </summary>
    /// <remarks>
    /// Filter names are not used on macOS. Use async version for Photino.Blazor as synchronous version crashes.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="multiSelect">Whether multiple selections are allowed</param>
    /// <param name="filters">Array of (Name, Extensions) filter definitions.</param>
    /// <returns>Array of file paths as strings</returns>
    public string[] ShowOpenFile(string title = "Choose file", string? defaultPath = null, bool multiSelect = false, (string Name, string[] Extensions)[]? filters = null) => ShowOpenDialog(false, title, defaultPath, multiSelect, filters);

    /// <summary>
    /// Async version is required for Photino.Blazor
    /// </summary>
    /// <remarks>
    /// Filter names are not used on macOS. Use async version for Photino.Blazor as synchronous version crashes.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="multiSelect">Whether multiple selections are allowed</param>
    /// <param name="filters">Array of (Name, Extensions) filter definitions.</param>
    /// <returns>Array of file paths as strings</returns>
    public Task<string[]> ShowOpenFileAsync(string title = "Choose file", string? defaultPath = null, bool multiSelect = false, (string Name, string[] Extensions)[]? filters = null)
    {
        return Task.Run(() => ShowOpenFile(title, defaultPath, multiSelect, filters));
    }

    /// <summary>
    /// Show an open folder dialog native to the OS.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="multiSelect">Whether multiple selections are allowed</param>
    /// <returns>Array of folder paths as strings</returns>
    public string[] ShowOpenFolder(string title = "Select folder", string? defaultPath = null, bool multiSelect = false) => ShowOpenDialog(true, title, defaultPath, multiSelect, null);

    /// <summary>
    /// Async version is required for Photino.Blazor
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="multiSelect">Whether multiple selections are allowed</param>
    /// <returns>Array of folder paths as strings</returns>
    public Task<string[]> ShowOpenFolderAsync(string title = "Select folder", string? defaultPath = null, bool multiSelect = false)
    {
        return Task.Run(() => ShowOpenFolder(title, defaultPath, multiSelect));
    }

    /// <summary>
    /// Shows a native OS save‑file dialog.
    /// </summary>
    /// <remarks>
    /// Filter names are not used on macOS.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="filters">Array of (Name, Extensions) filter definitions.</param>
    /// <param name="defaultFileName">Default file name.</param>
    /// <returns>The selected file path, or <c>null</c>.</returns>
    public string? ShowSaveFile(string title = "Save file", string? defaultPath = null, (string Name, string[] Extensions)[]? filters = null, string? defaultFileName = null)
    {
        defaultPath ??= Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        filters ??= [];
        defaultFileName ??= string.Empty;

        string? result = null;
        var nativeFilters = GetNativeFilters(filters);

        Invoke(() =>
        {
            var ptrResult = Photino_ShowSaveFile(_nativeInstance, title, defaultPath, nativeFilters, nativeFilters.Length, defaultFileName);
            try
            {
                result = ptrResult != IntPtr.Zero ? Marshal.PtrToStringUTF8(ptrResult) : null;
            }
            finally
            {
                if (ptrResult != IntPtr.Zero)
                    Photino_FreeString(ptrResult);
            }
        });

        return result;
    }

    /// <summary>
    /// Async version of <see cref="ShowSaveFile"/>. 
    /// Required for PhotinoX.Blazor and any UI scenarios where blocking the thread is not allowed.
    /// </summary>
    /// <remarks>
    /// Filter names are not used on macOS.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="filters">Array of (Name, Extensions) filter definitions.</param>
    /// <param name="defaultFileName">Default file name.</param>
    /// <returns> A <see cref="Task{TResult}"/> that returns the selected file path, or <c>null</c>.</returns>
    public Task<string?> ShowSaveFileAsync(string title = "Save file", string? defaultPath = null, (string Name, string[] Extensions)[]? filters = null, string? defaultFileName = null)
    {
        return Task.Run(() => ShowSaveFile(title, defaultPath, filters, defaultFileName));
    }

    /// <summary>
    /// Show a message dialog native to the OS.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized.
    /// </exception>
    /// <param name="title">Title of the dialog</param>
    /// <param name="text">Text of the dialog</param>
    /// <param name="buttons">Available interaction buttons <see cref="PhotinoDialogButtons"/></param>
    /// <param name="icon">Icon of the dialog <see cref="PhotinoDialogIcon"/></param>
    /// <returns><see cref="PhotinoDialogResult" /></returns>
    public PhotinoDialogResult ShowMessage(string title, string text, PhotinoDialogButtons buttons = PhotinoDialogButtons.Ok, PhotinoDialogIcon icon = PhotinoDialogIcon.Info)
    {
        var result = PhotinoDialogResult.Cancel;
        Invoke(() => result = Photino_ShowMessage(_nativeInstance, title, text, buttons, icon));
        return result;
    }

    /// <summary>
    /// Show a native open dialog.
    /// </summary>
    /// <param name="foldersOnly">Whether files are hidden</param>
    /// <param name="title">Title of the dialog</param>
    /// <param name="defaultPath">Default path. Defaults to <see cref="Environment.SpecialFolder.MyDocuments"/></param>
    /// <param name="multiSelect">Whether multiple selections are allowed</param>
    /// <param name="filters">Array of (Name, Extensions) filter definitions.</param>
    /// <returns>Array of paths</returns>
    private string[] ShowOpenDialog(bool foldersOnly, string title, string? defaultPath, bool multiSelect, (string Name, string[] Extensions)[]? filters)
    {
        defaultPath ??= Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        filters ??= [];

        var results = Array.Empty<string>();
        var nativeFilters = GetNativeFilters(filters, foldersOnly);

        Invoke(() =>
        {
            var ptrResults = foldersOnly ?
                Photino_ShowOpenFolder(_nativeInstance, title, defaultPath, multiSelect, out var resultCount) :
                Photino_ShowOpenFile(_nativeInstance, title, defaultPath, multiSelect, nativeFilters, nativeFilters.Length, out resultCount);
            results = PtrToStringUtf8ArrayAndFree(ptrResults, resultCount);
        });

        return results;
    }

    /// <summary>
    /// Returns an array of strings for native filters.
    /// </summary>
    /// <param name="filters">The filter definitions.</param>
    /// <param name="empty">Whether to return an empty filter list.</param>
    /// <returns>String array of filters.</returns>
    private static string[] GetNativeFilters((string Name, string[] Extensions)[] filters, bool empty = false)
    {
        if (empty || filters.Length == 0)
            return [];

        var nativeFilters = new List<string>();
        List<string>? extensions = null;

        foreach (var filter in filters)
        {
            if (Platform.IsMacOS)
            {
                foreach (var extension in filter.Extensions)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                        continue;

                    var value = extension.Trim();

                    nativeFilters.Add(value == "*" ? value : value.TrimStart('*', '.'));
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(filter.Name) || filter.Extensions.Length == 0)
                continue;

            extensions ??= [];
            extensions.Clear();

            foreach (var extension in filter.Extensions)
            {
                if (string.IsNullOrWhiteSpace(extension))
                    continue;

                var value = extension.Trim();

                extensions.Add(value == "*"
                    ? value
                    : value.StartsWith('.') ? $"*{value}" : (value.StartsWith("*.") ? value : $"*.{value}"));
            }

            if (extensions.Count > 0)
                nativeFilters.Add($"{filter.Name}|{string.Join(';', extensions)}");
        }

        return [.. nativeFilters];
    }
}
