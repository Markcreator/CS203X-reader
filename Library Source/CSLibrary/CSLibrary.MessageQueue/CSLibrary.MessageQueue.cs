using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using CSLibrary.Threading;

namespace CSLibrary.MessageQueue
{
    /// <summary>
    /// Represents a first-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">Type of element queue will contain.</typeparam>
    public class BlockingQueue<T> : IEnumerable<T>, ICollection
    {
        private bool isOpened = true;
        private readonly Queue<T> q;
        public MonitorEx synRoot = new MonitorEx();
        //private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the BlockingQueue class.
        /// </summary>
        public BlockingQueue()
        {
            q = new Queue<T>();
        }

        /// <summary>
        /// Initializes a new instance of the BlockingQueue class.
        /// </summary>
        /// <param name="capacity">The initial number of elements the queuecan contain.</param>
        public BlockingQueue(int capacity)
        {
            q = new Queue<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the BlockingQueue class.
        /// </summary>
        /// <param name="collection">A collection whose elements are copiedto the new queue.</param>
        public BlockingQueue(IEnumerable<T> collection)
        {
            q = new Queue<T>(collection);
        }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                synRoot.Enter();
                {
                    count = q.Count;
                }
                synRoot.Exit();
                return count;
            }
        }

        /// <summary>
        /// Remove all objects from the BlockingQueue<T>.
        /// </summary>
        public void Clear()
        {
            synRoot.Enter();
            {
                q.Clear();
            }
            synRoot.Exit();
        }

        /// <summary>
        /// Closes the queue.
        /// </summary>
        public void Close()
        {
            synRoot.Enter();
            {
                if (!this.isOpened)
                    return; // Already closed.

                isOpened = false;
                q.Clear();
                synRoot.PulseAll(); // resume any waiting threads so they see the queue is closed.
            }
            synRoot.Exit();
        }

        /// <summary>
        /// Gets a value indicating if queue is opened.
        /// </summary>
        public bool Opened
        {
            get
            {
                bool opened = false;
                synRoot.Enter();
                {
                    opened = this.isOpened;
                }
                synRoot.Exit();
                return opened;
            }
        }

        /// <summary>
        /// Determines whether an element is in the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to locate in the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
        /// <returns>true if item is found in the System.Collections.Generic.Queue<T>; otherwise, false.</returns>
        public bool Contains(T item)
        {
            bool contains = false;
            synRoot.Enter();
            {
                contains = q.Contains(item);
            }
            synRoot.Exit();
            return contains;
        }

        /// <summary>
        /// Copies the System.Collections.Generic.Queue<T> elements to an existing one-dimensional System.Array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements
        /// copied from System.Collections.Generic.Queue<T>. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            synRoot.Enter();
            {
                q.CopyTo(array, arrayIndex);
            }
            synRoot.Exit();
        }

        public T[] ToArray()
        {
            synRoot.Enter();
            {
                return q.ToArray();
            }
            synRoot.Exit();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BlockingQueue<T>.Enumerator(this, -1);
        }
        public IEnumerator<T> GetEnumerator(int millisecondsTimeout)
        {
            return new BlockingQueue<T>.Enumerator(this,
            millisecondsTimeout);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new BlockingQueue<T>.Enumerator(this, -1);
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the System.Collections.Generic.Queue<T>,
        /// if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            synRoot.Enter();
            {
                q.TrimExcess();
            }
            synRoot.Exit();
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>Object in queue.</returns>
        public T Dequeue()
        {
            return Dequeue(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <param name="timeout">Time to wait before returning (in milliseconds).</param>
        /// <returns>Object in queue.</returns>
        public T Dequeue(int millisecondsTimeout)
        {
            T queue;
            synRoot.Enter();
            try
            {
                while (isOpened && (q.Count == 0))
                {
                    if (!synRoot.Wait(millisecondsTimeout))
                        throw new TimeoutException("Operation timeout");
                }

                if (!isOpened)
                    throw new InvalidOperationException("Queue closed");
                queue = q.Dequeue();
            }
            catch
            {

            }
            synRoot.Exit();
            return queue;
        }

        public bool TryDequeue(int millisecondsTimeout, out T value)
        {
            bool rc = false;
            synRoot.Enter();
            {
                while (isOpened && (q.Count == 0))
                {
                    if (!MonitorEx.Wait(syncRoot, millisecondsTimeout))
                    {
                        value = default(T);
                        rc = false;
                        goto EXIT;
                    }
                }

                if (!isOpened)
                    throw new InvalidOperationException("Queue closed");
                value = q.Dequeue();
                rc = true;
            }
            EXIT:
            synRoot.Exit();
            return rc;
        }

        /// <summary>
        /// Returns the object at the beginning of the BlockingQueue<T>
        /// without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the BlockingQueue<T>.</returns>
        public T Peek()
        {
            return Peek(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Returns the object at the beginning of the BlockingQueue<T>
        /// without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the BlockingQueue<T>.</returns>
        /// <param name="millisecondsTimeout">Time to wait before returning (in milliseconds).</param>
        public T Peek(int millisecondsTimeout)
        {
            T peek;
            synRoot.Enter();
            {
                while (isOpened && (q.Count == 0))
                {
                    if (!synRoot.Wait(millisecondsTimeout))
                        throw new TimeoutException("Operation timeout");
                }

                if (!isOpened)
                    throw new InvalidOperationException("Queue closed");
                peek = q.Peek();
            }
            synRoot.Exit();
            return peek;
        }

        /// <summary>
        /// Adds an object to the end of the Queue.
        /// </summary>
        /// <param name="obj">Object to put in queue.</param>
        public void Enqueue(T item)
        {
            synRoot.Enter();
            {
                if (!isOpened)
                    throw new InvalidOperationException("Queue closed");
                q.Enqueue(item);
                synRoot.Pulse(); // Move 1 waiting thread to the "ready" queue in this monitor object.
            } // Exiting lock will free thread(s) in the "ready" queue for this monitor object.
            synRoot.Exit();
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private BlockingQueue<T> q;
            private IEnumerator<T> e;

            internal Enumerator(BlockingQueue<T> q, int timeout)
            {
                this.q = q;
                if (!q.synRoot.TryEnter())
                    throw new TimeoutException("Timeout waiting for enumerator lock on BlockingQueue<T>.");
                this.e = this.q.q.GetEnumerator(); // Get the contained Queue<T> enumerator.
            }

            public void Dispose()
            {
                this.e.Dispose();
                q.synRoot.Exit();
            }

            public bool MoveNext()
            {
                return e.MoveNext();
            }

            public T Current
            {
                get
                {
                    return e.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return ((IEnumerator)e).Current;
                }
            }

            void IEnumerator.Reset()
            {
                e.Reset();
            }
        }

        #region ICollection Members

        /// <summary>
        /// Copies the BlockingQueue<T> elements to an existing one-dimensional System.Array, starting at the specified array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Array array, int index)
        {
            this.synRoot.Enter();
            {
                ((ICollection)q).CopyTo(array, index);
            }
            this.synRoot.Exit();
        }

        /// <summary>
        /// Get a value that indicates if the queue is synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.synRoot.stateLock;
            }
        }

        #endregion
    }
}

