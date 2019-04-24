using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoInterface : GenericSingletonClass<ArduinoInterface>
{
    /// <summary>
    /// Triggered on every line read from the serial port stream. 
    /// Do not subscribe Unity methods to this event, as this event will not be raised on the Unity main thread.
    /// </summary>
    public Action<string> dataReceived;

    [Tooltip("Arduino VID, found from Device Manager. See README for instructions to getting this value.")]
    [SerializeField] private string VID = null;
    [Tooltip("Arduino PID, found from Device Manager. See README for instructions to getting this value.")]
    [SerializeField] private string PID = null;
    [Tooltip("Serial port baud rate.")]
    [SerializeField] private int baudRate = 0;
    [Tooltip("Initialize and open the serial port stream on Start?")]
    [SerializeField] private bool initializeOnStart = true;

    private SerialPort _stream;

    private CancellationTokenSource _cts;

    /// <summary>
    /// Closes the serial port stream.
    /// </summary>
    public void StopSerialMonitoring()
    {
        _cts.Cancel();
        if (_stream != null)
            _stream.Close();
    }

    /// <summary>
    /// Opens the serial port stream and starts constantly reading from it.
    /// </summary>
    public void StartSerialMonitoring()
    {
        if (_stream != null && !_stream.IsOpen)
        {
            _stream.Open();
            _cts = new CancellationTokenSource();
            Task.Factory.StartNew(ConstantlyReadSerialStream, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Attempts to find the Arduino specified by the VID and PID, and opens a serial port stream to it if found.
    /// </summary>
    /// <returns>True if the stream was opened successfully or is already open, false if an error occurred</returns>
    public bool InitializeAndOpenStream()
    {
        if (_stream != null && _stream.IsOpen)
        {
            Debug.LogWarning("Stream is already open!");
            return true;
        }

        List<string> names = ComPortNames(VID, PID);
        if (names.Count > 0)
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                if (names.Contains(s))
                {
                    Debug.Log("My Arduino port is " + s);
                    _stream = new SerialPort(s, baudRate);
                    StartSerialMonitoring();
                    return true;
                }
            }
            Debug.LogError("No matching COM ports found with given VID and PID");
            return false;
        }
        else
        {
            Debug.LogError("No COM ports found with given VID and PID");
            return false;
        }
    }

    private void ConstantlyReadSerialStream()
    {
        while (true)
        {
            _cts.Token.ThrowIfCancellationRequested();
            
            dataReceived?.Invoke(_stream.ReadLine());
        }
    }

    private void Start()
    {
        if (initializeOnStart)
            InitializeAndOpenStream();
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        if (_stream != null)
        { 
            _stream.Close();
        }
    }

    /// <summary>
    /// Compile an array of COM port names associated with given VID and PID
    /// </summary>
    /// <param name="VID"></param>
    /// <param name="PID"></param>
    /// <returns></returns>
    private List<string> ComPortNames(string VID, string PID)
    {
        string pattern = string.Format("^VID_{0}.PID_{1}", VID, PID);
        Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
        List<string> comports = new List<string>();
        RegistryKey rk1 = Registry.LocalMachine;
        RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
        foreach (string s3 in rk2.GetSubKeyNames())
        {
            RegistryKey rk3 = rk2.OpenSubKey(s3);
            foreach (string s in rk3.GetSubKeyNames())
            {
                if (_rx.Match(s).Success)
                {
                    RegistryKey rk4 = rk3.OpenSubKey(s);
                    foreach (string s2 in rk4.GetSubKeyNames())
                    {
                        RegistryKey rk5 = rk4.OpenSubKey(s2);
                        RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                        comports.Add((string)rk6.GetValue("PortName"));
                    }
                }
            }
        }
        return comports;
    }


}
