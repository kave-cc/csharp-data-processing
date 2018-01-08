/*
 * Copyright 2018 University of Zurich
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
using KaVE.Commons.Model.Events.VersionControlEvents;
using KaVE.Commons.Model.Naming;
using KaVE.Commons.Utils.Collections;
using KaVE.FeedbackProcessor.Preprocessing.Fixers;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Preprocessing.Fixers
{
    class VersionContorlEventSplitterTest
    {
        #region infrastructure

        private VersionControlEventSplitter _sut;
        private DateTimeOffset _now;
        private IEnumerable<IDEEvent> _output;

        [SetUp]
        public void SetUp()
        {
            _sut = new VersionControlEventSplitter();

            _now = DateTimeOffset.Now;
        }

        private DateTimeOffset Time(int offset)
        {
            return _now.AddMinutes(offset);
        }

        private IVersionControlAction A(VersionControlActionType type, int offset)
        {
            return new VersionControlAction
            {
                ActionType = type,
                ExecutedAt = Time(offset)
            };
        }

        private IDEEvent E(int offset = 0)
        {
            return new CommandEvent
            {
                TriggeredAt = Time(offset)
            };
        }

        private IDEEvent VCE(int offset, params IVersionControlAction[] actions)
        {
            return new VersionControlEvent
            {
                TriggeredAt = Time(offset),
                Actions = Lists.NewListFrom(actions)
            };
        }

        private IDEEvent VCE(int offset, VersionControlActionType type)
        {
            return new VersionControlEvent
            {
                TriggeredAt = Time(offset),
                Actions =
                {
                    new VersionControlAction
                    {
                        ExecutedAt = Time(offset),
                        ActionType = type
                    }
                }
            };
        }

        private void Process(params IDEEvent[] es)
        {
            _output = _sut.Process(es);
        }

        private void AssertStream(params IDEEvent[] expecteds)
        {
            CollectionAssert.AreEqual(expecteds, _output);
        }

        #endregion

        [Test]
        public void DoesNotAffectOtherEvents()
        {
            Process(E());
            AssertStream(E());
        }

        [Test]
        public void CanSplitAndSetsTime()
        {
            Process(
                VCE(0, A(VersionControlActionType.Checkout, -20), A(VersionControlActionType.Commit, -10)));

            AssertStream(
                VCE(-20, VersionControlActionType.Checkout),
                VCE(-10, VersionControlActionType.Commit));
        }

        [Test]
        public void PreservesAllInformation()
        {
            var expected = new VersionControlEvent
            {
                Id = "some id",
                KaVEVersion = "0.1234-Release",
                IDESessionUUID = "some session id",

                TriggeredAt = Time(1),
                TriggeredBy = EventTrigger.Shortcut,
                Duration = TimeSpan.FromSeconds(13),

                ActiveDocument = Names.Document("some doc"),
                ActiveWindow = Names.Window("some window"),

                Solution = Names.Solution("some solution"),
                Actions =
                {
                    new VersionControlAction
                    {
                        ExecutedAt = Time(1), // for brevity, use same time here 
                        ActionType = VersionControlActionType.Rebase
                    }
                }
            };

            Process(expected);
            AssertStream(expected);
        }

        [Test]
        public void NoSplitNecessaryButSetTime()
        {
            Process(
                VCE(0, A(VersionControlActionType.Checkout, -10)));

            Process(
                VCE(-10, VersionControlActionType.Checkout));
        }

        [Test]
        public void NoActionRecordedIsFiltered()
        {
            Process(VCE(0));
            AssertStream();
        }

        [Test]
        public void Integration()
        {
            Process(
                E(1),
                VCE(3, A(VersionControlActionType.Clone, -20), A(VersionControlActionType.Merge, -10)),
                E(5),
                VCE(6, A(VersionControlActionType.Checkout, -7)),
                E(10),
                VCE(12),
                E(15));

            AssertStream(
                E(1),
                VCE(-20, VersionControlActionType.Clone),
                VCE(-10, VersionControlActionType.Merge),
                E(5),
                VCE(-7, VersionControlActionType.Checkout),
                E(10),
                E(15));
        }
    }
}