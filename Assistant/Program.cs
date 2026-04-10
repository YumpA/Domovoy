using MQTTnet;
using MQTTnet.Client;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vosk;

namespace Assistant
{
	public class Program
	{
		private const string ModelPath = @"..\..\Models\vosk-model-small-ru-0.22";
		private static IMqttClient _mqttClient;

		static async Task Main(string[] args)
		{
			Console.WriteLine("Запуск голосового ассистента...");
			if (!Directory.Exists(ModelPath))
			{
				Console.WriteLine($"Ошибка: папка с моделью не найдена по пути '{ModelPath}'");
				Console.WriteLine("Пожалуйста, скачайте и распакуйте модель по указанному пути");
				return;
			}

			await ConnectMqtt();

			//инициализация модели Vosk
			using (var model = new Model(ModelPath))

			//распознаватель для обработки речевого потока
			using (var recognizer = new VoskRecognizer(model, 16000.0f)) //частота дискретизации 16кГц
			{
				recognizer.SetMaxAlternatives(0);
				recognizer.SetWords(false);
				
				Console.WriteLine("Настройка захвата звука с микрофона...");

				//захват звука с микрофона (частота 16кГц, 16 бит, моно)
				using(var waveIn=new WaveInEvent())
				{
					waveIn.DeviceNumber = 0; //певрое аудиоус-во
					waveIn.WaveFormat=new WaveFormat(16000, 16, 1); //формат должен совпадать с ожидаемым vosk
					waveIn.DataAvailable += async (sender, e) =>
					{
						//передаю полученные данные в распознаватель
						if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
						{
							//когда фраза завершена, получаю результат
							var result = recognizer.Result();
							var text = ExtractTextFromResult(result);
							if (!string.IsNullOrEmpty(text))
							{
								Console.WriteLine($"Распознано {text}");
								await ProcessCommand(text);
							}
						}
						else
						{
							//здесь можно обработать промежуточные результаты
						}
					};

					waveIn.StartRecording();
					Console.WriteLine("Голосовой ассистент запущен. Говорите...");
					Console.WriteLine("Для выхода нажмите 'q' и затем Enter");

					while (Console.ReadKey().KeyChar != 'q') { }

					waveIn.StopRecording();
				}
			}; 
		}

		/// <summary>
		/// Извлекает тест распознанной речи из JSON-результата Vosk.
		/// </summary>
		private static string ExtractTextFromResult(string resultJson)
		{
			const string textkey = "\"text\" : \"";
			int startIndex = resultJson.IndexOf(textkey);
			if (startIndex == -1) return null;
			startIndex += textkey.Length;
			int endIndex=resultJson.IndexOf("\"", startIndex);
			if (endIndex == -1) return null;
			return resultJson.Substring(startIndex, endIndex - startIndex);
		}

		/// <summary>
		/// Логика для отправки MQTT-команд
		/// </summary>
		private static async Task ProcessCommand(string recognizedText)
		{
			Console.WriteLine($"Обработка команды: {recognizedText}");
			string lowerText = recognizedText.ToLower();

			if (lowerText.Contains("включи свет"))
			{
				await SendMqttCommand("light_living_room", "on");
			}

			if (lowerText.Contains("выключи"))
			{
				await SendMqttCommand("light_living_room", "off");
			}

			else
			{
				Console.WriteLine("Неизвестная команда.");
			}
		}

		private static async Task ConnectMqtt()
		{
			var factory = new MqttFactory();
			_mqttClient = factory.CreateMqttClient();
			var options = new MqttClientOptionsBuilder().WithTcpServer("test.mosquitto.org", 1883).Build();

			await _mqttClient.ConnectAsync(options);
			Console.WriteLine("Подключено к MQTT брокеру.");
		}

		private static async Task SendMqttCommand(string deviceId, string command)
		{
			if (_mqttClient == null || !_mqttClient.IsConnected)
			{
				Console.WriteLine("MQTT клиент не подключен. Команда не отправлена.");
				return;
			}

			var topic = $"domovoy/device/{deviceId}/command";
			var message=new MqttApplicationMessageBuilder()
				.WithTopic(topic).WithPayload(command)
				.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce).Build();

			await _mqttClient.PublishAsync(message);
			Console.WriteLine($"Отправлена MQTT команда '{command}' на устройство '{deviceId}'");
		}
	}
}
