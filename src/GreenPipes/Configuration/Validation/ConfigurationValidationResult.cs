// Copyright 2012-2018 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace GreenPipes.Validation
{
    using System;


    [Serializable]
    public class ConfigurationValidationResult :
        ValidationResult
    {
        public ConfigurationValidationResult(ValidationResultDisposition disposition, string key, string value, string message)
        {
            Disposition = disposition;
            Key = key;
            Value = value;
            Message = message;
        }

        public ConfigurationValidationResult(ValidationResultDisposition disposition, string key, string message)
        {
            Disposition = disposition;
            Key = key;
            Message = message;
        }

        public ConfigurationValidationResult(ValidationResultDisposition disposition, string message)
        {
            Key = "";
            Disposition = disposition;
            Message = message;
        }

        public ValidationResultDisposition Disposition { get; }
        public string Key { get; }
        public string Value { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"[{Disposition}] {(string.IsNullOrEmpty(Key) ? Message : Key + " " + Message)}";
        }
    }
}