using Domovoy.Events;
using System;
using System.Collections;

namespace Domovoy.Core.Services
{
	public interface IDeviceService
	{
		// Основные операции
		bool TurnOnDevice(string deviceId, string initiatedBy);
		bool TurnOffDevice(string deviceId, string initiatedBy);
		bool ToggleDevice(string deviceId, string initiatedBy);

		// Групповые операции
		int TurnOffAllInLocation(string location, string initiatedBy);

		// Информация
		Hashtable GetDeviceInfo(string deviceId);
		Hashtable GetStatistics();

		// Управление
		bool RenameDevice(string deviceId, string newName, string initiatedBy);
		bool UpdateDeviceLocation(string deviceId, string newLocation, string initiatedBy);
	}
}
