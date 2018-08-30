``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Core i7-6600U CPU 2.60GHz (Max: 2.61GHz) (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.401
  [Host] : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT
  Core   : .NET Core 2.1.3 (CoreCLR 4.6.26725.06, CoreFX 4.6.26725.05), 64bit RyuJIT

Job=Core  Runtime=Core  InvocationCount=1  
UnrollFactor=1  

```
|            Method |         Mean |     Error |     StdDev |       Median | Allocated |
|------------------ |-------------:|----------:|-----------:|-------------:|----------:|
|      TryAddSingle |     9.693 us |  1.838 us |   5.243 us |     8.460 us |     376 B |
| TryAddSingleTwice |     9.569 us |  1.648 us |   4.809 us |     8.400 us |     648 B |
|        TryAdd1000 | 2,385.790 us | 98.249 us | 285.038 us | 2,273.145 us |  937984 B |
