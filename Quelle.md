# Quelle HMI v1.0 Specification

### I. Background

Most modern search engines, provide a mechanism for searching via a text input box where the user is expected to type search terms. While primitive, this interface was pioneered by major web-search providers and represented an evolution from the far more complex interfaces that came earlier. When you search for multiple terms, however, there seems to be only one basic paradigm: “find every term”. At AV Text Ministries, we believe that the vast world of search is rife for a search-syntax that moves us past only basic search expressions. To this end, we are proposing a Human-Machine-Interface (HMI) that can be invoked within a simple text input box. The syntax fully supports basic Boolean operations such as AND, OR, and NOT. While great care has been taken to support the construction of complex queries, greater care has been taken to maintain a clear and concise syntax.

Quelle, IPA: [kɛl], in French means "What? or Which?". As Quelle HMI is designed to obtain search-results from search-engines, this interrogative nature befits its name. An earlier interpreter, Clarity, served as inspiration for defining Quelle.  You could think of the Quelle HMI as version 3.0 of the Clarity HMI specification.  However, in order to create linguistic consistency in Quelle's Human-to-Machine command language, the resulting syntax varied so greatly from the baseline specification, that a name-change was in order.  Truly, Quelle HMI is a new specification that incorporates lessons learned after creating, implementing, and revising Clarity HMI for over a decade.

Every attempt has been made to make Quelle consistent with itself. Some constructs are in place to make parsing unambiguous, other constructs are biased toward ease of typing (such as minimizing the need for the shift key). In all, Quelle represents an easy to type and easy to learn HMI.  Moreover, simple search statements look no different than they appear today in a Google or Bing search box. Still, let's not get ahead of ourselves or even hint about where our simple specification might take us ;-)

### II. Overview

Quelle HMI maintains the assumption that proximity of terms to one another is an important aspect of searching unstructured data. Ascribing importance to the proximity between search terms is sometimes referred to as a *proximal* *search* technique. Proximal searches intentionally constrain the span of words that can be used to constitute a match.

Beyond search, a search API should provide a means of configuration and remembering some of the users more specific search tendencies, and even provide control over how results are rendered. The design of Quelle prefers privacy-first, and is therefore not cloud-first. Consequently, settings in Quelle are stored on your local system by default and operations in the cloud are designed to keep the user anonymous to any domain-specific Quelle-capable search engines. 

Any application can implement the Quelle specification without royalty. We provide this text-based HMI specification and a corresponding reference implementation of a command interpreter in C#. Both the specification and the reference implementation are shared with the broader community with a liberal MIT open source license.

### III. Quelle Syntax

The Quelle specification defines a declarative syntax for specifying search criteria using the *find* verb. Quelle also defines additional verbs to round out its syntax as a simple straightforward means to interact with custom applications where searching text is the fundamental problem at hand. As mentioned earlier, AV Text Ministries provides a reference implementation. This implementation is written in C# and runs on most operating systems (e.g. Windows, Mac, Linux, iOS, Android, etc).  As source code is provided, it can be seamlessly extended by application programmers.

Quelle Syntax comprises a standard set of twelve (12) verbs. Each verb corresponds to a basic operation:

- find
- print
- set
- get
- clear
- define
- expand
- remove
- help
- backup
- restore
- exit

The verbs listed above are for the English flavor of Quelle. As Quelle is an open and extensible standard, verbs for other languages can be defined without altering the overall syntax structure of the HMI. The remainder of this document describes Version 1.0 of the Quelle-HMI specification.  

In Quelle terminology, a statement is made up of clauses. Each clause has a single verb. While there are twelve verbs, there are only five distinct types of clauses:

1. SEARCH clauses
   - find
2. DISPLAY clauses
   - print
3. CONTROL clauses
   - get
   - set
   - clear
4. MACRO clauses
   - define
   - expand
   - remove
5. ENVIRONMENT clauses
   - help
   - backup
   - restore
   - exit

Each of the twelve verbs has a minimum and maximum number of parameters. Note that MACRO clauses also require special operators.  See the Table 3-1 below:

| Prefix | Verb        | Verb is Inferred | Clause Type | Clause Restriction | Required Arguments | Required Operators |
| :----: | ----------- | :--------------: | ----------- | :----------------: | ------------------ | :----------------: |
|        | **find**    |      **x**       | SEARCH      |                    | 1 or more          |                    |
|        | **set**     |      **x**       | CONTROL     |                    | 2                  |         =          |
|   #    | **get**     |                  | CONTROL     |                    | 1                  |                    |
|   #    | **clear**   |                  | CONTROL     |                    | 1                  |                    |
|   #    | **expand**  |                  | MACRO       |                    | 1                  |        { }         |
|   #    | **remove**  |                  | MACRO       |                    | 1                  |        { }         |
|   \|   | **define**  |                  | MACRO       |   **dependent**    | 1                  |        { }         |
|   \|   | **print**   |                  | DISPLAY     |   **dependent**    | 0 or more          |                    |
|   #    | **print**   |                  | DISPLAY     |     **simple**     | 0 or more          |                    |
|   #    | **help**    |                  | ENVIRONMENT |     **simple**     | 0 or 1             |                    |
|   #    | **backup**  |                  | ENVIRONMENT |     **simple**     | 1 to 3             |                    |
|   #    | **restore** |                  | ENVIRONMENT |     **simple**     | 1 or 3             |                    |
|   #    | **exit**    |                  | ENVIRONMENT |     **simple**     | 0                  |                    |

**TABLE 3-1 -- Detailed verb descriptions with syntax implications**

Phrase-restricted verbs are unique in that they cannot be used to construct compound statements.  Dependent phrases can be added as the final clause after an ordinary statement, but cannot be combined in any other way. Simple statements cannot be combined whatsoever.

Quelle clauses always have a verb, even if the verb might be "silent". From a linguistic standpoint, all Quelle clauses are verbal phrases issued in the imperative. The syntax for each clause is dependent upon the verb for the clause, and each clause type has its own parsing rules and special characters for the clause. The type of clause is controlled by the verb.

Quelle supports three types of statements:

1. Simple statements
2. Ordinary statements
3. Ordinary statements with a dependent clause

A simple statement always has only a single verb. Some verbs are constrained to be constructed only as simple statements as identified in the table above.  Ordinary statements can have any number of verb phrases; an ordinary statement with more than a single verb is also referred to as a compound statement.  In Quelle, these verb phrases are called "clauses".  So another way to describe a simple statement is that it must contain only one clause.  A special type of simple statement in Quelle is a dependent clause.  A dependent clause is still restricted to a single verb, but a dependent clause can be added to any ordinary statement.

Even before we describe Quelle syntax generally, let's look at these concepts using examples:

|                                            | Example                                       |
| ------------------------------------------ | --------------------------------------------- |
| Simple statement                           | "look for this text"                          |
| Dependent clause                           | \| print                                      |
| Ordinary statement                         | "look for this text" // "other text"          |
| Ordinary statement with a dependent clause | "look for this text" // "other text" \| print |

**TABLE 3-2 -- Examples of Quelle statement types**

In the last example in Table 3-2, the final verb phrase, namely *format*, is the dependent clause. Dependent clauses are identified as a clause that begins after the pipe symbol ( | ). There are two functions associated with dependent clauses: printing search results and defining macros.  Macro definitions are a mechanism of making Quelle extensible by the user.  Macros are defined in the next section and are also referred to as "statement labels". Printing is described in section IX.

Consider these two examples of Quelle statement (first CONTROL; then SEARCH):

search.domain = bible

#find "in the beginning"

Notice that each of the above, while ordinary statements, are .

If we had run these statements in the order listed above, the first match for the search would be in the book of Genesis. But as the source domain of our search is a key element of our search, we should have a way to express both of these in a single command. And this is the rationale behind a compound statement. A compound statement has more than one clause. To combine the previous two clauses into one compound statement, issue this command:

"in the beginning" // search.domain=bible

### IV. Statement Labels

In this section, we will examine how user-defined macros are used in Quelle.  A macro in Quelle is a way for the user to label a statement for subsequent use.  By applying a label to a statement, a shorthand mechanism is created for subsequent execution. This gives rise to two new definitions:

1. Labelling a statement (or defining a macro)

2. Utilization of a labelled statement (executing a macro)


Let’s say we want to name our previously identified SEARCH directive with a label; We’ll call it “genesis”. To accomplish this, we would issue this command:

“in the beginning” // search.domain=bible | define {genesis} 

It’s that simple, now instead of typing the entire statement, we can use the label to execute our newly saved statement. Here is how we would execute the macro:

{genesis}

Labelled statements also support compounding, as follows:

{genesis} // {my label can contain spaces}

As the previous command is valid syntax for a statement, it even follows that we can define this macro:

{genesis} // {my label can contain spaces} | define sample

Later I can issue this command:

{sample}

Which is obviously equivalent to executing these labeled statements:

{genesis} // {my label can contain spaces}

Labels can be defined in terms of an ordinary statement or using one or more labels inside of braces. And the two constructs can be mixed

{derived}: {original} // "find some more text"

To illustrate this further, here are four more examples of labeled statement definitions:

search.exact = 1| define {C1}

search.span  = 8 | define {C2}

Godhead | define {F1}

eternal | define {F2}

We can execute these as a compound statement by issuing this command:

 {C1} // {C2} // {F1} // {F2}

Similarly, we could define another label from these, by issuing this command:

{C1} // {C2} // {F1} // {F2} | {sample2}

if we expand this macro ...

expand {sample2}

The expansion would be:

search.exact = 1 //  search.span  = 8 // Godhead // eternal

### V. More about Segmentation of Quelle Statements

If an execution ONLY contains CONTROL verbs, then the key-value pairs are saved. Otherwise, they only affect the current statement.

| **Operative Clause** | Secondary Clauses       | Dependent Clauses | Example                     |
| -------------------- | ----------------------- | ----------------- | --------------------------- |
| SEARCH               | any ordinary verb       | allowed           | godhead // span=8           |
| CONTROL              | any CONTROL verb        | allowed           | domain = bible // span=7    |
| MACRO                | any ordinary MACRO verb | not allowed       | godhead \| define {trinity} |
| ENVIRONMENT          | not allowed             | not allowed       | #backup now                 |

**TABLE 5-1** -- **Primary**, **Secondary**, and **Dependent** clauses

CONTROL clauses that are coupled with SEARCH statements are not saved and only effect the current statement.

### VI. Quelle SEARCH clauses

Consider the proximity search where the search target is the bible. Here is an example search using Quelle syntax:

*domain=bible // beginning created earth*

Quelle syntax can alter the span by supplying a CONTROL clause:

*domain=bible // span=8 // beginning created earth*

The statement above has two CONTROL clauses and one SEARCH clause 

Now consider a different search:

*God created earth*

 Next, consider a search to find that God created heaven or earth:

*God created (earth heaven)*

The order in which the search terms are provided is insignificant. Additionally, the type-case is insignificant. 

Of course, there are times when word order is significant. Accordingly, searching for explicit strings can be accomplished using double-quotes as follows:

*“God created ... Earth”*

These constructs can even be combined. For example:

*”God created ... (Heaven Earth)”*

The search criteria above is equivalent to this search:

*“God created ... Heaven” // “God created ... Earth”*

In all cases, “...” means “followed by”, but the ellipsis allows other words to appear between created and heaven. Likewise, it allows words to appear between created and Earth.

AV Text Ministries imagines that Quelle HMI can be applied broadly in the computing industry and can easily be applied outside of the narrow domain of biblical studies. For example, the Quelle syntax could easily handle statements such as:

​     *domain=Wall Street Journal // “Biden ... tax increases”*

Of course, translating the commands into actual search results might not be trivial for the application developer. Still, the reference implementation that parses Quelle statements is freely available in the reference implementation.

Quelle is designed to be intuitive. It provides the ability to invoke Boolean logic on how term matching should be performed. As we saw earlier, parenthesis can be used to invoke Boolean multiplication upon the terms that compose a search expression. For instance, there are situations where the exact word within a phrase is not precisely known. For example, when searching the KJV bible, one might not recall which form of the second person pronoun was used in an otherwise familiar passage. Attempting to locate the serpent’s words to Eve in Genesis, one might execute a search such as:

*(you thou ye) shall not surely die*

This statement uses Boolean multiplication and is equivalent to this lengthier statement:

*you shall not surely die // thou shall not surely die // ye shall not surely die*

The example above also reveals how multiple search clauses can be strung together to form a compound search: logically speaking, each clause is OR’ed together; this implies that any of the three matches is acceptable. using parenthetical terms produces more concise search statements.

### VII. Quelle SEARCH Definitions

While some of these concepts have already been introduced, the following section can be used as a glossary for the terminology used in the Quelle HMI specification.

**Directives** are composed by verbs and are used to construct statements for the Quelle Command Interpreter. Each clause type has specialized syntax tailored to the imperative verb used in the statement. 

**Clauses:** Clauses are equivalent to an imperative [you-understood] verb phrase.  Most clauses have one or more arguments.  But just like English, a verb phrase can be a single word with no explicit subject and no explicit object.  Consider this English sentence:

Go!

The subject of this sentence is "you understood".  Similarly, all Quelle verbs are issued without an explicit subject. The object of the verb in the one word sentence above is also unstated.  Quelle operates in an analogous manner.  Consider this English sentence:

Go Home!

Like the earlier example, the subject is "you understood".  The object this time is defined and tells "you" where to go.  Some verbs always have objects, others sometimes do, and still others never do. Quelle follows this same pattern and each verb is defined to accept arguments or not.  See Table 3-1 where the column identified as "Argument Count" is identifying objects of the verb. 

**SEARCH statement**: Each statement contains one or more *search clauses*. If there is more than one SEARCH clause, each each clause is logically OR’ed with all other clauses.

**SEARCH clause**: Each clause contains one or more *search terms*. A SEARCH clause is either unquoted statement or quoted.

**Unquoted SEARCH clause:** an unquoted clause contains one or more search words. If there is more than one word, then each word is logically AND’ed with all other words within the clause. Like all other types of clauses, the end of the clause terminates with any of this punctuation:

- // [double-slash]
- /- [slash-minus]
- the end-of-the-line [newline]

**NOTE:**

The absence of double-quotes means that the statement is unquoted.

**Quoted SEARCH clause:** a quoted clause contains a single string of terms to search. An explicit match on the string is required. However, an ellipsis ( … ) can be used to indicate that wildcards may appear within the quoted string.

**NOTES:**

It is called *quoted,* as the entire clause is sandwiched on both sides by double-quotes ( " ).

 

**Parenthetical Terms:** When searching, there are situations when the exact word that appears in a text is not precisely known.

**Bracketed Terms:** When searching, there are part the order of some terms within a quoted are unknown. Square brackets can be used to identify such terms. For example, consider this SEARCH statement:

*“[God created] heaven and earth” // source=bible*

The above statement is equivalent to

*“God created heaven and earth” // “created God heaven and earth” // source=bible*

**and:** In Boolean logic, **and** means that all terms must be found. With Quelle-HMI, *and* is represented by terms that appear within an unquoted clause. 

**or:** In Boolean logic, **or** means that any term constitutes a match. With Quelle=HMI, *or* is represented by the double-slash ( **//** ) between SEARCH clauses. 

**not:** In Boolean logic, **not** means that the term must not be found. With Quelle, *not* is represented by a slash+minus ( **/-** ) and applies to an entire clause (it cannot be applied to individual words unless the search clause has only a single term). In other words, a ​/-​ means subtract results; it cancels-out matches against all matches of other clauses. Most clauses are additive as each additional clause increases search results. Contrariwise, a **not** clause is subtractive as it decreases search results.

**NOTE:**

The /- means that the clause will be subtracted from the search results while its absence means that the clause will be added to the search results. When statement only contains a single search clause, it is always positive. A single negative clause following the find imperative, while it might be grammatically valid syntax, will never match anything. Therefore, while permitted in theory, it would have no real-world meaning. Consequently, some implementations of Quelle-HMI may disallow such a construct.

**More Examples:**

Consider a query for all passages that contain God AND created, but NOT containing earth AND NOT containing heaven:

*domain = bible.old-testament // span = 15 // created GOD /- Heaven Earth*

*(this could be read as: find in the old testament using a span of 15, the words*

*created AND God, but NOT Heaven AND NOT Earth)*

The simplest form to find ALL of three words (in the beginning):

*in the beginning*

It should be noted that such a statement would find either of these strings in the text:

in the beginning

the beginning of summer in

 

If a specific string should be match, this can be stated explicitly:

"in the beginning"

 

If you are unsure what article should match, you could issue this statement:

"in (a the that) beginning"

Boolean multiplication would match only these strings of text:

in a beginning

in the beginning

in that beginning

 

If you are unsure which words might separate a phrase, you could issue this statement:

*"in ... beginning … heaven and earth"*

With this ellipsis in the find statement, it would match this string of text:

in a beginning, God created heaven and earth

 

If you are unsure about word order within a phrase, square brackets can be used:

find "in the beginning … [earth heaven]"

With this ellipsis and the final two bracketed terms, it would also match this string of text:

in a beginning, God created heaven and earth



### VIII. More about Segmentation of Quelle Statements

The "*print*" verb has very limited grammar. And it can only be used in a dependent clause of SEARCH.

 

**CONTROL::SETTING directives:**

| **Markdown**                | **HTML**                      | **Text**                      |
| --------------------------- | ----------------------------- | ----------------------------- |
| *display.content-type = md* | *display.content-type = html* | *display.content-type = text* |

**TABLE 8-1** -- **set** content-type command can be used to set the default content formatting for printing



| **example**                              | **explanation**                              |
| ---------------------------------------- | -------------------------------------------- |
| *quelle*.host = https://avbible.net/     | Setting a control variable for session       |
| #get *quelle*.host= https://avbible.net/ | Getting a control variable for system        |
| #clear *quelle*.host                     | clear a control variable (system or session) |

**TABLE 8-2** -- **get**/**set** and **#get/#set** command can be used to retrieve Quelle configuration settings



**CONTROL::REMOVAL directives:**

Control settings can be cleared using wildcards:

**#clear search.*** 

**#clear search.span.***

**#clear display.format.***

When *clear* verbs are used alongside *set* verbs, clear verbs are always executed after *set* verbs. 

#clear span // span = 7 `>> implies >>` #clear span

Otherwise, when multiple clauses contain the same setting, the last setting in the list is preserved.  Example:

md`>> implies >>` set format = text

| Control Name    | Short Name | Meaning                      | Values     | Visibility |
| --------------- | ---------- | ---------------------------- | ---------- | ---------- |
| search.span     | span       | proximity                    | 0 to 1000  | normal     |
| search.domain   | domain     | the domain of the search     | string     | normal     |
| search.exact    | exact      | exact match vs liberal/fuzzy | true/false | normal     |
| display.heading | heading    | heading of results           | string     | normal     |
| display.record  | record     | annotation of results        | string     | normal     |
| display.format  | format     | display format of results    | Table 8-1  | normal     |
| quelle.host     | host       | URL of driver                | string     | normal     |
| quelle.debug    | debug      | on or off                    | true/false | *hidden*   |
| quelle.data     | data       | quelle data format           | *reserved* | *hidden*   |

**TABLE 8-3 -- List of Controls** (The control parameters are applicable to ***set***, ***get*** and ***clear*** verbs)



| Representation | Abbreviated Name |
| -------------- | ---------------- |
| display.*      | display          |
| search.*       | search           |
| quelle.*       | quelle           |

**TABLE 8-4 -- Wildcard usage on Controls** (wildcard usage only applies to ***get*** and ***clear*** verbs)



### IX. Printing Results

The DISPLAY directive has only a single verb, but it is manifest both as a simple statement (#print) and also as a dependent clause ( | print ).

The arguments to both forms of the verb are identical, but the usage syntax varies.

Consider that there are two distinct methods because there are two fundamental types of searches:

- Searches that return a limited set of results
- Searches that either return loads of results of searches where the result count is unknown (and potentially very large)

Due to the latter the latter condition above, SEARCH, by default summarizes results (it does NOT automatically print every result found). The idea is that the user can drill down into the summary and print limited sets of results, using the #print command to selectively print portions of the most previously executed SEARCH.  The dependent clause can be used to circumvent the summary and immediately every record that is returned from the search. Here are two parallel examples:

"Jesus answered"			*this would summarize books that contain this phrase, with chapter references*

"Jesus answered" | format *			*this would would print every matching verse*

Consider this very general search

"he ... said"

I can sample the first three results after the search by executing

#find and implicit-search provide the same set of results.  However, the #find form implies a trailing |format statement and obviates the need for subsequent usage of #print.

#print and format verbs have additional options that can be used to further control how results are formatted.  Consult the remainder of this section for additional details.

To print all matching synopses of the most recently executed search:

*#print* [1,2,3]

Or I can add a header using this variant of print as a dependent clause:

heading = The first three results | print [1,2,3]

Or I can combine all three into a single statement:

"he ... said" // heading = The first three results | print [1,2,3]

While similar, there is flexibility made available to the user to control SEARCH vs DISPLAY.

The remainder of this section further describes the various arguments for DISPLAY phrases.

To print all results:

*print* [*]

To print only the first result:

*print* [1]

As we saw earlier, to print only the first three results

*print* [1,2,3]

Alternatively, this also works:

*print* [1] [2] [3]

and this:

*print* [1:3]

To print using a single display-coordinate:

*print* genesis:1:1

NOTE: Display-coordinates are driver-specific and not part of standard Quelle driver definition; The display-coordinate in the example above is compatible with the Quelle-AVX implementation

We can also decorate/annotate each record that we find. Using Quelle-AVX extensions, adding an annotation to each search result can be accomplished by adding this to the print statement:

display.record = %book% %chapter%\\:%verse% \\(KJV\\\)\\:\\n%text% | print

A more vanilla decoration might be:

*print* [1,2,3] + display.record= \<a href=\\"%url%\\"\>%abstract%\</a\>

Keep in mind, however, the above two examples above are purely notional, your Quelle driver must support such annotation variables for them to render as expected. Consult the documentation for your Quelle cloud-capable driver to determine what record annotation variables are available in your driver.

So to break open the fragment from the *print* example above:

In a separate example, we can label all results using the heading command:

*print* display.heading = Verses containing 'Godhead' + %heading% %label% *

The syntax above, while biased towards Quelle-AVX search results is standard Quelle-HMI syntax and supported in the standard Quelle driver implementation.

Final notes about *print*:

%heading% and %record% are always implied if not cleared)



### X. Wrap-up

In all cases, any number of spaces can be used between operators and terms. 

Also noteworthy: The reference Quelle implementation automatically adjusts the span of your to be inclusive of the number of search terms for the most broad search clause. So if you were to express:

**find span=1 + in the beginning (God Lord Jesus Christ Messiah)**

The minimum span has to be four(4). So the Quelle parser will adjust the search criteria as if the following command had been issued:

**find span=4 + in the beginning (God Lord Jesus Christ Messiah)**

**SPECIAL CHARACTERS FOR STATEMENTS, INCLUDING STATEMENT SEPERATORS:**

| TYPE                   | Special characters |
| ---------------------- | ------------------ |
| ADDITIVE SEPERATORS    | //                 |
| SUBTRACTIVE SEPERATORS | /-                 |



**PROGRAM HELP**

There is one final variant of the print statement:

*print* help

This will provide a help message in a Quelle interpreter.

------

[[1\]](#_ftnref1) The *clear* verbs are supported as secondary clauses, but not the *remove* verbs (The removal of labelled statements [aka macros] cannot be combined with any other clauses).  The *remove* verb is always limited to a simple statement construction (simple statements contain only a single clause). Additionally, a statement with the *remove* verb cannot be used to define a newly labelled statement.

[[2\]](#_ftnref2) The *get* verbs are supported as secondary clauses, but not the *expand* verbs (The expansion of labelled statements [aka macros] cannot be combined with any other clauses).  The *expand* verb is always limited to a simple statement construction (simple statements contain only a single clause). Additionally, a statement with the *expand* verb cannot be used to define a newly labelled statement.