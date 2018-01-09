/*
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
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Events.TestRunEvents;
using KaVE.Commons.Model.Events.UserProfiles;
using KaVE.Commons.Model.Events.VersionControlEvents;
using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.FeedbackProcessor.StatisticsUltimate;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.StatisticsUltimate
{
    internal class InteractionStatisticsExtractorTest
    {
        private Random _rng;

        private InteractionStatisticsExtractor _sut;

        [SetUp]
        public void Setup()
        {
            _rng = new Random();
            _sut = new InteractionStatisticsExtractor();
        }

        [Test]
        public void SomeNumbers()
        {
            var es = new List<IDEEvent>
            {
                EventAt(1234, 5, 6),
                EventAt(1234, 5, 6),
                EventAt(1234, 5, 7),
                EventAt(1234, 8, 9)
            };
            var actual = _sut.CreateStatistics(es);
            Assert.AreEqual(4, actual.NumEventsTotal);
            Assert.AreEqual(3, actual.NumDays);
            Assert.AreEqual(2, actual.NumMonth);
            Assert.AreEqual(GetTime_Exact(1234, 5, 6), actual.DayFirst);
            Assert.AreEqual(GetTime_Exact(1234, 8, 9), actual.DayLast);
        }

        [Test]
        public void SomeNumbers_Reverse()
        {
            var es = new List<IDEEvent>
            {
                EventAt(1234, 8, 9),
                EventAt(1234, 5, 7),
                EventAt(1234, 5, 6),
                EventAt(1234, 5, 6)
            };
            var actual = _sut.CreateStatistics(es);
            Assert.AreEqual(4, actual.NumEventsTotal);
            Assert.AreEqual(3, actual.NumDays);
            Assert.AreEqual(2, actual.NumMonth);
            Assert.AreEqual(GetTime_Exact(1234, 5, 6), actual.DayFirst);
            Assert.AreEqual(GetTime_Exact(1234, 8, 9), actual.DayLast);
        }

        [Test]
        public void DoesNotCrashForAbsentTriggerDates()
        {
            var e = EventAt(1234, 8, 9);
            e.TriggeredAt = null;

            var actual = _sut.CreateStatistics(
                new List<IDEEvent>
                {
                    e
                });
            Assert.AreEqual(1, actual.NumEventsTotal);
            Assert.AreEqual(1, actual.NumDays);
            Assert.AreEqual(1, actual.NumMonth);
            Assert.AreEqual(DateTimeOffset.MinValue, actual.DayFirst);
            Assert.AreEqual(DateTimeOffset.MinValue, actual.DayLast);
        }

        [Test]
        public void StoresLastEducationAndPosition()
        {
            var es = new List<IDEEvent>
            {
                new UserProfileEvent
                {
                    Education = Educations.Bachelor,
                    Position = Positions.Student
                },
                new UserProfileEvent
                {
                    Education = Educations.Master,
                    Position = Positions.SoftwareEngineer
                }
            };
            var actual = _sut.CreateStatistics(es);
            Assert.AreEqual(Educations.Master, actual.Education);
            Assert.AreEqual(Positions.SoftwareEngineer, actual.Position);
        }

        [Test]
        public void UnknownEducationAndPositionDoNotOverrideFormerValues()
        {
            var es = new List<IDEEvent>
            {
                new UserProfileEvent
                {
                    Education = Educations.Bachelor,
                    Position = Positions.Student
                },
                new UserProfileEvent
                {
                    Education = Educations.Unknown,
                    Position = Positions.Unknown
                }
            };
            var actual = _sut.CreateStatistics(es);
            Assert.AreEqual(Educations.Bachelor, actual.Education);
            Assert.AreEqual(Positions.Student, actual.Position);
        }

        [Test]
        public void CountsCompletionEvents()
        {
            var es = new List<IDEEvent>
            {
                new CompletionEvent()
            };
            var actual = _sut.CreateStatistics(es);
            Assert.AreEqual(1, actual.NumCodeCompletion);
        }

        [Test]
        public void CountsTestRunEvents()
        {
            var es = new List<IDEEvent>
            {
                new TestRunEvent()
            };
            var actual = _sut.CreateStatistics(es);
            Assert.AreEqual(1, actual.NumTestRuns);
        }

        [Test]
        public void ActiveTimeDefault()
        {
            var actual = Analyze();
            Assert.AreEqual(TimeSpan.Zero, actual.ActiveTime);
        }

        [Test]
        public void ActiveTime_IsAdded()
        {
            var actual = Analyze(E(1, 1), E(2, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(3), actual.ActiveTime);
        }

        [Test]
        public void ActiveTime_ShortBreaksAreMerged()
        {
            var actual = Analyze(E(1, 1), E(3, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(4), actual.ActiveTime);
        }

        [Test]
        public void ActiveTime_LargeBreaksAreSeparated()
        {
            var actual = Analyze(E(1, 1), E(31, 2));
            Assert.AreEqual(TimeSpan.FromSeconds(3), actual.ActiveTime);
        }

        [Test]
        public void ActiveTime_Complex()
        {
            // 1234567890123456789012345678901234567890123456789012345678901234567890
            // xxxxxxxxxx
            //  xxx
            //          xx                |
            //                              x                |
            //                                                  xxx
            //                                                      xxxxx
            var actual = Analyze(E(1, 10), E(2, 3), E(10, 2), E(30, 1), E(50, 3), E(54, 5));
            // test is flaky without this implicit rounding
            var diff = Math.Round((actual.ActiveTime - TimeSpan.FromSeconds(21)).TotalSeconds);
            Assert.AreEqual(0, diff);
        }

        [Test]
        public void NumberOfEvents_AllAreAlwaysRegistered()
        {
            var actual = Analyze().NumEventsDetailed;

            var expected = new Dictionary<Type, int>
            {
                {typeof(CompletionEvent), 0},
                {typeof(TestRunEvent), 0},
                {typeof(UserProfileEvent), 0},
                {typeof(VersionControlEvent), 0},

                {typeof(BuildEvent), 0},
                {typeof(DebuggerEvent), 0},
                {typeof(DocumentEvent), 0},
                {typeof(EditEvent), 0},
                {typeof(FindEvent), 0},
                {typeof(IDEStateEvent), 0},
                {typeof(InstallEvent), 0},
                {typeof(SolutionEvent), 0},
                {typeof(UpdateEvent), 0},
                {typeof(WindowEvent), 0},

                {typeof(ActivityEvent), 0},
                {typeof(CommandEvent), 0},
                {typeof(ErrorEvent), 0},
                {typeof(InfoEvent), 0},
                {typeof(NavigationEvent), 0},
                {typeof(SystemEvent), 0}
            };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NumberOfEvents_AllAreCounted()
        {
            var eventTypes = new List<IDEEvent>
            {
                new CompletionEvent(),
                new TestRunEvent(),
                new UserProfileEvent(),
                new VersionControlEvent(),

                new BuildEvent(),
                new DebuggerEvent(),
                new DocumentEvent(),
                new EditEvent(),
                new FindEvent(),
                new IDEStateEvent(),
                new InstallEvent(),
                new SolutionEvent(),
                new UpdateEvent(),
                new WindowEvent(),

                new ActivityEvent(),
                new CommandEvent(),
                new ErrorEvent(),
                new InfoEvent(),
                new NavigationEvent(),
                new SystemEvent()
            };

            var count = 1;
            var events = new List<IIDEEvent>();
            foreach (var t in eventTypes)
            {
                for (var i = 0; i < count; i++)
                {
                    events.Add(t);
                }
                count++;
            }
            var actual = Analyze(events.ToArray()).NumEventsDetailed;

            Assert.AreEqual(eventTypes.Count, actual.Keys.Count);
            count = 1;
            foreach (var t in eventTypes.Select(t => t.GetType()))
            {
                Assert.AreEqual(count, actual[t]);
                count++;
            }
        }

        private static IIDEEvent E(int startTimeInS, int duration)
        {
            var startTime = DateTimeOffset.MinValue.AddSeconds(startTimeInS);
            return new WindowEvent
            {
                TriggeredAt = startTime,
                Duration = startTime.AddSeconds(duration) - startTime
            };
        }

        private IInteractionStatistics Analyze(params IIDEEvent[] es)
        {
            return _sut.CreateStatistics(es);
        }

        private ActivityEvent EventAt(int year, int month, int day)
        {
            return new ActivityEvent {TriggeredAt = GetTime(year, month, day)};
        }

        private DateTimeOffset GetTime(int year, int month, int day)
        {
            var date =
                DateTimeOffset.MinValue.AddYears(year - 1)
                              .AddMonths(month - 1)
                              .AddDays(day - 1)
                              .AddSeconds(_rng.Next() % 60);
            return date;
        }

        private DateTimeOffset GetTime_Exact(int year, int month, int day)
        {
            var date =
                DateTimeOffset.MinValue.AddYears(year - 1)
                              .AddMonths(month - 1)
                              .AddDays(day - 1);
            return date;
        }
    }
}