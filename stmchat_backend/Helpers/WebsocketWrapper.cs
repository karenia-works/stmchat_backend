using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reactive;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Subjects;
using System.Text.Json;


namespace stmchat_backend.Helpers
{
    public class JsonWebsocketWrapper<TRecvMessage, TSendMessage>
    {
        public JsonWebsocketWrapper(
            WebSocket socket,
            JsonSerializerOptions serializerOptions = null,
            int defaultBufferSize = 8192)
        {
            this.socket = socket;
            this.serializerOptions = serializerOptions;
            this.recvBuffer = new byte[defaultBufferSize];
        }

        readonly WebSocket socket;
        readonly CancellationToken closeToken = new CancellationToken();
        readonly JsonSerializerOptions serializerOptions;

        byte[] recvBuffer;

        public Subject<TRecvMessage> Messages { get; } = new Subject<TRecvMessage>();

        protected void DoubleRecvCapacity()
        {
            var newBuffer = new byte[this.recvBuffer.Length * 2];
            this.recvBuffer.CopyTo(new Span<byte>(newBuffer));
            this.recvBuffer = newBuffer;
        }

        protected async Task EventLoop()
        {
            while (true)
            {
                var result = await this.socket.ReceiveAsync(new ArraySegment<byte>(recvBuffer), this.closeToken);
                var writtenBytes = 0;
                while (!result.EndOfMessage)
                {
                    writtenBytes += result.Count;
                    this.DoubleRecvCapacity();
                    result = await this.socket.ReceiveAsync(new ArraySegment<byte>(
                        recvBuffer,
                        writtenBytes,
                        this.recvBuffer.Length - writtenBytes), this.closeToken);
                }
                writtenBytes += result.Count;

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        try
                        {
                            var message = JsonSerializer.Deserialize<TRecvMessage>(new ArraySegment<byte>(this.recvBuffer, 0, writtenBytes), serializerOptions);
                            this.Messages.OnNext(message);
                        }
                        catch (Exception e)
                        {
                            this.Messages.OnError(e);
                        }
                        break;
                    case WebSocketMessageType.Binary:
                        this.Messages.OnError(new UnexpectedBinaryMessageException());
                        break;
                    case WebSocketMessageType.Close:
                        this.Messages.OnCompleted();
                        return;
                }
            }
        }

        public async Task WaitUntilClose()
        {
            await this.EventLoop();
        }

        public async Task SendMessage(TSendMessage message)
        {
            var buffer = JsonSerializer.SerializeToUtf8Bytes<TSendMessage>(message, this.serializerOptions);
            await this.socket.SendAsync(buffer, WebSocketMessageType.Text, true, this.closeToken);
        }

        public class UnexpectedBinaryMessageException : Exception { }
    }

}
