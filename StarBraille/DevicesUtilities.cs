using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace StarBraille
{
    internal static class DevicesUtilities
    {
        private const int ERROR_NO_MORE_ITEMS = 259;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }

        private static class SetupApi
        {
            public const uint DIGCF_PRESENT = 0x00000002;
            public const uint DIGCF_DEVICEINTERFACE = 0x00000010;

            [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr SetupDiGetClassDevs(in Guid ClassGuid, string Enumerator, IntPtr hwndParent, uint Flags);

            [DllImport("setupapi.dll", SetLastError = true)]
            public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

            [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, in Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

            [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, in SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, out int RequiredSize, IntPtr DeviceInfoData);
        }

        public static IEnumerable<string> EnumerateDeviceInterfacePath(Guid classGuid)
        {
            IntPtr hDis = SetupApi.SetupDiGetClassDevs(classGuid, null, IntPtr.Zero, SetupApi.DIGCF_PRESENT | SetupApi.DIGCF_DEVICEINTERFACE);

            if (hDis == INVALID_HANDLE_VALUE)
            {
                yield break;
            }

            try
            {
                int i = 0;
                while (true)
                {
                    SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>();
                    if (!SetupApi.SetupDiEnumDeviceInterfaces(hDis, IntPtr.Zero, classGuid, i, ref deviceInterfaceData))
                    {
                        if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    SetupApi.SetupDiGetDeviceInterfaceDetail(hDis, deviceInterfaceData, IntPtr.Zero, 0, out int bufSize, IntPtr.Zero);
                    if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                    {
                        continue;
                    }

                    // 手动分配和释放存储 SP_DEVICE_INTERFACE_DETAIL_DATA 结构体的缓冲区。因为该结构体末尾为柔型数组。
                    IntPtr DetailBuffer = Marshal.AllocHGlobal(bufSize);
                    // 根据平台指针长度手动写入 cbSize 成员的值。
                    Marshal.WriteInt32(DetailBuffer, IntPtr.Size == 8 ? 8 : 6);
                    if (SetupApi.SetupDiGetDeviceInterfaceDetail(hDis, deviceInterfaceData, DetailBuffer, bufSize, out int _, IntPtr.Zero))
                    {
                        // 手动计算偏移值获取字符串值。
                        string devicePath = Marshal.PtrToStringUni(DetailBuffer + sizeof(int)); ;
                        yield return devicePath;
                    }
                    Marshal.FreeHGlobal(DetailBuffer);
                    i++;
                }
            }
            finally
            {
                SetupApi.SetupDiDestroyDeviceInfoList(hDis);
            }
        }
    }
}
