using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

    private static Object objLock =
      new object();

    static async Task BeginPolling(
        ConcurrentQueue<string> queue,
        CancellationToken token,
        Stream dstStream,
        StreamWriter outStream
        )
    {
      string msg;
      StringBuilder builder;
      int n = 0;

      while (!token.IsCancellationRequested)
      {
        if (queue.TryDequeue(out msg))
        {
          //await outStream.WriteLineAsync(msg);

          builder = new StringBuilder(msg + "\n");
          n = msg.Length;
          int msgs = 1;

          while (queue.TryDequeue(out msg))
          {
            builder.AppendLine(msg);
            n += msg.Length;
            msgs += 1;

            if (n > 1 << 20 || msgs > 1024)
            {
              break;
            }
          }

          var finalMsg = builder.ToString();

          var buf = Encoding.UTF8.GetBytes(finalMsg);
          //Console.WriteLine(finalMsg);
#if DEBUG
          Debug.Write(finalMsg);
#endif
          await dstStream.WriteAsync(buf, 0, buf.Length);
        }
        else
        {
          await Task.Delay(16);
        }
      }
    }

    public static ConcurrentQueue<string> CreateStreamLogger(string key, Stream stream)
    {
      ConcurrentQueue<string> queue;

      lock (objLock)
      {
        if (queues.TryGetValue(key, out queue))
        {
          return queue;
        }

        StreamWriter outStream = new StreamWriter(stream);
        outStream.AutoFlush = true;

        queue = new ConcurrentQueue<string>();
        var tokSource = new CancellationTokenSource();
        var tok = tokSource.Token;
        var task = Task.Run(async () =>
        {
          try
          {
            await BeginPolling(queue, tok, stream, outStream);
          }
          catch (Exception _)
          {
            outStream.Close();
          }
        });

        queues.Add(key, queue);
        tokens.Add(key, tokSource);
        tasks.Add(key, task);
      }

      return queue;
    }
  }

  public enum LogLevel {
    Fatal = 10,
    Error = 20,
    Warn = 30,
    Info = 40,
    Debug = 50,
  }

  public abstract class Logger
  {
    abstract public LogLevel Level { get; }
    abstract public string Name { get; }
    
    abstract protected void Publish(string msg);

    abstract public Logger Fork(string name, LogLevel level);

    public void Log(LogLevel level, string format, params object[] args)
    {
      if (level <= this.Level)
      {
        string msg;

        if (args.Length == 0)
        {
          msg = format;
        }
        else
        {
          msg = String.Format(format, args);
        }

        var finalMsg = String.Format(
          "{0} \t| {1}:{2} \t| {3} \t| {4}", 
          DateTime.Now.ToString(), 
          Name, 
          Thread.CurrentThread.ManagedThreadId,
          level, 
          msg
        );

        this.Publish(finalMsg);
      }
    }

    public void Fatal(string format, params object[] args)
    {
      Log(LogLevel.Fatal, format, args);
    }

    public void Error(string format, params object[] args)
    {
      Log(LogLevel.Error, format, args);
    }

    public void Warn(string format, params object[] args)
    {
      Log(LogLevel.Warn, format, args);
    }

    public void Info(string format, params object[] args)
    {
      Log(LogLevel.Info, format, args);
    }

    public void Debug(string format, params object[] args)
    {
      Log(LogLevel.Debug, format, args);
    }
  }

  public class StreamLogger : Logger
  {
    ConcurrentQueue<string> queue;
    LogLevel level;
    string name;

    public StreamLogger(string name, LogLevel level, Stream stream)
    {
      this.name = name;
      this.level = level;
      this.queue = LogManager.CreateStreamLogger(name, stream);
    }

    private StreamLogger(string name, LogLevel level, ConcurrentQueue<string> queue)
    {
      this.name = name;
      this.level = level;
      this.queue = queue;
    }

    public override Logger Fork(string name, LogLevel level)
    {
      return new StreamLogger(name, level, this.queue);
    }

    protected override void Publish(string msg)
    {
      queue.Enqueue(msg);
    }

    public override LogLevel Level
    {
      get
      {
        return level;
      }
    }

    public override string Name
    {
      get
      {
        return name;
      }
    }
  }
}