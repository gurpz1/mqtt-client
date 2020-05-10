using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Config;
using MQTTClient.Mqtt;
using MQTTClient.Polling;
using MQTTClient.Polling.Models;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

namespace MQTTClient
{
    public class MQTTClientContext: ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ILogger _logger;
        
        private IManagedClientFacade _mqttMangedClientFacade;
        
        #region Supported Meeting Apps
        private IMeetingApplicationPoller _webex;
        private IMeetingApplicationPoller _lync;
        #endregion

        public MQTTClientContext(ILogger<MQTTClientContext> logger, 
            ManagedClientFacade mqttClientFacade,
            Webex webex,
            Lync lync)
        {
            _logger = logger;
            _mqttMangedClientFacade = mqttClientFacade;

            // Meeting apps
            _webex = webex;
            _lync = lync;

            var contextMenu = new ContextMenuStrip();

            _trayIcon = new NotifyIcon()
            {
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = "MQTT Client",
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
            };

            // Add minimum required buttons
            AddConnectionStatus(_mqttMangedClientFacade);
            AddExitButton();
            ShowLaunchBaloon();

            // Add stuff you want to monitor
            AddMeetingStatus(_webex);
            AddMeetingStatus(_lync);
            
            // Start the connection and polling
            _mqttMangedClientFacade.MqttClient.StartAsync(_mqttMangedClientFacade.ClientOptions);
            _webex.StartPolling();
            _lync.StartPolling();
        }
        #region Meeting Status Stuff
        private void AddMeetingStatus(IMeetingApplicationPoller meetingApplicationPoller)
        {
            _logger.LogDebug($"Adding {meetingApplicationPoller.ApplicationMetadata.ApplicationName} status to menu item");
            var statusItem = new ToolStripMenuItem();
            statusItem.Enabled = false;
            statusItem.Name = $"{meetingApplicationPoller.ApplicationMetadata.ApplicationName}_satus_item";
            statusItem.Text = GetMeetingApplicationStatusText(meetingApplicationPoller.ApplicationMetadata, meetingApplicationPoller.MeetingDetails);
            
            // Subscribe to change events 
            meetingApplicationPoller.MeetingDetails.PropertyChanged += (receivedMeetingDetails, args) =>
            {
                var meetingDetails = (MeetingDetails) receivedMeetingDetails;
                _logger.LogDebug($"Status Changed to {meetingDetails.MeetingState.ToString()}");
                statusItem.Text = GetMeetingApplicationStatusText(meetingApplicationPoller.ApplicationMetadata, meetingDetails);
                SendMqttMessage(meetingApplicationPoller.ApplicationMetadata, meetingDetails);
            };

            _trayIcon.ContextMenuStrip.Items.Insert(1,statusItem);
        }

        private void SendMqttMessage(IApplicationMetadata applicationMetadata, MeetingDetails meetingDetails)
        {
            var payload = $"{meetingDetails.MeetingState}";
            var message = new MqttApplicationMessage()
            {
                Topic = new MeetingMessage(applicationMetadata, meetingDetails,
                    _mqttMangedClientFacade.ClientOptions.ClientOptions.ClientId).Topic,
                Payload = Encoding.UTF8.GetBytes(payload),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            };
            _logger.LogDebug(payload);
            _mqttMangedClientFacade.MqttClient.PublishAsync(message);
        }

        private string GetMeetingApplicationStatusText(IApplicationMetadata applicationMetadata, IMeetingDetails meetingDetails)
        {
            if (!applicationMetadata.IsInstalled)
            {
                return $"{applicationMetadata.ApplicationName} not installed";
            }
            var availableMessage = $"{applicationMetadata.ApplicationName} available";
            var busyMessage = $"{applicationMetadata.ApplicationName} in meeting";
            switch (meetingDetails.MeetingState)
            {
                case (MeetingState.FREE):
                    return availableMessage;
                case (MeetingState.BUSY):
                    return busyMessage;
            }

            return $"{applicationMetadata.ApplicationName} unknown state";
        }
        #endregion
        private void AddConnectionStatus(IManagedClientFacade mqttClientFacade)
        {
            _logger.LogDebug("Adding MQTT connection status menu item");
            var statusItem = new ToolStripMenuItem();
            statusItem.Enabled = false;
            statusItem.Text = "Unknown Status";
            statusItem.Name = "connection_satus_item";

            _logger.LogDebug("Subscribing MQTT connection status events");
            mqttClientFacade.MqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(x =>
            {
                _logger.LogInformation($"Connected to {mqttClientFacade.BrokerUrl} as {mqttClientFacade.ClientOptions.ClientOptions.ClientId} with {mqttClientFacade.MqttClient.Options.ClientOptions.Credentials.Username}");
                statusItem.Text = $"Connected to {mqttClientFacade.BrokerUrl}";
            });

            mqttClientFacade.MqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(x =>
            {
                _logger.LogInformation($"Disconnected from {mqttClientFacade.BrokerUrl}");
                statusItem.Text = $"Disconnected from {mqttClientFacade.BrokerUrl}";
            });
            
            _trayIcon.ContextMenuStrip.Items.Add(statusItem);
        }

        private void ShowLaunchBaloon()
        {
            _trayIcon.BalloonTipIcon= ToolTipIcon.Info;

            _trayIcon.BalloonTipText = $"Connecting to {_mqttMangedClientFacade.BrokerUrl}";
            _trayIcon.BalloonTipTitle = "Home Status Notifier";
            _trayIcon.ShowBalloonTip(1500);
        }
        
        #region Exit Button
        private void AddExitButton()
        {
            var exitItem = new ToolStripMenuItem();
            exitItem.Name = "exit_item";
            exitItem.Text = "Exit";
            exitItem.Click += exitItem_Click;

            _trayIcon.ContextMenuStrip.Items.Add(exitItem);
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Exit button clicked");
            _webex.Dispose();
            _lync.Dispose();
            _trayIcon.Visible = false;
            Application.Exit();
        }
        #endregion
    }
}