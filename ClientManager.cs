﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace O2Yolo
{
    class ClientManager
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        public WebsocketServer ws;
        public Dictionary<string, byte[]> SptFiles = new Dictionary<string, byte[]>();
        public static int MAX_PAYLOAD = 10248;
        public static byte[] _data = new byte[MAX_PAYLOAD]; // Max payload
        public static bool flag = false;

        public ClientManager(short port)
        {
            Console.WriteLine("Loading Channel1.spt");
            byte[] Channel1 = File.ReadAllBytes(Application.StartupPath + @"\Spt\Channel1.Spt");
            SptFiles.Add("Channel1", Channel1);

            Console.WriteLine("Loading MusicList.spt");
            byte[] MusicList = File.ReadAllBytes(Application.StartupPath + @"\Spt\MusicList.Spt");
            SptFiles.Add("MusicList", MusicList);

            Console.WriteLine("Loading D007.spt");
            byte[] D007 = File.ReadAllBytes(Application.StartupPath + @"\Spt\D007.Spt");
            SptFiles.Add("D007", D007);

            Console.WriteLine("Loading D207.spt");
            byte[] D207 = File.ReadAllBytes(Application.StartupPath + @"\Spt\D207.Spt");
            SptFiles.Add("D207", D207);

            ws = new WebsocketServer(port);
            ws.Start();
            ws.OnUpdateStatus += this.onState;
        }

        private void onState(Object o, TCPData e)
        {
            _data = new byte[MAX_PAYLOAD];

            var state = e.State;
            ushort cmd = ToUint16(e.Buffer, 2);
            ushort length = state.Buffer[0];

            Console.WriteLine("[ DEBUG ] Got " + cmd + " which " + Convert.ToString(cmd, 16));
            Console.WriteLine();

            switch (cmd)
            {
                case 0x03f1:
                    {
                        Console.WriteLine("A client logged in!");
                        byte[] packets =
                        {
                            0x37, 0x00, 0xf2, 0x03, 0x02, 0x00, 0x00, 0x5d,
                            0xfe, 0xda, 0xad, 0xf5, 0x7f, 0x6b, 0x0e, 0x49,
                            0x2c, 0x3b, 0xba, 0x56, 0x17, 0xbb, 0x8b, 0x4c,
                            0x1d, 0x07, 0x28, 0x80, 0xd2, 0x51, 0x0c, 0xda,
                            0x54, 0x4a, 0xd1, 0x50, 0x35, 0x61, 0xa8, 0xfe,
                            0x67, 0xb5, 0xaa, 0xe1, 0x8b, 0x5d, 0x7c, 0x7b,
                            0x2a, 0xac, 0x22, 0xc3, 0x02, 0xf8, 0x1e
                        };

                        Write(state, packets);
                        break;
                    }

                case 0x03f3:
                    {
                        Console.WriteLine("A client login again!");
                        byte[] packets =
                        {
                            0x37, 0x00, 0xf4, 0x03, 0x40, 0xba, 0x11, 0x36,
                            0x84, 0x0d, 0x40, 0x7b, 0x78, 0x64, 0x2a, 0xc9,
                            0xc5, 0x19, 0xcc, 0xaa, 0x7d, 0xb1, 0x65, 0x3b,
                            0x70, 0x1e, 0x6c, 0x18, 0x58, 0x0f, 0x05, 0x22,
                            0xd8, 0x08, 0xc8, 0xd7, 0x1c, 0x15, 0x36, 0x84,
                            0x0d, 0x40, 0x7b, 0x78, 0x64, 0x2a, 0xc9, 0xc5,
                            0x19, 0xcc, 0xaa, 0x7d, 0xb1, 0x65, 0xef
                        };

                        Write(state, packets);
                        break;
                    }

                case 0xfff0:
                    {
                        Console.WriteLine("A logout or idle request?");
                        ws.Close(state);
                        break;
                    }

                case 0x03ec:
                    {
                        Console.WriteLine("Something after login!");
                        byte[] packets =
                        {
                            0x10, 0x00, 0xed, 0x03, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x87, 0x53, 0x00, 0x00
                        };

                        Write(state, packets);
                        break;
                    }

                case 0x03ea:
                    {
                        Console.WriteLine("Getting channel details");
                        int count = Copy(_data, SptFiles["Channel1"], 0);
                        DoWrite(state, count);
                        break;
                    }

                case 0x0fbe:
                    {
                        Console.WriteLine("MusicList");
                        int count = Copy(_data, SptFiles["MusicList"], 6);
                        DoWrite(state, count);
                        break;
                    }

                case 0x07d0:
                    {
                        Console.WriteLine("D007");
                        int count = Copy(_data, SptFiles["D007"], 20);
                        DoWrite(state, count);
                        break;
                    }

                case 0x07d2:
                    { 
                        Console.WriteLine("Entering lobby");
                        string _packets1 = "\x2e\x00\xdd\x07" +
                            "\x8c\x6e\x3f\x8f\xc1\x91" +
                            "\xa7\x00" +
                            "server emu by djask for comp6841" +
                            "\x00\x00";

                        byte[] packets = new byte[]
                        {
                            0x2e, 0x0, 0xdd, 0x07, 
                            0x8c, 0x6e, 0x3f, 0x8f, 0xc1, 0x91,
                            0xa7, 0x0,
                            0x4f, 0x32, 0x2d, 0x4a, 0x41, 0x4d, 0x20, 0x45, 0x6d, 0x75, 0x6c,
                            0x61, 0x74, 0x6f, 0x72, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                            0x0, 0x0
                        };

                        Buffer.BlockCopy(packets, 0, _data, 0, packets[0]);
                        int count = Copy(_data, SptFiles["D207"], 20, packets.Length);


                        DoWrite(state, count);
                        break;
                    }

                case 0x13a4:
                    {
                        Console.WriteLine("No idea lol");
                        byte[] packets =
                        {
                            0x08, 0x00, 0xa5, 0x13, 0xb3, 0x11, 0x01, 0x00
                        };

                        Write(state, packets);
                        break;
                    }

                case 0x07dc:
                    {
                        Console.WriteLine("Just a message, skip it to next packets!");
                        ws.ReadAgain(state);
                        break;
                    }

                case 0x03e8:
                    {
                        Console.WriteLine("Channel login!");
                        byte[] packets =
                        {
                            0x08, 0x00, 0xe9, 0x03, 0x00, 0x00, 0x00, 0x00
                        };

                        Write(state, packets);
                        break;
                    }

                case 0x07e8:
                    {
                        Console.WriteLine("Massive payload it says, but whatever");
                        byte[] packets =
                        {
                            0x04, 0x00, 0xe9, 0x07
                        };
                        Write(state, packets);
                        break;
                    }

                case 0x03ef:
                    {
                        Console.WriteLine("Got login credentials");
                        byte[] packets =
                        {
                            0x0c, 0x00, 0xf0, 0x03, 0x00, 0x00, 0x00, 0x00,
                            0xee, 0x60, 0x01, 0x00
                        };

                        Write(state, packets);
                        break;
                    }

                case 0x1771:
                    {
                        Console.WriteLine("TCP Echo");
                        byte[] packets = new byte[length];
                        Buffer.BlockCopy(state.Buffer, 0, packets, 0, length);

                        Write(state, packets);
                        break;
                    }

                default:
                    {
                        Console.WriteLine("Unknown packets {0}", cmd);
                        break;
                    }
            }
        }

        public int Copy(byte[] res, byte[] file, int offset, int resOffset = -1)
        {
            byte[] newFile = file.Skip(offset).ToArray();

            if (resOffset == -1)
            {
                Buffer.BlockCopy(newFile, 0, res, 0, newFile.Length);
            } else
            {
                Buffer.BlockCopy(newFile, 0, res, resOffset, newFile.Length);
            }

            return newFile.Length;
        }
    
        public void Write(WebsocketServer.SocketState state, byte[] payload)
        {
            ushort length = payload[0];
            Buffer.BlockCopy(payload, 0, _data, 0, payload[0]);
            DoWrite(state, length);
        }

        public void DoWrite(WebsocketServer.SocketState state, int length)
        {
            byte[] DataToSend = new byte[length];
            Buffer.BlockCopy(_data, 0, DataToSend, 0, length);

            ws.SendData(state, DataToSend);
        }

        public static ushort ToUint16(byte[] Buffer, int startIndex)
        {
            ushort value = BitConverter.ToUInt16(Buffer, startIndex);

            return value;
        }
    }
}
