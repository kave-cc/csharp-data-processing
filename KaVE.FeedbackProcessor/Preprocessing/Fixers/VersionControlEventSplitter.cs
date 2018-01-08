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

using System.Collections.Generic;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.VersionControlEvents;

namespace KaVE.FeedbackProcessor.Preprocessing.Fixers
{
    public class VersionControlEventSplitter : BaseFixer
    {
        public override IEnumerable<IDEEvent> Process(IDEEvent e)
        {
            var vce = e as VersionControlEvent;
            if (vce == null)
            {
                yield return e;
            }
            else
            {
                foreach (var a in vce.Actions)
                {
                    var clone = new VersionControlEvent
                    {
                        // ide event
                        Id = vce.Id,
                        KaVEVersion = vce.KaVEVersion,
                        IDESessionUUID = vce.IDESessionUUID,

                        TriggeredAt = a.ExecutedAt, // override with time of version control action
                        TriggeredBy = vce.TriggeredBy,
                        Duration = vce.Duration,

                        ActiveDocument = vce.ActiveDocument,
                        ActiveWindow = vce.ActiveWindow,

                        // version control event
                        Solution = vce.Solution,
                        Actions = {a}
                    };
                    yield return clone;
                }
            }
        }
    }
}