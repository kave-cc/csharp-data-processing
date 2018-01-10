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
using KaVE.Commons.Utils.IO.Archives;

namespace KaVE.FeedbackProcessor.Preprocessing.Model
{
    public class FailsafeIDEEventReadingArchive : IDisposable
    {
        private readonly Action<string, Exception> _log;
        private readonly ReadingArchive _ra;

        public FailsafeIDEEventReadingArchive(string zip, Action<string, Exception> log)
        {
            _log = log;
            _ra = new ReadingArchive(zip);
        }

        public void Dispose()
        {
            _ra.Dispose();
        }

        public IEnumerable<IDEEvent> ReadAllLazy()
        {
            while (_ra.HasNext())
            {
                IDEEvent e = null;
                try
                {
                    e = _ra.GetNext<IDEEvent>();
                }
                catch (Exception ex)
                {
                    _log(_ra.CurrentInternalFileName, ex);
                }
                if (e != null)
                {
                    yield return e;
                }
            }
        }
    }
}