// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace GreenPipes.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class Profile_Specs
    {
        [Test]
        public async Task Should_write_to_the_console()
        {
            IPipe<Input> pipe = Pipe.New<Input>(x =>
            {
                x.UseConsoleProfile();
                x.UseExecuteAsync(context => Task.Delay(10));
            });

            await Task.WhenAll(Enumerable.Range(0, 100).Select(x => pipe.Send(new Input("Hello"))));
        }
    }
}