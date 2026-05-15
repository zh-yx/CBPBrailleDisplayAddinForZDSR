using System;

namespace StarBraille
{
    internal class StarBrailleDisplay
    {
        private const int ROW_COUNT = 1;
        private const int CELL_COUNT = 40;

        private readonly Action<int, int> _setRowCellHandle;
        private readonly Action<string> _actionHandler;
        private readonly Action<int> _routingKeyHandler;

        public StarBrailleDisplay(Action<int, int> setHandle, Action<string> actionHandler, Action<int> routingKeyHandler)
        {
            _setRowCellHandle = setHandle;
            _actionHandler = actionHandler;
            _routingKeyHandler = routingKeyHandler;
        }

        public bool Connect()
        {
            if (StarBrailleDriver.OpenBrailleDisplay() != 1)
            {
                return false;
            }

            _setRowCellHandle(ROW_COUNT, CELL_COUNT);
            return true;
        }

        public void Disconnect()
        {
            StarBrailleDriver.CloseBrailleDisplay();
        }

        public void ShowBraille(byte[] cells)
        {
            StarBrailleDriver.ShowBraille(cells);
        }
    }
}
