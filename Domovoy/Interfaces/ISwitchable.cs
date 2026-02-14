using System;
using System.Text;

namespace Domovoy.Core.Interfaces
{
	public interface ISwitchable : IDevice
	{
		bool TurnOn();
		bool TurnOff();
		bool Toggle();
		bool IsOn { get; }
	}
}
