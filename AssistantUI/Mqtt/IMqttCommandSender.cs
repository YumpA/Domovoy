using SpeechCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechCore.Mqtt
{
	public interface IMqttCommandSender
	{
		Task ConnectAsync();
		Task SendCommandAsync(VoiceCommand command);
		void Disconnect();
		bool IsConnected { get; }
	}
}
