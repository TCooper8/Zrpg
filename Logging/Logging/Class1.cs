using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logging
{
  static class LogManager
  {
    private static Dictionary<string, Task> tasks =
        new Dictionary<string, Task>();

    private static Dictionary<string, ConcurrentQueue<string>> queues =
        new Dictionary<string, ConcurrentQueue<string>>();

    private static Dictionary<string, CancellationTokenSource> tokens =
        new Dictionary<string, CancellationTokenSource>();

    private static Dictionary<string, Stream> streams =
      new Dictionary<string, Stream>();

    private static Object objLock =
      new object();

    static async Task BeginPolling(
        ConcurrentQueue<string> queue,
        CancellationToken token,
        Stream outStream
        )
    {
      string msg;
      StringBuilder builder;
      int n = 0;

      while (!token.IsCancellationRequested)
      {
        if (queue.TryDequeue(out msg))
        {
          builder = new StringBuilder(msg);
          n = msg.Length;

          while (queue.TryDequeue(out msg))
          {
            builder.AppendLine(msg);
            n += msg.Length;

            if (n > 1 << 20)
            {
              break;
            }
          }

          var result = Encoding.UTF8.GetBytes(builder.ToString());

          await outStream.WriteAsync(result, 0, result.Length);
          await outStream.FlushAsync();
        }
        else
        {
          await Task.Delay(16);
        }
      }
    }

    public static ConcurrentQueue<string> CreateFileLogger(string key, string filepath)
    {
      ConcurrentQueue<string> queue;

      lock (objLock)
      {
        if (queues.TryGetValue(key, out queue))
        {
          return queue;
        }

        var stream = File.Open(
          filepath,
          FileMode.OpenOrCreate,
          FileAccess.Write,
          FileShare.Read
          );

        queue = new ConcurrentQueue<string>();
        var tokSource = new CancellationTokenSource();
        var tok = tokSource.Token;
        var task = Task.Run(async () =>
        {
          try
          {
            await BeginPolling(queue, tok, stream);
          }
          catch (Exception _)
          {
            stream.Close();
          }
        });

        queues.Add(key, queue);
        tokens.Add(key, tokSource);
        tasks.Add(key, task);
      }

      return queue;
    }
  }

  public class Class1
  {
  }
}