using System;
using System.Threading;
using System.Threading.Tasks;

namespace StarBraille
{
    internal delegate void ButtonEventHandler(int buttonId);

    internal class ButtonEventDetector
    {
        private const int DEVICE_OFFLINE = -1;
        private const int NO_BUTTON = 255;

        private readonly Func<int> _getButtonIdFunc;
        private readonly int _pollingIntervalMs = 50;

        private CancellationTokenSource _cts;
        private Task _pollTask;

        public ButtonEventDetector(Func<int> getButtonIdFunc)
        {
            _getButtonIdFunc = getButtonIdFunc ?? throw new ArgumentNullException(nameof(getButtonIdFunc));
        }

        public event ButtonEventHandler ButtonClick;

        public void Start()
        {
            if (_pollTask != null && !_pollTask.IsCompleted)
                return; // The task is already running.

            _cts = new CancellationTokenSource();
            _pollTask = Task.Run(() => PollLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            if (_cts == null) return;
            _cts.Cancel();
            try
            {
                _pollTask?.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException) { }
            finally
            {
                _pollTask = null;
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task PollLoopAsync(CancellationToken token)
        {
            int lastId = NO_BUTTON;

            while (!token.IsCancellationRequested)
            {
                int currentId = _getButtonIdFunc();

                // Button Released.
                if (lastId != DEVICE_OFFLINE && lastId != NO_BUTTON && lastId != currentId)
                {
                    this.ButtonClick?.Invoke(lastId);
                }

                lastId = currentId;
                await Task.Delay(_pollingIntervalMs, token);
            }
        }
    }
}
