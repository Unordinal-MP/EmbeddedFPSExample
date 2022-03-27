using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStats
{
    public static ClientStats instance { get; private set; } = new ClientStats();

    private const double _longWindow = 10;
    private const double _shortWindow = 2;
    public TimeBuffer MessagesIn { get; private set; } = new TimeBuffer() { WindowInSeconds = _longWindow };
    public TimeBuffer BytesIn { get; private set; } = new TimeBuffer() { WindowInSeconds = _longWindow };
    public TimeBuffer Reconciliations { get; private set; } = new TimeBuffer() { WindowInSeconds = _shortWindow };
}
