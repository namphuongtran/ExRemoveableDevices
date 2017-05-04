using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

namespace RDD
{
    public partial class Main : Form
    {
        private DriveDetector driveDetector = null;
        private List<DeviceInfo> devices = null;
        public Main()
        {
            InitializeComponent();
            driveDetector = new DriveDetector();
            devices = new List<DeviceInfo>();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
            driveDetector.QueryRemove += new DriveDetectorEventHandler(OnQueryRemove);
            ScanDevices();
            BindRemovable(devices);
        }

        private void ScanDevices()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                DeviceInfo deviceInfo = new DeviceInfo();
                deviceInfo.Directory = drive.Name;
                deviceInfo.MachineName = Environment.MachineName;
                if (drive.DriveType != DriveType.NoRootDirectory && drive.DriveType != DriveType.CDRom)
                {
                    //deviceInfo.DeviceName = drive.VolumeLabel;
                }

                deviceInfo.Username = Environment.UserName;
                deviceInfo.PluggedInTime = (drive.IsReady && drive.DriveType == DriveType.Removable) ? DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() : "";
                deviceInfo.RemovedTime = "";
                deviceInfo.PluggedInOrRemoved = (drive.IsReady || drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.CDRom) ? "Plugged In" : "Removed";
                deviceInfo.DeviceType = drive.DriveType.ToString();
                devices.Add(deviceInfo);
            }
        }

        private void BindRemovable(List<DeviceInfo> devices)
        {
            string noDeviceAvailable = "No removable devices available";
            int numberOfDevice = devices.Count;
            if (numberOfDevice > 0)
            {
                lblFound.Text = string.Format("{0} devices had been detected.", numberOfDevice);
                dgvDevices.DataSource = null;
                dgvDevices.Update();
                dgvDevices.Refresh();
                dgvDevices.DataSource = devices;
            }
            else
            {
                lblFound.Text = noDeviceAvailable;
            }
        }

        /// <summary>
        /// Export to CSV file
        /// Only CSV comma delimited files are supported 
        /// </summary>
        private void Export2Csv(List<DeviceInfo> devices)
        {
            string noDeviceAvailable = "No removable devices available";
            string filePath = ConfigurationManager.AppSettings["FilePath"];
            string fileName = string.Empty;
            StringBuilder sb = new StringBuilder();
            if (devices.Count > 0)
            {
                //add CSV file header
                sb.Append("USER NAME");
                sb.Append(",");
                sb.Append("MACHINE NAME");
                sb.Append(",");
                //sb.Append("DEVICE NAME");
                //sb.Append(",");
                sb.Append("DIRECTORY");
                sb.Append(",");
                sb.Append("PLUGGED IN TIME");
                sb.Append(",");
                sb.Append("REMOVED TIME");
                sb.Append(",");
                sb.Append("PLUGGED IN OR REMOVED");
                sb.Append(",");
                sb.Append("DEVICE TYPE");

                sb.AppendLine();
                foreach (DeviceInfo item in devices)
                {
                    sb.Append(item.Username);
                    sb.Append(",");
                    sb.Append(item.MachineName);
                    sb.Append(",");
                    //sb.Append(item.DeviceName);
                    //sb.Append(",");
                    sb.Append(item.Directory);
                    sb.Append(",");
                    sb.Append(item.PluggedInTime);
                    sb.Append(",");
                    sb.Append(item.RemovedTime);
                    sb.Append(",");
                    sb.Append(item.PluggedInOrRemoved);
                    sb.Append(",");
                    sb.Append(item.DeviceType);

                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine(noDeviceAvailable);
            }

            string csvData = sb.ToString();
            if (!string.IsNullOrEmpty(csvData))
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fileName = string.Concat(ConfigurationManager.AppSettings["FileName"], "_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".csv");
                string strFileLocation = Path.Combine(filePath, fileName);
                File.WriteAllText(strFileLocation, csvData);
                MessageBox.Show("Export to CSV file had been done, The CSV file was stored following " + strFileLocation + "!");
            }
        }

        /// <summary>
        /// Called by DriveDetector when removable device in inserted 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            // e.Drive is the drive letter for the device which just arrived, e.g. "E:\\"
            string directory = e.Drive;
            DeviceInfo deviceInfo = new DeviceInfo();
            deviceInfo.Directory = directory;
            deviceInfo.MachineName = Environment.MachineName;
            //deviceInfo.DeviceName = "";
            deviceInfo.Username = Environment.UserName;
            deviceInfo.PluggedInTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
            deviceInfo.RemovedTime = "";
            deviceInfo.PluggedInOrRemoved = "Plugged In";
            deviceInfo.DeviceType = "Removable";
            devices.Add(deviceInfo);
            BindRemovable(devices);
        }

        /// <summary>
        /// Called by DriveDetector after removable device has been unpluged 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            string directory = e.Drive;
            DeviceInfo deviceInfo = new DeviceInfo();
            deviceInfo.Directory = directory;
            deviceInfo.MachineName = Environment.MachineName;
            //deviceInfo.DeviceName = "";
            deviceInfo.Username = Environment.UserName;
            deviceInfo.PluggedInTime = "";
            deviceInfo.RemovedTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
            deviceInfo.PluggedInOrRemoved = "Removed";
            deviceInfo.DeviceType = "Removable";
            devices.Add(deviceInfo);
            BindRemovable(devices);
        }

        /// <summary>
        /// Called by DriveDetector when removable drive is about to be removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnQueryRemove(object sender, DriveDetectorEventArgs e)
        {

        }

        /// <summary>
        /// Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Export to csv file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, EventArgs e)
        {
            Export2Csv(devices);
        }
    }
}