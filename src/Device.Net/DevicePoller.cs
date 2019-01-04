﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using timer = System.Timers.Timer;

namespace Device.Net.UWP
{
    public class DevicePoller
    {
        #region Fields
        private readonly timer _PollTimer;
        private readonly SemaphoreSlim _PollingSemaphoreSlim = new SemaphoreSlim(1, 1);
        private Dictionary<VidPid, IDevice> _RegisteredDevices { get; } = new Dictionary<VidPid, IDevice>();
        #endregion

        #region Public Properties
        public uint? ProductId { get; }
        public uint? VendorId { get; }
        #endregion

        #region Constructor
        public DevicePoller(uint? productId, uint? vendorId, int pollMilliseconds)
        {
            _PollTimer = new timer(pollMilliseconds);
            _PollTimer.Elapsed += _PollTimer_Elapsed;
            _PollTimer.Start();
            ProductId = productId;
            VendorId = vendorId;
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await _PollingSemaphoreSlim.WaitAsync();

                var deviceInformations = await DeviceManager.Current.GetConnectedDeviceDefinitions(VendorId, ProductId);

                foreach (var deviceInformation in deviceInformations)
                {
                    //foreach (var )
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(DevicePoller));

                //Throw?
            }
            finally
            {
                _PollingSemaphoreSlim.Release();
            }
        }
        #endregion

        #region Public Methods
        public void RegisterDevice(string vendorId, string productId, IDevice device)
        {
            if (string.IsNullOrEmpty(vendorId) && string.IsNullOrEmpty(productId)) throw new ArgumentNullException();

            if (device == null) throw new ArgumentNullException(nameof(device));

            var vidPid = new VidPid { Vid = vendorId, Pid = productId };

            if (_RegisteredDevices.ContainsKey(vidPid)) throw new Exception("Vendor/Product Id combination already registered");

            _RegisteredDevices.Add(vidPid, device);
        }

        public void Stop()
        {
            _PollTimer.Stop();
        }
        #endregion
    }

    internal class VidPid
    {
        public string Vid { get; set; }
        public string Pid { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VidPid vidPid)
            {
                if (string.IsNullOrEmpty(vidPid.Pid) && string.IsNullOrEmpty(vidPid.Vid))
                {
                    return false;
                }

                return vidPid.Vid == Vid && vidPid.Pid == Pid;
            }

            return false;
        }
    }
}
