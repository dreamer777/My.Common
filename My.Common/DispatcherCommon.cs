#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using JetBrains.Annotations;

using NLog;


#endregion



namespace My.Common
{
    public abstract class DispatcherCommon
    {
        readonly Logger _logger = LogManager.GetLogger("DispatcherCommon");
        protected readonly ManualResetEvent StopEvent = new ManualResetEvent(false);

        readonly List<Thread> _workingThreads = new List<Thread>();
        readonly List<Thread> _oneusingThreads = new List<Thread>();


        public abstract void Start();


        protected void AddAndStartOneusingThread(string name, Action<ManualResetEvent> action, int beforeFirstSleepSeconds)
        {
            Thread t = new Thread(Dispatch) {Name = name};
            t.Start(new object[]
                    {
                            action,
                            0,
                            beforeFirstSleepSeconds
                    });
            _oneusingThreads.Add(t);
        }


        protected void AddAndStartUsualThread(string name, Action<ManualResetEvent> action, int sleepSeconds, int beforeFirstSleepSeconds = 0)
        {
            Thread t = new Thread(Dispatch) {Name = name};
            t.Start(new object[]
                    {
                            action,
                            sleepSeconds,
                            beforeFirstSleepSeconds
                    });
            _workingThreads.Add(t);
        }


        /// <summary>
        ///     Is executed in two separate theads. Parameter: (action, sleepSeconds, beforeFirstSleepSeconds)
        /// </summary>
        void Dispatch([NotNull] object o)
        {
            try
            {
                _logger.Trace("Dispatch");

                Action<ManualResetEvent> action = (Action<ManualResetEvent>) ((object[]) o)[0];
                int sleepSeconds = (int) ((object[]) o)[1];
                int beforeFirstSleepSeconds = (int) ((object[]) o)[2];

                _logger.Debug("sleepSeconds {0}, beforeFirstSleepSeconds {1}", sleepSeconds, beforeFirstSleepSeconds);

                if (!StopEvent.WaitOne(beforeFirstSleepSeconds*1000))
                    do
                    {
                        _logger.Debug("to exec " + action.Method.Name + "...");
                        if (StopEvent.WaitOne(0))
                        {
                            _logger.Info("Received stop event, breaking");
                            break;
                        }

                        // выйти должны только фатальные исключения. Остальные должны внутри проглотиться.
                        action(StopEvent);

                        if (sleepSeconds == 0)
                        {
                            _logger.Info(action.Method.Name + " ended, because of sleepSeconds==0");
                            return;
                        }
                        _logger.Info(action.Method.Name + " ended, sleeping for " + sleepSeconds + " seconds");
                    } while (!StopEvent.WaitOne(sleepSeconds*1000));

                _logger.Trace("end " + Thread.CurrentThread.Name + " by stop event");
            }
            catch (ThreadAbortException ex)
            {
                _logger.WarnException("ThreadAbort received", ex);
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                _logger.FatalException("", ex);
            }
        }


        /// <summary>
        ///     Is executed in another thread
        /// </summary>
        public void Stop()
        {
            _logger.Warn("Stop service");
            StopEvent.Set();

            foreach (Thread t in _oneusingThreads)
                StopThread(t, 10000);
            foreach (Thread t in _workingThreads)
                StopThread(t, 10000);

            _logger.Info("all thread was ended or aborted");
        }


        protected void StopThread(Thread t, int msToJoin)
        {
            if (t != null && t.ThreadState != ThreadState.Unstarted && !t.Join(msToJoin))
            {
                _logger.Warn("Aborting [{0}] thread", t.Name);
                t.Abort();
            }
        }
    }
}