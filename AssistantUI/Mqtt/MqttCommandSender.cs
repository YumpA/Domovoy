using MQTTnet;
using MQTTnet.Client;
using SpeechCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechCore.Mqtt
{
	public class MqttCommandSender : IMqttCommandSender
	{
		private readonly string _broker;
		private readonly int _port;
		private IMqttClient _client;
		private bool _connected;

		public bool IsConnected => throw new NotImplementedException();

		public MqttCommandSender(string broker = "broker.hivemq.com", int port = 1883)
		{
			_broker = broker;
			_port = port;
		}

		public async Task ConnectAsync()
		{
			var factory = new MqttFactory();
			_client = factory.CreateMqttClient();
			var options=new MqttClientOptionsBuilder().WithTcpServer(_broker, _port).Build();
			var result = await _client.ConnectAsync(options);
			_connected = result.ResultCode == MqttClientConnectResultCode.Success;
		}

		public void Disconnect()
		{
			if(_client!=null && _connected)
				_client.DisconnectAsync().GetAwaiter().GetResult();
			_connected = false;
		}

		public async Task SendCommandAsync(VoiceCommand command)
		{
			if (!_connected || _client == null)
				throw new InvalidOperationException("MQTT не подключён");

			string topic = $"domovoy/device/{command.DeviceId}/command";

			string payload;
			switch (command.Action)
			{
				case "on":
					payload = "on";
					break;
				case "off":
					payload = "off";
					break;
				case "toggle":
					payload = "toggle";
					break;
				case "blink":
					payload = $"{{\"command\":\"blink\",\"params\":{{\"count\":{command.BlinkCount},\"delay\":{command.BlinkDelay}}}}}";
					break;
				case "timer":
					payload = $"{{\"command\":\"timer\",\"params\":{{\"seconds\":{command.TimerSeconds},\"action\":\"{command.TimerAction}\"}}}}";
					break;
				default:
					payload = command.Action;
					break;
			}

			var message = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(Encoding.UTF8.GetBytes(payload))
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
				.Build();

			await _client.PublishAsync(message);
		}
	}
}
