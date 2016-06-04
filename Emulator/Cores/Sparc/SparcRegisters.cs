﻿/********************************************************
*
* Warning!
* This file was generated automatically.
* Please do not edit. Changes should be made in the
* appropriate *.tt file.
*
*/
using System;
using System.Collections.Generic;
using Emul8.Peripherals.CPU.Registers;
using Emul8.Utilities.Binding;

namespace Emul8.Peripherals.CPU
{
    public partial class Sparc
    {
        public override void SetRegisterUnsafe(int register, uint value)
        {
            SetRegisterValue32(register, value);
        }

        public override uint GetRegisterUnsafe(int register)
        {
            return GetRegisterValue32(register);
        }
        
        public override int[] GetRegisters()
        {
            return new int[] {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                11,
                12,
                13,
                14,
                15,
                16,
                17,
                18,
                19,
                20,
                21,
                22,
                23,
                24,
                25,
                26,
                27,
                28,
                29,
                30,
                31,
                32,
                33,
                34,
                35,
                36,
                37,
                38,
                39,
                40,
                41,
                42,
                43,
                44,
                45,
                46,
                47,
                48,
                49,
                50,
                51,
                52,
                53,
            };
        }

        [Register]
        public UInt32 PSR
        {
            get
            {
                return GetRegisterValue32((int)SparcRegisters.PSR);
            }
            set
            {
                SetRegisterValue32((int)SparcRegisters.PSR, value);
            }
        }

        [Register]
        public UInt32 TBR
        {
            get
            {
                return GetRegisterValue32((int)SparcRegisters.TBR);
            }
            set
            {
                SetRegisterValue32((int)SparcRegisters.TBR, value);
            }
        }

        [Register]
        public UInt32 Y
        {
            get
            {
                return GetRegisterValue32((int)SparcRegisters.Y);
            }
            set
            {
                SetRegisterValue32((int)SparcRegisters.Y, value);
            }
        }

        [Register]
        public override UInt32 PC
        {
            get
            {
                return GetRegisterValue32((int)SparcRegisters.PC);
            }
            set
            {
                SetRegisterValue32((int)SparcRegisters.PC, value);
                AfterPCSet(value);
            }
        }

        [Register]
        public UInt32 NPC
        {
            get
            {
                return GetRegisterValue32((int)SparcRegisters.NPC);
            }
            set
            {
                SetRegisterValue32((int)SparcRegisters.NPC, value);
            }
        }

        [Register]
        public UInt32 WIM
        {
            get
            {
                return GetRegisterValue32((int)SparcRegisters.WIM);
            }
            set
            {
                SetRegisterValue32((int)SparcRegisters.WIM, value);
            }
        }

        public RegistersGroup<UInt32> R { get; private set; }
        public RegistersGroup<UInt32> ASR { get; private set; }

        protected override void InitializeRegisters()
        {
            indexValueMapR = new Dictionary<int, SparcRegisters>
            {
                { 0, SparcRegisters.R0 },
                { 1, SparcRegisters.R1 },
                { 2, SparcRegisters.R2 },
                { 3, SparcRegisters.R3 },
                { 4, SparcRegisters.R4 },
                { 5, SparcRegisters.R5 },
                { 6, SparcRegisters.R6 },
                { 7, SparcRegisters.R7 },
                { 8, SparcRegisters.R8 },
                { 9, SparcRegisters.R9 },
                { 10, SparcRegisters.R10 },
                { 11, SparcRegisters.R11 },
                { 12, SparcRegisters.R12 },
                { 13, SparcRegisters.R13 },
                { 14, SparcRegisters.R14 },
                { 15, SparcRegisters.R15 },
                { 16, SparcRegisters.R16 },
                { 17, SparcRegisters.R17 },
                { 18, SparcRegisters.R18 },
                { 19, SparcRegisters.R19 },
                { 20, SparcRegisters.R20 },
                { 21, SparcRegisters.R21 },
                { 22, SparcRegisters.R22 },
                { 23, SparcRegisters.R23 },
                { 24, SparcRegisters.R24 },
                { 25, SparcRegisters.R25 },
                { 26, SparcRegisters.R26 },
                { 27, SparcRegisters.R27 },
                { 28, SparcRegisters.R28 },
                { 29, SparcRegisters.R29 },
                { 30, SparcRegisters.R30 },
                { 31, SparcRegisters.R31 },
            };
            R = new RegistersGroup<UInt32>(
                indexValueMapR.Keys,
                i => GetRegisterValue32((int)indexValueMapR[i]),
                (i, v) => SetRegisterValue32((int)indexValueMapR[i], v));

            indexValueMapASR = new Dictionary<int, SparcRegisters>
            {
                { 16, SparcRegisters.ASR16 },
                { 17, SparcRegisters.ASR17 },
                { 18, SparcRegisters.ASR18 },
                { 19, SparcRegisters.ASR19 },
                { 20, SparcRegisters.ASR20 },
                { 21, SparcRegisters.ASR21 },
                { 22, SparcRegisters.ASR22 },
                { 23, SparcRegisters.ASR23 },
                { 24, SparcRegisters.ASR24 },
                { 25, SparcRegisters.ASR25 },
                { 26, SparcRegisters.ASR26 },
                { 27, SparcRegisters.ASR27 },
                { 28, SparcRegisters.ASR28 },
                { 29, SparcRegisters.ASR29 },
                { 30, SparcRegisters.ASR30 },
                { 31, SparcRegisters.ASR31 },
            };
            ASR = new RegistersGroup<UInt32>(
                indexValueMapASR.Keys,
                i => GetRegisterValue32((int)indexValueMapASR[i]),
                (i, v) => SetRegisterValue32((int)indexValueMapASR[i], v));

        }

        [Import(Name = "tlib_set_register_value_32")]
        protected ActionInt32UInt32 SetRegisterValue32;
        [Import(Name = "tlib_get_register_value_32")]
        protected FuncUInt32Int32 GetRegisterValue32;

        private Dictionary<int, SparcRegisters> indexValueMapR;
        private Dictionary<int, SparcRegisters> indexValueMapASR;
    }

    public enum SparcRegisters
    {
        PSR = 32,
        TBR = 33,
        Y = 34,
        PC = 35,
        NPC = 36,
        WIM = 53,
        R0 = 0,
        R1 = 1,
        R2 = 2,
        R3 = 3,
        R4 = 4,
        R5 = 5,
        R6 = 6,
        R7 = 7,
        R8 = 8,
        R9 = 9,
        R10 = 10,
        R11 = 11,
        R12 = 12,
        R13 = 13,
        R14 = 14,
        R15 = 15,
        R16 = 16,
        R17 = 17,
        R18 = 18,
        R19 = 19,
        R20 = 20,
        R21 = 21,
        R22 = 22,
        R23 = 23,
        R24 = 24,
        R25 = 25,
        R26 = 26,
        R27 = 27,
        R28 = 28,
        R29 = 29,
        R30 = 30,
        R31 = 31,
        ASR16 = 37,
        ASR17 = 38,
        ASR18 = 39,
        ASR19 = 40,
        ASR20 = 41,
        ASR21 = 42,
        ASR22 = 43,
        ASR23 = 44,
        ASR24 = 45,
        ASR25 = 46,
        ASR26 = 47,
        ASR27 = 48,
        ASR28 = 49,
        ASR29 = 50,
        ASR30 = 51,
        ASR31 = 52,
    }
}
