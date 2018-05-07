using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NotifyProperty
{
    public class NotifyPropertyEx<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        private object _value { get; set; }

        public NotifyPropertyEx()
        {
            Value = default(T);
        }

        public static implicit operator NotifyPropertyEx<T>(T val)
        {
            return new NotifyPropertyEx<T> { Value = val };
        }

        public static implicit operator T(NotifyPropertyEx<T> prop)
        {
            return prop.Value;
        }

        public T Value
        {
            get => (T) _value;

            set => SetValue(value);
        }

        public void SetValue(T val)
        {
            if (val != null && !(val is T))
            {
                throw new InvalidCastException(string.Format("Cannot convert {0} to {1}", val.GetType(), typeof(T)));
            }

            if (Equals(val, _value))
            {
                return;
            }

            _value = val;

            OnPropertyChanged(nameof(Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
        }
    }
}
