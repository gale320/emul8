//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Threading.Tasks;

namespace Emul8.Utilities.GDB.Commands
{
    internal class KillCommand : Command
    {
        public KillCommand() : base("k")
        {
        }

        protected override PacketData HandleInner(Packet packet)
        {
            // we must call it in a separate thread in order
            // to avoid deadlock in ServerSocketTerminal serving
            // gdb stub
            // TODO: please remove it after resolving the issue
            Task.Run(() => Emulator.Exit());
            return PacketData.Success;
        }
    }
}

