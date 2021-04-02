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
        private Double BASE_SHIFT_TIME = 6;
        private Double BASE_LUNCH_TIME = 1;

        private DateTime displayDate;

        private readonly DataComponent dataComponent;
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenu contextMenu;
        private readonly MenuItem menuItem1;
        private bool close;
        private DispatcherTimer timer;
        private string invalidErrorMessage;

        private const int TICK_INTERVAL = 2;

        private bool fifteenMinNotificationEndShowed = false;
        private bool tenMinNotificationEndShowed = false;
        private bool fiveMinNotificationEndShowed = false;
        private bool oneMinNotificationEndShowed = false;

        private bool fifteenMinNotificationLunchShowed = false;
        private bool tenMinNotificationLunchShowed = false;
        private bool fiveMinNotificationLunchShowed = false;
        private bool oneMinNotificationLunchShowed = false;

        public MainWindow()
        {
            close = false;

            contextMenu = new ContextMenu();
            menuItem1 = new MenuItem
            {
                Index = 0,
                Text = "Exit"
            };
            menuItem1.Click += new EventHandler(OnQuitClick);

            contextMenu.MenuItems.AddRange(new MenuItem[]
            {
                menuItem1
            });

            notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.ClockIcon,
                Text = "Clock In Notifier",
                ContextMenu = contextMenu,
                Visible = false
            };
            notifyIcon.MouseDoubleClick += new MouseEventHandler(OnNotifyIconClick);
            notifyIcon.BalloonTipClicked += new EventHandler(OnBalloonTipClick);

            displayDate = DateTime.Now;
            dataComponent = new DataComponent()
            {
                HourDisplay = displayDate.ToShortTimeString()
            };

            DataContext = dataComponent;

            InitializeComponent();

            cbShiftTime.SelectedIndex = 1;
            UpdateShiftTime(null, null);
        }

        private void CalculateTimeToLeave()
        {
            if (ListView.Items.Count == 1 || ListView.Items.Count == 2)
            {
                var entryTime = DateTime.Parse((ListView.Items[0] as DataComponent).HourDisplay);
                dataComponent.EndShiftTime = entryTime.AddHours(BASE_SHIFT_TIME + BASE_LUNCH_TIME).ToShortTimeString();
            }
            else if (ListView.Items.Count == 3)
            {
                var firstPoint = DateTime.Parse((ListView.Items[0] as DataComponent).HourDisplay);
                var secondPoint = DateTime.Parse((ListView.Items[1] as DataComponent).HourDisplay);
                var thirdPoint = DateTime.Parse((ListView.Items[2] as DataComponent).HourDisplay);
                var diffThirdFirst = thirdPoint - firstPoint;
                var diffThirdSecond = thirdPoint - secondPoint;
                var timeDone = (diffThirdFirst - diffThirdSecond).TotalHours;

                dataComponent.EndShiftTime = thirdPoint.AddHours(BASE_SHIFT_TIME - timeDone).ToShortTimeString();
            }
            else if (ListView.Items.IsEmpty)
            {
                dataComponent.EndShiftTime = string.Empty;
            }
        }

        #region Form Events
        private void UpdateTime(object sender, RoutedEventArgs e)
        {
            string uid = ((System.Windows.Controls.Button)e.Source).Uid;

            switch (uid.ToUpper())
            {
                case "UPHOUR":
                    displayDate = displayDate.AddHours(1);
                    break;
                case "UPMINUTE":
                    displayDate = displayDate.AddMinutes(1);
                    break;
                case "DOWNHOUR":
                    displayDate = displayDate.AddHours(-1);
                    break;
                case "DOWNMINUTE":
                    displayDate = displayDate.AddMinutes(-1);
                    break;
            }

            dataComponent.HourDisplay = displayDate.ToShortTimeString();
        }

        private void AddHourRestry(object sender, RoutedEventArgs e)
        {
            if (!CanAddNewResgitry())
            {
                System.Windows.MessageBox.Show
                (
                    invalidErrorMessage,
                    "Invalid Operation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            var newItem = new DataComponent()
            {
                HourDisplay = HourTextBlock.Text
            };

            ListView.Items.Add(newItem);
            CalculateTimeToLeave();
        }

        private void RemoveRegistry(object sender, RoutedEventArgs e)
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
                ListView.Items.RemoveAt(ListView.SelectedIndex);

                if (ListView.Items.Count == 2)
                {
                    fifteenMinNotificationLunchShowed = false;
                    tenMinNotificationLunchShowed = false;
                    fiveMinNotificationLunchShowed = false;
                    oneMinNotificationLunchShowed = false;
                }

                fifteenMinNotificationEndShowed = false;
                tenMinNotificationEndShowed = false;
                fiveMinNotificationEndShowed = false;
                oneMinNotificationEndShowed = false;

                CalculateTimeToLeave();
            }
        }

        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show
            (
                "Are you sure you want to reset the hours? All the entries will be deleted.",
                "Reset Hours",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            );

            if (result == MessageBoxResult.No)
            {
                return;
            }

            fifteenMinNotificationEndShowed = false;
            tenMinNotificationEndShowed = false;
            fiveMinNotificationEndShowed = false;
            oneMinNotificationEndShowed = false;

            fifteenMinNotificationLunchShowed = false;
            tenMinNotificationLunchShowed = false;
            fiveMinNotificationLunchShowed = false;
            oneMinNotificationLunchShowed = false;

            BASE_LUNCH_TIME = 1;
            BaseLunchTime.IsChecked = true;

            ListView.Items.Clear();
            dataComponent.EndShiftTime = string.Empty;
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            ExitList.Visibility = ExitList.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }

        private void BtnClose_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void UpdateShiftTime(object sender, RoutedEventArgs e)
        {
            switch (cbShiftTime.SelectedIndex)
            {
                case 0: // 6h
                    BASE_SHIFT_TIME = 6;
                    break;
                case 1: // 8h
                    BASE_SHIFT_TIME = 8;
                    break;
                case 2: // 8h48
                    BASE_SHIFT_TIME = 8.48;
                    break;
            }

            CalculateTimeToLeave();
        }

        private void AddLunchTime(object sender, RoutedEventArgs e)
        {
            BASE_LUNCH_TIME = (bool)BaseLunchTime.IsChecked ? 1 : 0;
            CalculateTimeToLeave();
        }

        private void UpdateTimeNow(object sender, RoutedEventArgs e)
        {
            displayDate = DateTime.Now;
            dataComponent.HourDisplay = displayDate.ToShortTimeString();
        }
        #endregion

        #region Validation
        private bool CanAddNewResgitry()
        {
            if (ListView.Items.Count == 4)
            {
                invalidErrorMessage = "You can't add more than 4 registers.";
                return false;
            }
            if (ListView.Items.Count > 0)
            {
                var lastItemIndex = ListView.Items.Count - 1;
                var item1 = DateTime.Parse((ListView.Items[lastItemIndex] as DataComponent).HourDisplay);
                if (item1 == displayDate || item1 > displayDate)
                {
                    invalidErrorMessage = "You can't add a register that is equal or smaller than the previous register.";
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Handle Minimize, Open and Close Events
        private void OnNotifyIconClick
                    (
                        object sender,
                        MouseEventArgs e
                    )
        {
            if (e.Button == MouseButtons.Left)
            {
                dataComponent.HourDisplay = DateTime.Now.ToShortTimeString();
                WindowState = WindowState.Normal;
                notifyIcon.Visible = false;
                Show();
            }
        }

        private void OnBalloonTipClick
                    (
                        object sender,
                        EventArgs e
                    )
        {
            dataComponent.HourDisplay = DateTime.Now.ToShortTimeString();
            WindowState = WindowState.Normal;
            Show();
        }

        private void OnQuitClick(object sender, EventArgs e)
        {
            close = true;
            notifyIcon.Visible = false;
            Close();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipText = "Clock In Notifier is still running in background.";
                notifyIcon.ShowBalloonTip(1000);
                Hide();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateTimeToLeave();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(TICK_INTERVAL)
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.IsEnabled = true;
        }
        #endregion

        #region Async Thread That Runs The Notification
        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            if (ListView.Items.Count == 2)
            {
                var time = DateTime.Parse((ListView.Items[1] as DataComponent).HourDisplay);
                var diff = time.AddHours(1).Subtract(dt).TotalMinutes;

                if (diff <= 15 && diff > 10 && !fifteenMinNotificationLunchShowed)
                {
                    fifteenMinNotificationLunchShowed = true;
                    notifyIcon.BalloonTipText = "15 minutes to register back from lunch.";
                    notifyIcon.BalloonTipTitle = "Register your point";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 10 && diff > 5 && !tenMinNotificationLunchShowed)
                {
                    tenMinNotificationLunchShowed = true;
                    notifyIcon.BalloonTipText = "10 minutes to register back from lunch.";
                    notifyIcon.BalloonTipTitle = "Register your point";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 5 && diff > 1 && !fiveMinNotificationLunchShowed)
                {
                    fiveMinNotificationLunchShowed = true;
                    notifyIcon.BalloonTipText = "5 minutes to register back from lunch.";
                    notifyIcon.BalloonTipTitle = "Register your point";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 1 && diff > 0 && !oneMinNotificationLunchShowed)
                {
                    oneMinNotificationLunchShowed = true;
                    notifyIcon.BalloonTipTitle = "Register your point";
                    notifyIcon.BalloonTipText = "1 minute to register back from lunch.";
                    notifyIcon.ShowBalloonTip(1000);
                }
            }
            else if (ListView.Items.Count == 1)
            {
                var firstPoint = DateTime.Parse((ListView.Items[0] as DataComponent).HourDisplay);
                var diff = firstPoint.AddHours(BASE_SHIFT_TIME + BASE_LUNCH_TIME).Subtract(dt).TotalMinutes;

                if (diff <= 15 && diff > 10 && !fifteenMinNotificationEndShowed)
                {
                    fifteenMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "15 minutes to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 10 && diff > 5 && !tenMinNotificationEndShowed)
                {
                    tenMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "10 minutes to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 5 && diff > 1 && !fiveMinNotificationEndShowed)
                {
                    fiveMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "5 minutes to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 1 && diff > 0 && !oneMinNotificationEndShowed)
                {
                    oneMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "1 minute to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
            }
            else if (ListView.Items.Count == 3)
            {
                var firstPoint = DateTime.Parse((ListView.Items[0] as DataComponent).HourDisplay);
                var secondPoint = DateTime.Parse((ListView.Items[1] as DataComponent).HourDisplay);
                var minutesDone = secondPoint.Subtract(firstPoint).TotalMinutes;

                var thirdPoint = DateTime.Parse((ListView.Items[2] as DataComponent).HourDisplay);

                var diff = thirdPoint.AddMinutes((BASE_SHIFT_TIME * 60) - minutesDone)
                                    .Subtract(dt)
                                    .TotalMinutes;
                if (diff <= 15 && diff > 10 && !fifteenMinNotificationEndShowed)
                {
                    fifteenMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "15 minutes to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 10 && diff > 5 && !tenMinNotificationEndShowed)
                {
                    tenMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "10 minutes to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 5 && diff > 1 && !fiveMinNotificationEndShowed)
                {
                    fiveMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "5 minutes to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 1 && diff > 0 && !oneMinNotificationEndShowed)
                {
                    oneMinNotificationEndShowed = true;
                    notifyIcon.BalloonTipText = "1 minute to register end of journey.";
                    notifyIcon.ShowBalloonTip(1000);
                }
            }
        }
        #endregion
    }
}
