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

    private static Dictionary<string, StreamWriter> streams =
      new Dictionary<string, StreamWriter>();

    private static Object objLock =
      new object();

    static async Task BeginPolling(
        ConcurrentQueue<string> queue,
        CancellationToken token,
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
          //builder = new StringBuilder(msg);
          //n = msg.Length;
          //int msgs = 0;

          //while (queue.TryDequeue(out msg))
          //{
          //  builder.AppendLine(msg + "\n");
          //  n += msg.Length;
          //  msgs += 1;

          //  if (n > 1 << 20 || msgs > 128)
          //  {
          //    break;
          //  }
          //}

          //var finalMsg = builder.ToString() + "\n";

          //Debug.WriteLine("Logging {0}", finalMsg);
          //var result = Encoding.UTF8.GetBytes(finalMsg);

          //var result = Encoding.UTF8.GetBytes(msg + "\n");
          //await outStream.WriteAsync(result, 0, result.Length);
          await outStream.WriteLineAsync(msg);
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

        StreamWriter outStream;

        if (!streams.TryGetValue(filepath, out outStream))
        {
          var stream = File.Open(
            filepath,
            FileMode.Append,
            FileAccess.Write,
            FileShare.Read
            );

          outStream = new StreamWriter(stream);
          streams.Add(filepath, outStream);
        }

        queue = new ConcurrentQueue<string>();
        var tokSource = new CancellationTokenSource();
        var tok = tokSource.Token;
        var task = Task.Run(async () =>
        {
          try
          {
            await BeginPolling(queue, tok, outStream);
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
    Fatal,
    Error,
    Warn,
    Info,
    Debug,
  }

  public abstract class Logger
  {
    abstract protected LogLevel level { get; }
    abstract protected string name { get; }
    
    abstract protected void Publish(string msg);

    public void Log(LogLevel level, string format, params object[] args)
    {
      if (level <= this.level)
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
        var finalMsg = String.Format("{0} \t| {1} \t| {2} \t| {3}", DateTime.Now.ToString(), name, level, msg);
        this.Publish(finalMsg);
      }
    }

    public void Info(string format, params object[] args)
    {
      Log(LogLevel.Info, format, args);
    }
  }

  public class FileLogger : Logger
  {
    ConcurrentQueue<string> queue;
    LogLevel _level;
    string _name;

    public FileLogger(string name, LogLevel level, string filepath)
    {
      this._name = name;
      this._level = level;
      this.queue = LogManager.CreateFileLogger(name, filepath);
    }

    protected override void Publish(string msg)
    {
      queue.Enqueue(msg);
    }

    protected override LogLevel level
    {
      get
      {
        return _level;
      }
    }

    protected override string name
    {
      get
      {
        return _name;
      }
    }
  }
}