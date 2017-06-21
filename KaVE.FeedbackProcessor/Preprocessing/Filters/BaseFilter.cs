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
using KaVE.Commons.Model.Events;

namespace KaVE.FeedbackProcessor.Preprocessing.Filters
{
    public abstract class BaseFilter : IFilter
    {
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        public abstract Func<IDEEvent, bool> Func { get; }

        public bool Func2(IDEEvent e)
        {
            return Func(e);
        }
    }
}