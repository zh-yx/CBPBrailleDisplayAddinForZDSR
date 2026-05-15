using System;
using System.Threading.Tasks;
using ZDSR.BrailleDisplayAddin;

namespace StarBraille
{
    public class BrailleDisplayAddin : IBrailleDisplayAddin
    {
        private StarBrailleDisplay _BrailleDisplay;

        public string Name => "StarBraille";

        public string Description => "文星盲文显示器";

        public async Task<bool> ConnectAsync()
        {
            return await Task.Run(Connect).ConfigureAwait(false);
        }

        private bool Connect()
        {
            return _BrailleDisplay.Connect();
        }

        public void Disconnect()
        {
            _BrailleDisplay.Disconnect();
        }

        public int GetVersion()
        {
            return 1;
        }

        public bool Initial(Action<int, int> setHandle, Action<string> actionHandler, Action<int> routingKeyHandler)
        {
            if (!StarBrailleDriver.DriverReady())
            {
                return false;
            }

            _BrailleDisplay = new StarBrailleDisplay(setHandle, actionHandler, routingKeyHandler);
            return true;
        }

        public void WriteCells(byte[] cells)
        {
            _BrailleDisplay.ShowBraille(cells);
        }
    }
}
