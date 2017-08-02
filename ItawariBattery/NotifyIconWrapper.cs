using System;
using System.ComponentModel;
using System.Management;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace ItawariBattery
{
    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper()
        {
            InitializeComponent();

            toolStripMenuItem_Exit.Click += ToolStripMenuItem_Exit_Click;
            toolStripMenuItem_ShowBatteryInfo.Click += ToolStripMenuItem_ShowBatteryInfo_Click;

            CheckBatteryState(null,null);

            var dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal) {Interval = new TimeSpan(0, 0, 1, 0, 0)};
            dispatcherTimer.Tick += CheckBatteryState;
            dispatcherTimer.Start();
        }

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        private static void ToolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private static void ToolStripMenuItem_ShowBatteryInfo_Click(object sender, EventArgs e)
        {
            Window batteryInfomationWindow = new BatteryInformationWindow();
            batteryInfomationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            batteryInfomationWindow.Show();
        }


        public void CheckBatteryState(object sender, EventArgs e)
        {

            if (IsPluggedAcAdapter())
            {
                if (GetButteryPercentage() >= 80)
                {
                    MessageBox.Show("The battery has been charged to 80% or more.",
                        "Battery notification",
                        MessageBoxButton.OK, MessageBoxImage.Warning,
                        MessageBoxResult.OK,
                        MessageBoxOptions.ServiceNotification);
                }
            }
        }

        /// <summary>
        /// バッテリーの現在の残容量をパーセントで取得する。
        /// </summary>
        /// <returns>バッテリーの残容量(単位：%)</returns>
        public int GetButteryPercentage()
        {
            var managementClass = new ManagementClass("Win32_Battery");
            var managementObject = managementClass.GetInstances();
            var butteryPercentage = 0;

            foreach (var o in managementObject)
            {
                var mo = (ManagementObject) o;
                butteryPercentage = Convert.ToInt32(mo["EstimatedChargeRemaining"]);
            }

            managementClass.Dispose();
            managementObject.Dispose();

            return butteryPercentage;
        }

        /// <summary>
        /// ACアダプタが接続されているか確認する。(充電中もしくはAC電源)
        /// </summary>
        /// <returns>ACアダプタが接続されているとき trure </returns>
        public bool IsPluggedAcAdapter()
        {
            var managementClass = new ManagementClass("Win32_Battery");
            var managementObject = managementClass.GetInstances();
            var chargeStatuse = 0;

            foreach (var o in managementObject)
            {
                var mo = (ManagementObject) o;
                chargeStatuse = Convert.ToInt16(mo["BatteryStatus"]);
            }

            managementClass.Dispose();
            managementObject.Dispose();

            if (chargeStatuse == 2 || chargeStatuse == 6)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}