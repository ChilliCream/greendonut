``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6600U CPU 2.60GHz (Max: 2.61GHz) (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.401
  [Host] : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT
  Core   : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT

Job=Core  Runtime=Core  InvocationCount=1  
UnrollFactor=1  

```
|                               Method |       Mean |     Error |     StdDev |     Median | Rank | Allocated |
|------------------------------------- |-----------:|----------:|-----------:|-----------:|-----:|----------:|
|          ConcurrentTryGetValueSingle |   3.172 us | 0.2565 us |  0.7441 us |   3.195 us |    2 |       0 B |
| ConcurrentTryGetValueSingleNotExists |   2.052 us | 0.2638 us |  0.7612 us |   2.200 us |    1 |       0 B |
|            ConcurrentTryGetValue1000 |  59.745 us | 3.6551 us | 10.2492 us |  55.480 us |    5 |       0 B |
|           ImmutableTryGetValueSingle |   8.261 us | 0.8750 us |  2.5106 us |   7.910 us |    4 |       0 B |
|  ImmutableTryGetValueSingleNotExists |   3.887 us | 0.5500 us |  1.5869 us |   3.545 us |    3 |       0 B |
|             ImmutableTryGetValue1000 | 193.567 us | 7.1883 us | 20.0382 us | 185.375 us |    6 |       0 B |
