using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlightProjectDBGenerator
{
    class DataSet : INotifyPropertyChanged
    {
        private bool _isRandomEnabled;
        private int _nonRandomValue;
        private int _minRandomValue;
        private int _maxRandomValue;

        public ManualResetEvent DBCreated = new ManualResetEvent(false);
        public bool runThread = false;
        public int listSize;

        public bool IsRandomEnabled
        {
            get
            {
                return _isRandomEnabled;
            }
            set
            {
                _isRandomEnabled = value;
                OnPropertyChanged("IsRandomEnabled");
                OnPropertyChanged("IsRandomDisabled");
            }
        }

        public bool IsRandomDisabled
        {
            get
            {
                return !IsRandomEnabled;
            }
        }

        public int NonRandomValue
        {
            get
            {
                return _nonRandomValue;
            }
            set
            {
                _nonRandomValue = value;
                OnPropertyChanged("NonRandomValue");
            }
        }

        public int MinRandomValue
        {
            get
            {
                return _minRandomValue;
            }
            set
            {
                _minRandomValue = value;
                OnPropertyChanged("MinRandomValue");
            }
        }

        public int MaxRandomValue
        {
            get
            {
                return _maxRandomValue;
            }
            set
            {
                _maxRandomValue = value;
                OnPropertyChanged("MaxRandomValue");
            }
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
