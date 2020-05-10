using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTClient.Polling.Models;

namespace MQTTClient.Polling
{
    public abstract class ApplicationPoller:IApplicationPoller
    {
        #region Public Properties

        public IApplicationMetadata ApplicationMetadata { get; }
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
        /// <param name="pollingFrequency"></param>
        protected ApplicationPoller(ILogger logger, string applicationName, int pollingFrequency)
        {
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
            
            ApplicationMetadata = new ApplicationMetadata(applicationName, IsInstalled());
            PollingFrequency = pollingFrequency*1000;

            if (!ApplicationMetadata.IsInstalled)
            {
                _logger.LogError($"{ApplicationMetadata.ApplicationName} is not installed. Halting poller.");
                return;
            }

            _logger.LogInformation($"Starting to poll {ApplicationMetadata.ApplicationName} every {pollingFrequency}s");
        }

        public void StartPolling()
        {
            if (ApplicationMetadata.IsInstalled)
            {
                _task = Task.Run(async () =>
                {
                    while (!_token.IsCancellationRequested)
                    {
                        if (!CheckIsRunning())
                        {
                            _logger.LogDebug($"{ApplicationMetadata.ApplicationName} is not running. Will continue to poll");
                        }
                        else
                        {
                            SetState();
                        }
                    
                        await Task.Delay(PollingFrequency, _token);
                    }
                }, _token);   
            }
        }

        public void StopPolling()
        {
            _cancellationTokenSource.Cancel();
        }

        protected abstract void SetState();
        protected abstract bool IsInstalled();
        protected abstract bool CheckIsRunning();

        public void Dispose()
        {
            _logger.LogInformation($"Stopping {ApplicationMetadata.ApplicationName} polling");
            StopPolling();
        }
    }
}