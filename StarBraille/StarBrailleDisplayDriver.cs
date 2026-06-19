using System;
using System.ComponentModel;

namespace StarBraille
{
    internal class StarBrailleDisplayDriver
    {
        public const int GetButton_COMMUNICATION_FAILURE = -1;
        public const int GetButton_NO_BUTTON = 255;
        private const byte ENDPOINT_OUT = 0x01;
        private const byte ENDPOINT_IN = 0x81;
        private const int PACKAGE_SIZE = 64;
        private const byte COMMAND_SHOW_BRAILLE = 0x80;
        private const byte COMMAND_QUERY_BUTTON = 0x81;

        private static readonly string _deviceHardwareId = "usb#vid_04d8&pid_0053";
        private static readonly Guid _deviceInterfaceGuid = new Guid("{58D07210-27C1-11DD-BD0B-0800200C9A66}");

        private bool _disposed;
        private WinUsbDeviceConnection _deviceConnection;

        private readonly object _lockObj = new object();
        private readonly byte[] _showBraillePackage = new byte[PACKAGE_SIZE];
        private readonly byte[] _queryButtonPackage = new byte[PACKAGE_SIZE];
        private readonly byte[] _queryButtonBuffer = new byte[PACKAGE_SIZE];

        public readonly int CellCount = 40;

        private StarBrailleDisplayDriver(WinUsbDeviceConnection deviceConnection)
        {
            _deviceConnection = deviceConnection;
            _queryButtonPackage[0] = COMMAND_QUERY_BUTTON;
        }

        public static StarBrailleDisplayDriver OpenAvailableDevice()
        {
            foreach (var device in DevicesUtilities.EnumerateDeviceInterfacePath(_deviceInterfaceGuid))
            {
                if (!device.Contains(_deviceHardwareId))
                {
                    continue;
                }

                WinUsbDeviceConnection connection = new WinUsbDeviceConnection(device, ENDPOINT_OUT, ENDPOINT_IN);
                try
                {
                    connection.OpenDevice();
                }
                catch (Win32Exception)
                {
                    connection.Dispose();
                    continue;
                }

                byte[] testPack = new byte[PACKAGE_SIZE];
                testPack[0] = COMMAND_QUERY_BUTTON;
                byte[] buffer = new byte[PACKAGE_SIZE];
                try
                {
                    connection.WritePipe(testPack);
                    connection.ReadPipe(buffer);
                }
                catch (Win32Exception)
                {
                    connection.Dispose();
                    continue;
                }

                if (buffer[0] == COMMAND_QUERY_BUTTON)
                {
                    return new StarBrailleDisplayDriver(connection);
                }
                else
                {
                    connection.Dispose();
                }
            }
            return null;
        }


        public bool ShowBraille(byte[] cells)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(StarBrailleDisplayDriver));
            }

            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            if (cells.Length != CellCount)
            {
                throw new ArgumentException(nameof(cells));
            }

            lock (_lockObj)
            {
                _showBraillePackage[0] = COMMAND_SHOW_BRAILLE;
                cells.CopyTo(_showBraillePackage, 1);
                try
                {
                    _deviceConnection.WritePipe(_showBraillePackage);
                }
                catch (Win32Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetButton()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(StarBrailleDisplayDriver));
            }

            lock (_lockObj)
            {
                try
                {
                    _deviceConnection.WritePipe(_queryButtonPackage);
                }
                catch (Win32Exception)
                {
                    return GetButton_COMMUNICATION_FAILURE;
                }
                _deviceConnection.ReadPipe(_queryButtonBuffer);
                if (_queryButtonBuffer[0] == COMMAND_QUERY_BUTTON)
                {
                    return _queryButtonBuffer[1];
                }
            }
            return GetButton_COMMUNICATION_FAILURE;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _deviceConnection.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
