using System;
using System.Text;

namespace Domovoy.Interfaces
{
	public interface IMqttService
	{
		void PublishStatus(string deviceId, bool isOn);
		bool Connect();
		void Disconnect();
		bool IsConnected { get; }
	}
}
