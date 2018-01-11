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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils.Assertion;
using KaVE.Commons.Utils.Collections;
using KaVE.Commons.Utils.IO.Archives;

namespace KaVE.FeedbackProcessor
{
    public class SmokeTestRunner
    {
        private readonly object _lock = new object();

        private readonly string _baseDir;
        private readonly int _numThreads;
        private IKaVESet<string> _remainingZips;
        private int _numZips;

        public SmokeTestRunner(string baseDir, int numThreads)
        {
            Asserts.NotNull(baseDir);
            Asserts.That(Directory.Exists(baseDir));
            Asserts.IsLessOrEqual(1, numThreads);
            if (!baseDir.EndsWith(@"\"))
            {
                baseDir += @"\";
            }
            _baseDir = baseDir;
            _numThreads = numThreads;
        }

        public IKaVESet<string> FindRelativeZipPaths()
        {
            var zips = Directory.EnumerateFiles(_baseDir, "*.zip", SearchOption.AllDirectories)
                                .Select(f => f.Replace(_baseDir, ""));
            return Sets.NewHashSetFrom(zips);
        }

        private void Log(string s, params object[] args)
        {
            lock (_lock)
            {
                s = args.Length == 0 ? s : string.Format(s, args);
                Console.WriteLine("{0} {1}", DateTime.Now, s);
            }
        }

        public void Run()
        {
            Log(
                "Simple smoketest. Reads all events from all zips found in the provided base dir and calculates their hash codes to access all their fields.");
            Log("");
            Log("base dir: {0}", _baseDir);
            Log("number of threads: {0}", _numThreads);
            Log("");
            Log("Finding .zip files...");

            _remainingZips = FindRelativeZipPaths();
            _numZips = _remainingZips.Count;

            Log("Found {0} files.", _numZips);
            Log("");

            var tasks = new Task[_numThreads];
            for (var i = 0; i < _numThreads; i++)
            {
                var i1 = i;
                tasks[i] = Task.Factory.StartNew(() => { Process(i1); });
            }
            Task.WaitAll(tasks);

            Log("");
            Log("done (press key)");
            Console.Read();
        }

        private void Process(int taskId)
        {
            Log("({0}) starting", taskId);

            string relZip;
            while (GetNextZip(taskId, out relZip))
            {
                var zip = Path.Combine(_baseDir, relZip);
                using (var ra = new ReadingArchive(zip))
                {
                    while (ra.HasNext())
                    {
                        try
                        {
                            var e = ra.GetNext<IDEEvent>();
                            // ReSharper disable once UnusedVariable
                            var hc = e.GetHashCode();
                        }
                        catch
                        {
                            Log("Exception thrown in {0} ({1}).", zip, ra.CurrentInternalFileName);
                            throw;
                        }
                    }
                }
            }
            Log("({0}) stopping", taskId);
        }

        private bool GetNextZip(int taskId, out string relZip)
        {
            lock (_lock)
            {
                if (_remainingZips.Count > 0)
                {
                    relZip = _remainingZips.First();
                    _remainingZips.Remove(relZip);

                    var numStarted = _numZips - _remainingZips.Count;
                    var perc = 100 * numStarted / (double) _numZips;
                    Log(
                        "({0}) reading {1} ... ({2}/{3}, {4:0.0}% started)",
                        taskId,
                        relZip,
                        numStarted,
                        _numZips,
                        perc);

                    return true;
                }
                relZip = null;
                return false;
            }
        }
    }
}