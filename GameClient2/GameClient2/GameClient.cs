using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web;
using Zrpg.Game;
using System.Text;
using System.Net;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GameClient2
{
    class GameClientRunner
    {
        private class Work
        {
            AsyncMsg msg;
            Exception exn;

            AutoResetEvent mut;
            Reply value;
            Task<Reply> task;
            string id;

            public Work(AsyncMsg msg)
            {
                this.mut = new AutoResetEvent(false);
                this.value = null;
                this.exn = null;
                this.msg = msg;

                task = Task<Reply>.Factory.StartNew(() =>
                {
                    mut.WaitOne();

                    if (exn != null)
                    {
                        throw exn;
                    }
                    else {
                        return value;
                    }
                });
            }

            public void Resolve(Reply value)
            {
                this.value = value;
                mut.Set();
            }

            public void Reject(Exception exn)
            {
                this.exn = exn;
            }

            public string Id { get { return msg.id; } }

            public AsyncMsg Msg { get { return msg; } }

            public Task<Reply> Task { get { return task; } }
        }

        Encoding enc = Encoding.UTF8;
        CancellationToken token;
        MessageWebSocket socket;

        ConcurrentQueue<Work> workQueue = new ConcurrentQueue<Work>();
        ConcurrentDictionary<string, Work> replyEvents = new ConcurrentDictionary<string, Work>();
        Task task;

        int taskIdCount = 0;

        public GameClientRunner(CancellationToken token)
        {
            this.token = token;
            Debug.WriteLine("Starting runner");

            this.task = Task.Run(async () =>
            {
                while (token.IsCancellationRequested == false)
                {
                    Work work;
                    if (workQueue.TryDequeue(out work))
                    {
                        Debug.WriteLine("Got work {0} for msg {1}", work.Id, work.Msg);
                        try {
                            await this.publish(work);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Failed with {0}", e.StackTrace.ToString());
                            work.Reject(e);
                        }
                        Debug.WriteLine("Done.");
                    }
                    else
                    {
                        await Task.Delay(16);
                    }
                }
            });
        }

        private void Socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            Debug.WriteLine("Got response");
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    var data = reader.ReadString(reader.UnconsumedBufferLength);
                    var json = Regex.Unescape(data).Trim('"');
                    var reply = JsonConvert.DeserializeObject<AsyncReply>(json);
                    var id = reply.id;

                    Work work;
                    if (!replyEvents.TryGetValue(id, out work))
                    {
                        Debug.WriteLine("Got invalid reply {0}", reply);
                        return;
                    }

                    work.Resolve(reply.reply);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Got exception {0}", e);
            }
        }

        private async Task publish(Work work)
        {
            AsyncMsg letter = work.Msg;
            Msg msg = letter.msg;
            string reqJson = "";

            if (msg.IsAddGarrison)
            {
                reqJson = JsonConvert.SerializeObject(letter);
            }
            else
            {
                work.Reject(new Exception("Invalid msg"));
                return;
            }

            var msgWriter = new DataWriter(socket.OutputStream);
            Debug.WriteLine("Sending data {0}", reqJson);
            msgWriter.WriteString(reqJson);
            await msgWriter.StoreAsync();
            Debug.WriteLine("Data sent.");
        }

        public async Task Connect()
        {
            //socket = new StreamWebSocket();
            socket = new MessageWebSocket();
            socket.Control.MessageType = SocketMessageType.Utf8;
            socket.MessageReceived += Socket_MessageReceived;

            socket.Closed += (senderSocket, args) =>
            {
                Debug.WriteLine("Socket closed: {0} {1}", args.Code, args.Reason);
            };

            await socket.ConnectAsync(new Uri("ws://localhost:8080"));
        }

        public async Task<Reply> Handle(Msg msg)
        {
            var backoff = 4;
            while (workQueue.Count > 4) 
            {
                await Task.Delay(backoff);
                backoff = Math.Max(backoff * 2, 1 << 14);
            }

            var id = Interlocked.Increment(ref this.taskIdCount).ToString();
            var letter = new AsyncMsg(msg, id);
            var work = new Work(letter);

            while (!replyEvents.TryAdd(work.Id, work))
            {
                id = Interlocked.Increment(ref this.taskIdCount).ToString();
                letter = new AsyncMsg(msg, id);
                work = new Work(letter);
            }

            workQueue.Enqueue(work);
            return await work.Task;
        }

        public void Close()
        {
            if (this.socket != null)
            {
                this.socket.Dispose();
                this.socket = null;
            }
        }
    }

    public class GameClient
    {

        GameClientRunner runner;

        public GameClient()
        {
            runner = new GameClientRunner();

            //socket.Closed += async (socket, args) =>
            //{
            //}
        }

        public async Task Connect()
        {
            await runner.Connect();
        }

        private async Task<T> WorkAs<T>(Msg msg, Predicate<Reply> predicate)
        {
            var res = await runner.Handle(msg);

            if (predicate(res))
            {
                return (T)Convert.ChangeType(res, typeof(T));
            }
            else
            {
                var exn = String.Format(
                    "Expected {0} but got {1}",
                    typeof(T).FullName,
                    res
                );
                throw new Exception(exn);
            }
        }

        public Task<AddGarrisonReply> AddGarrison(AddGarrison addGarrison)
        {
            var msg = Msg.NewAddGarrison(addGarrison);
            return WorkAs<AddGarrisonReply>(msg, (reply) => reply.IsAddGarrisonReply);
        }

        public void Close()
        {
            runner.Close();
        }
    }
}
