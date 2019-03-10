﻿using System;
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

        private DataComponent dataComponent;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem menuItem1;
        private Boolean close;
        private DispatcherTimer timer;
        private string invalidErrorMessage;

        private const Int32 TICK_INTERVAL = 2;

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
            InitializeComponent();

            this.close = false;

            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Exit";
            this.menuItem1.Click += new System.EventHandler(this.OnQuitClick);

            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]
            {
                this.menuItem1
            });

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Icon = Properties.Resources.ClockIcon;
            this.notifyIcon.Text = "Clock In Notifier";
            this.notifyIcon.ContextMenu = this.contextMenu;
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnNotifyIconClick);

            dataComponent = new DataComponent()
            {
                HourDisplay = DateTime.Now.ToShortTimeString()
            };

            DataContext = dataComponent;
        }

        private void CalculateTimeToLeave()
        {
            if (this.ListView.Items.Count == 0)
            {
                this.dataComponent.EndShiftTime = String.Empty;
            }
            else if (this.ListView.Items.Count == 1 || this.ListView.Items.Count == 2)
            {
                var entryTime =
                    DateTime.Parse((this.ListView.Items[0] as DataComponent).HourDisplay);
                this.dataComponent.EndShiftTime = entryTime.AddHours(BASE_SHIFT_TIME).ToShortTimeString();
            }
            else if (this.ListView.Items.Count == 3)
            {
                var firstPoint = DateTime.Parse((this.ListView.Items[0] as DataComponent).HourDisplay);
                var secondPoint = DateTime.Parse((this.ListView.Items[1] as DataComponent).HourDisplay);
                var thirdPoint = DateTime.Parse((this.ListView.Items[2] as DataComponent).HourDisplay);
                var diffThirdFirst = thirdPoint - firstPoint;
                var diffThirdSecond = thirdPoint - secondPoint;
                var timeDone = (diffThirdFirst - diffThirdSecond).TotalHours;

                this.dataComponent.EndShiftTime = thirdPoint.AddHours(BASE_SHIFT_TIME - timeDone).ToShortTimeString();
            }
            else
            {
                this.dataComponent.EndShiftTime = String.Empty;
            }
        }

        #region Form Buttons Events
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

        private void AddHourRestry(object sender, RoutedEventArgs e)
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
            ListView.Items.Add(newItem);
            //DataGridList.Items.Add(newItem);
            this.CalculateTimeToLeave();
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
                //DataGridList.Items.RemoveAt(DataGridList.SelectedIndex);
                this.CalculateTimeToLeave();
            }
        }

        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            this.fifteenMinNotificationEndShowed = false;
            this.tenMinNotificationEndShowed = false;
            this.fiveMinNotificationEndShowed = false;
            this.oneMinNotificationEndShowed = false;
            this.fifteenMinNotificationLunchShowed = false;
            this.tenMinNotificationLunchShowed = false;
            this.fiveMinNotificationLunchShowed = false;
            this.oneMinNotificationLunchShowed = false;

            //this.DataGridList.Items.Clear();
            ListView.Items.Clear();
            this.dataComponent.EndShiftTime = String.Empty;
        }
        #endregion

        #region Validation
        private bool CanAddNewResgitry()
        {
            if (this.ListView.Items.Count == 4)
            {
                this.invalidErrorMessage = "You can't add more than 4 registers.";
                return false;
            }
            if (this.ListView.Items.Count > 0)
            {
                var lastItemIndex = this.ListView.Items.Count - 1;
                var item1 = DateTime.Parse((this.ListView.Items[lastItemIndex] as DataComponent).HourDisplay);
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
        #endregion

        #region Handle Minimize, Open and Close Events
        private void OnNotifyIconClick
                    (
                        object sender,
                        System.Windows.Forms.MouseEventArgs e
                    )
        {
            if (e.Button == MouseButtons.Left)
            {
                this.dataComponent.HourDisplay = DateTime.Now.ToShortTimeString();
                this.WindowState = WindowState.Normal;
                this.Show();
            }
        }

        private void OnQuitClick(object sender, EventArgs e)
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
        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //this.ListView.Items.Add(new DataComponent()
            //{
            //    // for five minutes left test
            //    //HourDisplay = DateTime.Now.AddHours(-7).AddMinutes(5).ToShortTimeString(),
            //    // for one minute left test
            //    HourDisplay = DateTime.Now.AddHours(-7).AddMinutes(15).ToShortTimeString(),
            //    bitmap = Properties.Resources.DeleteIcon
            //});
            //this.ListView.Items.Add(new DataComponent()
            //{
            //    HourDisplay = DateTime.Now.AddHours(-3).ToShortTimeString(),
            //    bitmap = Properties.Resources.DeleteIcon
            //});
            //this.ListView.Items.Add(new DataComponent()
            //{
            //    HourDisplay = DateTime.Now.AddHours(-2).ToShortTimeString(),
            //    bitmap = Properties.Resources.DeleteIcon
            //});
            /* 
             ####    TEST 6 HOURS SHIFT    ####
            */
            //this.DataGridList.Items.Add(new DataComponent()
            //{
            //    // for five minutes left test
            //    //HourDisplay = DateTime.Now.AddHours(-7).AddMinutes(5).ToShortTimeString(),
            //    // for one minute left test
            //    HourDisplay = DateTime.Now.AddHours(-7).AddMinutes(1).ToShortTimeString(),
            //    bitmap = Properties.Resources.DeleteIcon
            //});
            //this.DataGridList.Items.Add(new DataComponent()
            //{
            //    HourDisplay = DateTime.Now.AddHours(-3).ToShortTimeString(),
            //    bitmap = Properties.Resources.DeleteIcon
            //});
            //this.DataGridList.Items.Add(new DataComponent()
            //{
            //    HourDisplay = DateTime.Now.AddHours(-2).ToShortTimeString(),
            //    bitmap = Properties.Resources.DeleteIcon
            //});

            this.CalculateTimeToLeave();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(TICK_INTERVAL);
            this.timer.Tick += new EventHandler(this.Timer_Tick);
            this.timer.IsEnabled = true;
        }


        #region Async Thread That Runs The Notification
        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            if (this.ListView.Items.Count == 2)
            {
                var time = DateTime.Parse((this.ListView.Items[1] as DataComponent).HourDisplay);
                var diff = dt.Subtract(time).TotalMinutes;

                if (diff <= 15 && diff > 10 && !fifteenMinNotificationLunchShowed)
                {
                    this.fifteenMinNotificationLunchShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register back from lunch.";
                    this.notifyIcon.BalloonTipTitle = "Register your point";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 10 && diff > 5 && !tenMinNotificationLunchShowed)
                {
                    this.tenMinNotificationLunchShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register back from lunch.";
                    this.notifyIcon.BalloonTipTitle = "Register your point";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 5 && diff > 1 && !fiveMinNotificationLunchShowed)
                {
                    this.fiveMinNotificationLunchShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register back from lunch.";
                    this.notifyIcon.BalloonTipTitle = "Register your point";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 1 && diff > 0 && !oneMinNotificationLunchShowed)
                {
                    this.oneMinNotificationLunchShowed = true;
                    this.notifyIcon.BalloonTipTitle = "Register your point";
                    this.notifyIcon.BalloonTipText = "1 minute to register back from lunch.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
            }
            else if (this.ListView.Items.Count == 1)
            {
                var firstPoint = DateTime.Parse((this.ListView.Items[0] as DataComponent).HourDisplay);
                var diff = firstPoint.AddHours(BASE_SHIFT_TIME).Subtract(dt).TotalMinutes;

                if (diff <= 15 && diff > 10 && !fifteenMinNotificationEndShowed)
                {
                    this.fifteenMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "15 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 10 && diff > 5 && !tenMinNotificationEndShowed)
                {
                    this.tenMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "10 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 5 && diff > 1 && !fiveMinNotificationEndShowed)
                {
                    this.fiveMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 1 && diff > 0 && !oneMinNotificationEndShowed)
                {
                    this.oneMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "1 minute to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
            }
            else if (this.ListView.Items.Count == 3)
            {
                var firstPoint = DateTime.Parse((this.ListView.Items[0] as DataComponent).HourDisplay);
                var secondPoint = DateTime.Parse((this.ListView.Items[1] as DataComponent).HourDisplay);
                var minutesDone = secondPoint.Subtract(firstPoint).TotalMinutes;

                var thirdPoint = DateTime.Parse((this.ListView.Items[2] as DataComponent).HourDisplay);

                var diff = thirdPoint.AddMinutes(360 - minutesDone)
                                    .Subtract(dt)
                                    .TotalMinutes;
                if (diff <= 15 && diff > 10 && !fifteenMinNotificationEndShowed)
                {
                    this.fifteenMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "15 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 10 && diff > 5 && !tenMinNotificationEndShowed)
                {
                    this.tenMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "10 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 5 && diff > 1 && !fiveMinNotificationEndShowed)
                {
                    this.fiveMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "5 minutes to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
                else if (diff <= 1 && diff > 0 && !oneMinNotificationEndShowed)
                {
                    this.oneMinNotificationEndShowed = true;
                    this.notifyIcon.BalloonTipText = "1 minute to register end of journey.";
                    this.notifyIcon.ShowBalloonTip(1000);
                }
            }
        }
        #endregion

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void UpdateShiftTime(object sender, RoutedEventArgs e)
        {
            if ((bool)BtnSixHours.IsChecked)
            {
                BASE_SHIFT_TIME = 6;
            }
            else
            {
                BASE_SHIFT_TIME = 8.48;
            }
            CalculateTimeToLeave();
        }
    }
}
