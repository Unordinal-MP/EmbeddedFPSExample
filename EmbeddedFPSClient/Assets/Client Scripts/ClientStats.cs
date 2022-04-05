using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStats
{
    public static ClientStats Instance { get; private set; } = new ClientStats();

    private const double LongWindow = 10;
    private const double ShortWindow = 2;
    public TimeBuffer MessagesIn { get; private set; } = new TimeBuffer() { WindowInSeconds = LongWindow };
    public TimeBuffer BytesIn { get; private set; } = new TimeBuffer() { WindowInSeconds = LongWindow };
    public TimeBuffer Reconciliations { get; private set; } = new TimeBuffer() { WindowInSeconds = ShortWindow };
    public TimeBuffer Confirmations { get; private set; } = new TimeBuffer() { WindowInSeconds = ShortWindow };

    public int ReconciliationHistorySize => ownPlayer != null ? ownPlayer.ReconciliationHistorySize : 0;

    private ClientPlayer ownPlayer;

    public void SetOwnPlayer(ClientPlayer player)
    {
        if (!player.IsOwn)
        {
            throw new ArgumentException("player");
        }

        ownPlayer = player;
    }
}
