using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Meeting;
using MQTTClient.Mqtt;

namespace MQTTClient
{
    public class MQTTClientContext: ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ILogger _logger;
        private IConnectionSettings _connectionSettings;
        
        #region Supported Meeting Apps
        private Webex _webex;
        private SkypeForBusiness _skypeForBusiness;
        #endregion

        public MQTTClientContext(ILogger<MQTTClientContext> logger, 
            IOptions<ConnectionSettings> connectionSettings,
            Webex webex,
            SkypeForBusiness skypeForBusiness)
        {
            // Meeting apps
            _webex = webex;
            _skypeForBusiness = skypeForBusiness;


            _logger = logger;
            _connectionSettings = connectionSettings.Value;
            
            
            var contextMenu = new ContextMenuStrip();
            AddExitButton(contextMenu);
            
            _trayIcon = new NotifyIcon()
            {
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = "MQTT Client",
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
            };
            
            ShowLaunchBaloon();
        }

        #region OnLaunchToolTip
        
        private void ShowLaunchBaloon()
        {
            _trayIcon.BalloonTipIcon= ToolTipIcon.Info;
            _trayIcon.BalloonTipText = "Starting up Sagoo Status Notifier.";
            _trayIcon.BalloonTipTitle = "MQTTClient";
            _trayIcon.ShowBalloonTip(1500);

        }
        #endregion
        
        
        #region Exit Button
        
        private void AddExitButton(ContextMenuStrip contextMenu)
        {
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Name = "Exit";
            exitItem.Click += exitItem_Click;

            contextMenu.Items.Add(exitItem);
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