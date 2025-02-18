using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nutupsdrv
{
    /// <summary>
    /// A class for updating registry values for a UPS.
    /// </summary>
    /// <remarks>
    /// https://learn.microsoft.com/en-us/windows-hardware/drivers/battery/ups-status-registry-entries
    /// </remarks>
    class UpsRegistry : IDisposable
    {
        private RegistryKey regKey;

        public UpsRegistry()
        {
            using var rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            regKey = rootKey.OpenSubKey("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\UPS\\Status", true);
            // Set default values.
            BatteryCapacity = 100;
            BatteryStatus = BatteryStatusValues.Unknown;
            CommStatus = CommStatusValues.Unknown;
            FirmwareRev = string.Empty;
            SerialNumber = string.Empty;
            TotalUPSRuntime = 10;
            UtilityPowerStatus = UtilityPowerStatusValues.Unknown;
        }

        /// <summary>
        /// Gets or sets the percent of battery capacity remaining in the UPS. 
        /// This percent is represented as a value in the range of 0 through 100. (The displayed value is rounded to the nearest integer.)
        /// </summary>
        public UInt32 BatteryCapacity
        {
            get => Convert.ToUInt32(regKey.GetValue("BatteryCapacity", 0));
            set => regKey.SetValue("BatteryCapacity", value, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Values for BatteryStatus.
        /// </summary>
        public enum BatteryStatusValues : UInt32
        {
            Unknown = 0,
            OK = 1,
            NeedReplaced = 2
        }

        /// <summary>
        /// Gets or sets the current status of the UPS batteries. 
        /// </summary>
        public BatteryStatusValues BatteryStatus
        {
            get => (BatteryStatusValues)Convert.ToUInt32(regKey.GetValue("BatteryStatus", 0));
            set => regKey.SetValue("BatteryStatus", value, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Values for CommStatus.
        /// </summary>
        public enum CommStatusValues : UInt32
        {
            Unknown = 0,
            OK = 1,
            Lost = 2
        }

        /// <summary>
        /// Gets or sets the status of the communication path to the UPS. 
        /// </summary>
        public CommStatusValues CommStatus
        {
            get => (CommStatusValues)Convert.ToUInt32(regKey.GetValue("CommStatus", 0));
            set => regKey.SetValue("CommStatus", value, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Gets or sets the UPS firmware revision as a displayable string.
        /// </summary>
        public string FirmwareRev
        {
            get => regKey.GetValue("FirmwareRev", string.Empty).ToString();
            set => regKey.SetValue("FirmwareRev", value, RegistryValueKind.String);
        }

        /// <summary>
        /// Gets or sets the UPS serial number as a displayable string.
        /// </summary>
        public string SerialNumber
        {
            get => regKey.GetValue("SerialNumber", string.Empty).ToString();
            set => regKey.SetValue("SerialNumber", value, RegistryValueKind.String);
        }


        /// <summary>
        /// Gets or sets the amount of remaining UPS run time, in minutes.
        /// </summary>
        public UInt32 TotalUPSRuntime
        {
            get => Convert.ToUInt32(regKey.GetValue("TotalUPSRuntime", 0));
            set => regKey.SetValue("TotalUPSRuntime", value, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Values for UtilityPowerStatus.
        /// </summary>
        public enum UtilityPowerStatusValues : UInt32
        {
            Unknown = 0,
            OK = 1,
            Failure = 2
        }

        /// <summary>
        /// Gets or sets the status of utility-supplied power into the UPS.
        /// </summary>
        public UtilityPowerStatusValues UtilityPowerStatus
        {
            get => (UtilityPowerStatusValues)Convert.ToUInt32(regKey.GetValue("UtilityPowerStatus", 0));
            set => regKey.SetValue("UtilityPowerStatus", value, RegistryValueKind.DWord);
        }



        private volatile bool _disposed;
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    regKey.Close();
                    regKey.Dispose();
                    regKey = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
