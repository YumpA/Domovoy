using System;
using System.Text;

namespace Domovoy.Events
{
	public class DeviceStateChangedEventArgs : EventArgs
	{
		public string DeviceId { get; set; }
		public bool IsOn { get; set; }
		public string Source { get; set; }

		public DeviceStateChangedEventArgs(string deviceId, bool isOn, string source)
		{
			DeviceId = deviceId;
			IsOn = isOn;
			Source = source;
		}
	}
}
