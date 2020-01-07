using System;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views.HeartBeat
{
    public partial class HeartBeatView : UserControl
    {
        public HeartBeatView()
        {
            InitializeComponent();
        }
        
           
        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent PlcHeartBeatsEvent = EventManager.RegisterRoutedEvent(
            "PlcHeartBeats", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HeartBeatView));

        // Provide CLR accessors for the event
        public event RoutedEventHandler PlcHeartBeats
        {
            add { AddHandler(PlcHeartBeatsEvent, value); } 
            remove { RemoveHandler(PlcHeartBeatsEvent, value); }
        }

        // This method raises the PlcHeartBeats event
        void RaisePlcHeartBeatsEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(HeartBeatView.PlcHeartBeatsEvent);
            RaiseEvent(newEventArgs);
        }


        public static readonly DependencyProperty HeartBeatTimeProperty = DependencyProperty.Register(
            "HeartBeatTime", typeof(DateTime), typeof(HeartBeatView), new PropertyMetadata(default(DateTime), OnHeartBeatTimeChanged));

        private static void OnHeartBeatTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (HeartBeatView) d;
            view.RaisePlcHeartBeatsEvent();
        }

        public DateTime HeartBeatTime
        {
            get { return (DateTime) GetValue(HeartBeatTimeProperty); }
            set { SetValue(HeartBeatTimeProperty, value); }
        }
    }
}