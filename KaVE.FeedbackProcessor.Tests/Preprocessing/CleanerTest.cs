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
using System.IO;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils.Assertion;
using KaVE.Commons.Utils.Collections;
using KaVE.Commons.Utils.IO.Archives;
using KaVE.FeedbackProcessor.Preprocessing;
using KaVE.FeedbackProcessor.Preprocessing.Filters;
using KaVE.FeedbackProcessor.Preprocessing.Fixers;
using KaVE.FeedbackProcessor.Preprocessing.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Preprocessing
{
    internal class CleanerTest : FileBasedPreprocessingTestBase
    {
        #region setup and helper

        private Cleaner _sut;
        private ICleanerLogger _log;
        private IList<IDictionary<string, int>> _reportedCounts;
        private IKaVEList<string> _registeredFilters;
        private IKaVEList<string> _registeredFixers;

        [SetUp]
        public void Setup()
        {
            _reportedCounts = new List<IDictionary<string, int>>();
            _registeredFilters = Lists.NewList<string>();
            _registeredFixers = Lists.NewList<string>();

            _log = Mock.Of<ICleanerLogger>();
            Mock.Get(_log)
                .Setup(l => l.FinishedWriting(It.IsAny<IDictionary<string, int>>()))
                .Callback<IDictionary<string, int>>(d => _reportedCounts.Add(d));
            Mock.Get(_log)
                .Setup(l => l.RegisteredConfig(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Callback<IEnumerable<string>, IEnumerable<string>>(
                    (filters, fixers) =>
                    {
                        _registeredFilters.AddAll(filters);
                        _registeredFixers.AddAll(fixers);
                    });

            _sut = new Cleaner(Io, _log);
        }

        private static IDEEvent E(string id, int timeOffset)
        {
            return new CommandEvent
            {
                CommandId = id,
                TriggeredAt = DateTimeOffset.MinValue.AddSeconds(timeOffset)
            };
        }

        private void Add(string relFile, params IDEEvent[] events)
        {
            var file = Path.Combine(MergedDir, relFile);
            WriteZip(file, events);
        }

        private void Clean(string relZip)
        {
            _sut.Clean(relZip);
        }

        private void AssertEvents(string relFile, params IDEEvent[] expecteds)
        {
            var file = Path.Combine(FinalDir, relFile);
            var actuals = ReadZip<IDEEvent>(file);
            CollectionAssert.AreEqual(expecteds, actuals);
        }

        #endregion

        [Test]
        public void NoFiltersAndFixersByDefault()
        {
            Assert.NotNull(_sut.Filters);
            Assert.NotNull(_sut.Fixers);
            CollectionAssert.AreEqual(new IFilter[] { }, _sut.Filters);
            CollectionAssert.AreEqual(new IFixer[] { }, _sut.Fixers);
        }

        [Test]
        public void HappyPath()
        {
            Add("a", E("a", 1));

            Clean("a");

            AssertEvents("a", E("a", 1));
        }

        [Test]
        public void DuplicatesAreRemoved()
        {
            Add("a", E("a", 1), E("a", 1));

            Clean("a");

            AssertEvents("a", E("a", 1));
        }

        [Test]
        public void EventsAreOrdered()
        {
            Add("a", E("a", 2), E("b", 1));

            Clean("a");

            AssertEvents("a", E("b", 1), E("a", 2));
        }

        [Test]
        public void SubfoldersWork()
        {
            Add(@"sub\a", E("a", 2), E("b", 1));

            Clean(@"sub\a");

            AssertEvents(@"sub\a", E("b", 1), E("a", 2));
        }

        [Test]
        public void FiltersCanBeAddedAndTheyAreExecuted()
        {
            _sut.Filters.Add(new TestFilter("b"));

            Add("a", E("a", 1), E("b", 2), E("c", 3));

            Clean("a");

            AssertEvents("a", E("a", 1), E("c", 3));
        }

        [Test]
        public void FixersCanBeAddedAndTheyAreExecuted()
        {
            _sut.Fixers.Add(new TestFixer("b"));

            Add("a", E("a", 10), E("b", 20), E("c", 30));

            Clean("a");

            AssertEvents("a", E("a", 10), E("b", 20), E("b", 21), E("c", 30));
        }

        [Test]
        public void DeserializationIssuesNoNotCrashTheCleanerAndAreReported()
        {
            var zip = Path.Combine(MergedDir, "a.zip");
            using (var wa = new WritingArchive(zip))
            {
                wa.Add(E("a", 10));
                wa.AddAsPlainText("xxx");
                wa.Add(E("a", 20));
                wa.AddAsPlainText("yyy");
                wa.Add(E("a", 30));
            }

            Clean("a.zip");

            AssertEvents("a.zip", E("a", 10), E("a", 20), E("a", 30));
            Mock.Get(_log).Verify(
                l => l.DeserializationError(zip, "1.json", It.IsAny<JsonReaderException>()),
                Times.Once);
            Mock.Get(_log).Verify(
                l => l.DeserializationError(zip, "3.json", It.IsAny<JsonReaderException>()),
                Times.Once);
        }

        [Test, ExpectedException(typeof(AssertException))]
        public void ZipMustExist()
        {
            Clean("a");
        }

        [Test]
        public void IntegrationExample()
        {
            _sut.Filters.Add(new TestFilter("b"));
            _sut.Fixers.Add(new TestFixer("a"));

            Add("a", E("a", 3), E("b", 2), E("c", 1), E("a", 3));
            Add("b", E("d", 1));

            Clean("a");
            Clean("b");

            AssertEvents("a", E("c", 1), E("a", 3), E("a", 4));
            AssertEvents("b", E("d", 1));

            _sut.Dispose();

            Mock.Get(_log).Verify(l => l.WorkingIn(MergedDir + @"\", FinalDir + @"\"), Times.Exactly(1));
            Mock.Get(_log).Verify(
                l => l.RegisteredConfig(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()),
                Times.Exactly(1));
            Mock.Get(_log).Verify(l => l.ReadingZip("a"), Times.Exactly(1));
            Mock.Get(_log).Verify(l => l.ReadingZip("b"), Times.Exactly(1));
            Mock.Get(_log).Verify(l => l.WritingEvents(), Times.Exactly(2));
            Mock.Get(_log).Verify(l => l.FinishedWriting(It.IsAny<IDictionary<string, int>>()), Times.Exactly(2));

            CollectionAssert.AreEquivalent(new[] {"command filter: b"}, _registeredFilters);
            CollectionAssert.Contains(_reportedCounts, Res(4, 3, 5, 3, 3));
            CollectionAssert.Contains(_reportedCounts, Res(1, 1, 1, 1, 1));
        }

        private static IDictionary<string, int> Res(int numBefore,
            int numAfterFilter,
            int numAfterFixer,
            int numAfterDuplicate,
            int numAfterOrdering)
        {
            return new Dictionary<string, int>
            {
                {"before applying any filter", numBefore},
                {"after applying 'command filter: b'", numAfterFilter},
                {"after applying 'duplicator: a'", numAfterFixer},
                {"after removing duplicates", numAfterDuplicate},
                {"after ordering", numAfterOrdering}
            };
        }

        private class TestFilter : BaseFilter
        {
            private readonly string _filterId;

            public TestFilter(string filterId)
            {
                _filterId = filterId;
            }

            public override string Name
            {
                get { return "command filter: " + _filterId; }
            }

            public override Func<IDEEvent, bool> Func
            {
                get
                {
                    return e =>
                    {
                        var ce = e as CommandEvent;
                        if (ce != null)
                        {
                            return !ce.CommandId.Equals(_filterId);
                        }
                        return true;
                    };
                }
            }
        }

        private class TestFixer : BaseFixer
        {
            private readonly string _id;

            public TestFixer(string id)
            {
                _id = id;
            }

            public override string Name
            {
                get { return "duplicator: " + _id; }
            }

            public override IEnumerable<IDEEvent> Process(IDEEvent e)
            {
                yield return e;

                var ce = e as CommandEvent;
                if (ce != null && ce.TriggeredAt.HasValue && ce.CommandId.Equals(_id))
                {
                    yield return new CommandEvent
                    {
                        TriggeredAt = ce.TriggeredAt.Value.AddSeconds(1),
                        CommandId = ce.CommandId
                    };
                }
            }
        }
    }
}