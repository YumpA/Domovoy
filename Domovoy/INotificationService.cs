using Domovoy.Core.Enums;
using System;

namespace Domovoy.Core.Interfaces
{
	public interface INotificationService
	{
		void SendNotification(string title, string message, string recepient = "system");
		void SendDeviceStatusNotification(string deviceId, DeviceStatus oldSTatus, DeviceStatus newSTatus, string reason);
		void SendDeviceErrorNotification(string deviceId, string errorMessage);
		void SendDeviceOnlineNotification(string deviceId);
		void SendDeviceOfflineNotification(string deviceId);
		void SendDeviceEmergencyNotification(string title, string message);
	}
}
