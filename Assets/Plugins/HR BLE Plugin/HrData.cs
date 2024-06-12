using System;
using System.Collections.Generic;
using Assets.Plugin;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

/// <summary>
/// Contains the final data for the HR Plugin Data.
/// </summary>
public class HrData
{

    /// <summary>
    /// The enumeration indicating if we have established contact with the heart rate sensor.
    /// </summary>
    public enum ConnectionStatus
    {
        Disconnected,
        Connected,
        DataAvailable
    }


    /// <summary>
    /// Indicates if the sensor has skin contact.
    /// </summary>
    public enum SensorContactStatus
    {
        NotAvailable,
        NoContact,
        Contact
    }


    /// <summary>
    /// Contains the information, if we are currently connected since the last measurement.
    /// </summary>
    public  ConnectionStatus CurrentConnection { get; private set; }


    /// <summary>
    /// Returns the information whether the sensor has currently contact to the skin.
    /// </summary>
    public SensorContactStatus CurrentSensorContact { get; private set; }

    /// <summary>
    /// Contains the amount of milliseconds passed since the first reading. 
    /// </summary>
    public long MillisecondsSinceFirstReading { get; private set; }

    /// <summary>
    /// Indicates we have fresh data, important for processing RR intervals.
    /// </summary>
    public bool DataFresh { get; private set; }

  
    /// <summary>
    /// Asks if the amount of energy is available (= provided by Sensor)
    /// </summary>
    public bool EnergyAvailable => EnergyUsed >= 0;


    /// <summary>
    /// Contains the amount of energy that has been provided.
    /// </summary>
    public int EnergyUsed { get; private set; }

    /// <summary>
    /// Contains the heart rate.
    /// </summary>
    public int HeartRate { get; private set; }


    /// <summary>
    /// Contains the list with the last measured RR Intervals.
    /// </summary>
    public List<int> RrIntervals { get; } = new List<int>(5);


    /// <summary>
    /// Checks if we have rr intervals available.
    /// </summary>
    public bool RrIntervalAvailable => RrIntervals.Count > 0;

    /// <summary>
    /// Flags that the RRs were logged.
    /// </summary>
    private bool m_RrLogged;

    /// <summary>
    /// Gets a text for the heartrate.
    /// </summary>
    public string HrText =>
        (CurrentConnection == ConnectionStatus.DataAvailable) 
            ? HeartRate.ToString()
            : "---";


    /// <summary>
    /// Gets the description text for the HR sensor.
    /// </summary>
    public string DescriptionText
    {
        get
        {
            string text = "";
            text += $"Data Fresh: {DataFresh}\n";
            text += $"Contact: {CurrentSensorContact}\n";
            text += $"Energy available: {EnergyAvailable}\n";
            if (EnergyAvailable)
                text += $"Energy used: {EnergyUsed}\n";
            text += $"HR: {HeartRate}\n";
            text += $"RR available: {RrIntervalAvailable}\n";
            if (RrIntervalAvailable)
            {
                text += "RR Intervals: ";
                foreach (int interval in RrIntervals)
                {
                    text += $" {interval}";
                }

                text += "\n";
            }

            return text;

        }
    }

    /// <summary>
    /// Contains the header of the csv line.
    /// </summary>
    public const string CsvHeader = "HR Connection;Heart Rate; RR 1; RR 2; RR 3; RR 4; RR 5";

    /// <summary>
    /// Gets the line for the csv file of the HR sensor.
    /// </summary>
    public string CsvLine
    {
        get
        {
            string finalText = $"{CurrentConnection};{HeartRate}";
            int rRFilled = 0;

            if (RrIntervalAvailable && (!m_RrLogged))
            {
                m_RrLogged = true;
                rRFilled = RrIntervals.Count;
                rRFilled = Mathf.Min(rRFilled, 5);
                for (int i = 0; i < rRFilled; ++i)
                    finalText += $";{RrIntervals[i]}";
            }

            for (int i = rRFilled; i < 5; ++i)
                finalText += ";0";
            
            return finalText;
        }
      
    }

    /// <summary>
    /// At the beginning we start disconnected.
    /// </summary>
    public HrData()
    {
        CurrentConnection = ConnectionStatus.Disconnected;
    }


    /// <summary>
    /// Constructor from raw data.
    /// </summary>
    /// <param name="ptr">raw data pointer from the DLL</param>
    public void SetData(IntPtr ptr)
    {
   
        unsafe
        {
            HRInternal* rawData = (HRInternal*)ptr;

            MillisecondsSinceFirstReading = rawData->millisPassedSinceFirstReading;
            DataFresh = rawData->dataFresh == 1;
            if (DataFresh)
                m_RrLogged = false;

            switch (rawData->sensorContact)
            {
                case 0:
                    CurrentSensorContact = SensorContactStatus.NotAvailable;
                    break;
                case 1:
                    CurrentSensorContact = SensorContactStatus.NoContact;
                    break;
                case 2:
                    CurrentSensorContact = SensorContactStatus.Contact;
                    break;
                default:
                    Debug.Assert(false, "Unimplemented contact case.");
                    break;
            }

            switch (rawData->validityStatus)
            {
                case 0:
                    CurrentConnection = ConnectionStatus.Disconnected;
                    break;
                case 1:
                    CurrentConnection = ConnectionStatus.Connected;
                    break;
                case 2:
                    CurrentConnection = ConnectionStatus.DataAvailable;
                    break;
                default:
                    Debug.Assert(false, "Unimplemented connection status handler");
                    break;
            }

            EnergyUsed = rawData->energyUsed;
            HeartRate = rawData->heartRate;

            RrIntervals.Clear();
            if (rawData->rrDataAmount > 0)
                RrIntervals.Add(rawData->rrData1);
            if (rawData->rrDataAmount > 1)
                RrIntervals.Add(rawData->rrData2);
            if (rawData->rrDataAmount > 2)
                RrIntervals.Add(rawData->rrData3);
            if (rawData->rrDataAmount > 3)
                RrIntervals.Add(rawData->rrData4);
            if (rawData->rrDataAmount > 4)
                RrIntervals.Add(rawData->rrData5);
   
        }

    }

}
