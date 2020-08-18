using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuraLight.Exceptions;
using AuraLight.Models;
using HidSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuraLight
{
    public class AuraUsbController:IAuraUsbController
    {
        public AuraDeviceInfo AuraDeviceInfo { get; }
        
        private ILogger<AuraUsbController> _logger;
        
        private AuraLightSettings _settings;
        private HidDevice _auraDevice;
        private byte _messageHeader = 0xEC;
        private byte _messageLength = 65;
        
        public static UInt16 AsusVendorId { get; } = 0x0B05;

        public AuraUsbController(ILogger<AuraUsbController> logger, IOptions<AuraLightSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _auraDevice = GetAuraDevice(_settings.DeviceIndex);
            AuraDeviceInfo = GetDeviceInfo(_auraDevice);
        }
        
        /// <summary>
        /// Directly control LEDs
        /// </summary>
        /// <param name="ledsToChange"></param>
        public void DirectControl(IList<LED> ledsToChange)
        {
            DirectControl(ledsToChange, false);
        }

        /// <summary>
        /// Turn all LEDs off first before directly controlling LEDs 
        /// </summary>
        /// <param name="ledsToChange"></param>
        /// <param name="resetAll"></param>
        /// <exception cref="ArraySizeException"></exception>
        public void DirectControl(IList<LED> ledsToChange, bool resetAll)
        {
            if (ledsToChange.Count > _settings.LedCount)
            {
                throw new ArraySizeException($"You are trying to control too many LEDs. The limit is {_settings.LedCount}");
            }

            if (resetAll)
            {
                // turn off leds before we start doing anything
                IList<LED> off = new List<LED>(_settings.LedCount);
                for(var i = 0; i<_settings.LedCount; i++)
                {
                    off.Insert(i, new LED(0,0,0));
                }
                SetLeds(off);
                SendApplyMessage();                
            }

            SetLeds(ledsToChange);
            SendApplyMessage();
        }
        
        #region Comms
        
        /// <summary>
        /// Sends colour message to all of the LEDs in the array
        /// </summary>
        /// <param name="leds"></param>
        private void SetLeds(IList<LED> leds)
        {
            int index = 0;
            
            while (index < leds.Count)
            {
                int remaining = leds.Count - index;
                if (remaining >= _settings.LedsPerMessage)
                {
                    SendLedMessage(index, leds.Skip(index).Take(_settings.LedsPerMessage));
                }
                else
                {
                    SendLedMessage(index, leds.Skip(index).Take(_settings.LedsPerMessage-remaining));
                }

                index += _settings.LedsPerMessage;
            }
        }

        /// <summary>
        /// Helper function for SetLeds given that we can only send a limited set of
        /// instructions per message
        /// </summary>
        /// <param name="startLed"></param>
        /// <param name="leds"></param>
        private void SendLedMessage(int startLed, IEnumerable<LED> leds)
        {
            byte [] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x00;
            message[3] = (byte) startLed;
            message[4] = (byte) leds.Count();

            int index = 5;
            foreach (var led in leds)
            {
                byte[] rgb = led.ToByteArray();
                rgb.CopyTo(message, index);
                index += 3;
            }
            SendMessage(message);
        }

        /// <summary>
        /// Initialises the controller for messges
        /// </summary>
        private void SendInitMessage()
        {
            byte[] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = 0x35;
            message[5] = (byte) AuraEffect.DIRECT;
            using (var deviceStream = _auraDevice.Open())
            {
                deviceStream.Write(message, 0, _messageLength);
            }
            
            message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x84;
            message[4] = 0x02;
            SendMessage(message);;
            
            message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = 0x35;
            message[2] = 0x01;
            message[5] = (byte) AuraEffect.DIRECT;
            SendMessage(message);   
        }
        
        /// <summary>
        /// Let the Controller know that we are finished sending the messages and action the change
        /// </summary>
        private void SendApplyMessage()
        {
            byte[] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x80;
            SendMessage(message);
        }
        
        /// <summary>
        /// Sends message to device
        /// </summary>
        /// <param name="message"></param>
        private void SendMessage(byte[] message)
        {
            using (var deviceStream = _auraDevice.Open())
            {
                deviceStream.Write(message,0, _messageLength);
            }
        }
        #endregion

        #region Device Information
        /// <summary>
        /// Gets Aura Device Info
        /// </summary>
        /// <param name="auraDevice"></param>
        /// <returns></returns>
        /// <exception cref="DeviceNotFoundException"></exception>
        private AuraDeviceInfo GetDeviceInfo(HidDevice auraDevice)
        {
            // Get device name
            byte[] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = 0x82;
            byte[] recieved = new byte[65];
            using (var deviceStream = auraDevice.Open())
            {
                deviceStream.Write(message);
                deviceStream.Read(recieved);
            }

            string deviceName = "";
            if (recieved[1] == 0x02)
            {
                deviceName =  Encoding.Default.GetString(recieved.Skip(2).Take(16).ToArray());
                _logger.LogInformation($"Found {deviceName}.");
            }
            else
            {
                throw new DeviceNotFoundException("Unable to determine device name");
            }
            return new AuraDeviceInfo(deviceName);
        }
        
        /// <summary>
        /// Gets the Aura device
        /// </summary>
        /// <param name="deviceIndex"></param>
        /// <returns></returns>
        /// <exception cref="DeviceNotFoundException"></exception>
        private HidDevice GetAuraDevice(int deviceIndex)
        {
            var localDevices = DeviceList.Local;
            var auraDevices = localDevices.GetHidDevices(AsusVendorId)
                .Where(x => x.GetProductName() == "AURA LED Controller").ToList();

            if (auraDevices.Count < 1)
            {
                throw new DeviceNotFoundException("Unable to find LED Controller");
            }

            try
            {
                return auraDevices[deviceIndex];
            }
            catch
            {
                throw new DeviceNotFoundException($"Device index {deviceIndex} not found");
            }
        }
        #endregion
    }
}