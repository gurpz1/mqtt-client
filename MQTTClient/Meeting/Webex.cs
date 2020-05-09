using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace MQTTClient.Meeting
{
    public class Webex:MeetingPoller
    {

        public Webex(ILogger<Webex> logger, int pollingFrequency = 1500) : 
            base(logger, "WebEx", pollingFrequency, State.FREE)
        {
        }

        protected override void SetState()
        {
            Process[] pnames = Process.GetProcessesByName("webexmta");
            if (pnames.Length > 1)
            {
                _logger.LogDebug("In a meeting.");
                MeetingDetails.State =  State.IN_PROGRESS;
            }
            else
            {
                _logger.LogDebug("Not in a meeting");
                MeetingDetails.State = State.FREE;   
            }
        }

        protected override bool IsInstalled()
        {
            string registryPath = @"Software\WebEx\Config";
            try
            {
                RegistryKey path = Registry.CurrentUser.OpenSubKey(registryPath);
                var value = path.GetValue("IsRecordAudio");
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected override bool CheckIsRunning()
        {
            Process[] pnames = Process.GetProcessesByName("webexmta");
            return pnames.Length > 0;
        }
    }
}