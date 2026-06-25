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
            try
            {
                return _BrailleDisplay.Connect();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return false;
        }

        public void Disconnect()
        {
            try
            {
                _BrailleDisplay.Disconnect();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public int GetVersion()
        {
            return 1;
        }

        public bool Initial(Action<int, int> setHandle, Action<string> actionHandler, Action<int> routingKeyHandler)
        {
            Logger.SetLogFileName("StarBrailleForZdsr.log");
            _BrailleDisplay = new StarBrailleDisplay(setHandle, actionHandler, routingKeyHandler);
            return true;
        }

        public void WriteCells(byte[] cells)
        {
            try
            {
                _BrailleDisplay.ShowBraille(cells);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
