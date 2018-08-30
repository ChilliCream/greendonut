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
|      ConcurrentTryAddSingle |     4.127 us |   0.9229 us |   2.588 us |     3.040 us |    1 |      40 B |
| ConcurrentTryAddSingleTwice |     5.754 us |   1.1052 us |   3.135 us |     4.940 us |    2 |      40 B |
|        ConcurrentTryAdd1000 |   231.677 us |  13.1168 us |  37.423 us |   225.870 us |    4 |  200120 B |
|       ImmutableTryAddSingle |    12.485 us |   1.9846 us |   5.694 us |    11.025 us |    3 |     104 B |
|  ImmutableTryAddSingleTwice |    11.699 us |   1.6007 us |   4.515 us |    11.275 us |    3 |     104 B |
|         ImmutableTryAdd1000 | 2,497.809 us | 124.5514 us | 363.322 us | 2,353.060 us |    5 |  666560 B |
