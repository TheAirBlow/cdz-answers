using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace TheAirBlow.Solver.Library;

public class QueueSystem
{
    public static QueueSystem Instance;
    
    // ReSharper disable once InconsistentNaming
    public enum Website { SkySmart, Skills4u, Saharina, TestEdu }
    public enum Status { Waiting, Solving, Done }

    public class QueueItem : ICloneable
    {
        public Website Website;
        public string Uuid = Guid
            .NewGuid().ToString();
        public string SolverInput;
        public object SolverOutput;
        public bool Failed;
        
        public object Clone()
            => MemberwiseClone();
    }

    private volatile List<QueueItem> _finished = new();
    private volatile Queue<QueueItem> _queue = new();
    private Thread _thread;

    /// <summary>
    /// Initialize the queue system
    /// </summary>
    public QueueSystem()
    {
        Instance = this;
        _thread = new Thread(QueueProcessor);
        _thread.Start();
    }

    /// <summary>
    /// Queue processor thread
    /// </summary>
    private async void QueueProcessor()
    {
        while (true) {
            QueueItem item = null;
            var run = true;
            lock (_queue) {
                if (_queue.Count == 0)
                    run = false;
                else item = _queue.Peek();
            }

            if (run) {
                GlobalLogger.Instance.LogInformation(new EventId(), 
                    null, "Solving an exercise...");
                GlobalLogger.Instance.LogInformation(new EventId(), 
                    null, $"UUID: {item.Uuid}");
                GlobalLogger.Instance.LogInformation(new EventId(), 
                    null, $"Website: {item.Website}");
                GlobalLogger.Instance.LogInformation(new EventId(), 
                    null, $"Input: {item.SolverInput}");
                try {
                    switch (item.Website) {
                        case Website.Skills4u:
                            item.SolverOutput = await Skills4u
                                .GetAnswers(item.SolverInput);
                            break;
                        case Website.SkySmart:
                            item.SolverOutput = SkySmart
                                .GetAnswers(item.SolverInput);
                            break;
                        case Website.Saharina:
                            item.SolverOutput = await Saharina
                                .GetAnswersXml(item.SolverInput);
                            break;
                        case Website.TestEdu:
                            item.SolverOutput = await TestEdu
                                .GetAnswers(item.SolverInput);
                            break;
                    }
                } catch (Exception e) {
                    GlobalLogger.Instance.LogInformation(new EventId(), 
                        e, "An exception occured while solving!");
                    item.Failed = true;
                }
                
                GlobalLogger.Instance.LogInformation(new EventId(), 
                    null, "Item dequeued successfully!");
                lock(_finished) _finished.Add(item);
                lock(_queue) _queue.Dequeue();
            }
        }
    }

    /// <summary>
    /// Enqueue an item
    /// </summary>
    /// <param name="item">Item</param>
    /// <returns>UUID</returns>
    public string Enqueue(QueueItem item) {
        lock (_queue) _queue.Enqueue(item);
        return item.Uuid;
    }

    /// <summary>
    /// Exists in queue or finished
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <returns>Boolean value</returns>
    public bool ExistsInQueueOrFinished(string uuid)
    {
        lock (_finished) lock (_queue)
            return _finished.Any(x => x.Uuid == uuid)
                   || _queue.Any(x => x.Uuid == uuid);
    }
    
    /// <summary>
    /// Is item finished
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <returns>Boolean value</returns>
    public bool IsFinished(string uuid)
    {
        lock (_finished) return _finished.Any(
            x => x.Uuid == uuid);
    }

    /// <summary>
    /// Get item's position in queue
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <returns>Position in queue</returns>
    public int GetPositionInQueue(string uuid)
    {
        lock (_finished)
            if (_finished.Any(x =>
                    x.Uuid == uuid))
                return -2;
        lock (_queue)
            return Array.IndexOf(_queue.ToArray(),
                _queue.ToArray().FirstOrDefault(
                    x => x.Uuid == uuid)) - 1;
    }

    /// <summary>
    /// Get finished result
    /// </summary>
    /// <param name="uuid">UUID</param>
    /// <returns>Finished result</returns>
    public QueueItem GetFinishedResult(string uuid)
    {
        lock (_finished) {
            var item = _finished.Where(x => 
                x.Uuid == uuid).ToList()[0];
            return item;
        }
    }

    /// <summary>
    /// Get and Remove finished result
    /// </summary>
    /// <param name="uuid">UUID</param>
    public QueueItem RemoveFinishedResult(string uuid)
    {
        lock (_finished) {
            var item = _finished.FirstOrDefault(
                x => x.Uuid == uuid)!;
            _finished.Remove(item);
            return item;
        }
    }
}