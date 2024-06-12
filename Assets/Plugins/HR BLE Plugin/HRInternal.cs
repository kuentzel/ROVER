using System.Runtime.InteropServices;


namespace Assets.Plugin
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HRInternal
    {
        /// Flags we have fresh unprocessed data (important for RR interpretation)
        public int dataFresh;

        // The amount of milliseconds passed since first reading.
        public int millisPassedSinceFirstReading;

        // 0: Unititialized, 1: Initialized, 2 : data available
        public int validityStatus;

        // 0 : Unsupported, 1: Supported, 2: Sensor contact made.
        public int sensorContact;

        // -1: Unsupported, otherwise data.
        public int energyUsed;

        // The real heart rate.
        public int heartRate;

        // The amount of rr data availabe in the following field.
        public int rrDataAmount;

        public int rrData1;

        public int rrData2;

        public int rrData3;

        public int rrData4;

        public int rrData5;
         
    }

}
