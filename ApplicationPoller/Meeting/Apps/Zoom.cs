using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace ApplicationPoller.Meeting.Apps
{
    public class Zoom:MeetingApplicationPoller
    {

        public Zoom(ILogger<Zoom> logger,
            MeetingApplicationSettings configuration) : 
            base(logger, "Zoom", configuration.PollingFrequencySeconds, Availability.FREE)
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
            // First search for user local
            try
            {
                var zoomPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Zoom", "bin", "zoom.exe");
                _logger.LogDebug("Checking if installed in default location");
                if (File.Exists(zoomPath)) return true;
                throw new FileNotFoundException("Not found installed in %APPDATA%");
            }
            catch(Exception e)
            {
                _logger.LogWarning(e.Message);
            }

            // Then search registry
            string registryPath = @"SOFTWARE\Policies\Zoom\Zoom Meetings\General";
            try
            {
                _logger.LogInformation("Checking if installed via registry");
                using(var key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key == null)
                    {
                        return false;
                    }
                }
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
            _logger.LogWarning($"Unable to determine if Zoom was installed. Assuming not");
            return false;
        }

        protected override bool CheckIsRunning()
        {
            Process[] pnames = Process.GetProcessesByName("Zoom");
            return pnames.Length > 0;
        }
    }
}