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
    public class Webex:MeetingApplicationPoller
    {

        public Webex(ILogger<Webex> logger,
            IOptions<Dictionary<string,MeetingApplicationSettings>> configuration) : 
            base(logger, "WebEx", configuration.Value["WebEx"].PollingFrequencySeconds, Availability.FREE)
        {
        }

        protected override void SetState()
        {
            Process[] pnames = Process.GetProcessesByName("webexmta");
            if (pnames.Length > 1)
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
            string registryPath = @"Software\WebEx\Config";
            try
            {
                RegistryKey path = Registry.CurrentUser.OpenSubKey(registryPath);
                var value = path.GetValue("IsRecordAudio");
                return true;
            }
            catch (NullReferenceException)
            {
                // entirely expected if not found
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message);
            }
            _logger.LogWarning($"Unable to determine if Webex was installed. Assuming not");
            return false;
        }

        protected override bool CheckIsRunning()
        {
            Process[] pnames = Process.GetProcessesByName("webexmta");
            return pnames.Length > 0;
        }
    }
}