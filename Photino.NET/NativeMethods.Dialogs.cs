using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

internal static partial class NativeMethods
{
    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_ShowOpenFile(
        IntPtr inst,
        string? title,
        string? defaultPath,
        [MarshalAs(UnmanagedType.I1)] bool multiSelect,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str, SizeParamIndex = 5)]
        string[] filters,
        int filtersCount,
        out int resultCount);

    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_ShowOpenFolder(
        IntPtr inst,
        string? title,
        string? defaultPath,
        [MarshalAs(UnmanagedType.I1)] bool multiSelect,
        out int resultCount);

    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_ShowSaveFile(
        IntPtr inst,
        string? title,
        string? defaultPath,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str, SizeParamIndex = 4)]
        string[] filters,
        int filtersCount,
        string? defaultFileName);

    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial PhotinoDialogResult Photino_ShowMessage(
        IntPtr inst,
        string? title,
        string? text,
        PhotinoDialogButtons buttons,
        PhotinoDialogIcon icon);
}
