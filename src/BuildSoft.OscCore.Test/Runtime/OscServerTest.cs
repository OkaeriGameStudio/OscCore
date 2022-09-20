using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuildSoft.OscCore.Tests;
using NUnit.Framework;

namespace BuildSoft.OscCore.Test.Runtime;

[TestOf(typeof(OscServer))]
public class OscServerTest
{
    private OscServer _server = null!;

    [SetUp]
    public void Setup()
    {
        _server = new OscServer(7000);
    }

    [Test]
    public void CallbackTest()
    {
        MonitorCallback callback1 = (_, _) => { };
        MonitorCallback callback2 = (_, _) => { };

        _server.AddMonitorCallback(callback1);
        _server.AddMonitorCallback(callback1);

        Assert.IsTrue(_server.RemoveMonitorCallback(callback1));
        Assert.IsFalse(_server.RemoveMonitorCallback(callback2));
        Assert.IsTrue(_server.RemoveMonitorCallback(callback1));
        Assert.IsFalse(_server.RemoveMonitorCallback(callback1));
    }
}
