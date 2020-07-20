---
layout: post
title: Vectorization using .NET APIs
subtitle: Vector64&lt;T&gt; and Vector128&lt;T&gt;
tags: [work, arm64, intrinsics]
---


### Introduction

It has been few years now that [.NET added SIMD support](https://devblogs.microsoft.com/dotnet/the-jit-finally-proposed-jit-and-simd-are-getting-married/). Last year, in .NET Core 3.0, a new feature ["hardware intrinsics"](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/) was introduced. This feature gives access to various vectorized and non-vectorized hardware instructions that modern hardware support. .NET developers can access these instructions using set of APIs under `System.Runtime.Intrinsics` and `System.Runtime.Intrinsics.X86` for Intel x86/x64 architecture. In .NET Core 5.0, APIs are added under `System.Runtime.Intrinsics.Arm` for ARM architecture. 

`Vector64<T>`, `Vector128<T>` and `Vector256<T>` data types represents vectorized data of size 64, 128 and 256 bits respectively and are the ones on which majority of these intrinsic APIs operate on. `Vector128<T>` and `Vector256<T>` are used for Intel instructions while `Vector64<T>` and `Vector128<T>` operates on ARM instructions. In this post, I will describe about the data types that operate on ARM64.


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

#### `Vector64<byte> Create(byte value)`

Creates a `Vector64<byte>` with all elements initialized to `value`.

```csharp
Vector64<byte> data = Vector64.Create((byte)5);
Console.WriteLine(data);
// <5, 5, 5, 5, 5, 5, 5, 5>
```


#### `Vector<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7)`

Creates a `Vector64<byte>` with elements initialized to `e0`, `e1`,...., `e7`.

```csharp
Vector64<byte> data = Vector64.Create((byte)24, 25, 26, 27, 28, 29, 30, 31);
Console.WriteLine(data);
// <24, 25, 26, 27, 28, 29, 30, 31>
```
