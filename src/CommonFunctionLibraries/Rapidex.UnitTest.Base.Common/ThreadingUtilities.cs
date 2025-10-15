using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Rapidex.Theading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Base.Common;
public class ThreadingUtilities
{

    [Fact]

    public void T01_ThreadLocalStack()
    {
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        ThreadLocalStack<string> localStack = new();

        Assert.True(localStack.IsEmpty);
        Assert.Equal(0, localStack.Count);

        localStack.Push("A");
        Assert.False(localStack.IsEmpty);
        Assert.Equal(1, localStack.Count);

        int executingCount = 0;
        int executedCount = 0;

        Action act = () =>
        {
            Interlocked.Increment(ref executingCount);
            Assert.True(localStack.IsEmpty);
            localStack.Push($"B {executingCount:00}");
            Assert.False(localStack.IsEmpty);
            Assert.Equal(1, localStack.Count);
            Thread.Sleep(1000);
            Interlocked.Increment(ref executedCount);
        };

        Thread th1 = new Thread(new ThreadStart(act));
        Thread th2 = new Thread(new ThreadStart(act));

        th1.Start();
        th2.Start();

        th1.Join();
        th2.Join();

        Assert.Equal(2, executingCount);
        Assert.Equal(2, executedCount);


        Assert.Equal(1, localStack.Count);

#pragma warning restore xUnit1031 

    }


    protected async Task T03AsyncDelayMethod(AsyncLocalStack<string> stack, int delayMs)
    {
        stack.Push($"B");
        await Task.Delay(delayMs);
    }

    protected async Task AsyncLocalStackScope()
    {
        AsyncLocalStack<string> localStack = new();

        Assert.True(localStack.IsEmpty);
        Assert.Equal(0, localStack.Count);

        localStack.Push("A");
        Assert.False(localStack.IsEmpty);
        Assert.Equal(1, localStack.Count);

        await T03AsyncDelayMethod(localStack, 100);
        await T03AsyncDelayMethod(localStack, 100);

        Action act = () =>
        {
            localStack.Push($"C");
            Thread.Sleep(100);
        };

        Thread th1 = new Thread(new ThreadStart(act));
        Thread th2 = new Thread(new ThreadStart(act));

        th1.Start();
        th2.Start();

        th1.Join();
        th2.Join();

        Assert.Equal(5, localStack.Count);


    }

    [Fact]

    public async Task T03_AsyncLocalStackBasics()
    {
        await AsyncLocalStackScope();
        await AsyncLocalStackScope();


    }

}
