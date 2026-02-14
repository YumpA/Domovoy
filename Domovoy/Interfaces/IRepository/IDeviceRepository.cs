using Domovoy.Core.Enums;
using Domovoy.Core.Models;
using System;
using System.Collections;

namespace Domovoy.Core.Interfaces.IRepository
{
	public interface IDeviceRepository
	{
		DeviceData GetById(string deviceId);
		IList GetAll();
		void Add(DeviceData device);
		void Update(DeviceData device);
		void Delete(string deviceId);

		IList GetByLocation(string location);
		IList GetByType(DeviceType type);
		IList GetByStatus(DeviceStatus status);

		bool Exist(string deviceId);
		int GetCount();
	}
}
