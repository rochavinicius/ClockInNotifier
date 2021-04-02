using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClockInNotifier
{
    class DataComponent : INotifyPropertyChanged
    {
        private string endShiftTime;
        public string EndShiftTime
        {
            get
            {
                return endShiftTime;
            }
            set
            {
                endShiftTime = value;
                OnPropertyChanged();
            }
        }

        private string hourDisplay;
        public string HourDisplay
        {
            get
            {
                return hourDisplay;
            }
            set
            {
                hourDisplay = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
