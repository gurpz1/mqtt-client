﻿using System;
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
                State =  State.IN_PROGRESS;
            }
            else
            {
                _logger.LogDebug("Not in a meeting");
                State = State.FREE;   
            }
        }

        protected override bool CheckIsInstalled()
        {
            string registryPath = @"Software\WebEx\Uninstall\Online";
            try
            {
                RegistryKey path = Registry.CurrentUser.OpenSubKey(registryPath);
                string value =(string) path.GetValue("Meetings");
                return value == "Installed";
            }
            catch
            {
                return false;
            }
        }

        protected override bool CheckIsRunning()
        {
            Process[] pnames = Process.GetProcessesByName("webexmta");
            return pnames.Length > 0;
        }
    }
}