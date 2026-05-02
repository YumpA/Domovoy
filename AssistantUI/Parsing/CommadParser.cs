using SpeechCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechCore.Parsing
{
	public class CommadParser
	{
		public VoiceCommand Parse(string recognizedText)
		{
			if (string.IsNullOrWhiteSpace(recognizedText))
				return null;

			string lower = recognizedText.ToLower();
			var cmd=new VoiceCommand { DeviceId = "light_living_room" }; //по умолчанию

			if(lower.Contains("включи") || lower.Contains("зажги"))

				cmd.Action = "on";

			else if (lower.Contains("выключи") || lower.Contains("погаси"))
				cmd.Action = "off";
			else if (lower.Contains("переключи") || lower.Contains("переверни"))
				cmd.Action = "toggle";
			else if (lower.Contains("мигни") || lower.Contains("поморгай"))
			{
				cmd.Action = "blink";
				// можно вытащить числа, но для простоты оставим значения по умолчанию
				cmd.BlinkCount = 3;
				cmd.BlinkDelay = 200;
			}
			else if (lower.Contains("таймер") || lower.Contains("через"))
			{
				cmd.Action = "timer";
				cmd.TimerSeconds = 10;
				cmd.TimerAction = lower.Contains("включи") ? "on" : "off";
			}
			else
				return null; // не распознано

			return cmd;
		}
	}
}
