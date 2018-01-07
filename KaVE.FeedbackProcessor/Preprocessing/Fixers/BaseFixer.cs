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
using System.Linq;
using KaVE.Commons.Model.Events;

namespace KaVE.FeedbackProcessor.Preprocessing.Fixers
{
    public abstract class BaseFixer : IFixer
    {
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        public virtual IEnumerable<IDEEvent> Process(IEnumerable<IDEEvent> es)
        {
            return es.SelectMany(Process);
        }

        public abstract IEnumerable<IDEEvent> Process(IDEEvent input);
    }
}