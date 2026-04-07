using Domovoy;
using Domovoy.Core.Services;
using Domovoy.Interfaces;
using Infrastructure.Web;
using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Collections;
using System.Text;

namespace Infrastructure.MQTT
{
	public class MqttService : IMqttService
	{
		private readonly IDeviceService _deviceService;
		private readonly MqttClient _client;
		private bool _disposed;

		//параметры подключения, потом вынести в конфигурацию
		private const string BrokerAddress = "test.mosquitto.org";
		private const int BrokerPort = 1883;
		private const string ClientId = "DomovoyESP32";

		//топики
		private const string CommandTopic = "domovoy/device/+/command";
		private const string StatusTopicPrefix = "domovoy/device/";

		public bool IsConnected => _client.IsConnected;

		public void Dispose()
		{
			if (!_disposed)
			{
				Disconnect();
				_client?.Dispose();
				_disposed = true;
			}
		}

		public MqttService(IDeviceService deviceService, MqttConfig config)
		{
			_deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
			_client = new MqttClient(config.Broker, config.Port, false, null, null, MqttSslProtocols.None); //1

			_client.MqttMsgPublishReceived += OnMqttMessageReceived;
		}

		public bool Connect()
		{
			try
			{
				Console.WriteLine($"Connecting to MQTT broker {BrokerAddress}:{BrokerPort}...");
				var result = _client.Connect(ClientId);
				if (result == MqttReasonCode.Success)
				{
					Console.WriteLine("Mqtt connected");
					_client.Subscribe(new string[] { CommandTopic }, new MqttQoSLevel[] { MqttQoSLevel.AtMostOnce }); //3
					return true;
				}
				else
				{
					Console.WriteLine($"MQTT connection failed: {result}");
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"MQTT connection error: {ex.Message}");
				return false;
			}
		}
		private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
		{
			try
			{
				string topic = e.Topic;
				string payload = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

				Console.WriteLine($"MQTT: {topic} -> {payload}");

				//разбор топика
				string[] parts = topic.Split('/');
				if (parts.Length >= 4 && parts[2] != null && parts[3] == "command")
				{
					string deviceId = parts[2];

					string command = null;
					Hashtable parameters = null;

					if (payload.TrimStart().StartsWith("{"))
					{
						var json = JsonHelper.ParseJson(payload);

						if (json != null && json.Contains("command"))
						{
							command = json["command"].ToString();
							if (json.Contains("params") && json["params"] is Hashtable)
								parameters = (Hashtable)json["params"];
						}
					}

					else
					{
						command = payload.Trim().ToLower();
					}

					if (string.IsNullOrEmpty(command)) return;

					switch (command.ToLower())
					{
						case "on":
							_deviceService.TurnOnDevice(deviceId, "mqtt");
							break;
						case "off":
							_deviceService.TurnOffDevice(deviceId, "mqtt");
							break;
						case "toggle":
							_deviceService.ToggleDevice(deviceId, "mqtt");
							break;
						case "blink":
							int count = 3, delay = 200;
							if (parameters != null)
							{
								if (parameters.Contains("count")) count = (Int32)parameters["count"];
								if (parameters.Contains("delay")) delay = (Int32)parameters["delay"];
							}
							_deviceService.BlinkDevice(deviceId, count, delay, "mqtt");
							break;
						case "timer":
							int seconds = 10;
							bool turnOn = true;
							if (parameters != null)
							{
								if (parameters.Contains("seconds")) seconds = (Int32)parameters["seconds"];
								if (parameters.Contains("action")) turnOn = parameters["action"].ToString() == "on";
							}
							_deviceService.SetTimer(deviceId, seconds, turnOn, "mqtt");
							break;
						default:
							Console.WriteLine($"Unknown MQTT command: {command}");
							break;
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine($"MQTT processing error: {ex.Message}");
			}
		}

		public void PublishStatus(string deviceId, bool isOn)
		{
			try
			{
				if (!IsConnected) return;
				string topic = $"{StatusTopicPrefix}{deviceId}/status";
				string payload = isOn ? "on" : "off";
				Console.WriteLine(payload);
				byte[] data = Encoding.UTF8.GetBytes(payload);

				_client.Publish(topic, data, MqttQoSLevel.AtMostOnce.ToString(), null); //4
				Console.WriteLine($"Published {payload} to {topic}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Publish error: {ex.Message}");
			}
		}

		public void Disconnect()
		{
			try
			{
				if (_client != null && _client.IsConnected)
					_client.Disconnect();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Disconnect error: {ex.Message}");
			}
		}
	}
}
