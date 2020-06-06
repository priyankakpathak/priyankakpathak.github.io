---
layout: page
title: About me
# subtitle: Why you'd want to go on a date with me
---

<img align="right" width="50%" height="50%" src="/assets/img/Kunal-S-Pathak-aboutme.jpeg" />
<!-- <img align="right" src="assets/img/Kunal-S-Pathak-aboutme.jpg"> -->

This is the blog of Kunal Pathak. I am a software engineer at Microsoft working in JIT team of [.NET runtime](https://github.com/dotnet/runtime). I have special interest in programming language, compilers and developer tools. In the past, I worked on Microsoft's Javascript Runtime [Chakra team](https://github.com/microsoft/chakracore) and a collaborator of [Node.js on Chakracore](https://github.com/nodejs/node-chakracore). My expertise are in backend low-level stack, assembly debugging, performance and memory analysis of critical products and services.

Outside work, I love travelling, learning new things and watching movies. This blog includes some of the things I learn while working or an experience that I find amusing to share.

The opinions stated here are my own, not necessarily those of my employer.

## Work history

As a software engineer, I have spent almost half of my life in front of computer.

<img align="right" width="15%" height="15%" src="/assets/img/Microsoft.png" /> <b> Software Engineer (2009 - present) </b>


 * <b>.NET Runtime</b>: As part of JIT team, my focus currently is optimizing .NET 5 for ARM64. This involves doing perforamnce analysis of generated assembly code for ARM64 and comparing and contrasting it against Intel x64. The [epic issue](https://github.com/dotnet/runtime/issues/35853) highlights some of the improvement areas that I found.

 * <b>Prose</b>: I am fortunate enough to get chance work in [Prose](https://microsoft.github.io/prose/) team led by program syntheis pioneer [Sumit Gulwani](https://www.microsoft.com/en-us/research/people/sumitg/). It is a blended team of researchers and engineers working together to bring program synthesis technology in Microsoft products. Here I worked on various interesting projects like translating the programs learned by Prose framework to [Java](https://microsoft.github.io/prose/release-notes/#release-510--20180207), internal engine of [Code accelerator SDK for Python](https://docs.microsoft.com/en-us/python/api/overview/azure/prose/intro?view=prose-py-latest). I helped with integration of Prose technology in "import data from webpages" scenario of [PowerBI Desktop](https://docs.microsoft.com/en-us/power-bi/connect-data/desktop-connect-to-web-by-example) and [Power Query](https://www.decisivedata.net/blog/extracting-data-web-using-power-query) as well as [split column by positions](https://www.sumproduct.com/blog/article/power-bi-tips/power-bi-new-transform-split-column-by-positions).
 
* <b>Chakra</b>: Chakra was the first industry standard compiler codebase that I worked on. I played two different roles in this team. Initially I started as a Software Engineer in Test role where I wrote various tools to verify JIT optimizations like array check hoisting, array bound check eliminations, cross context verifications, etc. Most interesting tool that I worked on was to automatically produce minimum reproducible code that triggers Chakra bug on Edge browser. After moving to developer role in Chakra, I worked on ambition project of [enabling node.js to run with Chakra engine](https://github.com/nodejs/node/pull/4765) called node-chakra. I was responsible to verify, analyze and fix compatibility issues of node-chakra with popular node modules. Another area that was super fun to work on was doing [performance analysis](https://github.com/Microsoft/ChakraCore/issues?utf8=%E2%9C%93&q=is%3Aissue%20author%3Akunalspathak%20label%3APerformance%20) of node-chakra on [techempower](https://www.techempower.com/benchmarks/) and [acmeair](https://github.com/acmeair/acmeair-nodejs), comparing it against node.js numbers and [submitting fixes](https://github.com/microsoft/ChakraCore/pulls?q=is%3Apr+author%3Akunalspathak+is%3Aclosed). There were also several [memory leaks](https://github.com/Microsoft/ChakraCore/pulls?utf8=%E2%9C%93&q=is%3Apr%20author%3Akunalspathak%20is%3Aclosed%20memory) fixes that helped improved overall memory consumption of chakra engine.

 
 * <b>Windows Live ID</b>: This was the first team where I started my career at Microsoft. I joined this team after completing successful internship. The team worked on an internal product that manages SQL server farm and I was responsible to write tools to validate the latency, consistency, availability
and manageability of that product. Back then, the product acted as a backend for popular services like Hotmail, Address book and Skype.


<img align="right" width="10%" height="10%" src="/assets/img/Infosys.jpg" /> <b> Software Engineer (2004 - 2007) </b>

I joined Infosys, India straight out of college. It was lot of learning here, not only on technical front but also on etiquettes on working in corporate world. I was part of a MVC architecture based project to build quoting system for an insurance company using J2EE, JSP, Javascript, Servlets and SQL database. Here, I also played a role of an onsite coordinator at client site in US to walk through the requirements, project estimates, coordinating development with offshore team.

## Education history

With strong inclination towards computers from early age, I pursued my education in Computer Science.

<img align="right" width="12%" height="5%" src="/assets/img/rit.png" /> <b> Master's degree (2007 - 2009) </b>

Pursuing a US degree was a unique experience for me under the guidance of [Dr. James Heliotis](https://www.cs.rit.edu/~jeh/). My majors was in Programming Language and its worth mentioning some of the interesting projects I worked on.
* AspectJ - Developed an interface for [AspectJ 1.5](https://www.eclipse.org/aspectj/) using [BCEL](https://commons.apache.org/proper/commons-bcel/) to give the weaving ability inside iteration loops during load-time.

* Code parallelizer tool - Built a Java development tool that depending on user’s choice, automatically executes the "embarrassingly parallel" sequential program in parallel without having user to type in extra threading-related code.

* uXML - [uXML](https://github.com/kunalspathak/uxml) was an experiment to analyze the structure and syntax of popular programming paradigms viz. functional, object-oriented and imperative languages and design a universal language which can represent as much possible, the semantics of programming languages that are written in these paradigms. Due to gamut of parsing tools available to process XML files, the intermediate language is in XML format. Thus the uXML format will be more readable than any other intermediate format.

<img align="right" width="10%" height="5%" src="/assets/img/vit.png" /> <b> Bachelor's degree (2000 - 2004) </b>

Most of my computer science foundation was built at one of the prestigious college "Vishwakarma Institute of Technology" of a prestigious "Pune University" of India. Here I learned Computer Science fundamentals, introduction to various programming languages like Basic, Pascal, C, C++, COBOL, Java and assembly language.

## Patent

* <b>Scenario-based code trimming and code reduction</b>: While working Chakra, we got many behavior difference bugs where a certain portion of a webpage behaved differently in Edge browser. It was challenging to find the root cause of such bugs given 1000s of lines of webpage javascript code being executed. I came up with a tool to automatically remove javascript functions that were not needed to reproduce the wrong behavior thus making it easier for compiler developers to analyze the cause of the bug. Details can be read [here](https://patents.justia.com/patent/9436449).
