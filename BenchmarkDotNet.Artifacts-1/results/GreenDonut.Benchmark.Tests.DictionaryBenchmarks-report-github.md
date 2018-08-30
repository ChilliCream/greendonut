``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6600U CPU 2.60GHz (Max: 2.61GHz) (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.401
  [Host] : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT
  Core   : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT

Job=Core  Runtime=Core  InvocationCount=1  
UnrollFactor=1  

```
|                      Method |         Mean |      Error |     StdDev |       Median | Rank | Allocated |
|---------------------------- |-------------:|-----------:|-----------:|-------------:|-----:|----------:|
|      ConcurrentTryAddSingle |     4.456 us |   1.117 us |   3.186 us |     2.840 us |    1 |      40 B |
| ConcurrentTryAddSingleTwice |     5.539 us |   1.221 us |   3.483 us |     3.965 us |    2 |      40 B |
|        ConcurrentTryAdd1000 |   260.326 us |  16.089 us |  47.438 us |   256.820 us |    4 |  202560 B |
|       ImmutableTryAddSingle |    11.598 us |   1.871 us |   5.337 us |    10.425 us |    3 |     104 B |
|  ImmutableTryAddSingleTwice |    11.927 us |   1.736 us |   4.953 us |    10.830 us |    3 |     104 B |
|         ImmutableTryAdd1000 | 2,601.321 us | 136.684 us | 398.714 us | 2,535.620 us |    5 |  662976 B |
