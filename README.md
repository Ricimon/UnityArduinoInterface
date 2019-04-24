# UnityArduinoInterface
A robust way to read Arduino (or any serial bus) output into Unity  

Tested to work on 2018.3.6+, but will probably work on Unity 5+

# How to use
Clone or download this repo into any folder under Assets/

The ArduinoInterface class is a Singleton MonoBehaviour. To add the interface to your scene, simply add the script as a component to any GameObject.

![ss1](https://user-images.githubusercontent.com/24966782/56639156-dfbbe680-6624-11e9-902a-7baa90f6af3c.png)

Rather than using a specified COM port to connect to an Arduino, this interface uses the Arduino's VID and PID to automatically detect the COM port it's on. This allows the Arduino to be found regardless of which USB port it was plugged into.

Here's how to find your Arduino's VID and PID:
* Connect your Arduino to your computer.
* On Windows, open up Device Manager (search Device Manager in the start menu).
* Expand "Ports (COM & LPT)"
* Look for the Arduino, and double click it.
* Click on the Details tab
* Under Property, select "Device instance path"
* It should be a line similar to this:
    * USB\VID_2341&PID_0043\75736303336351B021D1
* In this case, the Arduino's VID is 2341, and its PID is 0043.

---

Since the interface is a singleton, it can be accessed by calling ArduinoInterface.instance  

Reading the data stream from the Arduino can be either manually or automatically started. To manually initialize and start the stream, call the InitializeAndOpenStream method.  

The stream can be closed by calling StopSerialMonitoring, and re-opened by calling StartSerialMonitoring.

---

To parse the Arduino stream data, subscribe a function to the dataReceived event to parse the incoming data. This event is raised every time a line is read by the interface, and it passes the contents of the line through the event.

If your Arduino stream outputs as fast as possible, the suggested implementation is to create an intermediate class that parses the data as it comes in and populates a data object, which other classes can read from in their Update methods. This way, Unity visuals will still update at Unity's FPS while data is updated at the Arduino's update rate.

*Note*: Do not call Unity methods in functions subscribed to the dataReceived event, as the event will not be raised on the Unity main thread.


# Internals

The internal implementation dedicates a thread to constantly read from the serial port, so that the Unity main thread is never blocked if the Arduino fails to refresh at a rate equal or faster to Unity's FPS.
