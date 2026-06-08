using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace StarBraille
{
    internal class WinUsbDeviceConnection : IDisposable
    {
        private static class WinKernel
        {
            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;
            public const uint GENERIC_READ_WRITE = GENERIC_READ | GENERIC_WRITE;

            public const uint FILE_SHARE_READ = 0x00000001;
            public const uint FILE_SHARE_WRITE = 0x00000002;
            public const uint FILE_SHARE_READ_WRITE = FILE_SHARE_READ | FILE_SHARE_WRITE;

            public const uint OPEN_EXISTING = 3;

            public const uint FILE_FLAG_OVERLAPPED = 0x40000000;

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        }

        private static class WinUsb
        {
            [DllImport("winusb.dll", SetLastError = true)]
            public static extern bool WinUsb_Initialize(SafeFileHandle DeviceHandle, out IntPtr InterfaceHandle);

            [DllImport("winusb.dll", SetLastError = true)]
            public static extern bool WinUsb_Free(IntPtr InterfaceHandle);

            [DllImport("winusb.dll", SetLastError = true)]
            public static extern bool WinUsb_WritePipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer, int BufferLength, out int LengthTransferred, IntPtr Overlapped);

            [DllImport("winusb.dll", SetLastError = true)]
            public static extern bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer, int BufferLength, out int LengthTransferred, IntPtr Overlapped);
        }

        private bool _isOpen = false;
        private SafeFileHandle _deviceHandle;
        private IntPtr _interfaceHandle;
        private bool _disposed = false;

        private readonly string _devicePath;
        private readonly byte _endpointOut;
        private readonly byte _endpointIn;

        public WinUsbDeviceConnection(string devicePath, byte endpointOut, byte endpointIn)
        {
            _devicePath = devicePath;
            _endpointOut = endpointOut;
            _endpointIn = endpointIn;
        }

        ~WinUsbDeviceConnection()
        {
            Dispose(false);
        }

        public void OpenDevice()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WinUsbDeviceConnection));
            }

            if (_isOpen)
            {
                throw new InvalidOperationException();
            }

            _deviceHandle = WinKernel.CreateFile(_devicePath, WinKernel.GENERIC_READ_WRITE, 0, IntPtr.Zero, WinKernel.OPEN_EXISTING, WinKernel.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (_deviceHandle.IsInvalid)
            {
                throw new Win32Exception();
            }

            if (!WinUsb.WinUsb_Initialize(_deviceHandle, out _interfaceHandle))
            {
                int errCode = Marshal.GetLastWin32Error();
                _deviceHandle.Close();
                throw new Win32Exception(errCode);
            }
            _isOpen = true;
        }

        public int WritePipe(byte[] packData)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WinUsbDeviceConnection));
            }

            if (!_isOpen)
            {
                this.OpenDevice();
            }

            if (!WinUsb.WinUsb_WritePipe(_interfaceHandle, _endpointOut, packData, packData.Length, out int lengthTransferred, IntPtr.Zero))
            {
                throw new Win32Exception();
            }
            return lengthTransferred;
        }

        public int ReadPipe(byte[] packBuffer)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WinUsbDeviceConnection));
            }

            if (!_isOpen)
            {
                this.OpenDevice();
            }

            if (!WinUsb.WinUsb_ReadPipe(_interfaceHandle, _endpointIn, packBuffer, packBuffer.Length, out int lengthTransferred, IntPtr.Zero))
            {
                throw new Win32Exception();
            }
            return lengthTransferred;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_interfaceHandle != IntPtr.Zero)
            {
                WinUsb.WinUsb_Free(_interfaceHandle);
                _interfaceHandle = IntPtr.Zero;
            }

            if (disposing)
            {
                if (_deviceHandle != null)
                {
                    _deviceHandle.Dispose();
                    _deviceHandle = null;
                }
            }

            _isOpen = false;

            _disposed = true;
        }
    }
}

