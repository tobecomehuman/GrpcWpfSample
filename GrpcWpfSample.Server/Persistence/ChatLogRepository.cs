﻿using GrpcWpfSample.Common;
using GrpcWpfSample.Server.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace GrpcWpfSample.Server.Persistence
{
    public class ChatLogRepository : IChatLogRepository
    {
        private readonly List<ChatLog> m_storage = new List<ChatLog>();
        private readonly AwaitableEvent<ChatLog> m_addedEvent = new AwaitableEvent<ChatLog>();

        public void Add(ChatLog chatLog)
        {
            m_storage.Add(chatLog);
            m_addedEvent.Invoke(chatLog);
        }

        public IAsyncEnumerable<ChatLog> GetAllAsync()
        {
            var oldLogs = m_storage.ToAsyncEnumerable();

            var next = m_addedEvent.Next;
            var newLogs = AsyncEnumerable.Repeat(null as ChatLog)
                .SelectAsync(async (x) => await next)
                .Do((x) => next = x.Next)
                .Select((x) => x.Args);

            return oldLogs.Concat(newLogs);
        }
    }
}
