``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6600U CPU 2.60GHz (Max: 2.61GHz) (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.401
  [Host] : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT
  Core   : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT

Job=Core  Runtime=Core  InvocationCount=1  
UnrollFactor=1  

```
|                               Method |       Mean |     Error |     StdDev | Rank | Allocated |
|------------------------------------- |-----------:|----------:|-----------:|-----:|----------:|
|          ConcurrentTryGetValueSingle |   3.520 us | 0.3018 us |  0.8513 us |    2 |       0 B |
| ConcurrentTryGetValueSingleNotExists |   2.256 us | 0.2241 us |  0.6574 us |    1 |       0 B |
|            ConcurrentTryGetValue1000 |  72.399 us | 5.6782 us | 15.8287 us |    5 |       0 B |
|           ImmutableTryGetValueSingle |   5.919 us | 0.7222 us |  2.0486 us |    4 |       0 B |
|  ImmutableTryGetValueSingleNotExists |   3.919 us | 0.5360 us |  1.5549 us |    3 |       0 B |
|             ImmutableTryGetValue1000 | 189.144 us | 6.9992 us | 19.1602 us |    6 |       0 B |
