---
layout: post
title: Debugging a hang on Ubuntu
subtitle: Tale of a bug while porting node-chakracore to ubuntu
# cover-img: /assets/img/ubuntu-hang.jpg
tags: [ubuntu, debugging]
---

This is the blog post that I [wrote 4 years back](https://github.com/kunalspathak/kunalsblog/blob/master/_posts/2016-11-04-Debugging-Hang-On-Ubuntu.md) but never published it. Well, better to be late than never.

In this post I will talk about a hang that we started noticing after enabling JIT for [node-chakracore](https://github.com/nodejs/node-chakracore) in Ubuntu. Before debugging for this hang, I had never worked on Ubuntu (forget about debugging) and the only knowledge I had was from what I learnt in `Unix programming` back in school. Here I will share my experience in debugging a bug on a new platform, how I approached this bug, what things I read to understand what is going on and finally how I solved it. During investigation I read about concepts that were later found to be unnecessary for fixing the bug, however reading about it had added those concepts in my knowledge base which will definitely prove to be beneficial in future.

### Problem statement

Recently our team ported `node-chakracore` on Ubuntu. We took our dependency on [coreclr's platform abstraction layer](https://github.com/dotnet/coreclr/tree/master/src/pal) for cross-platform effort. The cross-platform work was done in two phases. In first phase, chakracore's interpreter was ported to Ubuntu and in second phase its JIT was ported. After we enabled JIT for `node-chakracore` on Ubuntu platform, we started seeing hangs in unit test. The unit test worked fine on Windows OS. The reproducible code was very simple as shown below.

```js
const spawn = require('child_process').spawn;
const child = spawn('sleep', ['5']);

child.stdout.on('data', (data) => {
  console.log(`stdout: ${data}`);
});

child.stderr.on('data', (data) => {
  console.log(`stderr: ${data}`);
});

child.on('close', (code) => {
  console.log(`child process exited with code ${code}`);
});

console.log('parent done');
```

Parent process spawns a child process and waits for its exit. Once child process exits, parent process prints child process's exit code and the test terminates. With `node-chakracore` parent process was forever waiting for child process to exit even after child process terminates.

## Background on Windows

Instead of starting to debug the test case on Ubuntu, I wanted to understand how the underlying `node.js` code works. Being a windows user and having a good hang of debugging in Windows, I started understanding how processes are spawned internally with `nodejs`.
As most `node.js` users might know that node's backbone is based on top of  [libuv](http://docs.libuv.org/en/v1.x/design.html). It has two great features that makes it perfect to work with `nodejs`.
First, it provides a platform abstraction layer for low level system APIs I/O, threading, processes, sockets, etc. such that developers of host (in this case `nodejs`) don't have to worry about underlying targeted platform for these operations. For example, file open operation is abstracted out using `uv_file_open()` which calls Windows's `CreateFileW()` or Unix's `open()` API. Second and most important is it's I/O or event-based loop in a single threaded environment. The loop lets the user perform asynchronous i.e. non-blocking I/O operations and provide a way to fire appropriate callbacks using platform's polling mechanism when the I/O operations are complete.

I started reading about polling mechanism, I/O Completion Ports for Windows aka [IOCP](https://msdn.microsoft.com/en-us/library/windows/desktop/aa365198.aspx). Main thread creates a completion port for a file handle and starts an asynchronous operation on the file handle. The completion port is a way to tell Windows that after I/O operation for that file handle is complete, queue a completion packet for the corresponding port which will be consumed by the main thread. Alternatively, other threads of the process can also queue the completion packet on a particular completion port. Later provides a way to do communication between threads in a process.  When main thread completes its other task, it comes back and check if completion packet for a port has arrived or not. If not, either it can wait indefinitely or can check its status again after some time. If the completion packet did arrive, it means the I/O operation on the file handle was completed. At this point main thread can fire a callback to be executed after file operations. This is exactly how `libuv` uses `IOCP`. For example, `fs.readFile(filePath, callback)` API of `nodejs` would call `libuv`'s API to start an asynchronous read operation. `libuv` would then create an I/O completion port for file handle to read and as long as there are other events to service, event-loop will continue processing those request while periodically checking if I/O completion packet has arrived or not. Once the completion packet has arrived, it will fire a callback, which `nodejs` has hooked up to user's `callback` method.

With this knowledge, I started debugging the flow on Windows to understand what happens when child process is started and how does it communicate to parent process that it has terminated. `child_process.spawn()` calls into `uv_spawn()` which starts a child process using [RegisterWaitForSingleObject](https://github.com/nodejs/node-chakracore/blob/4e0ecc909a30ac7be435550330fa90b4f21c8990/deps/uv/src/win/process.c#L1156). Reading more about RegisterWaitForSingleObject on [msdn](https://msdn.microsoft.com/en-us/library/windows/desktop/ms685061.aspx) it lets you set a callback that will get triggered when child process exits. In the exit callback, [I/O completion packet is queued](https://github.com/nodejs/node-chakracore/blob/4e0ecc909a30ac7be435550330fa90b4f21c8990/deps/uv/src/win/process.c#L850) which is then received by the I/O loop of the main process. Once the completion packet is received it terminates because this was the only incomplete operation that parent process was waiting for.

At this point, I got a fair understanding of how `IOCP` is used by `libuv` to make child to parent process communication possible in `nodejs`on Windows. It was now time to carry this knowledge on Ubuntu.

## Debugging on Ubuntu

I started on Ubuntu by reading man-pages for [epoll_wait](http://man7.org/linux/man-pages/man2/epoll_wait.2.html#RETURN_VALUE) (`epoll_wait` on Ubuntu is similar to `IOCP` on windows) and added some tracing around `libuv`'s [call to epoll_wait](https://github.com/nodejs/node-chakracore/blob/4e0ecc909a30ac7be435550330fa90b4f21c8990/deps/uv/src/unix/linux-core.c#L275) to see how many IO completion request are received (`nfsd` value) in the loop. I had another repository of `node-chakracore` that didn't have newly added JIT support changes and hence it didn't hang on the repro. Henceforth I will call it good repo. I used this repo as my baseline. Whatever tracing I added to my buggy repo, I added it in good repo. On good repo, I noticed that `nfsd` value was always one greater than that of buggy repo. What does it mean? After more code reading around `libuv` code it was clear that while buggy repo was waiting forever for one I/O completion packet to arrive, good repo waits momentarily until child process exits after which it receives all I/O completion packets and it exits. No doubt the pending completion packet is related to some kind of communication from child process that is not reaching parent process.

I did some more research on `fork()` syscall and read about how child process uses `SIGCHLD` signal to communicate its exit status to parent process. It could be that parent process is not receiving `SIGCHLD` from child process, hence the hang. But how could I confirm if that is really the case? How can I check the state of state of signals for a process at given time? Little more search on internet landed me on [excellent article](http://www.computerworld.com/article/2693548/unix-viewing-your-processes-through-the-eyes-of-proc.html) that explains how to use `proc/<pid>/status` to view the state of signals for a given process. For my repro, I noted the `SIGCHLD` is in `SigBlk` (blocked signal) and `ShdPnd` (process-wide shared pending signal) state. This was making some sense now. Basically child process did terminate and it sent `SIGCHLD` signal that parent process was expecting to receive. However that signal in the parent process was in pending/blocked state which means the signal was not yet received by the parent process, thus leading to indefinitely waiting and causing hang. Just to confirm this is the case, I used gdb's [signal level breakpoints](https://sourceware.org/gdb/onlinedocs/gdb/Signals.html#Signals) to tell `gdb` what action to take when particular signal is received by the process. I ran good and buggy repo with `gdb` and found that parent process of good repo receives `SIGCHLD` at one point and that of buggy repo never receives `SIGCHLD` signal.

Next task was to understand why `SIGCHLD` signal was going to blocked state on the buggy repo. Found gnu's [Blocking Signals](https://www.gnu.org/software/libc/manual/html_node/Blocking-Signals.html) documentation which answered most of questions. For those who are not aware, it talks about system APIs that lets process decide action (like block or unblock) that should be taken when particular signal(s) are received by the process. For example, if the program wants that it shouldn't get interrupted when it is performing critical task, it can block all the signals before the task and unblock them after the task.
```bash
block some or all signals
...
perform critical task
...
unblock some or all signals
```
Every process has an associated signal mask (bitmap in which every bit corresponds to signal number as explained in [article](http://www.computerworld.com/article/2693548/unix-viewing-your-processes-through-the-eyes-of-proc.html)). Ther are system APIs like  `sigprocmask(int action, sigset *newSigmask, sigset *oldSigmask)` (`pthread_sigmask()` API for multi-threaded programs) that lets program specify `action` to be taken when signals that are ON in the `newSigmask` are received. The API also lets user save current sigmask in `oldSigmask` so it can be later use it to undo the changes.

At this point, I knew that I need to put breakpoint at `sigmask()` and `pthread_sigmask()` and inspect the value of action/sigmask that is passed to these APIs. I can do similar tracing in good repro and by mere comparison in traces, I should be able to pin-point, why buggy repro is blocking `SIGCHLD` signal that good repro is not. After running good and bad repro example in their own `gdb` session, I found out that after sometime, there was an extra call to `pthread_sigmask()` in buggy repo and inspecting the values of action/sigmask, it was for blocking `SIGCHILD` call. Looking at the call stack, it was coming from pal library's [CThreadSuspensionInfo::InitializePreCreate](https://github.com/dotnet/coreclr/blob/d2a17589e4e0b159a562256feb85242da0d1d223/src/pal/src/thread/threadsusp.cpp#L1049) . This function was blocking all the signals and was never resetting it back. We started taking dependency on `InitializePreCreate()` for creating job queue as part of our effort to enable JIT. That was it! Mystery solved.

I thought of opening an issue on [dotnet/coreclr](https://github.com/dotnet/coreclr/), but realized that this was issue was already fixed in [dotnet/coreclr#4863](https://github.com/dotnet/coreclr/pull/4863). So I just had to [cherry-pick](https://github.com/Microsoft/ChakraCore/pull/1795) these changes in chakracore and that would stop `node-chakracore` from hanging in simple repro case.

If you have reached this point, I am very grateful that you read this far. I hope this post will give you insights on how you can debug through complex and weird bugs on completely new platform.

Namaste!