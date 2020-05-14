using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WakeOnLan
{
    class Program
    {
        private static bool suppressMessages = false;
        static void Main(string[] args)
        {

            switch (args.Length)
            {
                case 3:
                case 4:
                    if (args.Length > 3 && "-a".Equals(args[3]))
                        suppressMessages = true;
                    PrintLine("*** WakeOnLan commandline by Tushar ***");
                    var macAddress = args[0];
                    var ipAddress = IPAddress.Parse(args[1]);
                    var netMask = IPAddress.Parse(args[2]);
                    var broadcastAddress = ipAddress.GetBroadcastAddress(netMask);
                    if (isMacOk(macAddress) && isBroadcastOk(broadcastAddress.ToString()))
                    {
                        WakeFunction(macAddress, broadcastAddress);
                        PrintLine("Sig ON sent to : " + broadcastAddress.ToString());
                    }
                    break;
                default:
                    PrintLine("Usage: \n" +
                        "WakeOnLan [MAC] [IP] [Subnet] [-a]\n" +
                        "-a (optional): Automatic and suppressed messages");
                    suppressMessages = true;
                    break;
            }

            if (!suppressMessages)
            {
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }
        private static int WakeFunction(string MAC_ADDRESS, IPAddress BroadcastAddress)
        {
            int position = 0;
            byte[] buffer = new byte[1024];

            UdpClient WOL = new UdpClient();
            WOL.Connect(BroadcastAddress, 7);
            WOL.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
            for (int i = 0; i < 6; i++)
            {
                buffer[position++] = 0xFF;
            }
            for (int i = 0; i < 16; i++)
            {
                int x = 0;
                for (int j = 0; j < 6; j++)
                {
                    buffer[position++] = byte.Parse(MAC_ADDRESS.Substring(x, 2), NumberStyles.HexNumber);
                    x += 3;
                }
            }
            return WOL.Send(buffer, 1024);
        }

        private static void PrintLine(string str)
        {
            if (!suppressMessages)
                Console.WriteLine(str);
        }

        private static Boolean isMacOk(String MacAddress)
        {
            if (Regex.IsMatch(MacAddress, @"((([a-f]|[0-9]|[A-F]){2}\:){5}([a-f]|[0-9]|[A-F]){2}\b)|((([a-f]|[0-9]|[A-F]){2}\-){5}([a-f]|[0-9]|[A-F]){2}\b)"))
            {
                return true;
            }
            return false;
        }

        private static Boolean isBroadcastOk(String BroadcastAddress)
        {
            if (Regex.IsMatch(BroadcastAddress, @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b"))
            {
                return true;
            }
            return false;
        }
    }
    static class Util
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }
    }
}
