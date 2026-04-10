using System;
using System.Text;
using System.Threading;
using nanoFramework.Networking;

namespace Infrastructure.Network
{
	public static class WifiHelper
	{
		public static bool ConnectToWifi(string ssid, string password, int timeoutSeconds = 30)
		{
			Console.WriteLine($"Подключение к Wi-Fi {ssid}...");

			CancellationTokenSource cs= new (60000);
			bool succes = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token);

			if (succes)
			{
				var ip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
				Console.WriteLine($"✓ Подключено. IP адрес: {ip}");
				return true;
			}

			else
			{
				Console.WriteLine($"✗ Не удалось подключиться к Wi-Fi");
				return false;
			}
		}
	}
}
