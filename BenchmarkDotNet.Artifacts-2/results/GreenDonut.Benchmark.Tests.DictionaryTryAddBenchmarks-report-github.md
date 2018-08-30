``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6600U CPU 2.60GHz (Max: 2.61GHz) (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.401
  [Host] : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT
  Core   : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT

Job=Core  Runtime=Core  InvocationCount=1  
UnrollFactor=1  

```
|                      Method |         Mean |       Error |     StdDev |       Median | Rank | Allocated |
|---------------------------- |-------------:|------------:|-----------:|-------------:|-----:|----------:|
|      ConcurrentTryAddSingle |     3.401 us |   0.7933 us |   2.185 us |     2.360 us |    1 |      40 B |
| ConcurrentTryAddSingleTwice |     5.184 us |   1.2291 us |   3.467 us |     3.645 us |    2 |      40 B |
|        ConcurrentTryAdd1000 |   206.077 us |  17.9117 us |  51.103 us |   199.490 us |    4 |  128304 B |
|       ImmutableTryAddSingle |     8.973 us |   1.9403 us |   5.567 us |     7.570 us |    3 |     104 B |
|  ImmutableTryAddSingleTwice |    10.569 us |   2.1536 us |   6.248 us |     8.675 us |    3 |     104 B |
|         ImmutableTryAdd1000 | 2,327.463 us | 105.6636 us | 303.169 us | 2,219.210 us |    5 |  664128 B |
