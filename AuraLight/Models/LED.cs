using System.Collections.Generic;
using AuraLight.Exceptions;

namespace AuraLight.Models
{
    public class LED
    {
        public byte R { get; set; } = 0x00;
        public byte G { get; set; }= 0x00;
        public byte B { get; set; }= 0x00;

        public LED(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
        public byte[] ToByteArray()
        {
            return new [] {R, G, B};
        }

        /// <summary>
        /// Return LED object from integer array
        /// </summary>
        /// <param name="leds"></param>
        /// <returns></returns>
        public static LED Parse(int[] leds)
        {
            if (leds.Length != 3)
            {
                throw new ArraySizeException("Array format must be [R,G,B]");
            }
            
            return new LED((byte) leds[0], (byte) leds[1], (byte) leds[2]);
        }
    }
}