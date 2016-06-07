//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Linq;
using System.Reflection;

namespace Emul8.Robot
{
    internal class Keyword
    {
        public Keyword(KeywordManager manager, MethodInfo info)
        {
            this.manager = manager;
            methodInfo = info;
        }

        public object Execute(object[] arguments)
        {
            var obj = manager.GetOrCreateObject(methodInfo.DeclaringType);
            return methodInfo.Invoke(obj, SmartParser.Instance.Parse(arguments, methodInfo.GetParameters().Select(x => x.ParameterType).ToArray()));
        }

        private readonly MethodInfo methodInfo;
        private readonly KeywordManager manager;
    }
}

