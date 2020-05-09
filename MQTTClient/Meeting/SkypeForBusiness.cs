using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Lync.Model;
namespace MQTTClient.Meeting
{
    public class SkypeForBusiness : MeetingPoller
    {
        protected LyncClient _lyncClient;
        
        public SkypeForBusiness(ILogger<SkypeForBusiness> logger, int pollingFrequeny = 1500)
            : base(logger, "SkypeForBusiness", pollingFrequeny, State.FREE)
        {
            // Stop the background threads.
            Dispose();
            
            if (!CheckIsInstalled())
            {
                _logger.LogInformation($"Listening for events instead for");
            }
        }

        protected override void SetState()
        {
            if(!CheckIsInstalled()) {return;}
            
            // Start listening for events when signed in
            _lyncClient.StateChanged += _lyncClient_StateChanged;
            // Do a sanity check incase they are already signed in
            SubscribeWhensignedIn(_lyncClient.State);
        }
        
        private void _lyncClient_StateChanged(object? sender, ClientStateChangedEventArgs e)
        {
            SubscribeWhensignedIn(e.NewState);
        }

        private void SubscribeWhensignedIn(ClientState lyncClientState)
        {
            if (lyncClientState == ClientState.SignedIn)
            {
                _logger.LogDebug("Lync is signed in");
                var s = _lyncClient.Self.Contact.Uri;
                _lyncClient.Self.Contact.ContactInformationChanged += _lyncClient_ContactInformationChanged;
            }
            else
            {
                _logger.LogDebug($"Lync is {lyncClientState}");
                _lyncClient.Self.Contact.ContactInformationChanged -= _lyncClient_ContactInformationChanged;
            }
        }

        private void _lyncClient_ContactInformationChanged(object? sender, ContactInformationChangedEventArgs e)
        {
            _logger.LogDebug($"User state changed");
            if (e.ChangedContactInformation.Contains(ContactInformationType.Activity) || e.ChangedContactInformation.Contains(ContactInformationType.Availability))
            {
                var activity = _lyncClient.Self.Contact.GetContactInformation(ContactInformationType.ActivityId);
                ContactAvailability availability = (ContactAvailability)_lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
                if (availability == ContactAvailability.Busy && activity.ToString().ToLower() == "on-the-phone")
                {
                    State = State.IN_PROGRESS;
                }
                else
                {
                    State = State.FREE;
                }
            }
        }

        protected override bool CheckIsInstalled()
        {
            try
            {
                _lyncClient = LyncClient.GetClient();
            }
            catch (ClientNotFoundException)
            {
                return false;
            }
            catch (SystemException ex)
            {
                return !IsLyncException(ex);
            }
            _logger.LogDebug($"{ApplicationName} not installed.");
            return true;
        }

        private bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidComObjectException ||
                ex is InvalidCastException;
        }

        protected override bool CheckIsRunning()
        {
            try
            {
                _lyncClient = LyncClient.GetClient();
            }
            catch (NotStartedByUserException)
            {
                return false;
            }
            catch (LyncClientException)
            {
                return false;
            }
            catch (SystemException ex)
            {
                _logger.LogError("Unknown exception occured");
                _logger.LogError(ex.Message);
                return false;
            }
            return true;
        }
    }
}