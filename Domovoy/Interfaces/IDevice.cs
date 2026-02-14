using Domovoy.Core.Enums;
using System;
using System.Text;

namespace Domovoy.Core.Interfaces
{
	public interface IDevice
	{
		string Id {  get; }
		string Name { get; }
		string Location { get; }
		DeviceType Type { get; }
		DeviceStatus Status { get; }

		/*event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;
		event EventHandler<DeviceErrorEventArgs> ErrorOccurred;
		event EventHandler<DeviceUpdatedEventArgs> Updated;
		event EventHandler<DeviceInitializedEventArgs> Initialized;*/

		bool Initialize();
		bool Shutdown();
	}
}
