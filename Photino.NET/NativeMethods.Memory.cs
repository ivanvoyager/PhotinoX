using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

internal static partial class NativeMethods
{
    //Memory management for strings and arrays of strings returned from the native code.

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_AllocateMemory(int size);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_FreeMemory(IntPtr value);


    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_AllocateString(int size);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_FreeString(IntPtr value);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_FreeStringArray(IntPtr values, int count);

    internal static string[] PtrToStringUtf8ArrayAndFree(IntPtr values, int count)
    {
        if (values == IntPtr.Zero)
            return [];

        try
        {
            if (count <= 0)
                return [];

            var result = new string[count];

            for (var i = 0; i < count; i++)
            {
                var itemPtr = Marshal.ReadIntPtr(values, i * IntPtr.Size);
                result[i] = itemPtr != IntPtr.Zero
                    ? Marshal.PtrToStringUTF8(itemPtr) ?? string.Empty
                    : string.Empty;
            }

            return result;
        }
        finally
        {
            Photino_FreeStringArray(values, count);
        }
    }

    internal static IntPtr CopyUtf8StringToNative(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return IntPtr.Zero;

        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var buffer = Photino_AllocateString(bytes.Length + 1);

        if (buffer == IntPtr.Zero)
            throw new OutOfMemoryException();

        Marshal.Copy(bytes, 0, buffer, bytes.Length);
        Marshal.WriteByte(buffer, bytes.Length, 0);

        return buffer;
    }

    internal static IntPtr CopyBytesToNative(byte[] bytes)
    {
        if (bytes.Length == 0)
            return IntPtr.Zero;

        var buffer = Photino_AllocateMemory(bytes.Length);

        if (buffer == IntPtr.Zero)
            throw new OutOfMemoryException();

        Marshal.Copy(bytes, 0, buffer, bytes.Length);
        return buffer;
    }
}
