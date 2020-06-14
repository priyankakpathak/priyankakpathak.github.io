---
layout: post
title: Memory barriers in ARM64
subtitle: The cost of weakly-ordered memory model
tags: [arm64, performance, debugging, work]
comments: true
---

This is the 2nd of the blog posts series that talks about ARM64 performance investigation for .NET core. You can read the previous blog at [Part 1 - ARM64 performance of .Net Core](..\2020-06-30-Dotnet-Arm64-Performance).

In this post, I will describe the implication of weakly-ordered memory model of ARM64 on generated code by .NET.

### Memory ordering

The ARM architecture has weakly-ordered memory model. The processor can re-order the memory access instructions to improve the performance of the processor. It can rearrange the instructions to reduce the time processor takes to access memory. The order in which user has written the code is not guaranteed to be executed in same order and can be weakly defined depending on the memory access cost for given code. This is fine for single core machine but imagine a situation in multi-core machine where core A is about to read a memory that just got overwritten by core B. The user might have intended to have code that is executed by core A to read memory before the code executed by core B writes. As you can see, this can easily get complicated and problematic. Luckily, in such situations, there are instructions to tell processors not to re-arrange memory access at a particular point inside code. The technical term for such instructions that restricts this re-arrangement is called "memory barriers". The `dmb` instruction in ARM64 acts as a barrier prohibiting the processors from moving the instructions across. There are various types of barriers depending on if the user intent to restrict the re-ordering of just memory load or memory store or both. There are instructions to also have a barrier one-way i.e. instructions can be moved from after the barrier to before the barrier but not other way round or vice-versa. More details can be read in [ARM documentation](https://developer.arm.com/docs/den0024/a/memory-ordering) or some of the excellent articles [here](https://preshing.com/20120930/weak-vs-strong-memory-models/) and [here](https://afana.me/archive/2015/07/10/memory-barriers-in-dot-net.aspx/).


### C# memory barrier using volatile

There are various ways to express the desire in C# to add  [memory barriers](https://en.wikipedia.org/wiki/Memory_barrier). One such way is by using [volatile variable](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/volatile) in C#. With `volatile` variable, it is guaranteed that the runtime, JIT or the processor will not rearrange reads and writes to memory locations for performance. In order to make this happen, RyuJIT would emit `dmb` (data memory barrier) instruction for ARM64 everytime there is an access (read/write) to a `volatile` variable. 

For example, below is the C# code taken from [microbenchmarks](https://github.com/dotnet/performance/blob/master/src/benchmarks/micro/libraries/System.Threading/Perf.Volatile.cs#L17). It does a volatile read of local field `_location`.

```csharp
public class Perf_Volatile
{
    private double _location = 0;
    
    [Benchmark]
    public double Read_double() => Volatile.Read(ref _location);
}
```

The generated relevant  machine code for `Read_double` is:

```
; Assembly listing for method Program:Read_double():double:this
; Emitting BLENDED_CODE for generic ARM64 CPU - Windows

G_M49790_IG02:
        91002000          add     x0, x0, #8
        FD400000          ldr     d0, [x0]
        D50339BF          dmb     ishld
```

The code first gets the address of `_location` field, loads the value in `d0` register and then execute `dmb ishld` that acts as a data memory barrier.

Although this guarantees the memory ordering, there is a cost for it. The processor has to guarantee that all the data access done before the memory barrier is visible to all the cores after the barrier instruction. This means that the barrier requires all memory operations to complete before letting the cores cross the barrier instruction and this is time consuming.


### System.Collection.Concurrent.ConcurrentDictionary

While going through the benchmarks that are slower on ARM64, I noticed that [a simple benchmark](https://github.com/dotnet/performance/blob/master/src/benchmarks/micro/libraries/System.Collections/Concurrent/Count.cs#L37) that gets the count of number of items stored inside `System.Collection.Concurrent.ConcurrentDictionary` was slower. On inspecting [GetCountInternal](https://github.com/dotnet/runtime/blob/0f834db1fd80cf82e5ef27f72c48af1c911775da/src/libraries/System.Collections.Concurrent/src/System/Collections/Concurrent/ConcurrentDictionary.cs#L1005), I noticed that there was an access of `volatile` variable inside a loop. That means, for ARM64 we were executing memory barriers inside a loop. Interesting part was that this was despite the fact that [appropriate locks](https://github.com/dotnet/runtime/blob/0f834db1fd80cf82e5ef27f72c48af1c911775da/src/libraries/System.Collections.Concurrent/src/System/Collections/Concurrent/ConcurrentDictionary.cs#L974) were already taken before the start of loop. More details can be seen in [this issue](https://github.com/dotnet/runtime/issues/34198). If I just hoist the access of `volatile` variable outside the loop, that would be a tremendous win. A simple proof of concept showed that `GetCountInternal()` would improve by <b>30%</b>. That's a huge win. The [fix to hoist volatile variable access out of loop](https://github.com/dotnet/runtime/pull/34225) was merged. In it, I was able to also hoist the access for some other methods `CopyTo`, `GetKeys` and `GetValues`. I am pretty sure there are other places in .NET code base where similar code is there and someday I need to sit and scan through it to get more performance wins.

### Summary

If you are writing C# code targetting ARM64, make sure to double check the `volatile` variable's usage.



Namaste!
