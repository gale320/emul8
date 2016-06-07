//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.Robot
{
    internal class SmartParser
    {
        public static SmartParser Instance = new SmartParser();

        public object Parse(object input, Type outputType)
        {
            if(input.GetType() == outputType)
            {
                return input;
            }

            ParseDelegate parser;
            if(!parsers.TryGetValue(Tuple.Create(input.GetType(), outputType), out parser))
            {
                throw new ArgumentException();
            }
            return parser(input);
        }

        public object[] Parse(object[] input, Type[] outputType)
        {
            if(input.Length != outputType.Length)
            {
                throw new ArgumentException();
            }

            var result = new object[input.Length];
            for(var i = 0; i < input.Length; i++)
            {
                result[i] = Parse(input[i], outputType[i]);
            }

            return result;
        }

        private SmartParser()
        {
        }

        private static readonly Dictionary<Tuple<Type, Type>, ParseDelegate> parsers = new Dictionary<Tuple<Type, Type>, ParseDelegate>
        {
            { Tuple.Create(typeof(string), typeof(int)), x => int.Parse((string)x) }
        };

        private delegate object ParseDelegate(object input);
    }
}

