using System;
using System.Collections;
using System.Text;

namespace Domovoy
{
	public class DeviceConfig
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string Type { get; set; }
		public int Pin { get; set; }
		public int DimmerPin { get; set; }
	}

	public class MqttConfig
	{
		public string Broker { get; set; } = "test.mosquitto.org";
		public int Port { get; set; } = 1883;
		public string ClientId { get; set; } = "DomovoyESP32";
		public bool UseTls { get; set; }
	}

	public class WifiConfig
	{
		public string Ssid { get; set; }
		public string Password { get; set; }
	}

	public class AppConfig
	{
		public WifiConfig Wifi { get; set; }
		public MqttConfig Mqtt { get; set; }
		public ArrayList Devices { get; set; }
	}
}
