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

using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils.Collections;
using KaVE.FeedbackProcessor.Cleanup.Processors;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Cleanup.Processors
{
    [TestFixture]
    internal class UnnamedCommandFilterProcessorTest
    {
        private UnnamedCommandFilterProcessor _uut;

        [SetUp]
        public void Setup()
        {
            _uut = new UnnamedCommandFilterProcessor();
        }

        [Test]
        public void FiltersUnnamedCommand()
        {
            var commandEvent = new CommandEvent
            {
                CommandId = "{5EFC7975-14BC-11CF-9B2B-00AA00573819}:331:"
            };

            Assert.AreEqual(Sets.NewHashSet<IDEEvent>(), _uut.Map(commandEvent));
        }

        [Test]
        public void ShouldNotFilterNamedCommands()
        {
            var commandEvent = new CommandEvent
            {
                CommandId = "{5EFC7975-14BC-11CF-9B2B-00AA00573819}:26:Edit.Paste"
            };

            Assert.AreEqual(Sets.NewHashSet<IDEEvent>(commandEvent), _uut.Map(commandEvent));
        }
    }
}