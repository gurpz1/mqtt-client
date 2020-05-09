using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MQTTClient.Meeting
{
    public abstract class MeetingPoller:IMeetingPoller
    {
        #region Public Properties

        public IMeetingApplication MeetingApplication { get; }
        public IMeetingDetails MeetingDetails { get;}
        public int PollingFrequency{get;} 
        #endregion

        #region Private Properties
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _token;
        private Task _task;
        #endregion

        protected ILogger _logger;

        /// <summary>
        /// Polls for Meeting Status
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="applicationName"></param>
        /// <param name="pollingFrequeny"></param>
        /// <param name="initialMeetingState"></param>
        protected MeetingPoller(ILogger logger, string applicationName, int pollingFrequeny, State initialMeetingState)
        {
            MeetingApplication = new MeetingApplication(applicationName, IsInstalled());
            PollingFrequency = pollingFrequeny;
            MeetingDetails = new MeetingDetails()
            {
                State = initialMeetingState
            };
            
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
            _logger = logger;
            
            if (!MeetingApplication.IsInstalled)
            {
                _logger.LogError($"{MeetingApplication.ApplicationName} is not installed. Halting poller.");
                return;
            }

            _logger.LogInformation($"Starting {MeetingApplication.ApplicationName} polling");

            _task = Task.Run(async () =>
            {
                while (!_token.IsCancellationRequested)
                {
                    if (!CheckIsRunning())
                    {
                        _logger.LogDebug($"{MeetingApplication.ApplicationName} is not running. Will continue to poll");
                    }
                    else
                    {
                        SetState();
                    }
                    
                    await Task.Delay(pollingFrequeny, _token);
                }
            }, _token);
            _logger.LogInformation($"Started");
        }
        
        protected abstract void SetState();
        protected abstract bool IsInstalled();
        protected abstract bool CheckIsRunning();

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _logger.LogInformation($"Stopping {MeetingApplication.ApplicationName} polling");
        }
    }
}