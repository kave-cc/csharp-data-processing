/*
 * Copyright 2014 Technische Universitšt Darmstadt
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

namespace KaVE.FeedbackProcessor.WatchdogExports.Model
{
    public enum DocumentType
    {
        Undefined,
        Production,
        FilenameTest,
        PathnameTest,
        TestFramework,
        Test
    }

    public abstract class FileInterval : Interval
    {
        public string FileName { get; set; }

        public DocumentType FileType { get; set; }

        protected bool Equals(FileInterval other)
        {
            return base.Equals(other) && string.Equals(FileName, other.FileName) && FileType == other.FileType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((FileInterval) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) FileType;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", base.ToString(), FileName);
        }
    }
}