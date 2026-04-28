using NAudio.Wave;
using SpeechCore.Models;
using SpeechCore.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vosk;

namespace SpeechCore.Recognition
{
	public class VoskVoiceRecognizer : IVoiceRecognizer
	{
		private readonly string _modelPath;
		private CommadParser _parser;
		private Model _model;
		private VoskRecognizer _recognizer;
		private WaveInEvent _waveIn;
		private bool _disposed;

		public event EventHandler<VoiceCommand> CommandRecognizer;
		public event EventHandler<string> PartialResult;

		public VoskVoiceRecognizer(string modelPath)
		{
			_modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));

			_parser = new CommadParser();
		}

		public void Dispose()
		{
			if (_disposed) return;
			Stop();
			_waveIn?.Dispose();
			_recognizer?.Dispose();
			_model?.Dispose();
			_disposed = true;
		}

		public void Start()
		{
			if (!Directory.Exists(_modelPath))
				throw new DirectoryNotFoundException($"Модель не найдена: {_modelPath}");

			_model = new Model(_modelPath);
			_recognizer = new VoskRecognizer(_model, 16000.0f);
			_recognizer.SetMaxAlternatives(0);
			_recognizer.SetWords(false);

			_waveIn = new WaveInEvent
			{
				DeviceNumber = 0,
				WaveFormat = new WaveFormat(16000, 16, 1)
			};
			_waveIn.DataAvailable += OnDataAvailable;
			_waveIn.StartRecording();
		}

		private void OnDataAvailable(object sender, WaveInEventArgs e)
		{
			if (_recognizer == null) return;

			if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
			{
				string result = _recognizer.Result();
				string text = ExtractText(result);
				if (!string.IsNullOrEmpty(text))
				{
					var command = _parser.Parse(text);
					if (command != null)
						CommandRecognizer?.Invoke(this, command);
				}
			}
			else
			{
				string partial = _recognizer.PartialResult();
				string partialText = ExtractText(partial);
				if (!string.IsNullOrEmpty(partialText))
					PartialResult?.Invoke(this, partialText);
			}
		}

		private string ExtractText(string result)
		{
			const string textkey = "\"text\" : \"";
			int startIndex = result.IndexOf(textkey);
			if (startIndex == -1) return null;
			startIndex += textkey.Length;
			int endIndex = result.IndexOf("\"", startIndex);
			if (endIndex == -1) return null;
			return result.Substring(startIndex, endIndex - startIndex);
		}

		public void Stop()
		{
			_waveIn?.StopRecording();
		}
	}
}
