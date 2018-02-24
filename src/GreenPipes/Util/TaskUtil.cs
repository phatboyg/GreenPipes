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
namespace GreenPipes.Util
{
    using System.Threading.Tasks;


    public static class TaskUtil
    {
        public static Task Completed => Cached.CompletedTask;
        public static Task<bool> False => Cached.FalseTask;
        public static Task<bool> True => Cached.TrueTask;


        static class Cached
        {
            public static readonly Task CompletedTask = Task.FromResult(true);
            public static readonly Task<bool> TrueTask = Task.FromResult(true);
            public static readonly Task<bool> FalseTask = Task.FromResult(false);
        }
    }
}