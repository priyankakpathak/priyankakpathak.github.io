---
layout: post
title: ARM64 performance of .Net Core
subtitle: The foundation
# cover-img: /assets/img/ubuntu-hang.jpg
tags: [tools, arm64, performance, work]
---

### Background 
As I mentioned in the [previous post](..\2020-06-27-Joining-Dotnet-Runtime-JIT), soon after joining .NET runtime's JIT team, I started with a project to evaluate .NET core performance on ARM64 and analyzing how good or bad it performs with respect to Intel's x64. I did not have any prior experience working on ARM64, so this was going to be a roller coaster ride.

To give little background, RyuJIT is the .NET runtime's just-in-time compiler. The [Roslyn compiler]( https://en.wikipedia.org/wiki/Roslyn_(compiler)) compiles C# into intermediate language (IL) which is saved on disk as `.dll` or `.exe`. During execution, the .NET runtime invokes RyuJIT to compile the IL into target machine code and finally the generated machine code gets executed. ARM64 architecture support was added in RyuJIT during .NET core 2.1. If you are interested, you can read a [nice blog post]( https://devblogs.microsoft.com/dotnet/the-ryujit-transition-is-complete/) that talks about the history of RyuJIT. ARM64 had functional parity so far, but hardly any time was spent in investigating the quality of code generated.

### Benchmarks
I started my investigation looking at the [BechmarkDotNet](https://benchmarkdotnet.org/)  x64 vs. ARM64 numbers that are published [here](https://aka.ms/dotnetperfindex). There was 1000+ [Microbenchmarks](https://github.com/dotnet/performance/tree/master/src/benchmarks/micro) in the report with lot of variation in the ratio of x64/arm64 numbers. ARM64 benchmark numbers were slower anywhere between 2X ~ 50X. With that big difference, it becomes very difficult to figure out where to focus the attention towards. I started looking at the outliers i.e. the ones that were slowest on ARM64 grouped in similar area i.e. threading, locking, object creation, etc. This helped me study the characteristics of benchmark code and stay focus on particular area that is slower for ARM64, but won't tell me why it is slow.

### Building benchmarks on Windows ARM64
Next step in the investigation was running the benchmarks locally on ARM64 machine so I can do further analysis. With the choice of Windows and Linux, I preferred Windows ARM64 because that is the OS and tools, I am most familiar with. I cloned the `dotnet/performance` repository and followed the [process to build and execute the benchmarks]( https://github.com/dotnet/performance/blob/master/src/benchmarks/micro/README.md). After [a small fix](https://github.com/dotnet/performance/pull/1243) I was able to run the microbenchmarks on Windows ARM64.  There is a great collection of [documentation](https://github.com/dotnet/performance/tree/master/docs) present. However, I would just summarize the steps to build and run microbenchmarks. These steps worked correctly at the time of writing this post. This information can also be found in the documentation.

The assumtion is that the `dotnet/performance` repository is cloned at `<repo_location>`.

#### Environment variables 

```bash
# Do not resolve the path of runtime, shared framework or SDK from other locations to ensure build determinism
set DOTNET_MULTILEVEL_LOOKUP=0

# Avoid spawning any-long living compiler processes to avoid getting "file in use" errors while running or cleaning the benchmark code
set UseSharedCompilation=false

# .NET core version to run benchmarks for
set PERFLAB_TARGET_FRAMEWORKS=netcoreapp5.0

# Location to pick up dotnet.exe from
set DOTNET_ROOT=<repo_location>\tools\dotnet\arm64

# Add the dotnet.exe to the path
set path=%DOTNET_ROOT%;%PATH%
```
<p/>

#### Restore and build

```bash
# Restore all the nuget packages
dotnet restore <repo_location>\\src\\benchmarks\\micro\\MicroBenchmarks.csproj --packages <repo_location>\\artifacts\\packages

# Build the MicroBenchmarks.csproj
dotnet build <repo_location>\\src\\benchmarks\\micro\\MicroBenchmarks.csproj --configuration Release --framework netcoreapp5.0 --no-restore /p:NuGetPackageRoot=<repo_location>\\artifacts\\packages -o <repo_location>\\artifacts
```
<p/>

#### Execute

```bash
dotnet <repo_location>\\artifacts\\MicroBenchmarks.dll
```
<p/><p/>

## Strategy of performance investigation

Now that I have the list of outlier benchmarks and steps to run them, I could just run and compare them on Intel x64 (my dev box) vs. ARM64. But how would I know what is causing the slowness on ARM64? If you see [RyuJIT phases](https://github.com/dotnet/runtime/blob/master/docs/design/coreclr/jit/ryujit-tutorial.md#ryujit-phases), they all work on an IR that is independent of target architecture. It is the backend phases like lowering, register allocation and code generation that are target specific. So most likely the IR that reaches the lower phase is target neutral and is same for all architecture. I tried exploring some of the performance profiling tools like [perfview](https://github.com/microsoft/perfview) and [ETW profiler](https://adamsitnik.com/ETW-Profiler/) to profile the slower benchmarks. But I was not able to get a satisfactory answer. To recap, I wanted to know why a benchmark runs slow on on ARM64 than x64. The profiling tool would tell me in what portion of code most time was spent during benchmark execution and most likely it will be same for both x64 and ARM64. Most time is spent in the code or API that is getting benchmarked. Consider [Read_double](https://github.com/dotnet/performance/blob/master/src/benchmarks/micro/libraries/System.Threading/Perf.Volatile.cs#L17) benchmark. This is a single line of benchmark code. When I run this benchmark most of the time `Read_double` is on the call stack and hence will show up in the profiling tool as being hot. But that is the benchmark I am running so it ought to be hot! I need to figure out different strategy for my investigation. 

I then decided to start doing comparison at machine code level. In RyuJIT's non-release mode, you can view the dump or generated machine code by setting [environment variables](https://github.com/dotnet/runtime/blob/master/docs/design/coreclr/jit/viewing-jit-dumps.md). I started dumping the generated machine code produced for the benchmark on both architectures and then comparing them. It was not easy to do such comparison using any `diff` tool because the instruction set of Intel and ARM are very different. While Intel's x64 has [CISC](https://en.wikipedia.org/wiki/Complex_instruction_set_computer) and ARM is a [RISC](https://en.wikipedia.org/wiki/Reduced_instruction_set_computer) architecture, it becomes extremely difficult to map the instructions one-to-one during comparison. However, by looking at the generated code for both the architectures over and over, the brain starts doing the mapping automatically because both the code is generated from similar IR.

To get a taste, here is the example of generated code for `Read_double` benchmark that I mentioned above on x64 and ARM64:

Intel x64:

```
; Assembly listing for method System.Threading.Tests.Perf_Volatile:Read_double():double:this
; Emitting BLENDED_CODE for X64 CPU with AVX - Windows

G_M28692_IG01:
       C5F877               vzeroupper
                                                ;; bbWeight=1    PerfScore 1.00
G_M28692_IG02:
       4883C108             add      rcx, 8
       C5FB1001             vmovsd   xmm0, qword ptr [rcx]
                                                ;; bbWeight=1    PerfScore 2.25
G_M28692_IG03:
       C3                   ret
                                                ;; bbWeight=1    PerfScore 1.00

```

ARM64:

```
; Assembly listing for method Program:Read_double():double:this
; Emitting BLENDED_CODE for generic ARM64 CPU - Windows

G_M49790_IG01:
        A9BF7BFD          stp     fp, lr, [sp,#-16]!
        910003FD          mov     fp, sp
                                                ;; bbWeight=1    PerfScore 1.50
G_M49790_IG02:
        91002000          add     x0, x0, #8
        FD400000          ldr     d0, [x0]
        D50339BF          dmb     ishld
                                                ;; bbWeight=1    PerfScore 13.50
G_M49790_IG03:
        A8C17BFD          ldp     fp, lr, [sp],#16
        D65F03C0          ret     lr
                                                ;; bbWeight=1    PerfScore 2.00

```

If you see above, the generated code of both architecture has 3 [basic blocks](https://en.wikipedia.org/wiki/Basic_block) i.e. `IG01`, `IG02` and `IG03`. `IG01` being the prolog while `IG03` being the epilog. `IG02` contains the code to read and return the value of `volatile` variable `_location`.

With that, it was now easy to reason why any benchmark was slower or faster on ARM64. Just look at the generated machine code for the benchmark and compare it against that generated for x64. We can go further and see the cycles taken to execute the instructions for these architectures, but most of the time that level of analysis is not needed. From the machine code, it is easy to draw some preliminary conclusions.

Over the next few weeks, I will talk about various issues I found while doing the investigation using approach I just described. Some of the issues that I will discuss are listed [here](https://github.com/dotnet/runtime/issues/35853).

Namaste!