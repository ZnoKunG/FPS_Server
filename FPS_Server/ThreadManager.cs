using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class ThreadManager
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> duplicatedExecuteOnMainThread = new List<Action>();
    private static bool hasActionOnMainThread;

    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Console.WriteLine("Action to execute on main thread is NULL");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            hasActionOnMainThread = true;
        }
    }

    /// <summary>
    /// Execute all actions meant to be run on main thread.
    /// </summary>
    public static void UpdateMainThread()
    {
        if (!hasActionOnMainThread) return;

        duplicatedExecuteOnMainThread.Clear();

        // Lock action list for its current thread (if another thread is using the list, it will wait until the list is released.)
        lock (executeOnMainThread)
        {
            // Transfer Action from main thread to duplicated list
            duplicatedExecuteOnMainThread.AddRange(executeOnMainThread);
            executeOnMainThread.Clear();
            hasActionOnMainThread = false;
        }

        // Execute actions on any thread
        foreach (Action execute in duplicatedExecuteOnMainThread)
        {
            execute();
        }
    }
}
