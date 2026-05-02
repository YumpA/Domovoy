using MQTTnet;
using MQTTnet.Client;
using SpeechCore.Models;
using SpeechCore.Mqtt;
using SpeechCore.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AssistantUI
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private IMqttClient _mqttClient;
		private IVoiceRecognizer _recognizer;
		private MqttCommandSender _mqtt;
		private const string MqttBroker = "broker.hivemq.com";
		private const string DeviceId = "light_living_room";

		private readonly string _modelPath = @"..\..\Models\vosk-model-small-ru-0.22";

		public MainWindow()
		{
			InitializeComponent();
			Loaded += OnLoaded;
			Closed += OnClosed;
		}

		private void OnClosed(object sender, EventArgs e)
		{
			_recognizer?.Stop();
			_mqtt?.Disconnect();
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_mqtt = new MqttCommandSender(MqttBroker, 1883);
				await _mqtt.ConnectAsync();
				LogMessage($"Mqtt подключен");

				//голосовой распознаватель
				_recognizer = new VoskVoiceRecognizer(_modelPath);
				_recognizer.CommandRecognizer += OnCommandRecognized;
				_recognizer.PartialResult += OnPartialResult;
				_recognizer.Start();
				LogMessage("Распознавание запущено");
			}

			catch (Exception ex)
			{
				LogMessage($"Ошибка: {ex.Message}");
			}
		}

		private async void OnCommandRecognized(object sender, VoiceCommand cmd)
		{
			await Dispatcher.InvokeAsync(async () =>
			{
				LogMessage($"Команда: {cmd.Action} для {cmd.DeviceId}");
				await _mqtt.SendCommandAsync(cmd);
				LogMessage($"Отправлено: {cmd.Action}");
			});
		}

		private void OnPartialResult(object sender, string partial)
		{
			Dispatcher.Invoke(() => txtRecognized.Text = partial);
		}

		private void LogMessage(string msg)
		{
			Dispatcher.Invoke(() =>
			{
				lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} - {msg}");
				lstLog.ScrollIntoView(lstLog.Items[lstLog.Items.Count - 1]);
			});
		}

		private async void BtnOn_Click(object sender, RoutedEventArgs e)
		{
			await _mqtt.SendCommandAsync(new VoiceCommand { DeviceId = DeviceId, Action="on" });
			LogMessage("Ручная команда: включить");
		}

		private async void BtnOff_Click(object sender, RoutedEventArgs e)
		{
			await _mqtt.SendCommandAsync(new VoiceCommand { DeviceId = DeviceId, Action = "off" });
			LogMessage("Ручная команда: выключить");
		}

		private void BtnToggle_Click(object sender, RoutedEventArgs e)
		{

		}

		private void BtnStatus_Click(object sender, RoutedEventArgs e)
		{

		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
