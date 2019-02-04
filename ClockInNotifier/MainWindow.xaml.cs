using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ClockInNotifier
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataComponent dataComponent;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem menuItem1;
        private Boolean close;
        private DispatcherTimer timer;
        private string invalidErrorMessage;

        public MainWindow()
        {
            InitializeComponent();

            this.close = false;

            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Exit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);

            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]
            {
                this.menuItem1
            });

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Icon = Properties.Resources.ClockIcon;
            this.notifyIcon.Text = "Clock In Notifier";
            this.notifyIcon.ContextMenu = this.contextMenu;
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);


            dataComponent = new DataComponent()
            {
                HourDisplay = DateTime.Now.ToShortTimeString()
            };
            this.CalculateTimeToLeave();
            DataContext = dataComponent;
        }

        private void CalculateTimeToLeave()
        {
            if (this.DataGridList.Items.Count == 0)
            {
                this.dataComponent.EndShiftTime = String.Empty;
            }
            else if (this.DataGridList.Items.Count == 1 || this.DataGridList.Items.Count == 2)
            {
                var entryTime = 
                    DateTime.Parse((this.DataGridList.Items[0] as DataComponent).HourDisplay);
                this.dataComponent.EndShiftTime = entryTime.AddHours(6).ToShortTimeString();
            }
            else if (this.DataGridList.Items.Count == 3)
            {
                var firstPoint = DateTime.Parse((this.DataGridList.Items[0] as DataComponent).HourDisplay);
                var secondPoint = DateTime.Parse((this.DataGridList.Items[1] as DataComponent).HourDisplay);
                var thirdPoint = DateTime.Parse((this.DataGridList.Items[2] as DataComponent).HourDisplay);
                var diffThirdFirst = thirdPoint - firstPoint;
                var diffThirdSecond = thirdPoint - secondPoint;
                var timeDone = (diffThirdFirst - diffThirdSecond).TotalHours;

                this.dataComponent.EndShiftTime = thirdPoint.AddHours(6 - timeDone).ToShortTimeString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Parse(dataComponent.HourDisplay);
            string uid = ((System.Windows.Controls.Button)e.Source).Uid;

            switch (uid.ToUpper())
            {
                case "UPHOUR":
                    dt = dt.AddHours(1);
                    break;
                case "UPMINUTE":
                    dt = dt.AddMinutes(1);
                    break;
                case "DOWNHOUR":
                    dt = dt.AddHours(-1);
                    break;
                case "DOWNMINUTE":
                    dt = dt.AddMinutes(-1);
                    break;
            }

            dataComponent.HourDisplay = dt.ToShortTimeString();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!CanAddNewResgitry())
            {
                System.Windows.MessageBox.Show
                (
                    this.invalidErrorMessage,
                    "Invalid Operation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            var txtTime = HourTextBlock.Text;

            var newItem = new DataComponent()
            {
                HourDisplay = HourTextBlock.Text,
                bitmap = Properties.Resources.DeleteIcon
            };
            DataGridList.Items.Add(newItem);
            this.CalculateTimeToLeave();
        }

        private bool CanAddNewResgitry()
        {
            if (this.DataGridList.Items.Count == 4)
            {
                this.invalidErrorMessage = "You can't add more than 4 registers.";
                return false;
            }
            if (this.DataGridList.Items.Count > 0)
            {
                var lastItemIndex = this.DataGridList.Items.Count - 1;
                var item1 = DateTime.Parse((this.DataGridList.Items[lastItemIndex] as DataComponent).HourDisplay);
                var newItem = DateTime.Parse(HourTextBlock.Text);
                if (item1 == newItem || item1 > newItem)
                {
                    this.invalidErrorMessage = "You can't add a register that is equal or smaller than the previous register.";
                    return false;
                }
            }
            if (DateTime.Parse(HourTextBlock.Text) > DateTime.Now)
            {
                this.invalidErrorMessage = "You can't add a register from the future.";
                return false;
            }
            return true;
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show
            (
                "Are you sure about that?",
                "Delete Hour Registry",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            );

            if (result == MessageBoxResult.Yes)
            {
                DataGridList.Items.RemoveAt(DataGridList.SelectedIndex);
                this.CalculateTimeToLeave();
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState.Minimized == this.WindowState)
            {
                this.notifyIcon.BalloonTipText = "Clock In Notifier is still running in background.";
                this.notifyIcon.ShowBalloonTip(1000);
                this.Hide();
            }
            else if (WindowState.Normal == this.WindowState)
            {
                this.Show();
            }
        }

        private void notifyIcon1_MouseDoubleClick
                    (
                        object sender,
                        System.Windows.Forms.MouseEventArgs e
                    )
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.close = true;
            this.Close();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!this.close)
            {
                e.Cancel = true;
                this.notifyIcon.BalloonTipText = "Clock In Notifier is still running in background.";
                this.notifyIcon.ShowBalloonTip(1000);
                this.Hide();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DataGridList.Items.Add(new DataComponent()
            {
                // for five minutes left test
                //HourDisplay = DateTime.Now.AddHours(-5.933).ToShortTimeString(),
                // for one minute left test
                HourDisplay = DateTime.Now.AddHours(-5.993).ToShortTimeString(),
                bitmap = Properties.Resources.DeleteIcon
            });
            this.DataGridList.Items.Add(new DataComponent()
            {
                HourDisplay = DateTime.Now.AddHours(-3).ToShortTimeString(),
                bitmap = Properties.Resources.DeleteIcon
            });
            this.DataGridList.Items.Add(new DataComponent()
            {
                HourDisplay = DateTime.Now.AddHours(-2).ToShortTimeString(),
                bitmap = Properties.Resources.DeleteIcon
            });

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(2);
            this.timer.Tick += new EventHandler(this.Timer_Tick);
            this.timer.IsEnabled = true;
        }

        private bool fiveMinNotificationEndShowed = false;
        private bool oneMinNotificationEndShowed = false;
        private bool fiveMinNotificationLunchShowed = false;
        private bool oneMinNotificationLunchShowed = false;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.DataGridList.Items.Count == 2)
            {
                var time = DateTime.Parse((this.DataGridList.Items[1] as DataComponent).HourDisplay);
                var diff = time.AddHours(1).Subtract(time).TotalMinutes;
                if (diff < 6 && diff > 4 && !fiveMinNotificationLunchShowed)
                {
                    this.fiveMinNotificationLunchShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register back from lunch.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff < 2 && diff > 0 && !oneMinNotificationLunchShowed)
                {
                    this.oneMinNotificationLunchShowed = true;
                    this.notifyIcon.BalloonTipText = "1 minute to register back from lunch.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
            }
            else if (this.DataGridList.Items.Count == 1)
            {
                var firstPoint = DateTime.Parse((this.DataGridList.Items[0] as DataComponent).HourDisplay);
                var x = firstPoint.AddHours(6).Subtract(DateTime.Now).TotalMinutes;
                var diff = DateTime.Now.Subtract(firstPoint).TotalMinutes;

                if (diff < 6 && diff > 4 && !fiveMinNotificationEndShowed)
                {
                    this.fiveMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff < 2 && diff > 0 && !oneMinNotificationEndShowed)
                {
                    this.oneMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "1 minute to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
            }
            else if (this.DataGridList.Items.Count == 3)
            {
                var firstPoint = DateTime.Parse((this.DataGridList.Items[0] as DataComponent).HourDisplay);
                var secondPoint = DateTime.Parse((this.DataGridList.Items[1] as DataComponent).HourDisplay);
                var minutesDone = secondPoint.Subtract(firstPoint).TotalMinutes;

                var thirdPoint = DateTime.Parse((this.DataGridList.Items[2] as DataComponent).HourDisplay);

                var diff = DateTime.Now.Subtract(thirdPoint).TotalMinutes;
                diff = diff - minutesDone;

                if (diff < 6 && diff > 4 && !fiveMinNotificationEndShowed)
                {
                    this.fiveMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff < 2 && diff > 0 && !oneMinNotificationEndShowed)
                {
                    this.oneMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "1 minute to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
            }
        }
    }
}
