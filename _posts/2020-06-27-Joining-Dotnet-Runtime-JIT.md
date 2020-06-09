---
layout: post
title: Joining the JIT team of .NET Runtime
subtitle: The journey
# cover-img: /assets/img/ubuntu-hang.jpg
tags: [life, work]
---

I am fascinated about compilers and low level programming since my undergrad when I first read the [Dragon Book](https://en.wikipedia.org/wiki/Compilers:_Principles,_Techniques,_and_Tools). I always had an urge to build a compiler of my own to see how the moving pieces come together and become the foundation of the entire computer industry. During my Master’s I did built [a XML based intermediate language](https://github.com/kunalspathak/uxml). It was super fun to work on since it gave me an exposure to the vast problem space a compiler developer needs to handle while building it.

I joined Microsoft as a tester in Windows Live ID team. The work in the team was great but my hunger for compilers led me to join the [Chakra team](https://github.com/microsoft/ChakraCore). Here, I got an opportunity to work with lot of smart people and learn the compiler domain from production-level compiler’s perspective. Not only was this my first time working on C++ code base, I also got my hands-on various debugging and analysis tools like `windbg` and `performance explorer` that I always shy away from. I should admit, the compilers built in schools which is primarily based on theory is way different than the one that is built in a software company. After working for almost 5 years in Chakra, I got attracted to the new kid on the block, "Artificial Intelligence". I wanted to join a team which works in machine learning but at the same time has its roots back to programming language/compilers. [Prose](https://microsoft.github.io/prose/) was a perfect match for me. After working in PROSE for almost 2 years, I decided to switch back to the low-level compiler team. That is when I got an opportunity to join the [JIT team of .NET Runtime](https://github.com/dotnet/runtime). 

It is a privilege to be part of this team, not only because every .NET user will use the code we write in this team, but there are lot of challenging projects to work on. For next few months, I will be evaluating the ARM64 performance of .NET 5 in comparison with that of x64. I hope my performance analysis skillset and expertise I learned in the past prove useful in this team as well. I will cover some of the interesting ARM64 performance issues I find on the way in a separate blog post.

Till then, Namaste!