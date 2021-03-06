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
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Events.TestRunEvents;
using KaVE.Commons.Model.Events.UserProfiles;
using KaVE.Commons.Model.Events.VersionControlEvents;
using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.Commons.Utils.Assertion;

namespace KaVE.FeedbackProcessor.StatisticsUltimate
{
    public class InteractionStatisticsExtractor
    {
        public readonly List<Type> AllEventTypes = new List<Type>
        {
            typeof(CompletionEvent),
            typeof(TestRunEvent),
            typeof(UserProfileEvent),
            typeof(VersionControlEvent),

            typeof(BuildEvent),
            typeof(DebuggerEvent),
            typeof(DocumentEvent),
            typeof(EditEvent),
            typeof(FindEvent),
            typeof(IDEStateEvent),
            typeof(InstallEvent),
            typeof(SolutionEvent),
            typeof(UpdateEvent),
            typeof(WindowEvent),

            typeof(ActivityEvent),
            typeof(CommandEvent),
            typeof(ErrorEvent),
            typeof(InfoEvent),
            typeof(NavigationEvent),
            typeof(SystemEvent)
        };

        public const int TimeOutInS = 16;

        public InteractionStatistics CreateStatistics(IEnumerable<IIDEEvent> events)
        {
            var stats = new InteractionStatistics();
            var days = new HashSet<DateTimeOffset>();
            var months = new HashSet<DateTimeOffset>();

            var lastTriggeredAt = DateTimeOffset.MinValue;

            DateTimeOffset? curStart = null;
            DateTimeOffset? curEnd = null;

            foreach (var t in AllEventTypes)
            {
                stats.NumEventsDetailed[t] = 0;
            }

            foreach (var e in events)
            {
                if (e.TriggeredAt.HasValue && e.TerminatedAt.HasValue)
                {
                    // make sure it is ordered!
                    Asserts.IsLessOrEqual(lastTriggeredAt, e.TriggeredAt.Value);
                    lastTriggeredAt = e.TriggeredAt.Value;

                    // if timeout
                    if (curStart.HasValue && curEnd.Value.AddSeconds(TimeOutInS) < e.TriggeredAt.Value)
                    {
                        stats.ActiveTime += curEnd.Value - curStart.Value;
                        curStart = null;
                        curEnd = null;
                    }

                    // start new interva, if required
                    if (curStart.HasValue)
                    {
                        curEnd = curEnd > e.TerminatedAt ? curEnd : e.TerminatedAt;
                    }
                    else
                    {
                        curStart = e.TriggeredAt;
                        curEnd = e.TerminatedAt;
                    }
                }

                if (stats.NumEventsDetailed.ContainsKey(e.GetType()))
                {
                    stats.NumEventsDetailed[e.GetType()]++;
                }
                else
                {
                    stats.NumEventsDetailed[e.GetType()] = 1;
                }

                if (!e.TriggeredAt.HasValue)
                {
                    e.TriggeredAt = DateTimeOffset.MinValue;
                }

                var date = new DateTimeOffset(e.TriggeredAt.Value.Date, e.TriggeredAt.Value.Offset);

                days.Add(date);
                var firstDayOfMonth = date.AddDays(-(date.Day - 1));
                months.Add(firstDayOfMonth);

                if (stats.DayFirst == DateTimeOffset.MinValue || date < stats.DayFirst)
                {
                    stats.DayFirst = date;
                }

                if (stats.DayLast == DateTimeOffset.MinValue || date > stats.DayLast)
                {
                    stats.DayLast = date;
                }

                var upe = e as UserProfileEvent;
                if (upe != null)
                {
                    if (upe.Education != Educations.Unknown)
                    {
                        stats.Education = upe.Education;
                    }
                    if (upe.Position != Positions.Unknown)
                    {
                        stats.Position = upe.Position;
                    }
                }

                var cce = e as CompletionEvent;
                if (cce != null)
                {
                    stats.NumCodeCompletion++;
                }

                var tre = e as TestRunEvent;
                if (tre != null)
                {
                    stats.NumTestRuns++;
                }
            }

            if (curStart.HasValue)
            {
                stats.ActiveTime += curEnd.Value - curStart.Value;
            }

            stats.NumDays = days.Count;
            stats.NumMonth = months.Count;
            return stats;
        }
    }
}