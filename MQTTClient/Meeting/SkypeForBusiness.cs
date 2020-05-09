using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Lync.Model;
using Microsoft.Win32;

namespace MQTTClient.Meeting
{
    public class SkypeForBusiness : MeetingPoller
    {
        private LyncClient _lyncClient;
        
        public SkypeForBusiness(ILogger<SkypeForBusiness> logger, int pollingFrequeny = 1500)
            : base(logger, "SkypeForBusiness", pollingFrequeny, State.FREE)
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
                    MeetingDetails.State = State.IN_PROGRESS;
                }
                MeetingDetails.State = State.FREE;
            }
        }

        protected override bool IsInstalled()
        {
            IDictionary<string,string> _lyncVersions = new Dictionary<string, string>()
            {
                {@"SOFTWARE\Wow6432Node\Microsoft\Office\16.0\Registration\{03CA3B9A-0869-4749-8988-3CBC9D9F51BB}","x86 Skype for Business 2016" },
                {@"SOFTWARE\Microsoft\Office\16.0\Registration\{03CA3B9A-0869-4749-8988-3CBC9D9F51BB}", "x64 Skype for Business 2016"}
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
                catch
                {
                    _logger.LogDebug($"{lyncVersion.Value} not found");
                }
            }

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