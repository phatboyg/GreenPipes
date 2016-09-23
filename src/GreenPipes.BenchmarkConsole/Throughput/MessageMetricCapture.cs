// Copyright 2012-2016 Chris Patterson
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
namespace GreenPipes.BenchmarkConsole.Throughput
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;


    public class MessageMetricCapture :
        IReportConsumerMetric
    {
        readonly TaskCompletionSource<TimeSpan> _consumeCompleted;
        readonly ConcurrentBag<ConsumedMessage> _consumedMessages;
        readonly long _messageCount;
        readonly TaskCompletionSource<TimeSpan> _sendCompleted;
        readonly ConcurrentBag<SentMessage> _sentMessages;
        readonly Stopwatch _stopwatch;
        long _consumed;
        long _sent;

        public MessageMetricCapture(long messageCount)
        {
            _messageCount = messageCount;

            _consumedMessages = new ConcurrentBag<ConsumedMessage>();
            _sentMessages = new ConcurrentBag<SentMessage>();
            _sendCompleted = new TaskCompletionSource<TimeSpan>();
            _consumeCompleted = new TaskCompletionSource<TimeSpan>();

            _stopwatch = Stopwatch.StartNew();
        }

        public Task<TimeSpan> SendCompleted => _sendCompleted.Task;
        public Task<TimeSpan> ConsumeCompleted => _consumeCompleted.Task;

        Task IReportConsumerMetric.Consumed<T>(Guid messageId)
        {
            _consumedMessages.Add(new ConsumedMessage(messageId, _stopwatch.ElapsedTicks));

            var consumed = Interlocked.Increment(ref _consumed);
            if (consumed == _messageCount)
                _consumeCompleted.TrySetResult(_stopwatch.Elapsed);

            return TaskUtil.Completed;
        }

        public void Sent(Guid messageId)
        {
            var sendTimestamp = _stopwatch.ElapsedTicks;

            _sentMessages.Add(new SentMessage(messageId, sendTimestamp));

            var sent = Interlocked.Increment(ref _sent);
            if (sent == _messageCount)
                _sendCompleted.TrySetResult(_stopwatch.Elapsed);
        }

        public MessageMetric[] GetMessageMetrics()
        {
            return _sentMessages.Join(_consumedMessages, x => x.MessageId, x => x.MessageId,
                (sent, consumed) => new MessageMetric(sent.MessageId, Math.Max(0, consumed.Timestamp - sent.SendTimestamp)))
                .ToArray();
        }


        struct SentMessage
        {
            public readonly Guid MessageId;
            public readonly long SendTimestamp;

            public SentMessage(Guid messageId, long sendTimestamp)
            {
                MessageId = messageId;
                SendTimestamp = sendTimestamp;
            }
        }


        struct ConsumedMessage
        {
            public readonly Guid MessageId;
            public readonly long Timestamp;

            public ConsumedMessage(Guid messageId, long timestamp)
            {
                MessageId = messageId;
                Timestamp = timestamp;
            }
        }
    }
}