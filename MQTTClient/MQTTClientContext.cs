using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ApplicationPoller;
using ApplicationPoller.Meeting;
using ApplicationPoller.Meeting.Apps;
using AuraLight.Models;
using Microsoft.Extensions.Logging;
using MQTTClient.Mqtt;
using MQTTClient.Mqtt.Handlers;
using MQTTClient.Mqtt.Messages;
using Application = System.Windows.Forms.Application;

namespace MQTTClient
{
    public class MQTTClientContext: ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ILogger _logger;
        
        private readonly IMqttClientFacade _mqttClientFacade;

        public MQTTClientContext(ILogger<MQTTClientContext> logger, 
            IMqttClientFacade mqttClientFacade,
            IEnumerable<IMeetingApplicationPoller> meetingApplicationPollers,
            ICommandTriager commandTriager)
        {
            _logger = logger;
            _mqttClientFacade = mqttClientFacade;

            var contextMenu = new ContextMenuStrip();

            _trayIcon = new NotifyIcon()
            {
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = "MQTT Client",
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
            };

            // Add minimum required buttons
            AddConnectionStatus(_mqttClientFacade);
            AddExitButton();
            ShowLaunchBaloon();
            
            // Start MQTT client
            _mqttClientFacade.OnMessage(commandTriager.BaseTopic, commandTriager.OnReceive);
            _mqttClientFacade.Start();
            
            // Register all meeting pollers
            foreach (IMeetingApplicationPoller meetingApplicationPoller in meetingApplicationPollers)
            {
                AddMeetingStatus(meetingApplicationPoller); 
                meetingApplicationPoller.StartPolling();
            }
        }
        #region Meeting Status Stuff
        /// <summary>
        /// Adds the meting statuses to the tray icon and subscribes to change in meeting status.
        /// </summary>
        /// <param name="meetingApplicationPoller"></param>
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
                
                var message = new MeetingStatusMessage(
                    _mqttClientFacade.ConnectionSettings.ClientID, application, meetingDetails);
                _mqttClientFacade.Publish(message);
            };

            _trayIcon.ContextMenuStrip.Items.Insert(1,statusItem);
        }
        /// <summary>
        /// Gets the text to put on the status tray icon
        /// </summary>
        /// <param name="application"></param>
        /// <param name="meetingDetails"></param>
        /// <returns></returns>
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
        
        #region Connection Stuff
        /// <summary>
        /// Adds connection status text
        /// </summary>
        /// <param name="mqttClientFacade"></param>
        private void AddConnectionStatus(IMqttClientFacade mqttClientFacade)
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
        #endregion

        private void ShowLaunchBaloon()
        {
            _trayIcon.BalloonTipIcon= ToolTipIcon.Info;

            _trayIcon.BalloonTipText = $"Connecting to {_mqttClientFacade.ConnectionSettings.BrokerURL}";
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