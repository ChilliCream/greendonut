``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6600U CPU 2.60GHz (Max: 2.61GHz) (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.401
  [Host] : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT
  Core   : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT

Job=Core  Runtime=Core  InvocationCount=1  
UnrollFactor=1  

```
|            Method |       Mean |     Error |    StdDev |     Median | Allocated |
|------------------ |-----------:|----------:|----------:|-----------:|----------:|
|      TryAddSingle |   5.158 us |  1.214 us |  3.522 us |   3.590 us |     144 B |
| TryAddSingleTwice |   6.170 us |  1.444 us |  4.118 us |   4.550 us |     240 B |
|        TryAdd1000 | 290.619 us | 14.041 us | 39.140 us | 282.410 us |  319320 B |
