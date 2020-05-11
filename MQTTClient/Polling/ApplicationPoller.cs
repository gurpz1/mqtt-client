using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTClient.Polling.Models;

namespace MQTTClient.Polling
{
    public abstract class ApplicationPoller:IApplicationPoller
    {
        #region Public Properties

        public IApplication Application { get; }
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
            
            Application = new Application(applicationName, IsInstalled());
            PollingFrequency = pollingFrequency*1000;

            if (!Application.IsInstalled)
            {
                _logger.LogWarning($"{Application.ApplicationName} is not installed. Halting poller.");
                return;
            }

            _logger.LogInformation($"Starting to poll {Application.ApplicationName} every {pollingFrequency}s");
        }

        public void StartPolling()
        {
            if (Application.IsInstalled)
            {
                _logger.LogInformation($"Starting to poll {Application.ApplicationName}");
                _task = Task.Run(async () =>
                {
                    while (!_token.IsCancellationRequested)
                    {
                        if (!CheckIsRunning())
                        {
                            _logger.LogDebug($"{Application.ApplicationName} is not running. Will continue to poll");
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
        
        

        public virtual void Dispose()
        {
            _logger.LogInformation($"Stopping {Application.ApplicationName} polling");
            StopPolling();
        }
    }
}