﻿using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using SharpChannel.Tools;
using SharpModbus;

namespace SharpChannel.Channels.ModbusChannel
{
    class Program
    {
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine(((Exception)e.ExceptionObject).Message);
            Console.Error.Flush();
            Environment.Exit(1);
        }

        public static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var cmdline = string.Join(" ", args);

            switch (cmdline)
            {
                case "--list":
                    var names = SerialPort.GetPortNames();
                    foreach (var name in names) Console.WriteLine(name);
                    return;
            }

            var serial = new SerialPort();
            Config.Parse(serial, cmdline);
            serial.Open();

            Task.Factory.StartNew(() => CheckLoop(serial), TaskCreationOptions.LongRunning);

            var stream = new ModbusSerialStream(serial, 800);
            var tcpScanner = new ModbusTCPScanner();
            var rtuProtocol = new ModbusRTUProtocol();

            var line = Console.ReadLine();

            while (line != null)
            {
                var bytes = Convert.FromBase64String(line);
                tcpScanner.Append(bytes, 0, bytes.Length);
                var tcpRequestCommand = tcpScanner.Scan();
                while (tcpRequestCommand != null)
                {
                    var rtuRequestCommand = rtuProtocol.Wrap(tcpRequestCommand.Wrapped);
                    var rtuRequestBytes = new byte[rtuRequestCommand.RequestLength];
                    var rtuResponseBytes = new byte[rtuRequestCommand.ResponseLength];
                    rtuRequestCommand.FillRequest(rtuRequestBytes, 0);
                    stream.Write(rtuRequestBytes);
                    stream.Read(rtuResponseBytes);
                    var rtuResponse = rtuRequestCommand.ParseResponse(rtuResponseBytes, 0);
                    var tcpResponseBytes = new byte[tcpRequestCommand.ResponseLength];
                    tcpRequestCommand.FillResponse(tcpResponseBytes, 0, rtuResponse);
                    WriteLine(Convert.ToBase64String(tcpResponseBytes, 0, tcpResponseBytes.Length));
                    tcpRequestCommand = tcpScanner.Scan();
                }
                line = Console.ReadLine();
            }
        }

        private static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.Out.Flush();
        }

        private static void CheckLoop(SerialPort serial)
        {
            var disposer = new Disposer();
            disposer.Add(() => Environment.Exit(1));

            using (disposer)
            {
                //removed ports require an IO call to be detected
                while (serial.IsOpen) Thread.Sleep(10);
            }
        }
    }
}
