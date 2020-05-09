using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTClient.Meeting;

namespace MQTTClient
{
    public class MQTTClientContext: ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ILogger _logger;
        private IConnectionSettings _connectionSettings;
        
        // Meeting apps
        private Webex _webex;
        private SkypeForBusiness _skypeForBusiness;

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
            _trayIcon.BalloonTipText = $"Connecting to {_connectionSettings.BrokerURL}";
            _trayIcon.BalloonTipTitle = "MQTT Client";
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