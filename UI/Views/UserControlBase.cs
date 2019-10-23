using System;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// User control that provide first lode event
    /// </summary>
    public class UserControlBase : UserControl
    {
        public bool HasLoaded { get; set; } 

        public UserControlBase()
        {
            HasLoaded = false;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            Loaded -= OnLoaded;
            OnFirstLoaded();
        }

        public event Action FirstLoaded;

        protected virtual void OnFirstLoaded()
        {
            FirstLoaded?.Invoke();
        }
    }
}