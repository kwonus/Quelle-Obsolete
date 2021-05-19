# Quelle HMI Specification

##### version 1.0.1.5i

### I. Background

Most modern search engines, provide a mechanism for searching via a text input box where the user is expected to type search terms. While primitive, this interface was pioneered by major web-search providers and represented an evolution from the far more complex interfaces that came earlier. When you search for multiple terms, however, there seems to be only one basic paradigm: “find every term”. At AV Text Ministries, we believe that the vast world of search is rife for a search-syntax that moves us past only basic search expressions. To this end, we are proposing a Human-Machine-Interface (HMI) that can be invoked within a simple text input box. The syntax fully supports basic Boolean operations such as AND, OR, and NOT. While great care has been taken to support the construction of complex queries, greater care has been taken to maintain a clear and concise syntax.

Quelle, IPA: [kɛl], in French means "What? or Which?". As Quelle HMI is designed to obtain search-results from search-engines, this interrogative nature befits its name. An earlier interpreter, Clarity, served as inspiration for defining Quelle.  You could think of the Quelle HMI as version 2.0 of the Clarity HMI specification.  However, in order to create linguistic consistency in Quelle's Human-to-Machine command language, the resulting syntax varies so significantly from the baseline specification that a new name was the best way forward.  Truly, Quelle HMI is a new specification that incorporates lessons learned after creating, implementing, and revising Clarity HMI for over a decade.

Every attempt has been made to make Quelle consistent with itself. Some constructs are in place to make parsing unambiguous, other constructs are biased toward ease of typing (such as minimizing the need for the shift key). In all, Quelle represents an easy to type and easy to learn HMI.  Moreover, simple search statements look no different than they might appear today in a Google or Bing search box. Still, let's not get ahead of ourselves or even hint about where our simple specification might take us ;-)

### II. Overview

Quelle HMI maintains the assumption that proximity of terms to one another is an important aspect of searching unstructured data. Ascribing importance to the proximity between search terms is sometimes referred to as a *proximal* *search* technique. Proximal searches intentionally constrain the span of words that can be used to constitute a match.

Beyond search, a search API should provide a means of configuration and remembering some of the users more specific search tendencies, and even provide control over how results are rendered. The design of Quelle prefers privacy-first, and is therefore not cloud-first. Consequently, settings in Quelle are stored on your local system by default and operations in the cloud are designed to keep the user anonymous to any domain-specific Quelle-capable search engines. 

Any application can implement the Quelle specification without royalty. We provide this text-based HMI specification and a corresponding reference implementation of a command interpreter in C#. Both the specification and the reference implementation are shared with the broader community with a liberal MIT open source license.

### III. Quelle Syntax

The Quelle specification defines a declarative syntax for specifying search criteria using the *find* verb. Quelle also defines additional verbs to round out its syntax as a simple straightforward means to interact with custom applications where searching text is the fundamental problem at hand. As mentioned earlier, AV Text Ministries provides a reference implementation. This implementation is written in C# and runs on most operating systems (e.g. Windows, Mac, Linux, iOS, Android, etc).  As source code is provided, it can be seamlessly extended by application programmers.

Quelle Syntax comprises fourteen (14) verbs. Each verb corresponds to a basic action:

- find
- set
- get
- clear
- print
- save
- delete
- execute
- show
- help
- status
- review
- invoke
- exit

As Quelle is an open and extensible standard, additional verbs, and verbs for other languages can be defined without altering the overall syntax structure of the Quelle HMI. The remainder of this document describes Version 1.0 of the Quelle-HMI specification.  

In Quelle terminology, a statement is made up of actions. Each action has a single verb. While there are fourteen verbs, there are only six syntax categories:

1. SEARCH
   - find
   - status
2. CONTROL
   - set
   - clear
   - get
4. DISPLAY
   - print
4. SYSTEM
   - help
   - exit
5. HISTORY
   - review
   - invoke
6. LABEL
   - save
   - delete
   - show
   - execute

### IV. Fundamental Quelle Commands

Learning just five verbs is all that is necessary to effectively use Quelle. In the table below, each verb has a minimum and maximum number of parameters.  Each of these verbs are described below and in this section.

| Verb       | Action Type | Syntax Category | Required Parameters     | Required Operators | Optional Operators |
| ---------- | :---------: | :-------------- | ----------------------- | :----------------: | :----------------: |
| *find*     |  implicit   | SEARCH          | **1**: *search spec*    |                    | **" " [ ] \| & !** |
| *set*      |  implicit   | CONTROL         | **2**: *name* = *value* |       **=**        |                    |
| **@print** |  explicit   | DISPLAY         | **0+**: *identifiers*   |                    |    **[ ]  > !**    |
| **@help**  |  explicit   | SYSTEM          | 0 or 1                  |                    |                    |
| **@exit**  |  explicit   | SYSTEM          | 0                       |                    |                    |

**TABLE 4-1** -- **The five fundamental Quelle commands with corresponding syntax summaries**

From a linguistic standpoint, all Quelle commands are issued in the imperative. The subject of the verb is always "you understood". As the user, you are commanding Quelle what to do. Some verbs have direct objects [aka required parameters]. These parameters instruct Quelle <u>what</u> to act upon. The syntax category of the verb dictates the required parameters.

Quelle supports two types of actions:

1. Implicit actions [implicit actions are inferred from the syntax of their parameters]
2. Explicit actions [The verb needs to be explicitly stated in the command and it begins with **@**]

Implicit actions can be combined into compound statements.  However, compound statements are limited to contain ONLY implicit actions. This means that explicit actions cannot be used to construct a compound statement.

Constructing a compound statement with multiple implicit actions, involves delimiting each action with semi-colons. As search is a fundamental concern of Quelle, it is optimized to make compound implicit actions easy to construct with a concise and and intuitive syntax.

Even before we describe Quelle syntax generally, let's examine these concepts using examples:

| Description                             | Example                                  |
| --------------------------------------- | :--------------------------------------- |
| Explicit SYSTEM action                  | @help                                    |
| Explicit DISPLAY action                 | @print [*]                               |
| Implicit single SEARCH action           | this is some text expected to be found   |
| Compound statement: two SEARCH actions  | "this quoted text" ; other unquoted text |
| Compound statement: two CONTROL actions | span=7 ; exact = true                    |
| Compound statement: CONTROL & SEARCH    | span=7; "Moses said"                     |

**TABLE 4-2** -- **Examples of Quelle statement types**

Consider these two examples of Quelle statements (first CONTROL; then SEARCH):

search.domain = bible

"in the beginning"

Notice that both statements above are single actions.  If we had run these statements in the order listed above, the first match for the search would be in the book of Genesis. But as the source domain of our search is a key element of our search, we should have a way to express both of these in a single command. And this is the rationale behind a compound statement. A compound statement has more than one action. To combine the previous two actions into one compound statement, issue this command:

"in the beginning" ; search.domain=bible

### V. Deep Dive into Quelle SEARCH actions

Consider the proximity search where the search target is the bible. Here is an example search using Quelle syntax:

*domain=bible ; beginning created earth*

Quelle syntax can alter the span by also supplying an additional CONTROL action:

*domain=bible span=8 ; beginning created earth*

The statement above has two CONTROL actions and one SEARCH action

Now consider a different search:

*God created earth*

Next, consider a search to find that God created heaven or earth:

*God created earth|heaven*

The order in which the search terms are provided is insignificant. Additionally, the type-case is insignificant. 

Of course, there are times when word order is significant. Accordingly, searching for explicit strings can be accomplished using double-quotes as follows:

*“God created ... Earth”*

These constructs can even be combined. For example:

*”God created ... Heaven|Earth”*

The search criteria above is equivalent to this search:

*“God created ... Heaven” ; “God created ... Earth”*

In all cases, “...” means “followed by”, but the ellipsis allows other words to appear between created and heaven. Likewise, it allows words to appear between created and Earth.

AV Text Ministries imagines that Quelle HMI can be applied broadly in the computing industry and can easily be applied outside of the narrow domain of biblical studies. For example, the Quelle syntax could easily handle statements such as:

​     *domain=Wall Street Journal ; “Biden ... tax increases”*

Of course, translating the commands into actual search results might not be trivial for the application developer. Still, the reference implementation that parses Quelle statements is freely available in the reference implementation.

Quelle is designed to be intuitive. It provides the ability to invoke Boolean logic on how term matching should be performed. As we saw earlier, parenthesis can be used to invoke Boolean multiplication upon the terms that compose a search expression. For instance, there are situations where the exact word within a phrase is not precisely known. For example, when searching the KJV bible, one might not recall which form of the second person pronoun was used in an otherwise familiar passage. Attempting to locate the serpent’s words to Eve in Genesis, one might execute a search such as:

*you|thou|ye shall not surely die*

This statement uses Boolean multiplication and is equivalent to this lengthier statement:

*you shall not surely die ; thou shall not surely die ; ye shall not surely die*

The example above also reveals how multiple search actions can be strung together to form a compound search: logically speaking, each action is OR’ed together; this implies that any of the three matches is acceptable. Using parenthetical terms produces more concise search statements.

**More SEARCH Examples:**

Consider a query for all passages that contain God AND created, but NOT containing earth AND NOT containing heaven:

*domain = bible.old-testament ; span = 15 ; created GOD ; -- Heaven Earth*

*(this could be read as: find in the old testament using a span of 15, the words*

*created AND God, but NOT Heaven AND NOT Earth)*

The simplest form to find ALL of three words:

*in the beginning*

It should be noted that such a statement would find either of these strings in the text:

in the beginning

the beginning of summer in

 

If a specific string should be match, this can be stated explicitly:

"in the beginning"

 

If you are unsure what article should match, you could issue this statement:

"in a|the|that beginning"

Boolean multiplication would match only these strings of text:

in a beginning

in the beginning

in that beginning

or more generally where /det/ marks any word that is marked as a determiner (Not all Quelle drivers handle part-of-speech marking in this manner; but Quelle recognizes this syntax):

"in /det/ beginning"

 

If you are unsure which words might separate a phrase, you could issue this statement:

*"in ... beginning … heaven and earth"*

With this ellipsis in the find statement, it would match this string of text:

in a beginning, God created heaven and earth

 

If you are unsure about word order within a phrase, square brackets can be used:

find "in the beginning … [earth heaven]"

With this ellipsis and the final two bracketed terms, it would also match this string of text:

in a beginning, God created heaven and earth

### VI. Printing Results

Consider that there are two fundamental types of searches:

- Searches that return a limited set of results
- Searches that either return loads of results; or searches where the result count is unknown (and potentially very large)

Due to the latter condition above, SEARCH, by default summarizes results (it does NOT automatically print every result found). The idea is that the user can drill down into the summary and print limited sets of results on demand using subsequent discrete @print commands. *@print* statements operate against the most previously executed SEARCH in the current Quelle session.  Here are two parallel examples:

"Jesus answered"			*this would summarize books that contain this phrase, with chapter references*

@print [*]			             *this would would print every matching verse*

Consider this very general search

"he ... said"

To print all matching synopses of the most recently executed search:

*@print* [*]

Alternatively, I can sample the first three results after the search by executing

*@print* [1,2,3]

Or I can add a header using this variant of print as a dependent clause:

heading = The first three results:

@print [1,2,3]

The remainder of this section further describes the various arguments for DISPLAY phrases.

To print all results:

*@print* [*]

To print only the first result:

*@print* [1]

As we saw earlier, to print only the first three results

*@print* [1,2,3]

and this:

*@print* [1:3]

To print using a single display-coordinate:

*@print* genesis:1:1

NOTE: Display-coordinates are driver-specific and not part of standard Quelle driver definition; The display-coordinate in the example above is compatible with the Quelle-AVX implementation

We can also decorate/annotate each record that we find. Using Quelle-AVX extensions, adding an annotation to each search result can be accomplished by adding this to the print statement:

display.record = %book% %chapter%:%verse% (KJV):\\n%text%  *@print*

A more vanilla decoration might be:

display.record = \<a href="%url%">%abstract%\</a\> *@print* [1,2,3]

Keep in mind, however, the above two examples above are purely notional, your Quelle driver must support such annotation-variables for them to render as expected. Consult the documentation for your Quelle Search provider to determine what record annotation-variables are available.

So to break open the fragment from the *print* example above:

In a separate example, we can label all results using the heading command:

heading = Verses containing 'Godhead' record = %book% %chapter%\\:%verse% (KJV): @print [*]

The syntax above, while biased towards Quelle-AVX search results is standard Quelle-HMI syntax and supported in the standard Quelle driver implementation.

### VII. Program Help

*@help*

This will provide a help message in a Quelle interpreter.

Or for specific topics:

*@help* find

*@help* set

@help print

etc ...

### VIII. Exiting Quelle

Type this to terminate the Quelle interpreter:

*@exit*

### IX. Additional SEARCH & CONTROL actions

| Verb        | Action Type | Syntax Category | Required Parameters     | Required Operators | Optional Operators | Primary Verb |
| ----------- | :---------: | --------------- | ----------------------- | :----------------: | ------------------ | ------------ |
| *find*      |  implicit   | SEARCH          | **1**: *search spec*    |                    | **" " [ ] \| &**   | yes          |
| **@status** |  explicit   | SEARCH          | 0                       |                    |                    |              |
| *set*       |  implicit   | CONTROL         | **2**: *name* = *value* |       **=**        |                    | yes          |
| *clear*     |  implicit   | CONTROL         | **1**: *control_name*   |       **=@**       |                    |              |
| **@get**    |  explicit   | CONTROL         | **0+**: *control_names* |                    |                    |              |

**TABLE 9-1** -- **Listing of all SEARCH & CONTROL actions** *(the two primary verbs find and set are listed here again just for clarity)*

**CONTROL::SETTING directives:**

| **Markdown**          | **HTML**                | **Text**                |
| --------------------- | ----------------------- | ----------------------- |
| *display.format = md* | *display.format = html* | *display.format = text* |

**TABLE 9-2** -- **set** format command can be used to set the default content formatting for printing



| **example**                          | **explanation**                                              |
| ------------------------------------ | ------------------------------------------------------------ |
| *search*.host = https://avbible.net/ | Assign a control setting                                     |
| **@get** *search*.host               | get a control setting                                        |
| *search*.host=@                      | Clear the control setting; restoring the Quelle driver default setting |

**TABLE 9-3** -- **set/clear/get** action operate on configuration settings



**CONTROL::REMOVAL directives:**

When *clear* verbs are used alongside *set* verbs, *clear* verbs are always executed after *set* verbs. 

span=@ ; span = 7 `>> implies >>` span=@

Otherwise, when multiple clauses contain the same setting, the last setting in the list is preserved.  Example:

set format = md  set format = text`>> implies >>` set format = text

The control names are applicable to ***set***, ***clear***, and ***@get*** verbs. The control name has a fully specified name and also a short name. Either form of the control name is permitted in all Quelle statements.

| Fully Specified Name | Short Name | Meaning                      | Values     | Visibility |
| -------------------- | ---------- | ---------------------------- | ---------- | ---------- |
| search.host          | host       | URL of driver                | string     | normal     |
| search.span          | span       | proximity                    | 0 to 1000  | normal     |
| search.domain        | domain     | the domain of the search     | string     | normal     |
| search.exact         | exact      | exact match vs liberal/fuzzy | true/false | normal     |
| display.heading      | heading    | heading of results           | string     | normal     |
| display.record       | record     | annotation of results        | string     | normal     |
| display.format       | format     | format of results            | Table 9-2  | normal     |

**TABLE 9-4** -- **Summary of standard Quelle Control Names**

Table 9-4 lists Control-Names for SEARCH and DISPLAY actions. The *@get* command will list the values associated with these. The *@get* command takes zero or more arguments. Zero arguments lists all control settings.  With one or more arguments, get only lists the values of the controls that are specified.  Examples of the command are below (both the long form and the short form of control names are accepted):

*@get*

*@get* host

*@get* search.host

*@get* search

*@get* search.domain

Control settings can be cleared using implicit wildcards, by using the shared control-prefix:

search=@

display=@

For example, this control statement with an implied wildcard:

search=@

It is exactly equivalent to this compound statement:

search.span=@ ; search.domain=@ ; search.exact=@

Likewise, it is exactly equivalent to this similar compound statement:

span=@ ; domain=@ ; exact=@

**GETTING STATUS**

@status is a unique command as it actually executes a status request against the currently configured Quelle Search Provider [the server represented by search.host].

This command will test the connection with the currently configured Quelle Search Provider:

*@status*

If no search.host has been configured, the default host is http://localhost:1611

### X. Reviewing Statement History and re-invoking statements

| Verb        | Action Type | Syntax Category | Required Arguments | Required Operators |
| ----------- | ----------- | --------------- | ------------------ | :----------------: |
| **@review** | explicit    | HISTORY         | 0 or 1             |                    |
| *invoke*    | implicit    | HISTORY         | 1: historic-id     |      **{ }**       |

##### **TABLE 10-1 -- Reviewing history and re-invoking previous commands**

**REVIEW SEARCH HISTORY**

*@review* gets the user's search activity for the current session.  To show the last ten searches, type:

*@review*

To show the last three searches, type:

*@review* 3

To show the last twenty searches, type:

*@review* 20 

**INVOKE**

The *invoke* command works a little bit like a macro, albeit with different syntax.  After executing a *@review* command, the user might receive a response as follows.

*@review*

1>  span = 7

2>  exact = true

3> eternal power

And the invoke command can re-invoke any command listed.

{3}

would be shorthand to re-invoke the search specified as:

eternal power

or we could re-invoke all three commands in a single statement as:

{1} ; {2} ; {3}

which would be interpreted as:

span = 7 ; exact = true ; eternal power

### XI. Labelling Statements for subsequent execution

| Verb        | Action Type | Syntax Category | Required Arguments     | Required Operators | Optional Operators |
| ----------- | ----------- | --------------- | ---------------------- | :----------------: | :----------------: |
| **@save**   | dependent   | LABEL           | **1**: *macro_label*   |      **{ }**       |                    |
| **@delete** | independent | LABEL           | **1+**: *macro_label*s |      **{ }**       |                    |
| **@show**   | independent | LABEL           | **0+**: *macro_labels* |                    |      **{ }**       |
| *execute*   | implicit    | LABEL           | 1: *macro_label*       |      **{ }**       |                    |

**TABLE 11-1** -- **Executing labelled statements and related commands**

In this section, we will examine how user-defined macros are used in Quelle.  A macro in Quelle is a way for the user to label a statement for subsequent use.  By applying a label to a statement, a shorthand mechanism is created for subsequent execution. This gives rise to two new definitions:

1. Labelling a statement (or defining a macro)

2. Utilization of a labelled statement (executing a macro)


Let’s say we want to name our previously identified SEARCH directive with a label; We’ll call it “genesis”. To accomplish this, we would issue these two commands:

search.domain=bible ; “in the beginning”

@save {genesis} 

It’s that simple, now instead of typing the entire statement, we can use the label to execute our newly saved statement. Here is how we would execute the macro:

{genesis}

Labelled statements also support compounding using the semi-colon ( ; ), as follows; we will label it also:

{genesis} ; {my label can contain spaces}

@save sample

Later I can issue this command:

{sample}

Which is obviously equivalent to executing these labeled statements:

{genesis} ; {my label can contain spaces}

To illustrate this further, here are four more examples of labeled statement definitions:

search.exact=1 @save {C1}

search.span=8  @save {C2}

Godhead  @save {F1}

eternal  @save {F2}

We can execute these as a compound statement by issuing this command:

{C1} ; {C2} ; {F1} ; {F2}

Similarly, we could define another label from these, by issuing this command:

{C1} ; {C2} ; {F1} ; {F2}  @save {sample2}

This expands to:

search.exact = 1  search.span = 8 ; Godhead ; eternal

There are two restrictions on macro definitions:

1. Macro definition must represent a valid Quelle statements:
   - The syntax is verified prior to saving the statement label.
2. The statement cannot contain explicit actions:
   - Only implicit actions are permitted in a labelled statement.

Finally, there are two additional ways that a labelled statement or can be referenced. In last macro definition above where we created {sample2}, the user could see the expansion in Quelle by issuing this command:

@list {sample2}

If the user wanted to remove this definition, the @delete action is used.  Here is an example:

@delete {sample2}

NOTE: Labels must begin with a letter [A-Z] or [a-z], but they may contain numbers, spaces, hyphens, periods, commas, underscores, and single-quotes (no other punctuation or special characters are supported).

### XII. Wrap-up

In all cases, any number of spaces can be used between operators and terms. 

Also noteworthy: The reference Quelle implementation automatically adjusts the span of your to be inclusive of the number of search terms for the most broad search clause. So if you were to express:

**find span=1 ; in the beginning God|Lord|Jesus|Christ|Messiah**

The minimum span has to be four(4). So the Quelle parser will adjust the search criteria as if the following command had been issued:

**find span=4 ; in the beginning God|Lord|Jesus|Christ|Messiah**



### Appendix A. Glossary of Quelle Terminology

**Syntax Categories:** Each syntax category defines rules by which verbs can be expressed in the statement. 

**Actions:** Actions are complete verb-clauses issued in the imperative [you-understood].  Many actions have one or more parameters.  But just like English, a verb phrase can be a single word with no explicit subject and no explicit object.  Consider this English sentence:

Go!

The subject of this sentence is "you understood".  Similarly, all Quelle verbs are issued without an explicit subject. The object of the verb in the one word sentence above is also unstated.  Quelle operates in an analogous manner.  Consider this English sentence:

Go Home!

Like the earlier example, the subject is "you understood".  The object this time is defined, and insists that "you" should go home.  Some verbs always have objects, others sometimes do, and still others never do. Quelle follows this same pattern and some Quelle verbs require direct-objects; and some do not.  See Table 4-1 where the column identified as "Parameter Count" identifies objects of the verb. 

**Statement**: A statement is composed of one or more *actions*. If there is more than one SEARCH actions issued by the statement, then search action is logically OR’ed together.

**Unquoted SEARCH segments:** an unquoted search segment contains one or more search words. If there is more than one word in the segment, then each word is logically AND’ed together. Like all other types of clauses, the end of the clause terminates with any of this punctuation:

- ; [semi-colon]
- @ [at-symbol: the beginning of an explicit verb]
- the end-of-the-line [newline]

**Quoted SEARCH segments:** a quoted clause contains a single string of terms to search. An explicit match on the string is required. However, an ellipsis ( … ) can be used to indicate that wildcards may appear within the quoted string.

**NOTES:**

- It is called *quoted,* as the entire clause is sandwiched on both sides by double-quotes ( " )
- The absence of double-quotes means that the statement is unquoted

**Parenthetical Terms:** When searching, there are situations when the exact word that appears in a text is not precisely known.

**Bracketed Terms:** When searching, there are part the order of some terms within a quoted are unknown. Square brackets can be used to identify such terms. For example, consider this SEARCH statement:

*“[God created] heaven and earth” ; source=bible*

The above statement is equivalent to

*“God created heaven and earth” ; “created God heaven and earth” ; source=bible*

**and:** In Boolean logic, **and** means that all terms must be found. With Quelle-HMI, *and* is represented by terms that appear within an unquoted clause. 

**or:** In Boolean logic, **or** means that any term constitutes a match. With Quelle=HMI, *or* is represented by the semi-colon ( **;** ) between SEARCH clauses. 

**not:** In Boolean logic, **not** means that the term must not be found. With Quelle, *not* is represented by a minus,minus ( **--** ) and applies to an entire clause (it cannot be applied to individual words unless the search clause has only a single term). In other words, a ​--​ means subtract results; it cancels-out matches against all matches of other clauses. Most clauses are additive as each additional clause increases search results. Contrariwise, a **not** clause is subtractive as it decreases search results.

The -- means that the clause will be subtracted from the search results while its absence means that the clause will be added to the search results. When statement only contains a single search clause, it is always positive. A single negative clause following the find imperative, while it might be grammatically valid syntax, will never match anything. Therefore, while permitted in theory, it would have no real-world meaning. Consequently, some implementations of Quelle-HMI may disallow such a construct.