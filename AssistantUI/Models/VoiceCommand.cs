using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechCore.Models
{
	public class VoiceCommand
	{
		public string DeviceId { get; set; }
		public string Action { get; set; }      // "on", "off", "toggle", "blink", "timer"
		public int? BlinkCount { get; set; }
		public int? BlinkDelay { get; set; }
		public int? TimerSeconds { get; set; }
		public string TimerAction { get; set; } // "on" или "off"

		public override string ToString() => $"{Action} on {DeviceId}";
	}
}
