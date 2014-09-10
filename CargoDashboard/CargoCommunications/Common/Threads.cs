using System;
using System.Diagnostics;
using System.Threading;

namespace L3.Cargo.Communications.Common
{
    /// <summary>
    /// L3 Threads is a utility class that provides methods that support the management of
    /// persistent threads (threads that operate until explicitly comamnded to stop).</summary>
    /// <example><code>
    /// private void AgentMethod() /*Thread Agent Method template*/ {
    ///     threadException = null;
    ///     try {
    ///         while (/*run?*/ !threadEndEvent.WaitOne(</code>delay<code>)) {
    ///             </code>...perform preparation...<code>
    ///             try { </code>...prosecute actions...<code> }
    ///             catch { </code>...recover to try again...<code> }
    ///             finally { </code>...tidy preparations...<code> }
    ///          }
    ///     }
    ///     catch(Exception ex)  { threadException = ex; }
    ///     finally { </code>...perform final tidying...<code> }
    /// }
    ///
    /// // The agent method terminates when its thread ending <see cref="ManualResetEvent"/> is
    /// // <see cref="ManualResetEvent.Set"/>. During execution, the event is used to provide the
    /// // same service as <see cref="Thread.Sleep"/> but with the benefit that awakening occurs
    /// // immediately on <see cref="ManualResetEvent.Set"/>.
    /// private volatile ManualResetEvent threadEndEvent;
    ///
    /// // If the agent method must terminate because it detects an anomaly that it cannot resolve,
    /// // the agent places the corresponding <see cref="Exception"/> in the thread exception
    /// // holder, which may be examined even after the thread has terminated and been tidied.
    /// private volatile Exception threadException;
    ///
    /// // In order to manage the thread, a reference to the thread must be held in an accessible
    /// // location.
    /// private Thread threadReference;
    /// </code>...<code>
    /// // Create, but do not start, the thread, ensuring that the ending event and exception are
    /// // properly prepared.
    /// threadReference = ThreadCreate(AgentMethod, ref threadEndEvent, "Agent thread");
    /// </code>...<code>
    /// // Start the thread.
    /// theExc = null;
    /// threadReference.Start();
    /// </code>...<code>
    /// // Terminate the thread.
    /// try { threadReference = ThreadDispose(threadReference, ref endEvent); }
    /// catch { </code>...handle anomaly...<code> }
    /// finally { threadReference = null; } /*recommended paradigm*/
    /// if (threadException != null) </code>...</example>
    public static class Threads
    {
        /// <summary>Thread Create creates (but does not start) a persistent thread.</summary>
        /// <param name="agentMethod">
        /// Agent Method specifies the method that will prosecute the thread's actions. If this
        /// argument specifies null, an exception is thrown.</param>
        /// <param name="endEvent">
        /// End Event specifies a <see cref="ManualResetEvent"/> that is to be used to inform the
        /// agent that it should terminate, gracefully. This method creates and assigns a new
        /// <see cref="ManualResetEvent"/> with a false initialState.</param>
        /// <param name="identity">
        /// Identity specifies, optionally, the identity of the thread. This value is applied as
        /// the thread's <see cref="Thread.Name"/>. If this argument's value is empty, null or
        /// white space, <see cref="string.Empty"/> is used.</param>
        /// <returns><see cref="Thread"/> referencing the newly created thread</returns>
        /// <remarks>
        /// Note that this method does not <see cref="Thread.Start"/> the created thread. That
        /// allows tailoring (such as the setting of properties) to occur before the thread
        /// begins working.</remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="agentMethod"/> specifies null, an exception is thrown.</exception>
        public static Thread Create(ThreadStart agentMethod, ref ManualResetEvent endEvent, string identity = null)
        {
            if (/*invalid?*/ agentMethod == null)
                throw new ArgumentNullException("agentMethod == null");
            endEvent = new ManualResetEvent(false);
            endEvent.Reset();

            // Ensure that the thread is marked as a background thread (just proper practice) and
            // then set the thread's name. If the candidate identity is null or white space, set
            // the name to string.Empty.
            Thread thread = new Thread(new ThreadStart(agentMethod));
            thread.IsBackground = true;
            thread.Name = string.IsNullOrWhiteSpace(identity) ? string.Empty : identity.Trim();
            return thread;
        }

        /// <summary>
        /// Thread, Dispose stops and disposes of a thread. It may be used as an initializer to
        /// ensure that a particular thread and its reference are in a known state. If available,
        /// the logging facility is used to announce the disposal.
        /// </summary>
        /// <param name="thread">
        /// Thread specifies the thread to be terminated. If this argument specifies null, no
        /// termination activities occur.</param>
        /// <param name="endEvent">
        /// End Event specifies a <see cref="ManualResetEvent"/> that is to be used to inform the
        /// agent that it should terminate, gracefully. This method performs a
        /// <see cref="ManualResetEvent.Set"/> to signal the termination. If this argument specifies
        /// null, no <see cref="ManualResetEvent.Set"/> occurs.</param>
        /// <param name="timeOut">
        /// Time Out specifies (optionally; default is <see cref="ThreadDisposeDEFAULT"/>), in
        /// milliseconds, the <see cref="Thread.Join"/> timeout. Values less than
        /// <see cref="ThreadDisposeMINIMUM"/> are adjusted upward to
        /// <see cref="ThreadDisposeMINIMUM"/>; values greater than <see cref="short.MaxValue"/>
        /// are adjusted downward to <see cref="short.MaxValue"/>. The special value zero (0) means
        /// that no <see cref="Thread.Join"/> should be performed.</param>
        /// <returns>null, as a convenience, to be applied to the thread reference</returns>
        /// <exception cref="Exception">
        /// If the specified thread does not <see cref="Thread.Join"/> in the time specified by
        /// <paramref name="timeOut"/>, that thread is <see cref="Thread.Abort"/>ed and an
        /// exception is thrown.</exception>
        public static Thread Dispose(Thread thread, ref ManualResetEvent endEvent, int timeOut = DisposeTimeOutDftValue)
        {
            // Ensure that the time out parameter is within the valid domain.
            if (/*not immediate?*/ timeOut != 0)
                timeOut = Math.Min((int)short.MaxValue, Math.Max(DisposeTimeOutMinValue, timeOut));
            Debug.Assert((timeOut >= DisposeTimeOutMinValue) && (timeOut <= (int)short.MaxValue));

            // Prosecute the termination.
            if (/*specified?*/ thread != null)
            {
                string identity = Identity(thread, new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name.Trim());

                // Request termination and then join/witness the thread's termination.
                if (/*OK?*/ endEvent != null)
                    endEvent.Set();
                if (/*alive & joinable?*/ thread.IsAlive)
                    if (/*not immediate?*/ timeOut >= DisposeTimeOutMinValue)
#if DEBUG
                        try
                        {
                            // For debugging, use a long time out and measure the time to complete. Don't
                            // use StopWatch as it will be disposed of by the system.
                            DateTime timer = DateTime.UtcNow;
                            if (/*failed to stop?*/ !thread.Join(Utilities.Time10SECONDS))
                                throw new Exception("no termination response from " + identity + " for " + DateTime.UtcNow.Subtract(timer).ToString("c"));
                            Debug.WriteLine(identity + " took " + DateTime.UtcNow.Subtract(timer).ToString("c") + " to Join");
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
#else
                        if (/*won't die?*/ thread.Join(timeOut))
                            throw new Exception("no termination response from " + identity + " for " + timeOut.ToString() + "ms");
#endif
                // DO NOT Thread.Abort ... not only does it not always end the thread, sometimes it locks up!
            }
            return null;
        }

        private const int DisposeTimeOutDftValue = Utilities.TimeSECOND;
        private const int DisposeTimeOutMinValue = Utilities.TimeTENTH;

        /// <summary>
        /// Thread Identity makes a thread identity string: a form of augmented
        /// <see cref="Thread.Name"/>.</summary>
        /// <param name="thread">
        /// Thread specifies (optionally, the default is null) the thread being identified. If this
        /// argument specifies null, the <see cref="Thread.CurrentThread"/> value is used. If the
        /// <see cref="Thread.Name"/> property is null or white space, the value specified by
        /// <paramref name="identity"/> is used, instead.</param>
        /// <param name="identity">
        /// Identity specifies (optionally, the default is null) an alternate identity (such as the
        /// calling method's name). If this argument specifies null or white space, the actual,
        /// first-level calling method's name is used instead.</param>
        /// <returns>thread identity, trimmed of white space</returns>
        public static string Identity(Thread thread = null, string identity = null)
        {
            if (/*null?*/ thread == null)
                thread = Thread.CurrentThread;
            if (/*null/empty?*/ string.IsNullOrWhiteSpace(identity))
                identity = new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name.Trim();
            Debug.Assert(!string.IsNullOrWhiteSpace(identity));
            if (/*substantive?*/ !string.IsNullOrWhiteSpace(thread.Name))
                identity = thread.Name.Trim() + "/" + identity;
            return identity;
        }
    }
}