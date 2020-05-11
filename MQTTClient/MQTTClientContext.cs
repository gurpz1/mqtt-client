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
using Application = System.Windows.Forms.Application;

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
        private IMeetingApplicationPoller _zoom;
        #endregion

        public MQTTClientContext(ILogger<MQTTClientContext> logger, 
            ManagedClientFacade mqttClientFacade,
            Webex webex,
            Lync lync,
            Zoom zoom)
        {
            _logger = logger;
            _mqttMangedClientFacade = mqttClientFacade;

            // Meeting apps
            _webex = webex;
            _lync = lync;
            _zoom = zoom;

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
            AddMeetingStatus(_zoom);
            
            // Start the connection and polling
            _mqttMangedClientFacade.Start();
            _webex.StartPolling();
            _lync.StartPolling();
            _zoom.StartPolling();
        }
        #region Meeting Status Stuff
        private void AddMeetingStatus(IMeetingApplicationPoller meetingApplicationPoller)
        {
            _logger.LogDebug($"Adding {meetingApplicationPoller.Application.ApplicationName} status to menu item");
            var statusItem = new ToolStripMenuItem();
            statusItem.Enabled = false;
            statusItem.Name = $"{meetingApplicationPoller.Application.ApplicationName}_satus_item";
            statusItem.Text = GetMeetingApplicationStatusText(meetingApplicationPoller.Application, meetingApplicationPoller.MeetingDetails);
            
            // Subscribe to change events 
            meetingApplicationPoller.MeetingDetails.PropertyChanged += (receivedMeetingDetails, args) =>
            {
                var meetingDetails = (MeetingDetails) receivedMeetingDetails;
                var application = meetingApplicationPoller.Application;
                statusItem.Text = GetMeetingApplicationStatusText(application, meetingDetails);
                
                var message = MqttMessage.GenerateMqttMessageForMeetingStatus(
                    _mqttMangedClientFacade.ConnectionSettings.ClientID, application, meetingDetails);
                _mqttMangedClientFacade.Publish(message);
            };

            _trayIcon.ContextMenuStrip.Items.Insert(1,statusItem);
        }

        private string GetMeetingApplicationStatusText(IApplication application, IMeetingDetails meetingDetails)
        {
            if (!application.IsInstalled)
            {
                return $"{application.ApplicationName} not installed";
            }
            var availableMessage = $"{application.ApplicationName} available";
            var busyMessage = $"{application.ApplicationName} in meeting";
            switch (meetingDetails.Availability)
            {
                case (Availability.FREE):
                    return availableMessage;
                case (Availability.BUSY):
                    return busyMessage;
            }

            return $"{application.ApplicationName} unknown state";
        }
        #endregion
        private void AddConnectionStatus(IManagedClientFacade mqttClientFacade)
        {
            _logger.LogDebug("Adding MQTT connection status menu item");
            var statusItem = new ToolStripMenuItem();
            statusItem.Enabled = false;
            statusItem.Text = "Unknown Status";
            statusItem.Name = "connection_satus_item";
            
            mqttClientFacade.OnConnected(() => statusItem.Text = $"Connected to {mqttClientFacade.ConnectionSettings.BrokerURL}");
            mqttClientFacade.OnDisconnected(() =>statusItem.Text = $"Disconnected from {mqttClientFacade.ConnectionSettings.BrokerURL}");
            
            _trayIcon.ContextMenuStrip.Items.Add(statusItem);
        }

        private void ShowLaunchBaloon()
        {
            _trayIcon.BalloonTipIcon= ToolTipIcon.Info;

            _trayIcon.BalloonTipText = $"Connecting to {_mqttMangedClientFacade.ConnectionSettings.BrokerURL}";
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
            _trayIcon.Visible = false;
            Application.Exit();
        }
        #endregion
    }
}