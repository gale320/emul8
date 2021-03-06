//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Logging;
using Emul8.Core;

namespace Emul8.Peripherals.Input
{
    public class PS2Keyboard : IPS2Peripheral, IKeyboard
    {
        public PS2Keyboard(Machine machine)
        {
            this.machine = machine;
            data = new Queue<byte>();
            Reset();
            data.Enqueue((byte)Command.SelfTestPassed);
        }

        public byte Read()
        {
            if(data.Count > 0)
            {
                var result = data.Dequeue();
                NotifyParent();
                return result;
            }
            this.Log(LogLevel.Warning, "Attempted to read while no data in buffer. Returning 0.");
            return 0;
        }

        public void Write(byte value)
        {
            switch((Command)value)
            {
            case Command.Reset:
                Reset();
                lock(data)
                {
                    SendAck();
                    data.Enqueue((byte)Command.SelfTestPassed);
                }
                break;
            case Command.ReadId:
                lock(data)
                {
                    SendAck();
                    data.Enqueue((byte) (deviceId >> 8));
                    data.Enqueue((byte) (deviceId & 0xff));
                }
                break;
            default:
                this.Log(LogLevel.Warning, "Unhandled PS2 keyboard command: {0}", value);
                break;
            }
        }

        public void Press(KeyScanCode scanCode)
        {
            machine.ReportForeignEvent(scanCode, PressInner);
        }

        public void Release(KeyScanCode scanCode)
        {
            machine.ReportForeignEvent(scanCode, ReleaseInner);
        }

        public void Reset()
        {
            data.Clear();
        }

        public IPS2Controller Controller { get; set; }

        private void PressInner(KeyScanCode scanCode)
        {
            var key = PS2ScanCodeTranslator.Instance.GetCode(scanCode);
            data.Enqueue((byte)(key & 0x7f));
            NotifyParent();
        }

        private void ReleaseInner(KeyScanCode scanCode)
        {
            var key = PS2ScanCodeTranslator.Instance.GetCode(scanCode);
            data.Enqueue((byte)Command.Release);
            data.Enqueue((byte)(key & 0x7f));
            NotifyParent();

        }

        private void SendAck()
        {
            data.Enqueue((byte)Command.Acknowledge);
            NotifyParent();
        }

        private void NotifyParent()
        {
            if(Controller != null)
            {
                if(data.Count > 0)
                {
                    Controller.Notify();
                }
            }
            else
            {
                this.Log(LogLevel.Noisy, "PS2 device not connected to any controller issued an update.");
            }
        }

        private readonly Queue<byte> data;
        private readonly Machine machine;
        private const ushort deviceId = 0xABBA;

        private enum Command
        {
            Reset = 0xFF,
            Acknowledge = 0xFA,
            ReadId = 0xF2,
            SetResetLeds = 0xED,
            Release = 0xF0,
            SelfTestPassed = 0xAA,
        }
    }
}
