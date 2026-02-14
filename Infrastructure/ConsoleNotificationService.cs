using Domovoy.Core.Enums;
using Domovoy.Core.Interfaces;
using System;
using System.Text;

namespace Infrastructure
{
	public class ConsoleNotificationService : INotificationService
	{
		public void SendDeviceEmergencyNotification(string title, string message)
		{
			Console.WriteLine("!!! ЭКСТРЕННОЕ УВЕДОМЛЕНИЕ !!!");
			Console.WriteLine($"ЗАГОЛОВОК: {title}");
			Console.WriteLine($"СООБЩЕНИЕ: {message}");
			Console.WriteLine($"ВРЕМЯ: {DateTime.UtcNow:HH:mm:ss}");
			Console.WriteLine("!!!!!!!!!!!!!!!!!");
			Console.WriteLine();
		}

		public void SendDeviceErrorNotification(string deviceId, string errorMessage)
		{
			Console.WriteLine("!ОШИБКА УСТРОЙСТВА");
			Console.WriteLine($"Устройство: {deviceId}");
			Console.WriteLine($"Ошибка: {errorMessage}");
			Console.WriteLine($"Время: {DateTime.UtcNow:HH:mm:ss}");
			Console.WriteLine("==================");
			Console.WriteLine();
		}

		public void SendDeviceOfflineNotification(string deviceId)
		{
			Console.WriteLine("!Устройство Оффлайн");
			Console.WriteLine($"Устройство: {deviceId}");
			Console.WriteLine($"Отключилось от системы");
			Console.WriteLine($"Время: {DateTime.UtcNow:HH:mm:ss}");
			Console.WriteLine("==================");
			Console.WriteLine();
		}

		public void SendDeviceOnlineNotification(string deviceId)
		{
			Console.WriteLine("!УСТРОЙСТВО ОНЛАЙН");
			Console.WriteLine($"Устройство: {deviceId}");
			Console.WriteLine($"Подключилось к ситеме");
			Console.WriteLine($"Время: {DateTime.UtcNow:HH:mm:ss}");
			Console.WriteLine("==================");
			Console.WriteLine();
		}

		public void SendDeviceStatusNotification(string deviceId, DeviceStatus oldStatus, DeviceStatus newStatus, string reason)
		{
			Console.WriteLine("=== Статус изменился ===");
			Console.WriteLine($"Устройство: {deviceId}");
			Console.WriteLine($"Было: {oldStatus} -> стало :{newStatus}");
			Console.WriteLine($"Причина: {reason}");
			Console.WriteLine($"Время: {DateTime.UtcNow:HH:mm:ss}");
			Console.WriteLine();
		}

		public void SendNotification(string title, string message, string recepient = "system")
		{
			Console.WriteLine("=== УВЕДОМЛЕНИЕ ===");
			Console.WriteLine($"Для: {recepient}");
			Console.WriteLine($"Заголовок: {title}");
			Console.WriteLine($"Сообщение: {message}");
			Console.WriteLine("==================");
			Console.WriteLine();
		}
	}
}
