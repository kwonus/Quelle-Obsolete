# Quelle-HMI

#### version 1.0.1.5i

With the C# code contained herein, and companion github/kwonus projects, the
simplicity of creating a Quelle driver using this Quelle HMI library is exemplified.
This code can be used as a template to create your own driver, or it can be subclassed
to extend behavior.
<br/></br>
The value proposition of Quelle is that parsing is tedious. And starting your search CLI with a concise syntax
with an easy to digest parsing library could easily save your team a person-year in design-time and coding.
Quelle source code is licensed with a liberal MIT license.
<br/></br>
The design incentive for Quelle HMI and the standard driver/interpreter is to support the broader Digital-AV
effort: That project [Digital-AV] provides a command-line interface for searching and publishing the KJV bible.
Every attempt has been made to keep the Quelle syntax agnostic about the search domain, yet the Quelle
user documentation itself is biased in its syntax examples. Still, Quelle syntax has the potential for ubiquity.
<br/></br>
More complete user-oriented documentation can be found here:</br>

https://github.com/kwonus/Quelle/blob/master/Quelle.md

</br>
Developer notes about the implementation can be found at:</br>

https://github.com/kwonus/Quelle/blob/master/Quelle-Developer-Notes.md

<br/></br>
Tandem C++ projects, AVXLib and AVXLib-dotnet, are also on github that implement a working Quelle driver for the text of the KJV bible.  Interop between dotnet 5 and native C++ libraries on Linux and MacOS hosts is still being investigated.  A brute-force integration using Microsoft C++/CLR for dotnet framework 4.8 is the only current working reference implementation (and, being dotnet 4.8, it is WIndows only).  Still, the HMI library itself, being dotnet 5, works on macOS, Linux, and Windows.  