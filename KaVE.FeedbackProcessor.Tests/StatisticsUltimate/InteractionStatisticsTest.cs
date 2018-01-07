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
using KaVE.Commons.Model.Events.UserProfiles;
using KaVE.FeedbackProcessor.StatisticsUltimate;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.StatisticsUltimate
{
    internal class InteractionStatisticsTest
    {
        [Test]
        public void DefaultValues()
        {
            var sut = new InteractionStatistics();
            Assert.AreEqual(DateTimeOffset.MinValue, sut.DayFirst);
            Assert.AreEqual(DateTimeOffset.MinValue, sut.DayLast);
            Assert.AreEqual(0, sut.NumDays);
            Assert.AreEqual(0, sut.NumMonth);
            Assert.AreEqual(0, sut.NumEventsTotal);
            Assert.AreEqual(new Dictionary<Type, int>(), sut.NumEventsDetailed);
            Assert.AreEqual(Educations.Unknown, sut.Education);
            Assert.AreEqual(Positions.Unknown, sut.Position);
            Assert.AreEqual(0, sut.NumCodeCompletion);
            Assert.AreEqual(0, sut.NumTestRuns);
            Assert.AreEqual(TimeSpan.Zero, sut.ActiveTime);

            Assert.AreNotEqual(0, sut.GetHashCode());
            Assert.AreNotEqual(1, sut.GetHashCode());
        }

        [Test]
        public void SettingValues()
        {
            var now = DateTimeOffset.Now;
            var sut = new InteractionStatistics
            {
                DayFirst = now,
                DayLast = now,
                NumDays = 1,
                NumMonth = 2,
                NumEventsDetailed = {{typeof(int), 3}},
                Education = Educations.Bachelor,
                Position = Positions.Student,
                NumCodeCompletion = 4,
                NumTestRuns = 5,
                ActiveTime = TimeSpan.FromSeconds(3)
            };
            Assert.AreEqual(now, sut.DayFirst);
            Assert.AreEqual(now, sut.DayLast);
            Assert.AreEqual(1, sut.NumDays);
            Assert.AreEqual(2, sut.NumMonth);
            Assert.AreEqual(new Dictionary<Type, int> {{typeof(int), 3}}, sut.NumEventsDetailed);
            Assert.AreEqual(Educations.Bachelor, sut.Education);
            Assert.AreEqual(Positions.Student, sut.Position);
            Assert.AreEqual(4, sut.NumCodeCompletion);
            Assert.AreEqual(5, sut.NumTestRuns);
            Assert.AreEqual(TimeSpan.FromSeconds(3), sut.ActiveTime);
        }

        [Test]
        public void NumEventsTotal_0()
        {
            var sut = new InteractionStatistics();
            Assert.AreEqual(0, sut.NumEventsTotal);
        }

        [Test]
        public void NumEventsTotal_1()
        {
            var sut = new InteractionStatistics
            {
                NumEventsDetailed =
                {
                    {typeof(int), 1}
                }
            };
            Assert.AreEqual(1, sut.NumEventsTotal);
        }

        [Test]
        public void NumEventsTotal_3()
        {
            var sut = new InteractionStatistics
            {
                NumEventsDetailed =
                {
                    {typeof(int), 1},
                    {typeof(double), 2},
                    {typeof(float), 3}
                }
            };
            Assert.AreEqual(6, sut.NumEventsTotal);
        }

        [Test]
        public void EqualityDefault()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics();
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ReallyEqual()
        {
            var now = DateTime.Now;
            var a = new InteractionStatistics
            {
                DayFirst = now,
                DayLast = now,
                NumDays = 1,
                NumMonth = 2,
                NumEventsDetailed =
                {
                    {typeof(int), 3}
                },
                Education = Educations.Bachelor,
                Position = Positions.Student,
                NumCodeCompletion = 4,
                NumTestRuns = 5,
                ActiveTime = TimeSpan.FromSeconds(3)
            };
            var b = new InteractionStatistics
            {
                DayFirst = now,
                DayLast = now,
                NumDays = 1,
                NumMonth = 2,
                NumEventsDetailed =
                {
                    {typeof(int), 3}
                },
                Education = Educations.Bachelor,
                Position = Positions.Student,
                NumCodeCompletion = 4,
                NumTestRuns = 5,
                ActiveTime = TimeSpan.FromSeconds(3)
            };
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_DayFirst()
        {
            var now = DateTime.Now;
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                DayFirst = now
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_DayLast()
        {
            var now = DateTime.Now;
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                DayLast = now
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_NumDays()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                NumDays = 1
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_NumMonth()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                NumMonth = 2
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_NumEventsDetails()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                NumEventsDetailed =
                {
                    {typeof(int), 1}
                }
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_Education()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                Education = Educations.Bachelor
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_Position()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                Position = Positions.Student
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_NumCodeCompletion()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                NumCodeCompletion = 4
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_NumTestRuns()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                NumTestRuns = 5
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Different_ActiveTime()
        {
            var a = new InteractionStatistics();
            var b = new InteractionStatistics
            {
                ActiveTime = TimeSpan.FromSeconds(3)
            };
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}