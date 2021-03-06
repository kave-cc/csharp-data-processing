﻿/*
 * Copyright 2014 Technische Universität Darmstadt
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using KaVE.Commons.Utils;

namespace KaVE.FeedbackProcessor.Preprocessing.Logging
{
    public class ConsoleLogger : IPrepocessingLogger
    {
        private readonly IDateUtils _dateUtils;
        private bool _isFirstLine = true;

        public ConsoleLogger(IDateUtils dateUtils)
        {
            _dateUtils = dateUtils;
        }

        public void Log()
        {
            Log("");
        }

        public void Log(string text, params object[] args)
        {
            if (!_isFirstLine)
            {
                Console.WriteLine();
            }
            _isFirstLine = false;

            var content = args.Length == 0 ? text : string.Format(text, args);
            Console.Write(@"{0} {1}", _dateUtils.Now, content);
        }

        public void Append(string text, params object[] args)
        {
            _isFirstLine = false;
            Console.Write(args.Length == 0 ? text : string.Format(text, args));
        }
    }
}