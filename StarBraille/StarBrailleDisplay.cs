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
        private readonly ButtonEventDetector _buttonDetector;

        private StarBrailleDisplayDriver _brailleDriver;

        public StarBrailleDisplay(Action<int, int> setHandle, Action<string> actionHandler, Action<int> routingKeyHandler)
        {
            _setRowCellHandle = setHandle;
            _actionHandler = actionHandler;
            _routingKeyHandler = routingKeyHandler;
            _buttonDetector = new ButtonEventDetector(this.GetButtonId);
            _buttonDetector.ButtonClick += Device_ButtonClick;
        }

        public bool Connect()
        {
            var driver = StarBrailleDisplayDriver.OpenAvailableDevice();

            if (driver == null)
            {
                return false;
            }

            _setRowCellHandle(ROW_COUNT, CELL_COUNT);
            _brailleDriver = driver;
            _buttonDetector.Start();
            return true;
        }

        public void Disconnect()
        {
            _buttonDetector.Stop();
            _brailleDriver.Dispose();
            _brailleDriver = null;
        }

        public void ShowBraille(byte[] cells)
        {
            _brailleDriver.ShowBraille(cells);
        }

        private void Device_ButtonClick(int buttonId)
        {
            if (buttonId > 0 && buttonId <= 40)
            {
                _routingKeyHandler(buttonId);
            }

            switch (buttonId)
            {
                case 41:
                    _actionHandler("br_PreviousLine");
                    break;
                case 42:
                    _actionHandler("br_PreviousScreen");
                    break;
                case 43:
                    _actionHandler("kb_Tab");
                    break;
                case 44:
                    _actionHandler("br_NextLine");
                    break;
                case 45:
                    _actionHandler("br_NextScreen");
                    break;
                case 46:
                    _actionHandler("kb_Return");
                    break;
            }
        }

        private int GetButtonId()
        {
            int id = _brailleDriver.GetButton();
            return id;
        }
    }
}
