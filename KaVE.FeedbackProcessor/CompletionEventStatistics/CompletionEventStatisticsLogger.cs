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
using System.Collections.Generic;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Naming.CodeElements;
using KaVE.Commons.Model.Naming.Types;
using KaVE.JetBrains.Annotations;

// ReSharper disable LocalizableElement

namespace KaVE.FeedbackProcessor.CompletionEventStatistics
{
    public interface ICompletionEventStatisticsLogger
    {
        void FoundZips(IList<string> zips);
        void StartingZip(string zip);
        void FoundAppliedCompletionEvents(IList<ICompletionEvent> ces);
        void Store([NotNull] IMethodName methodName);
        void FoundOtherProposal();
        void DoneWithZip();
        void Done();
    }

    public class CompletionEventStatisticsLogger : ICompletionEventStatisticsLogger
    {
        private int _numTotal;
        private int _current;

        public void FoundZips(IList<string> zips)
        {
            _numTotal = zips.Count;
            Log("Found {0} zips", _numTotal);
        }

        public void StartingZip(string zip)
        {
            Log();
            Log(
                "### ({1}/{2}) Processing '{0}' -- {3:0.0}% #####################",
                zip,
                ++_current,
                _numTotal,
                (_current - 1) / (0.01 * _numTotal));
        }

        public void FoundAppliedCompletionEvents(IList<ICompletionEvent> ces)
        {
            Log("{0} applied proposals: ", ces.Count);
        }

        private readonly Dictionary<ITypeName, int> _counts = new Dictionary<ITypeName, int>();

        public void Store(IMethodName m)
        {
            var t = m.DeclaringType;
            if (_counts.ContainsKey(t))
            {
                _counts[t]++;
            }
            else
            {
                _counts[t] = 1;
            }

            Append("x");
        }

        public void FoundOtherProposal()
        {
            Append(".");
        }

        public void DoneWithZip()
        {
            Log();
        }

        public void Done()
        {
            Log("### done (100%) #####################");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("How often was a method selected that was declared in each of the following types?");
            Console.WriteLine();
            Console.WriteLine("type\tassembly\tcount");

            var totalApplies = 0;
            var totalTypes = 0;

            foreach (var t in _counts.Keys)
            {
                totalTypes++;
                var numApplies = _counts[t];
                totalApplies += numApplies;
                Console.WriteLine("{0}\t{1}\t{2}", t.FullName, t.Assembly, numApplies);
            }

            Console.WriteLine();
            Console.WriteLine(
                "overall, we have {0} applied completions that span over {1} different types",
                totalApplies,
                totalTypes);
        }

        private static void Log()
        {
            Console.WriteLine();
        }

        private static void Log(string s, params object[] args)
        {
            Console.Write(DateTime.Now);
            Console.Write(' ');
            Console.WriteLine(s, args);
        }

        private static void Append(string s, params object[] args)
        {
            Console.Write(s, args);
        }
    }
}