using SpeechCore.Models;
using System;

namespace SpeechCore.Recognition
{
	public interface IVoiceRecognizer : IDisposable
	{
		event EventHandler<VoiceCommand> CommandRecognizer;
		event EventHandler<string> PartialResult;
		void Start();
		void Stop();
	}
}
