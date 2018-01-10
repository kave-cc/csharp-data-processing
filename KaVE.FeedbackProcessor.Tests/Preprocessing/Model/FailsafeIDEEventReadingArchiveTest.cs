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
using System.IO;
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.Commons.TestUtils;
using KaVE.Commons.Utils.IO.Archives;
using KaVE.FeedbackProcessor.Preprocessing.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Preprocessing.Model
{
    internal class FailsafeIDEEventReadingArchiveTest : FileBasedTestBase
    {
        private string _zip;

        [SetUp]
        public void SetUp()
        {
            _zip = Path.Combine(DirTestRoot, "a.zip");

            using (var wa = new WritingArchive(_zip))
            {
                wa.Add(Event(1));
                wa.AddAsPlainText("xxx");
                wa.Add(Event(2));
            }
        }

        [Test]
        public void CanReadAllEvents()
        {
            var actuals = new List<IDEEvent>();
            using (var sut = new FailsafeIDEEventReadingArchive(_zip, (f, ex) => { }))
            {
                foreach (var e in sut.ReadAllLazy())
                {
                    actuals.Add(e);
                }
            }
            var expecteds = new[] {Event(1), Event(2)};
            CollectionAssert.AreEqual(expecteds, actuals);
        }

        [Test]
        public void ReportsException()
        {
            string internalFile = null;
            Exception e = null;
            using (var sut = new FailsafeIDEEventReadingArchive(
                _zip,
                (f, ex) =>
                {
                    internalFile = f;
                    e = ex;
                }))
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                sut.ReadAllLazy().ToList();
            }
            Assert.AreEqual("1.json", internalFile);
            Assert.That(e is JsonReaderException);
        }

        [Test]
        public void NoExceptionNoReport()
        {
            using (var wa = new WritingArchive(_zip))
            {
                wa.Add(Event(1));
                wa.Add(Event(2));
            }
            Exception e = null;
            using (var sut = new FailsafeIDEEventReadingArchive(_zip, (f, ex) => { e = ex; }))
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                sut.ReadAllLazy().ToList();
            }
            Assert.Null(e);
        }

        private CommandEvent Event(int id)
        {
            return new CommandEvent {CommandId = "cmd" + id};
        }
    }
}