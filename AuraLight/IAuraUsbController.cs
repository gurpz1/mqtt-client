using System.Collections.Generic;
using AuraLight.Models;

namespace AuraLight
{
    public interface IAuraUsbController
    {
        /// <summary>
        /// Directly controls the LEDs on the list. Position in list indicates which LED to change.
        /// LEDs not contained in the list are not changed
        /// </summary>
        /// <param name="ledsToChange"></param>
        void DirectControl(IList<LED> ledsToChange);
        
        /// <summary>
        /// Directly controls the LEDs on the list. Position in list indicates which LED to change.
        /// Turns off LEDs not contained in the list
        /// </summary>
        /// <param name="ledsToChange"></param>
        /// <param name="resetAll"></param>
        void DirectControl(IList<LED> ledsToChange, bool resetAll);
    }
}