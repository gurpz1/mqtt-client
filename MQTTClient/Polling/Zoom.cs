using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using MQTTClient.Config;
using MQTTClient.Polling.Models;

namespace MQTTClient.Polling
{
    public class Zoom:MeetingApplicationPoller
    {

        public Zoom(ILogger<Zoom> logger,
            IOptions<Dictionary<string,MeetingApplicationSettings>> configuration) : 
            base(logger, "Zoom", configuration.Value["Zoom"].PollingFrequencySeconds, Availability.FREE)
        {
        }

        protected override void SetState()
        {
            Process[] pnames = Process.GetProcessesByName("CptHost");
            if (pnames.Length > 0)
            {
                _logger.LogDebug("In a meeting.");
                MeetingDetails.Availability =  Availability.BUSY;
            }
            else
            {
                _logger.LogDebug("Not in a meeting");
                MeetingDetails.Availability = Availability.FREE;   
            }
        }

        protected override bool IsInstalled()
        {
            string registryPath = @"Software\Zoom\MSI";
            try
            {
                RegistryKey path = Registry.LocalMachine.OpenSubKey(registryPath);
                var value = path.GetValue("Home");
            }
            catch (Exception e)
            {
                _logger.LogDebug(e.Message);
                _logger.LogWarning("Unable to determine if Zoom was installed. Assuming not");
                return false;
            }

            return true;
        }

        protected override bool CheckIsRunning()
        {
            Process[] pnames = Process.GetProcessesByName("Zoom");
            return pnames.Length > 0;
        }
    }
}