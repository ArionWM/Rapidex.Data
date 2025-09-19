using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex
{
    //From pro-core
    public class LockScope : IDisposable
    {

        protected readonly Mutex Mutex;

        public LockScope(string key)
        {
            this.Mutex = new Mutex(false, key);
            this.Mutex.WaitOne();
        }

        public void Dispose()
        {
            this.Mutex.ReleaseMutex();
            this.Mutex.Dispose();
        }

        public static LockScope Lock(string key)
        {
            var scope = new LockScope(key);
            return scope;
        }

        public static void Lock(string key, Action action)
        {
            using (var scope = new LockScope(key))
            {
                action.NotNull().Invoke();
            }
        }
    }
}
