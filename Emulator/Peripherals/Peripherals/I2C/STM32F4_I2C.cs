//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Core.Structure;
using Emul8.Logging;
using Emul8.Peripherals.Bus;
using System.Collections.Generic;
using System.Linq;
using Emul8.Core.Structure.Registers;

namespace Emul8.Peripherals.I2C
{
    [AllowedTranslations(AllowedTranslation.WordToDoubleWord)]
    public sealed class STM32F4_I2C : SimpleContainer<II2CPeripheral>, IDoubleWordPeripheral, IBytePeripheral, IKnownSize
    {
        public STM32F4_I2C(Machine machine) : base(machine)
        {
            EventInterrupt = new GPIO();
            ErrorInterrupt = new GPIO();
            CreateRegisters();
            Reset();
        }

        public byte ReadByte(long offset)
        {
            if((Registers)offset == Registers.Data)
            {
                byteTransferFinished.Value = false;
                Update();
                return (byte)data.Read();
            }
            else
            {
                this.LogUnhandledRead(offset);
                return 0;
            }
        }

        public void WriteByte(long offset, byte value)
        {
            if((Registers)offset == Registers.Data)
            {
                data.Write(offset, value);
            }
            else
            {
                this.LogUnhandledWrite(offset, value);
            }
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        public override void Reset()
        {
            state = State.Idle;
            EventInterrupt.Unset();
            ErrorInterrupt.Unset();

            registers.Reset();
            data.Reset();
        }

        public GPIO EventInterrupt
        {
            get;
            private set;
        }

        public GPIO ErrorInterrupt
        {
            get;
            private set;
        }

        public long Size
        {
            get
            {
                return 0x400;
            }
        }

        private void CreateRegisters()
        {
            var control1 = new DoubleWordRegister(this).WithFlag(15, writeCallback: SoftwareResetWrite, name:"SWRST").WithFlag(9, FieldMode.Read, name:"StopGen")
                .WithFlag(8, FieldMode.Read, writeCallback: StartWrite, name:"StartGen").WithFlag(0, writeCallback: PeripheralEnableWrite, name:"PeriEn");
            var control2 = new DoubleWordRegister(this).WithValueField(0, 6, name:"Freq");
            var status1 = new DoubleWordRegister(this);
            var status2 = new DoubleWordRegister(this);
            data = new DoubleWordRegister(this);

            acknowledgeEnable = control1.DefineFlagField(10);

            bufferInterruptEnable = control2.DefineFlagField(10, changeCallback: InterruptEnableChange);
            eventInterruptEnable = control2.DefineFlagField(9, changeCallback: InterruptEnableChange);
            errorInterruptEnable = control2.DefineFlagField(8);

            dataRegister = data.DefineValueField(0, 8, readCallback: DataRead, writeCallback: DataWrite);

            acknowledgeFailed = status1.DefineFlagField(10, FieldMode.ReadToClear | FieldMode.WriteZeroToClear, changeCallback: (_,__) => Update());
            dataRegisterEmpty = status1.DefineFlagField(7, FieldMode.Read);
            dataRegisterNotEmpty = status1.DefineFlagField(6, FieldMode.Read);
            byteTransferFinished = status1.DefineFlagField(2, FieldMode.Read);
            addressSentOrMatched = status1.DefineFlagField(1, FieldMode.Read, readCallback: (_,__) => {
                if(state == State.StartInitiated)
                {
                    state = State.AwaitingAddress;
                }
            });
            startBit = status1.DefineFlagField(0, FieldMode.Read);

            transmitterReceiver = status2.DefineFlagField(2, FieldMode.Read);
            masterSlave = status2.DefineFlagField(0, FieldMode.Read, readCallback: (_,__) => {
                addressSentOrMatched.Value = false;
                dataRegisterNotEmpty.Value |= state == State.HasDataToRead;
                Update();
            });

            var registerDictionary = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.RiseTime, DoubleWordRegister.CreateRWRegister(0x2)},
                {(long)Registers.ClockControl, DoubleWordRegister.CreateRWRegister()},
                {(long)Registers.OwnAddress1, DoubleWordRegister.CreateRWRegister()},
                {(long)Registers.OwnAddress2, DoubleWordRegister.CreateRWRegister()},
                {(long)Registers.NoiseFilter, DoubleWordRegister.CreateRWRegister()},
                {(long)Registers.Control1, control1},
                {(long)Registers.Control2, control2},
                {(long)Registers.Status1, status1},
                {(long)Registers.Status2, status2},
                {(long)Registers.Data, data},
            };
            registers = new DoubleWordRegisterCollection(this, registerDictionary);
        }

        private void InterruptEnableChange(bool oldValue, bool newValue)
        {
            machine.ExecuteIn(Update);
        }

        private void Update()
        {
            EventInterrupt.Set(eventInterruptEnable.Value && (startBit.Value || addressSentOrMatched.Value || byteTransferFinished.Value
                || (bufferInterruptEnable.Value && (dataRegisterEmpty.Value || dataRegisterNotEmpty.Value))));
            ErrorInterrupt.Set(errorInterruptEnable.Value && acknowledgeFailed.Value);
        }

        private void DataRead(uint oldValue, uint newValue)
        {
            if(dataToReceive != null && dataToReceive.Any())
            {
                dataRegister.Value = dataToReceive.Dequeue();
            }
            if(state == State.HasDataToRead)
            {
                state = State.AwaitingRestartOrStop;
            }
            if(dataToReceive != null && dataToReceive.Count  < 4)
            {
                byteTransferFinished.Value = true;
            }
            dataRegisterNotEmpty.Value = dataToReceive != null && dataToReceive.Any();
            Update();
        }

        private void DataWrite(uint oldValue, uint newValue)
        {
            //moved from WriteByte
            byteTransferFinished.Value = false;
            Update();
            this.Log(LogLevel.Error, "writing data from {0}", machine.SystemBus.FindSymbolAt(machine.SystemBus.GetCurrentCPU().PC));
            switch(state)
            {
            case State.AwaitingAddress:
                startBit.Value = false;
                willReadOnSelectedSlave = (newValue & 1) == 1; //LSB is 1 for read and 0 for write
                var address = (int)(newValue >> 1);
                if(ChildCollection.ContainsKey(address))
                {
                    selectedSlave = ChildCollection[address];
                    addressSentOrMatched.Value = true; //Note: ADDR is not set after a NACK reception - from documentation

                    transmitterReceiver.Value = !willReadOnSelectedSlave; //true when transmitting

                    if(willReadOnSelectedSlave)
                    {
                        dataToReceive = new Queue<byte>(selectedSlave.Read());
                        dataRegister.Value = 0u;
                        if(dataToReceive.Any())
                        {
                            dataRegister.Value = dataToReceive.Dequeue();
                            dataRegisterNotEmpty.Value = true;
                        }
                        state = State.HasDataToRead;
                    }
                    else
                    {
                        state = State.AwaitingData;
                        dataToTransfer = new List<byte>();
                    }
                }
                else
                {
                    state = State.Idle;
                    acknowledgeFailed.Value = true;
                }
                machine.ExecuteIn(Update);
                break;
            case State.AwaitingData:
                dataToTransfer.Add((byte)newValue);
                dataRegisterEmpty.Value = true;
                machine.ExecuteIn(() =>
                {
                    byteTransferFinished.Value = true;
                    state = State.AwaitingRestartOrStop;
                    Update();
                });
                Update();
                break;
            default:
                this.Log(LogLevel.Warning, "Writing {0} to DataRegister in unsupported state {1}.", newValue, state);
                break;
            }
        }

        private void SoftwareResetWrite(bool oldValue, bool newValue)
        {
            if(newValue)
            {
                Reset();
            }
        }

        private void StartWrite(bool oldValue, bool newValue)
        {
            if(selectedSlave != null && dataToTransfer != null && dataToTransfer.Count > 0)
            {
                selectedSlave.Write(dataToTransfer.ToArray());
                dataToTransfer.Clear();
            }
            //TODO: TRA cleared on repeated Start condition. Is this always here?
            transmitterReceiver.Value = false;
            dataRegisterEmpty.Value = true;
            byteTransferFinished.Value = false;
            startBit.Value = true;
            if(newValue)
            {
                switch(state)
                {
                case State.Idle:
                case State.AwaitingRestartOrStop:
                case State.AwaitingData: //HACK! Should not be here, forced by ExecuteIn somehow.
                    state = State.StartInitiated;
                    masterSlave.Value = true;
                    Update();
                    break;
                }
            }
        }

        private void PeripheralEnableWrite(bool oldValue, bool newValue)
        {
            if(!newValue)
            {
                acknowledgeEnable.Value = false;
                masterSlave.Value = false;
                acknowledgeFailed.Value = false;
                transmitterReceiver.Value = false;
                dataRegisterEmpty.Value = false;
                byteTransferFinished.Value = false;
                Update();
            }
        }

        private DoubleWordRegister data;
        private IFlagRegisterField acknowledgeEnable;
        private IFlagRegisterField bufferInterruptEnable, eventInterruptEnable, errorInterruptEnable;
        private IValueRegisterField dataRegister;
        private IFlagRegisterField acknowledgeFailed, dataRegisterEmpty, dataRegisterNotEmpty, byteTransferFinished, addressSentOrMatched, startBit;
        private IFlagRegisterField transmitterReceiver, masterSlave;

        private DoubleWordRegisterCollection registers;

        private State state;
        private List<byte> dataToTransfer;
        private Queue<byte> dataToReceive;
        private bool willReadOnSelectedSlave;
        private II2CPeripheral selectedSlave;

        private enum Registers
        {
            Control1 = 0x0,
            Control2 = 0x4,
            OwnAddress1 = 0x8,
            OwnAddress2 = 0xC,
            Data = 0x10,
            Status1 = 0x14,
            Status2 = 0x18,
            ClockControl = 0x1C,
            RiseTime = 0x20,
            NoiseFilter = 0x24,
        }

        private enum State
        {
            Idle,
            StartInitiated,
            AwaitingAddress,
            AwaitingData,
            AwaitingRestartOrStop,
            HasDataToRead
        }
    }
}
