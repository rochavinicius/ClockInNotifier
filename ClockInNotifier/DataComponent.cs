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
        private const Int32 BASE_SHIFT_TIME = 6;
        public Int32 BaseShiftTime
        {
            get
            {
                return BASE_SHIFT_TIME;
            }
        }

        private String endShiftTime;
        public String EndShiftTime
        {
            get
            {
                return this.endShiftTime;
            }
            set
            {
                this.endShiftTime = value;
                OnPropertyChanged();
            }
        }

        private String hourDisplay;
        public String HourDisplay
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

        public Bitmap bitmap { get; set; }

        public ImageSource Image
        {
            get
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                        bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()
                    );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
