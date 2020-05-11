using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Lync.Model;
using Microsoft.Win32;

namespace ApplicationPoller.Meeting.Apps
{
    public class Lync : MeetingApplicationPoller
    {
        private LyncClient _lyncClient;
        
        public Lync(ILogger<Lync> logger,
            MeetingApplicationSettings configuration)
            : base(logger, "Lync", configuration.PollingFrequencySeconds, Availability.FREE)
        {
        }

        protected override void SetState()
        {
            _logger.LogDebug($"User currently {_lyncClient.State}");
            if (_lyncClient.State == ClientState.SignedIn)
            {
                var activity = _lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Activity);
                _logger.LogDebug($"User is {activity}");
                if(activity.ToString() == "In a call")
                {
                    MeetingDetails.Availability = Availability.BUSY;
                }
                MeetingDetails.Availability = Availability.FREE;
            }
        }

        protected override bool IsInstalled()
        {
            IDictionary<string,string> _lyncVersions = new Dictionary<string, string>()
            {
                {@"SOFTWARE\Microsoft\Office\16.0\Registration\{03CA3B9A-0869-4749-8988-3CBC9D9F51BB}", "x64 Skype for Business 2016"},
                {@"SOFTWARE\Wow6432Node\Microsoft\Office\16.0\Registration\{03CA3B9A-0869-4749-8988-3CBC9D9F51BB}","x86 Skype for Business 2016" },
                {@"SOFTWARE\Microsoft\Office\15.0\Registration\{0EA305CE-B708-4D79-8087-D636AB0F1A4D}", "x86 Microsoft Lync 2013"},
                {@"SOFTWARE\Wow6432Node\Microsoft\Office\15.0\Registration\{0EA305CE-B708-4D79-8087-D636AB0F1A4D}", "x64 Microsoft Lync 2013"}
            };
            foreach (var lyncVersion in _lyncVersions)
            {
                try
                {
                    RegistryKey path = Registry.LocalMachine.OpenSubKey(lyncVersion.Key);
                    var value = path.GetValue("ProductName");
                    _logger.LogInformation($"Found {value}");
                    return true;
                }
                catch (NullReferenceException)
                {
                    // Entirely Expected
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                }
                _logger.LogInformation($"{lyncVersion.Value} not found");
            }
            _logger.LogWarning("Unable to determine if Lync was installed. Assuming not");
            return false;
        }
        
        protected override bool CheckIsRunning()
        {
            try
            {
                _lyncClient = LyncClient.GetClient();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            return true;
        }
    }
}