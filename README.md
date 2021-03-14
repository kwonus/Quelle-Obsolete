# Quelle-HMI

#### version 1.0.1.3E

With the C# code contained herein, and companion github/kwonus projects, the
simplicity of creating a Quelle driver using this Quelle HMI library is exemplified.
This code can be used as a template to create your own driver, or it can be subclassed
to extend behavior.
<br/></br>
A tandem Rust project will land on github soon.  It will be a REST server built on Gotham and Askana.
The message transport will be MessagePack as it has great support on both Rust and DotNet 5.  The REST server combined with Quelle interpreter will provide a complete working  reference implementation.
<br/></br>
The value proposition here is that parsing is tedious. And starting your search CLI with a concise syntax
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