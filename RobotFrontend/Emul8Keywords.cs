//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using AntShell.Commands;
using Emul8.Core;
using Emul8.Robot;
using Emul8.UserInterface;
using Emul8.Utilities;

namespace Emul8.RobotFrontend
{
    internal class Emul8Keywords : IRobotFrameworkKeywordProvider
    {
        public Emul8Keywords()
        {
            interaction = new MemoryCommandInteraction();
            TypeManager.Instance.PluginManager.Init("CLI");
            monitor = new Monitor();
            monitor.Interaction = interaction;
        }

        public void Dispose()
        {
        }

        [RobotFrameworkKeyword]
        public void StartEmulation()
        {
            EmulationManager.Instance.CurrentEmulation.StartAll();
        }

        [RobotFrameworkKeyword]
        public string ExecuteCommand(string command)
        {
            if(!monitor.Parse(command))
            {
                throw new KeywordException("Could not execute command: {0}", interaction.Error);
            }

            return interaction.Output;
        }

        [RobotFrameworkKeyword]
        public void SetEmul8Variable(string variableName, string path)
        {
            ExecuteCommand("${0}=@{1}".FormatWith(variableName, path));
        }

        [RobotFrameworkKeyword]
        public void AddBreakpoint(string function)
        {
            ExecuteCommand("sysbus.cpu AddBreakpoint `sysbus GetSymbolAddress \"{0}\"`".FormatWith(function));
        }

        [RobotFrameworkKeyword]
        public void ContinueFromBreakpoint()
        {
            ExecuteCommand("sysbus.cpu RemoveBreakpoint `sysbus.cpu PC`; s");
        }

        [RobotFrameworkKeyword]
        public void MachineShouldBePaused()
        {
            if(!monitor.Machine.IsPaused)
            {
                throw new Exception();
            }
        }

        [RobotFrameworkKeyword]
        public string FindCurrentSymbol()
        {
            return ExecuteCommand("sysbus FindSymbolAt `sysbus.cpu PC`");
        }

        [RobotFrameworkKeyword]
        public string ExecuteScript(string path)
        {
            if(!monitor.TryExecuteScript(path))
            {
                throw new KeywordException("Could not execute script: {0}", interaction.Error);
            }

            return interaction.Output;
        }

        [RobotFrameworkKeyword]
        public void Quit()
        {
            RobotFrontend.Shutdown();
        }

        private readonly Monitor monitor;
        private readonly MemoryCommandInteraction interaction;

        private class MemoryCommandInteraction : ICommandInteraction
        {
            public MemoryCommandInteraction()
            {
                output = new StringBuilder();
                error = new StringBuilder();
            }

            public string Output
            {
                get
                {
                    lock(output)
                    {
                        var result = output.ToString();
                        output.Clear();
                        return result;
                    }
                }
            }

            public string Error
            {
                get
                {
                    lock(error)
                    {
                        var result = error.ToString();
                        error.Clear();
                        return result;
                    }
                }
            }

            public string CommandToExecute { get; set; }

            public bool QuitEnvironment { get; set; }

            public string ReadLine()
            {
                throw new InvalidOperationException("Reading from memory command interaction is impossible");
            }

            public void Write(char c, ConsoleColor? color = default(ConsoleColor?))
            {
                lock(output)
                {
                    output.Append(c);
                }
            }

            public void WriteError(string errorMessage)
            {
                lock(error)
                {
                    error.Append(errorMessage);
                }
            }

            private readonly StringBuilder output;
            private readonly StringBuilder error;
        }
    }
}

