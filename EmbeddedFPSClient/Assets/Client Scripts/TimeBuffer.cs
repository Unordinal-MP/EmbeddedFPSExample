using System.Collections.Generic;
using UnityEngine;

public sealed class TimeBuffer
{
    private Queue<KeyValuePair<double, double>> _samples = new Queue<KeyValuePair<double, double>>();

    public double WindowInSeconds { get; set; } = 1;

    public void AddNow(double metric = 1)
    {
        double deadline = GetNow() + WindowInSeconds;
        _samples.Enqueue(new KeyValuePair<double, double>(deadline, metric));
    }

    private void Update()
    {
        double now = GetNow();
        while (_samples.Count > 0 && _samples.Peek().Key < now)
        {
            _samples.Dequeue();
        }
    }

    private double GetNow()
    {
        return Time.timeAsDouble;
    }

    public double GetWindowSum()
    {
        Update();

        double sum = 0;
        foreach (var kv in _samples)
        {
            sum += kv.Value;
        }

        return sum;
    }

    public double GetWindowRate()
    {
        return GetWindowSum() / WindowInSeconds;
    }
}
