using System.Collections.Generic;
using UnityEngine;

//a better implementation would probably have a fixed number N of time slices, because it puts a bound on memory use
public sealed class TimeBuffer
{
    private readonly Queue<KeyValuePair<double, double>> samples = new Queue<KeyValuePair<double, double>>();

    public double WindowInSeconds { get; set; } = 1;

    public void AddNow(double metric = 1)
    {
        double deadline = GetNow() + WindowInSeconds;
        samples.Enqueue(new KeyValuePair<double, double>(deadline, metric));
    }

    private void Update()
    {
        double now = GetNow();
        while (samples.Count > 0 && samples.Peek().Key < now)
        {
            samples.Dequeue();
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
        foreach (var kv in samples)
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
