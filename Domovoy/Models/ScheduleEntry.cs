using System;
using System.Text;

namespace Domovoy.Models
{
	public class ScheduleEntry
	{
		public string DeviceId { get; set; }
		public string Command { get; set; }
		public TimeSpan TimeOfDay { get; set; }
		public bool IsActive { get; set; }
	}
}
