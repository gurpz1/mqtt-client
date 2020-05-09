using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MQTTClient.Meeting
{
    public abstract class MeetingPoller:IMeetingPoller,IDisposable
    {
        public string ApplicationName { get; private set; }
        public State State { get; protected set; }
        public int PollingFrequency{get; set; }
        
        protected ILogger _logger;

        // Private, not shared
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _token;
        private Task _task;

        /// <summary>
        /// Polls for Meeting Status
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="applicationName"></param>
        /// <param name="pollingFrequeny"></param>
        /// <param name="initialMeetingState"></param>
        protected MeetingPoller(ILogger logger,
            string applicationName,
            int pollingFrequeny,
            State initialMeetingState)
        {
            ApplicationName = applicationName;
            PollingFrequency = pollingFrequeny;
            State = initialMeetingState;
            
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger;

            if (!CheckIsInstalled())
            {
                _logger.LogError($"{ApplicationName} is not installed. Halting poller.");
                return;
            }
            _logger.LogInformation($"Starting {ApplicationName} polling");
            
            _token = _cancellationTokenSource.Token;

            _task = Task.Run(async () =>
            {
                while (!_token.IsCancellationRequested)
                {
                    if (!CheckIsRunning())
                    {
                        _logger.LogDebug($"{ApplicationName} is not running. Will continue to poll");
                    }
                    else
                    {
                        SetState();
                    }
                    
                    await Task.Delay(pollingFrequeny, _token);
                }
            }, _token);
        }
        
        protected abstract void SetState();
        protected abstract bool CheckIsInstalled();
        protected abstract bool CheckIsRunning();

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _logger.LogInformation($"Stopping {ApplicationName} polling");
        }
    }
}