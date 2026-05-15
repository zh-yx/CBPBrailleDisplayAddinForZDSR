using System;
using System.Runtime.InteropServices;

namespace StarBraille
{
    /// <summary>
    /// 一些实用工具方法的静态类。
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// 尝试载入 DLL 文件，以测试该 DLL 是否可用。
        /// </summary>
        /// <param name="dllName">要尝试载入 DLL 的文件名。</param>
        /// <returns>如果该 DLL 可用返回 true； 否则返回 false。</returns>
        public static bool CanLoadDll(string dllName)
        {
            const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
            const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;

            IntPtr handle = LoadLibraryEx(dllName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE);
            if (handle != IntPtr.Zero)
            {
                FreeLibrary(handle);
                return true;
            }
            return false;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
    }
}
