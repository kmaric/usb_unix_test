using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using HidSharp;

namespace usb_unix_test
{
    class Program
    {
        public static bool CancelationToken { get; set; }
        public static SerialDevice billAcceptor = null;
        
        static void Main(string[] args)
        {
            Program.HidSharpMethod();
        }

        private static void HidSharpMethod()
        {
            Console.WriteLine("HIDSharp looking for a USB Serial devices...");

            List<SerialDevice> devices = DeviceList.Local.GetSerialDevices().ToList();

            if (!devices.Any())
            {
                Console.WriteLine("No Devices Found");
                return;
            }
            
            foreach (SerialDevice device in devices)
            {
                Console.WriteLine($"Found {device}");

                if (device.DevicePath.Contains("GBA ST2"))
                {
                    Console.WriteLine($"Found Match! { device.GetFriendlyName()}");
                    if (billAcceptor == null)
                    {
                        billAcceptor = device;
                        break;
                    }
                }
            }

            if (billAcceptor == null)
            {
                Console.WriteLine("Bill Acceptor not found!");
                return;
            }
            
            StringBuilder builder;
            SerialStream stream = billAcceptor.Open();
            using (stream)
            {
                while (!CancelationToken)
                {
                    byte[] buffer = new byte[8];
                    int count = 0;

                    try
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                    }
                    catch (TimeoutException e)
                    {
                        Console.WriteLine("Timeout - new session started");
                        continue;
                    }

                    if (count > 0)
                    {
                        string converted = Convert.ToBase64String(buffer);
                        Console.WriteLine($"Evaluating: {converted}");
                        
                        switch (converted)
                        {
                            case "B/lPAj0BAZA=":
                            case "B/lPAj0EA5U=":
                            case "B/lPAj0EBJY=":
                            case "B/lPAj0BAI8=":
                                Console.WriteLine("10 kn entered");
                                break;
                            default:
                                ReactToUnknownBill();
                                break;
                        }

                        count = 0;
                    }
                }
            }
                        
            Console.WriteLine("Closing session");
        }

        private static void ReactToUnknownBill()
        {
            Console.WriteLine("Unknown Bill!");
        }
    }
}