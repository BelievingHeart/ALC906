using System;
using System.Collections.Generic;
using System.Timers;


namespace Core.ViewModels.Popup
{
    /// <summary>
    /// Handles the traffics of popup windows
    /// </summary>
    public class PopupQueue
    {
        private Queue<PopupViewModel> _popups = new Queue<PopupViewModel>();
        private readonly object _lockerOfPopups = new object();
        private Timer _timer;
        /// <summary>
        /// Determine whether the first popup in the queue can be shown
        /// </summary>
        private Predicate<object> _canPopup;

        public int PopupCount
        {
            get
            {
                lock (_lockerOfPopups)
                {
                    return  _popups.Count;
                }
            }
        }

        public event Action<PopupViewModel> NewPopupDequeued; 

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="canPopup">Determine whether the first popup in the queue can be shown</param>
        /// <param name="timerScannerInterval">The interval <see cref="canPopup"/> is evaluated </param>
        public PopupQueue(Predicate<object> canPopup, double timerScannerInterval=50)
        {
            _timer = new Timer(timerScannerInterval);
            _timer.Elapsed += OnTimerClick;
            _timer.Start();
            _canPopup = canPopup;
        }
        
        public void EnqueuePopupThreadSafe(PopupViewModel popup)
        {
            lock (_lockerOfPopups)
            {
                _popups.Enqueue(popup);
            }
        }

        private void OnTimerClick(object state, ElapsedEventArgs elapsedEventArgs)
        {
            if (!_canPopup.Invoke(null)) return;
            PopupViewModel newPopup = null;
            lock (_lockerOfPopups)
            {
                var newPopupExists = _popups.Count > 0;
                if (newPopupExists) newPopup = _popups.Dequeue();
            }

            if (newPopup == null) return;
            OnNewPopupDequeued(newPopup);
        }

        protected virtual void OnNewPopupDequeued(PopupViewModel obj)
        {
            NewPopupDequeued?.Invoke(obj);
        }
    }
}