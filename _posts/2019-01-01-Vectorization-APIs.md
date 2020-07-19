---
layout: post
title: Vectorization using .NET APIs
subtitle: Vector64&lt;T&gt; and Vector128&lt;T&gt;
tags: [work, arm64, intrinsics]
---


### Introduction

It has been few years now that [.NET added SIMD support](https://devblogs.microsoft.com/dotnet/the-jit-finally-proposed-jit-and-simd-are-getting-married/). Last year, in .NET Core 3.0, a new feature ["hardware intrinsics"](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/) was introduced. This feature gives access to various vectorized and non-vectorized hardware instructions that modern hardware support. .NET developers can access these instructions using set of APIs under `System.Runtime.Intrinsics` and `System.Runtime.Intrinsics.X86` for Intel x86/x64 architecture. In .NET Core 5.0, APIs are added under `System.Runtime.Intrinsics.Arm` for ARM architecture. 

`Vector64<T>`, `Vector128<T>` and `Vector256<T>` data types represents vectorized data of size 64, 128 and 256 bits respectively and are the ones on which majority of these intrinsic APIs operate on. `Vector128<T>` and `Vector256<T>` are used for Intel instructions while `Vector64<T>` and `Vector128<T>` operates on ARM instructions. In this post, I will describe about the data types that operate on ARM64.

TODO: Intent is to show examples


### Vector128

```cmd
        ------------------------------128-bits---------------------------
        |                                                               |
        V                                                               V
        -----------------------------------------------------------------
        |              D                |                D              |  V0.2D
        -----------------------------------------------------------------
        |       S       |       S       |       S       |       S       |  V0.4S
        ----------------------------------------------------------------|
        |   H   |   H   |   H   |   H   |   H   |   H   |   H   |   H   |  V0.8H
        -----------------------------------------------------------------
        | B | B | B | B | B | B | B | B | B | B | B | B | B | B | B | B |  V0.16B
        -----------------------------------------------------------------        
```

- `V0.2D` : Holds 2 64-bits values of type `double`, `ulong` or `long`. They are represented by `Vector128<double>`, `Vector128<ulong>` and `Vector128<long>` data type respectively.

- `V0.4S` : Holds 4 32-bits values of type `float`, `uint` or `int`. They are represented by `Vector128<float>`, `Vector128<uint>` and `Vector128<int>` data type respectively.

- `V0.8H` : Holds 8 16-bits values of type `ushort` or `short`, They are represented by `Vector128<ushort>` and `Vector128<short>`respectively.

- `V0.16B` : Holds 16 8-bits values of type `byte` or `sbyte`, They are represented by `Vector128<byte>` and `Vector128<sbyte>`respectively.

### Vector64


```cmd
                                        ------------- 64-bits -----------
                                        |                               |
                                        V                               V
        -----------------------------------------------------------------
        |           Unused              |                D              |  V19.1D
        -----------------------------------------------------------------
        |           Unused              |       S       |       S       |  V19.2S
        ----------------------------------------------------------------|
        |           Unused              |   H   |   H   |   H   |   H   |  V19.4H
        -----------------------------------------------------------------
        |           Unused              | B | B | B | B | B | B | B | B |  V0.16B
        -----------------------------------------------------------------        
```

- `V19.1D` : Holds 1 64-bits values of type `double`, `ulong` or `long`. They are represented by `Vector64<double>`, `Vector64<ulong>` and `Vector64<long>` data type respectively.

- `V19.2S` : Holds 2 32-bits values of type `float`, `uint` or `int`. They are represented by `Vector64<float>`, `Vector64<uint>` and `Vector64<int>` data type respectively.

- `V19.4H` : Holds 4 16-bits values of type `ushort` or `short`, They are represented by `Vector64<ushort>` and `Vector64<short>`respectively.

- `V19.8B` : Holds 8 8-bits values of type `byte` or `sbyte`, They are represented by `Vector64<byte>` and `Vector128<sbyte>`respectively.

### Data representation

Let us understand how the data is interpreted in various data types. We will take an example of `Vector64` but is applicable to `Vector128` as well.
Suppose you are operating on 8 8-bits `<11, 12, 13, 14, 15, 16, 17, 18>` . Let us see how they are stored in binary format.

```cmd
lane:     0           1         2          3         4           5          6          7 
      -----------------------------------------------------------------------------------------
      | 00001011 | 00001100 | 00001101 | 00001110 | 00001111 | 00010000 | 00010001 | 00010010 | 
      -----------------------------------------------------------------------------------------
data:     11          12        13         14          15         16         17         18 
```

However, same data can be interpreted in 4 16-bits as `<3083, 3597, 4111, 4625>`.

```cmd
lane:          0                  1                   2                 3          
      ------------------------------------------------------------------------------
      | 0000110000001011 | 0000111000001101 | 0001000000001111 | 0001001000010001  | 
      ------------------------------------------------------------------------------
data:          3083              3597                4111              4625 
```

Next, it can be interpreted as 2 32-bits to get `<235736075, 303108111>`.

```cmd
lane:                  0                                  1
      -----------------------------------------------------------------------
      | 00001110000011010000110000001011 | 00010010000100010001000000001111 |
      -----------------------------------------------------------------------
data:               235736075                           303108111
```

Lastly, it will be `<1301839424133073931>` if interpreted as 1 64-bits value.

```cmd
lane:                                 0
      --------------------------------------------------------------------
      | 0001001000010001000100000000111100001110000011010000110000001011 |
      --------------------------------------------------------------------
data:                          1301839424133073931
```


### APIs examples

`1. Vector64<byte> Create(byte value)`

Creates a `Vector64<byte>` with all elements initialized to `value`.

```csharp
Vector64<byte> data = Vector64.Create((byte)5);
Console.WriteLine(data);
// <5, 5, 5, 5, 5, 5, 5, 5>
```

Similar APIs that operate on different sizes:

```csharp
public static unsafe Vector64<double> Create(double value)
public static unsafe Vector64<short> Create(short value)
public static unsafe Vector64<int> Create(int value)
public static unsafe Vector64<long> Create(long value)
public static unsafe Vector64<sbyte> Create(sbyte value)
public static unsafe Vector64<float> Create(float value)
public static unsafe Vector64<ushort> Create(ushort value)
public static unsafe Vector64<uint> Create(uint value)
```

------

`2. Vector64<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7)`

Creates a `Vector64<byte>` with elements initialized to `e0`, `e1`,...., `e7`.

```csharp
Vector64<byte> data = Vector64.Create((byte)24, 25, 26, 27, 28, 29, 30, 31);
Console.WriteLine(data);
// <24, 25, 26, 27, 28, 29, 30, 31>
```

Similar APIs that operate on different sizes:

```csharp
public static unsafe Vector64<short> Create(short e0, short e1, short e2, short e3)
public static unsafe Vector64<int> Create(int e0, int e1)
public static unsafe Vector64<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7)
public static unsafe Vector64<float> Create(float e0, float e1)
public static unsafe Vector64<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3)
public static unsafe Vector64<uint> Create(uint e0, uint e1)
public static unsafe Vector64<ulong> Create(ulong value)
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.create?view=netcore-3.1).

------

`3. Vector64<byte> CreateScalar(byte value)`

Creates a `Vector64<byte>` with first element initialized to `value` and remaining elements initialized to zero.

```csharp
Vector64<byte> data = Vector64.CreateScalar((byte)11);
Console.WriteLine(data);
// <11, 0, 0, 0, 0, 0, 0, 0>
```

Similar APIs that operate on different sizes:

```csharp
public static unsafe Vector64<short> CreateScalar(short value)
public static unsafe Vector64<int> CreateScalar(int value)
public static unsafe Vector64<sbyte> CreateScalar(sbyte value)
public static unsafe Vector64<float> CreateScalar(float value)
public static unsafe Vector64<ushort> CreateScalar(ushort value)
public static unsafe Vector64<uint> CreateScalar(uint value)
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.createscalar?view=netcore-3.1).

------
`4. Vector64<byte> CreateScalarUnsafe(byte value)`

Creates a `Vector64<byte>` with first element initialized to `value` and remaining elements left uninitialized.

```csharp
Vector64<byte> data = Vector64.CreateScalarUnsafe((byte)11);
Console.WriteLine(data);
// <11, 0, 0, 0, 0, 0, 0, 0>
```

Similar APIs that operate on different sizes:

```csharp
public static unsafe Vector64<short> CreateScalarUnsafe(short value)
public static unsafe Vector64<int> CreateScalarUnsafe(int value)
public static unsafe Vector64<sbyte> CreateScalarUnsafe(sbyte value)
public static unsafe Vector64<float> CreateScalarUnsafe(float value)
public static unsafe Vector64<ushort> CreateScalarUnsafe(ushort value)
public static unsafe Vector64<uint> CreateScalarUnsafe(uint value)
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.createscalarunsafe?view=netcore-3.1).

------
`5. T GetElement<T>(this Vector64<T> vector, int index)`

Gets element from `vector` at specified `index`.

```csharp
Vector64<byte> inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Console.WriteLine(inputs.GetElement(5));
// 16
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.getelement?view=netcore-3.1).

------

`6. Vector64<T> WithElement<T>(this Vector64<T> vector, int index, T value)`

Creates a new `Vector64<T>` with element at `index` set to `value` while remaining elements set to the same value as that in `vector`.

```csharp
Vector64<byte> inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Vector64<byte> updated = inputs.WithElement(5, (byte)100);
Console.WriteLine(updated);
// <11, 12, 13, 14, 15, 100, 17, 18>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.withelement?view=netcore-3.1).

------

`7. T ToScalar<T> ()`

Converts the vector to scalar by returning value of first element.

```csharp
Vector64<byte> inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Console.WriteLine(inputs.ToScalar());
// 11
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.toscalar?view=netcore-3.1).

------

`8. Vector128<T> ToVector128<T> ()`

Creates a `Vector128<T>` with lower 64-bits initialized to this vector and upper 64-bits initialized to zero.

```csharp
Vector64<byte> inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Vector128<byte> input128 = inputs.ToVector128();
Console.WriteLine(input128);
// <11, 12, 13, 14, 15, 16, 17, 18, 0, 0, 0, 0, 0, 0, 0, 0>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.tovector128?view=netcore-3.1).

------

`9. Vector128<T> ToVector128Unsafe<T> ()`

Creates a `Vector128<T>` with lower 64-bits initialized to this vector and upper 64-bits remain uninitialized.

```csharp
Vector64<byte> inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Vector128<byte> input128 = inputs.ToVector128Unsafe();
Console.WriteLine(input128);
// <11, 12, 13, 14, 15, 16, 17, 18, 0, 0, 0, 0, 0, 0, 0, 0>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.tovector128unsafe?view=netcore-3.1).

------

`10. Vector64<ushort> AsUInt16<T> ()`

Reinterprets a `Vector64<T>` as new `Vector64` of type `ushort`.

```csharp
Vector64<byte>  inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Vector<ushort> converted = inputs.AsUInt16();
Console.WriteLine(converted);
// <3083, 3597, 4111, 4625>
```

Similar APIs that operate on different sizes:

```csharp
public static Vector64<byte> AsByte<T>(this Vector64<T> vector)
public static Vector64<double> AsDouble<T>(this Vector64<T> vector)
public static Vector64<short> AsInt16<T>(this Vector64<T> vector)
public static Vector64<int> AsInt32<T>(this Vector64<T> vector)
public static Vector64<long> AsInt64<T>(this Vector64<T> vector)
public static Vector64<sbyte> AsSByte<T>(this Vector64<T> vector)
public static Vector64<float> AsSingle<T>(this Vector64<T> vector)
public static Vector64<uint> AsUInt32<T>(this Vector64<T> vector)
public static Vector64<ulong> AsUInt64<T>(this Vector64<T> vector)
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.asbyte?view=netcore-3.1).

------

`11. Vector64<U> As<T, U>()`

Reinterprets a `Vector64<T>` as new `Vector64` of type `U`.

```csharp
Vector64<byte> inputs = Vector64.Create((byte)11, 12, 13, 14, 15, 16, 17, 18);
Vector64<ushort> converted = inputs.As<byte, ushort>();
Console.WriteLine(converted);
// <3083, 3597, 4111, 4625>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector64.as?view=netcore-3.1).

------

`12. Vector64<T> GetLower<T> (this Vector128<T> vector)`

This is an API on `Vector128<T>` that gets the lower 64-bits from 128-bits.

```csharp
Vector128<uint> input = Vector128.Create((uint)11, 12, 13, 14);
Vector64<uint> lower = input.GetLower();
Console.WriteLine(lower);
// <11, 12>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector128.getlower?view=netcore-3.1).

------

`13. Vector64<T> GetUpper<T> (this Vector128<T> vector)`

This is an API on `Vector128<T>` that gets the upper 64-bits from 128-bits.

```csharp
Vector128<uint> input = Vector128.Create((uint)11, 12, 13, 14);
Vector64<uint> upper = input.GetUpper();
Console.WriteLine(upper);
// <13, 14>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector128.getupper?view=netcore-3.1).

------

`14. Vector128<T> WithLower<T> (this Vector128<T> vector, Vector64<T> value)`

Creates a new `Vector128<T>` with lower 64-bits set to the specified `value` and upper 64-bits remain same value.

```csharp
Vector128<uint> input = Vector128.Create((uint)11, 12, 13, 14); // <11, 12, 13, 14>
Vector64<uint> lowered = Vector64.Create((uint)100); // <100, 100>
Vector128<uint> newly = input.WithLower(lowered);
Console.WriteLine(newly);
// <100, 100, 13, 14>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector128.withlower?view=netcore-3.1).

------

`15. Vector128<T> WithUpper<T> (this Vector128<T> vector, Vector64<T> value)`

Creates a new `Vector128<T>` with upper 64-bits set to the specified `value` and lower 64-bits remain same value.

```csharp
Vector128<uint> input = Vector128.Create((uint)11, 12, 13, 14); // <11, 12, 13, 14>
Vector64<uint> uppered = Vector64.Create((uint)100); // <100, 100>
Vector128<uint> newly = input.WithUpper(uppered);
Console.WriteLine(newly);
// <11, 12, 100, 100>
```

See MSDN reference [here](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.vector128.withupper?view=netcore-3.1).

