using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

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

        public Bitmap Bitmap { get; set; }

        public ImageSource Image
        {
            get
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                        Bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()
                    );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
