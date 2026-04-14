using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TestRunner
{
    public class WorkerThread
    {
        public int Id { get; }
        public Thread Thread { get; }
        public bool IsWorking { get; set; }
        public bool ShouldExit { get; set; }
        public DateTime LastFinishedWork { get; set; }
        public DateTime CurrentWorkStartTime { get; set; }

        public WorkerThread(int id, Thread thread)
        {
            Id = id; Thread = thread; IsWorking = false; ShouldExit = false; LastFinishedWork = DateTime.Now;
        }
    }

    public class MyThreadPool : IDisposable
    {
        private readonly int _minThreads;
        private readonly int _maxThreads;
        private readonly TimeSpan _idleTimeout;
        private readonly TimeSpan _hungTimeout;

        private readonly Queue<Action> _taskQueue = new Queue<Action>();
        private readonly List<WorkerThread> _workers = new List<WorkerThread>();
        private readonly object _lockObj = new object();

        private bool _isDisposed = false;
        private int _threadCounter = 0;
        private readonly Thread _supervisorThread;

        public event Action<string, ConsoleColor> OnPoolEvent;

        public MyThreadPool(int minThreads, int maxThreads, int idleTimeoutMs = 3000, int hungTimeoutMs = 5000)
        {
            _minThreads = minThreads; _maxThreads = maxThreads;
            _idleTimeout = TimeSpan.FromMilliseconds(idleTimeoutMs);
            _hungTimeout = TimeSpan.FromMilliseconds(hungTimeoutMs);

            for (int i = 0; i < _minThreads; i++) CreateWorker();

            _supervisorThread = new Thread(SupervisorLoop) { Name = "supervisor", IsBackground = true };
            _supervisorThread.Start();
        }

        public void EnqueueTask(Action task)
        {
            lock (_lockObj)
            {
                if (_isDisposed) throw new ObjectDisposedException("pool disposed");
                _taskQueue.Enqueue(task);
                Monitor.Pulse(_lockObj);
            }
        }

        private void CreateWorker()
        {
            int id = Interlocked.Increment(ref _threadCounter);
            var thread = new Thread(() => WorkerLoop(id)) { Name = $"worker-{id}", IsBackground = true };
            var worker = new WorkerThread(id, thread);
            _workers.Add(worker);
            thread.Start();
        }

        private void WorkerLoop(int workerId)
        {
            WorkerThread me;
            lock (_lockObj) { me = _workers.First(w => w.Id == workerId); }

            while (true)
            {
                Action task = null;
                lock (_lockObj)
                {
                    while (_taskQueue.Count == 0 && !me.ShouldExit && !_isDisposed) Monitor.Wait(_lockObj, 500);

                    if (_isDisposed || me.ShouldExit)
                    {
                        _workers.Remove(me);
                        return;
                    }

                    task = _taskQueue.Dequeue();
                    me.IsWorking = true;
                    me.CurrentWorkStartTime = DateTime.Now;
                }

                try { task.Invoke(); }
                catch { }
                finally
                {
                    lock (_lockObj) { me.IsWorking = false; me.LastFinishedWork = DateTime.Now; }
                }
            }
        }

        private void SupervisorLoop()
        {
            while (!_isDisposed)
            {
                Thread.Sleep(1000);
                lock (_lockObj)
                {
                    int qCount = _taskQueue.Count;
                    int act = _workers.Count(w => w.IsWorking);
                    int tot = _workers.Count;


                    OnPoolEvent?.Invoke($"[stat] threads: {tot} (work: {act}), q: {qCount}", ConsoleColor.Cyan);

                    if (qCount > 0 && act == tot && tot < _maxThreads)
                    {
                        OnPoolEvent?.Invoke($"[+] q {qCount}, adding thread", ConsoleColor.Yellow);
                        CreateWorker();
                    }

                    if (tot > _minThreads)
                    {
                        var idle = _workers.Where(w => !w.IsWorking && (DateTime.Now - w.LastFinishedWork) > _idleTimeout).ToList();
                        foreach (var worker in idle)
                        {
                            if (_workers.Count <= _minThreads) break;
                            OnPoolEvent?.Invoke($"[-] worker {worker.Id} idle, removing", ConsoleColor.DarkYellow);
                            worker.ShouldExit = true;
                        }
                    }

                    var hung = _workers.Where(w => w.IsWorking && (DateTime.Now - w.CurrentWorkStartTime) > _hungTimeout).ToList();
                    foreach (var zombie in hung)
                    {
                        OnPoolEvent?.Invoke($"[!] worker {zombie.Id} hung, replacing", ConsoleColor.Magenta);
                        zombie.ShouldExit = true;
                        _workers.Remove(zombie);
                        CreateWorker();
                    }
                }
            }
        }

        public void WaitAllAndDispose()
        {
            while (true)
            {
                lock (_lockObj) { if (_taskQueue.Count == 0 && _workers.All(w => !w.IsWorking)) break; }
                Thread.Sleep(200);
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                lock (_lockObj) { Monitor.PulseAll(_lockObj); }
            }
        }
    }
}