using Domovoy.Core.Services;
using Domovoy.Models;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Infrastructure.Scheduling
{
	public class ScheduleManager
	{
		private readonly IDeviceService _deviceService;
		private ArrayList _schedules = new ArrayList();
		private Timer _chekTimer;

		public ScheduleManager(IDeviceService deviceService)
		{
			_deviceService = deviceService;
		}

		public void AddSchedule(ScheduleEntry entry)
		{
			_schedules.Add(entry);
			Console.WriteLine($"Schedule added: {entry.TimeOfDay} -> {entry.DeviceId} {entry.Command}");
			Console.WriteLine(_schedules.Count.ToString());
		}

		public void Start()
		{
			_chekTimer = new Timer(CheckSchedules, null, 1000, 1000);
		}

		public void Stop()
		{
			_chekTimer?.Dispose();
		}

		private void CheckSchedules(object state)
		{
			DateTime now = GetLocalTime();
			TimeSpan currentTime = now.TimeOfDay;
			
			//Console.WriteLine(currentTime.ToString());
			//Console.WriteLine($"Checking schedules, current time: {DateTime.UtcNow:HH+5:mm:ss}");

			foreach (ScheduleEntry entry in _schedules)
			{
				if (!entry.IsActive) continue;

				double abs = (entry.TimeOfDay - currentTime).TotalSeconds;
				if (abs < 0) abs = abs * (-1);
				if (abs < 30)
				{
					ExecuteSchedule(entry);
					entry.IsActive = false; //выполнить 1 раз, можно добавить флаг для повторения
				}
			}
		}

		private void ExecuteSchedule(ScheduleEntry entry)
		{
			switch (entry.Command.ToLower())
			{
				case "on":
					_deviceService.TurnOnDevice(entry.DeviceId, "scheduler");
					break;
				case "off":
					_deviceService.TurnOffDevice(entry.DeviceId, "scheduler");
					break;
				case "toggle":
					_deviceService.ToggleDevice(entry.DeviceId, "scheduler");
					break;
			}
			Console.WriteLine($"Executed schedule: {entry.TimeOfDay} -> {entry.DeviceId} {entry.Command}");
		}

		public static DateTime GetLocalTime()
		{
			var utcNow = DateTime.UtcNow;
			var localTime = utcNow.AddHours(5);
			return localTime;
		}
	}
}
