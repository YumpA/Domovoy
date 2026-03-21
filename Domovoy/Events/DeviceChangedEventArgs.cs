using System;
using System.Text;

namespace Domovoy.Events
{
	public class DeviceChangedEventArgs : EventArgs
	{
		public string DeviceId { get; set; }
		public string PropertyName { get; set; }
		public object OldValue { get; set; }
		public object NewValue { get; set; }

		public DeviceChangedEventArgs(string deviceId, string propertyName, object oldValue, object newValue)
		{
			DeviceId = deviceId;
			PropertyName = propertyName;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}
