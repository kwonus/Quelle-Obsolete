# Clarity Syntax

Most modern search engines, provide a mechanism for searching via a text input box where the user is expected to type search terms. While primitive, this interface was pioneered by major web-search providers and represented an evolution from the far more complex interfaces that came earlier. When you search for multiple terms, however, there seems to be only one basic paradigm: “find every term”. At AV Text Ministries, we believe that the vast world of search is rife for a search-syntax that moves us past only basic search expressions. To this end, we are proposing a Human-Machine-Interface (HMI) that can be invoked within a simple text input box. The syntax fully supports basic Boolean operations such as AND, OR, and NOT. While great care has been taken to support the construction of complex queries, greater care has been taken to maintain a clear and concise syntax. As Clarity was our primary concern, it became the name of our specification. In the spirit of open source licensing, any application can implement the Clarity-HMI specification without royalty. We provide this text-based HMI specification and a corresponding reference implementation of a command interpreter. Both the specification and the reference implementation are shared with the broader community with a liberal MIT open source license.

The Clarity-HMI maintains the assumption that proximity of terms to one another is an important aspect of searching unstructured data. Ascribing importance to the proximity between search terms is sometimes referred to as a *proximal* *search* technique. Proximal searches intentionally constrain the number of words that can be used to constitute a match. The Clarity HMI specification defines that range between search terms as the *span*.

The Clarity specification defines a declarative syntax for specifying search criteria using the *find* verb. Clarity also defines additional verbs to round out its syntax as a simple straightforward means to interact with custom applications where searching text is the fundamental problem at hand. As mentioned earlier, AV Text Ministries provides a reference implementation. This implementation is written in C# and runs on most operating systems (e.g. Windows, Mac, Linux, iOS, Android, etc).  As source code is provided, it can be seamlessly extended by application programmers.

***Clarity syntax:\***

Clarity Syntax comprises a standard set of seven (7) verbs. Each verb corresponds to a basic operation:

- find
- print
- set
- get
- clear
- expand
- remove

The verbs listed above are for the English flavor of Clarity. As Clarity is an open and extensible standard, verbs for other languages can be defined without altering the overall syntax structure of the HMI. The remainder of this document describes Version 1.5 of the Clarity-HMI specification.

In Clarity terminology, each verb is considered to be a directive. While there are seven distinct verbs, there are only five types of directives:

1. SEARCH Directives
   - find
2. DISPLAY Directives
   - print
3. CONTROL Directives
   - set
   - #set
   - @set
4. STATUS Directives
   - get
   - #get
   - @get
   - expand      [corresponds only to the expansion of labelled statements (aka macros)]
   - #expand   [corresponds only to the expansion of labelled statements (aka macros)]
   - @expand  [corresponds only to the expansion of labelled statements (aka macros)]
5. REMOVAL Directives
   - clear      [corresponds only to clearing values established via CONTROL directives]
   - #clear   [corresponds only to clearing values established via CONTROL directives]
   - @clear  [corresponds only to clearing values established via CONTROL directives]
   - remove    [corresponds only to the removal of labelled statements (aka macros)]
   - #remove   [corresponds only to the removal of labelled statements (aka macros)]
   - @remove  [corresponds only to the removal of labelled statements (aka macros)]

Most directives operate at the session level, but CONTROL, STATUS, and REMOVAL directives can be qualified to operate globally for the user across the current and all future sessions.

The syntax of a Clarity statement always begins with a verb. From a linguistic standpoint, all Clarity statements begin with the verb and are they are issued in the imperative. After the verb, a Clarity statement can be broken into one or more segments. The syntax for each segment is dependent upon the type of directive for the segment. And the type of directive is controlled by the verb. We will refer to the directive of which a verb is a member as the verb-class. For example, find is a member of the SEARCH verb-class. All verbs of a verb-class share the same syntax rules.

Clarity supports two types of statements:

1. Simple statements

2. Compound statements

A simple statement is merely a verb followed by a single segment. In simple statements, the verb-class fully constrains the segment to be of that same type.

Here is an example of a simple statement using a SEARCH directive:

find "in the beginning"

We will get into the particular way that this statement is parsed later in this document. But for now, we should notice that we have one verb and one segment. The type of the segment agrees with the verb and there is only one segment. Consequently, this is, by definition, a simple statement. We should notice that we have not informed our search engine what to search. So here is another example of a simple statement using a CONTROL  directive:

set source=bible

If we had run this configuration command prior to the search command listed above, our first match would be found in Genesis 1:1. But as the source domain of our search is a key element of our search, we should have a way to express both of these in a single command. And this is the rationale behind a compound statement. A compound statement has more than one segment. To combine the previous two statements into one compound statement, we can issue this command:

find "in the beginning" + source=bible

Segments in compound statements are delimited by plus-signs. For all compound statements, at least one segment must agree with the verb-class. Each verb-class defines whether additional segment types are permissible. In the case of OUPUT directives and SEARCH directives, CONTROL  segments are also valid segment types. All other verb-classes require that each segment agrees with the verb-class of the statement.

Before we go deep into the syntax of compound statements, we should define one more abstraction defined in the Clarity HMI specification. So far, we have considered only ordinary statements. Ordinary statements always begin with a verb and contain at least one segment.

In this section, we will introduce the abstraction of labelled-commands: We can apply a label to a statement to provide a shorthand for subsequent execution. This gives rise to two new definitions:

1. Labeled-Statement definitions

   Commands that allow us to save a statement with a label.

2. Labeled-Statement executions

   The ability to execute a previously labeled statement

 Let’s say we want to name our previously identified SEARCH directive with a label; We’ll call it “genesis”. To accomplish this, we would issue this command:

{genesis} := find “in the beginning” + source=bible

It’s that simple, now instead of typing the entire statement, we can use the label as shorthand to execute our newly saved command. Here is the command:

{genesis}

By default, labeled commands are scoped to the session. When evaluating a command label, the session is examined first, and if not defined within the session, the global label is expanded. However, when defining the label, complete control over user-scope versus session scope is available. As with all clarity directives, session-scope is the default. Prefixing a label with the at-symbol ( @ ) or hash-tag ( # ) defines the label globally. Of course, without these, the label is defined only within the current session.  The global nature of CONTROL [aka saving] is different between # and @. While the pound prefix will save data onto your local hard-drive for a PC or Mac, the @ prefix saves your data in the cloud (This may require becoming a registered user for a Clarity hosting service; and is not fully implemented at the time of this publication; However, the design of clarity syntax supports this feature. Digital-AV is expected to be the first available Clarity-Cloud host but that project is still in active development for its Clarity v1.5 support)

Whenever an expression begins with open-brace ( { ) and ends with close-brace ( } ), then it invokes a previously-labeled statement. As we saw earlier, if a command contains :=, then the label before the statement becomes registered as shorthand for the statement.

It should be noted that compound statements also work with labels.

Let’s label another statement: 

{my label can contain spaces} := set span=8

Compound execution of labeled statements can be accomplished as follows:

{genesis} + {my label can contain spaces}

 

As the previous command is valid syntax for a statement, it even follows that we can define this macro:

{sample} :={genesis} + {my label can contain spaces}

 

Later I can issue this command:

{sample}

Which is equivalent to executing these labeled statements:

{genesis} + {my label can contain spaces}

 

Labels can be defined in terms of an ordinary statement or using one or more labels inside of braces. And the two constructs can be mixed

{derived}: {original} + find: foo

Here are four more examples of labeled statement definitions:

{C1} := SET search=strict

{C2} := SET span=8

{F1} := FIND Godhead

{F2} := FIND eternal

 

We can execute these as a compound statement by issuing this command:

 {C1} + {C2} + {F1} + {F2}

 

Similarly, we could define another label from these, by issuing this command:

{sample2} := {C1} + {C2} + {F1} + {F2}

 

Prior to running compound labeled statements, a normalization process occurs.

Example of normalization for the sample2 label:

FIND godhead + eternal + search=strict + span=8

Interestingly, when CONTROL macros are combined with another verb, the key-value pairs apply ONLY to the execution of the other verb [SEARCH or DISPLAY], not to the entire session.

This concludes our discussion of labeled statements. Now let’s go deeper into extended statements. Just keep in mind that regardless of the complexity of an extended command, it can be labeled for shorthand execution.

However, if an execution ONLY contains CONTROL verbs, then the key-value pairs affect the session (or saved for future sessions if #set or @set is used). SESSION scope is always implied when paired with a SEARCH or OUPUT directive. The primary verb of the command always defines the scope. For example, configuration variables are always execution scope when combined with a SEARCH directive. See the table below for compatibility of directives and the implicit scope of the command.

| **Primary Directive** | **Secondary Directive(s)** | **Scope**                            |
| --------------------- | -------------------------- | ------------------------------------ |
| SEARCH                | SEARCH(ES), CONTROL(S)     | SEARCH: Session; CONTROL: Execution  |
| DISPLAY               | DISPLAY(S), CONTROL(S)     | DISPLAY: Session; CONTROL: Execution |
| CONTROL               | CONTROL(S)                 | Session                              |
| REMOVAL               | REMOVAL[[1\]](#_ftn1)(ES)  | Session                              |
| STATUS                | STATUS[[2\]](#_ftn2)(ES)   | Session, System, or Cloud            |
| #CONTROL              | #CONTROL(S)                | System                               |
| #REMOVAL              | REMOVAL[[1\]](#_ftn1)(ES)  | System                               |
| #STATUS               | #STATUS[[2\]](#_ftn2)(ES)  | System, or Cloud                     |
| @CONTROL              | CONTROL(S)                 | Cloud                                |
| @REMOVAL              | REMOVAL[[1\]](#_ftn1)(ES)  | Cloud                                |
| @STATUS               | @STATUS[[2\]](#_ftn2)(ES)  | Cloud                                |

When part of a command contains a SEARCH directive, then SEARCH becomes the primary directive. Likewise, when a command contains a DISPLAY directive, then DISPLAY becomes the primary directive. No other directive types can be combined with other directive types.  And only CONTROL directives are compatible with SEARCH or DISPLAY directives. When CONTROL symbols (# or @) are included and combined with a SEARCH or DISPLAY command, this causes downgrading of CONTROL to execution-scope, and have no effect on the session (e.g. When CONTROL segments are combined with a SEARCH segments, they only impact the execution of the search, not the session)

When multiple CONTROL directives compose a single statement, then the lowest scope of any segment ALWAYS applies to all segments of the statement.

Example:

@set x = 1 + #set y = 2 + set z = 3

is synonymous after downgrading with:

set x = 1 + set y = 2 + set z = 3

Moreover, it can be expressed more concisely as (and is is synonymous with):

x = 1 + y = 2 + z = 3

because *set* is the default verb for CONTROL and CONTROL segments are auto-detected by the presence of an equals sign.  Only the *set* and *find* verbs can be auto-detected.  Therefore these two segments would both be be autodetected as *find*:

beginning God + word flesh

Without an explicit verb and without an equals sign, segment type always default to find. So the previous statement is expanded by Clarity to:

*find* beginning God + *find* word flesh

Just to be clear, if you wanted to find the word "find", the verb is no longer optional. You be required to be explicit as as follows:

*find* find

In fact, this applies to any other Clarity verb. To find the word "set" or the the word "get", the find verb becomes required here too:

*find* set + *find* get

Consider the proximity search where the search target is the bible. Here is an example search using Clarity syntax:

**find** *source=bible + beginning created earth*

Clarity syntax can alter the span by supplying a CONTROL segment:

**find** *source=bible + **span=8 +** beginning created earth*

 

Assignment clauses can also be standalone to avoid redundancy with successive find commands:

**set source=bible + span=7**

**set search=strict**

 

*Now consider a different search:*

**find** *God created earth*

 

Next, consider a search to find that God created heaven or earth:

**find** *God created (earth heaven)*

 

The order in which the search terms are provided is insignificant. Additionally, the type-case is insignificant. 

Of course, there are times when word order is significant. Accordingly, searching for explicit strings can be accomplished using double-quotes as follows:

**find** *“God created ... Earth”*

 

These constructs can even be combined. For example:

**find** *”God created ... (Heaven Earth)”*

 

As Clarity supports multiple segments, the above search criteria would be equivalent to this search:

**find** *“God created ... Heaven” + “God created ... Earth”*

 

In all cases, “...” means “followed by”, but the ellipsis allows other words to appear between created and heaven. Likewise, it allows words to appear between created and Earth.

AV Text Ministries imagines that Clarity HMI can be applied broadly in the computing industry and can easily be applied outside of the narrow domain of biblical studies. For example, the Clarity syntax could easily handle statements such as:

​     **find: source=Wall Street Journal *** *“Trump ... tax cuts”*

 

Of course, translating the commands into actual search results might not be trivial for the application developer. Still, the reference implementation that parses a Clarity command is freely available in the reference implementation.

Clarity is designed to be intuitive. It provides the ability to invoke Boolean logic on how term matching should be performed. Parenthesis can be used to invoke Boolean multiplication upon the terms that compose a search expression. For instance, there are situations where the exact word within a phrase is not precisely known. For example, when searching the KJV bible, one might not recall which form of the second person pronoun was used in an otherwise familiar passage. Attempting to locate the serpent’s words to Eve in Genesis, one might execute a search such as:

​    **find** (you thou ye) shall not surely die 

This statement uses Boolean multiplication and is equivalent to this lengthier statement:

​    **find**  you shall not surely die + thou shall not surely die + ye shall not surely die

 

The example above also reveals how multiple search segments can be strung together to form a compound search: logically speaking, each segment is OR’ed together; this implies that any of the three matches is acceptable. Parenthetical Terms provide a shorthand for this type of search.

Similar to CONTROL scoping, when multiple STATUS directives compose a single statement, then the lowest Scope of any segment ALWAYS applies to all segments of the statement.

Likewise, when multiple REMOVAL directives compose a single statement, then the lowest Scope of any segment ALWAYS applies to all segments of the statement.

**Definitions:**

While some of these concepts have already been introduced, the following section can be used as a glossary for the terminology used in the Clarity HMI specification.

 

**Directives** are composed by verbs and are used to construct statements for the Clarity Command Interpreter. Each directive has specialized syntax tailored to the imperative verb used in the statement. The directive limits the type of segments that may follow. Most directives permit only a single segment type. DISPLAY and SEARCH directives also allow SCOPE segments. There are five types of directives. These correspond exactly to five verb classes. While there are nine verbs, there are only five verb-classes. The verb-classes correspond exactly to one of the five directive types.

**Segments:** the verb is followed by one or more segments. Each segment has a type, and the type of the segment must be compatible with the directive. As there are five types of directives, it not a coincidence that there are five types of segments. It is noteworthy that the syntax of a STATUS segment is identical to the syntax of a RESET segment, but we still consider the segment types to be distinct.

**SEARCH statement**: Each statement contains one or more *search segments*. If there is more than one SEARCH segment, each each segment is logically OR’ed with all other segments.

**SEARCH segment**: Each segment contains one or more *search terms*. A SEARCH segment is either unquoted statement or quoted.

**Unquoted SEARCH segment:** an unquoted segment contains one or more search words. If there is more than one word, then each word is logically AND’ed with all other words within the segment. Like all other types of segments, the end of the segment terminates with a plus-sign or a newline.

**NOTE:**

The absence of double-quotes means that the statement is unquoted.

**Quoted SEARCH segment:** a quoted segment contains a single string of terms to search. An explicit match on the string is required. However, an ellipsis ( … ) can be used to indicate that wildcards may appear within the quoted string.

**NOTES:**

It is called *quoted,* as the entire segment is sandwiched on both sides by double-quotes ( " ).

 

**Parenthetical Terms:** When searching, there are situations when the exact word that appears in a text is not precisely known.

**Bracketed Terms:** When searching, there are part the order of some terms within a quoted are unknown. Square brackets can be used to identify such terms. For example, consider this SEARCH statement:

*find “[God created] heaven and earth” + source=bible*

The above statement is equivalent to

*find “God created heaven and earth” + “created God heaven and earth” + source=bible*

**and:** In Boolean logic, ***and\*** means that all terms must be found. With Clarity-HMI, *and* is represented by terms that appear within an unquoted segment. 

**or:** In Boolean logic, ***or\*** means that any term constitutes a match. With Clarity=HMI, *or* is represented by the plus-sign ( **+** ) or [ **+** ] between SEARCH segments. 

**not:** In Boolean logic, ***not\*** means that the term must not be found. With Clarity, *not* is represented by a minus-sign ( **-** ) or [ **-** ] and applies to an entire segment (it cannot be applied to individual words unless the search segment has only a single term). In other words, a minus-sign means subtract results; it cancels-out matches against all matches of other segments. Most segments are additive as each additional segment increases search results. Contrariwise, a ***not\*** segment is subtractive as it decreases search results.

**NOTE:**

The minus-sign means that the segment will be subtracted from the search results while its absence means that the segment will be added to the search results. When only a single segment follows a SEARCH directive, it is always positive. A single negative segment following the find imperative, while it might be grammatically valid syntax, will never match anything. Therefore, while permitted in theory, it would have no real-world meaning. Consequently, some implementations of Clarity-HMI may disallow such a construct.

 

It should be noted that in some dialects of Clarity (e.g. Clarity AVX), hyphens in words are never required in Clarity AVX. For example, xray would be a valid shorthand for x-ray (making the hyphen superfluous).

 

**More Examples:**

Consider a query for all passages that contain God AND created, but NOT containing earth AND NOT containing heaven:

***Find source=old testament of bible + span = 15:\*** *created GOD ~ Heaven Earth*

*(this could be read as: find in the old testament using a span of 15, the words*

*created AND God, but NOT Heaven AND Earth)*

 

The simplest form to find ALL of three words (in the beginning):

*find in the beginning*

It should be noted that such a statement would find either of these strings in the text:

in the beginning

the beginning of summer in

 

If a specific string should be match, this can be stated explicitly:

find "in the beginning"

 

If you are unsure what article should match, you could issue this statement:

find "in (a the that) beginning"

Boolean multiplication would match only these strings of text:

in a beginning

in the beginning

in that beginning

 

If you are unsure which words might separate a phrase, you could issue this statement:

find "in the beginning … heaven and earth"

With this ellipsis in the find statement, it would match this string of text:

in a beginning, God created heaven and earth

 

To issue a query to find ANY words, separating each word with a plus sign would be the simplest form to find ANY of these five words:

*find Lord + God + messiah + Jesus + Christ*

 

If you are unsure about word order within a phrase, square brackets can be used:

find "in the beginning … [earth heaven]"

With this ellipsis and the final two bracketed terms, it would also match this string of text:

in a beginning, God created heaven and earth

 

The "*print*" verb has very limited grammar. For simplicity, consider the basic variants:

print output="C:\user\me\Documents\genesis.txt" + format = text + selection=genesis 

print format = text + output="C:\user\me\Documents\genesis.txt" + selection=genesis 

print format = html + output = "C:\user\me\Documents\exodus.html" + selection=exodus

print format = docx + "C:\user\me\Documents\Leviticus.docx" + selection=leviticus

 

**CONTROL directives:**

The **set** format command can be used to set the default print formats:

| **SCOPE**     | **docx**                      | **html**                      | **text**                      |
| ------------- | ----------------------------- | ----------------------------- | ----------------------------- |
| Session scope | *set display.format = docx*   | *set display.format = html*   | *set display.format = text*   |
| System scope  | *#set  display.format = docx* | *#set  display.format = html* | *#set  display.format = text* |
| Cloud scope   | *@set display.format = docx*  | *@set display.format = html*  | *@set display.format = text*  |

The **get**/**set** and directives can be used to store & retrieve numerous other settings:

| **SCOPE**                        | **example**                            |
| -------------------------------- | -------------------------------------- |
| Session scope                    | *set span = 7*                         |
| Cloud or System or Session scope | *get span*                             |
| *System scope*                   | *#set cloud.host= http://avbible.net/* |
| Cloud or System scope            | *#get cloud.host                       |
| Cloud scope                      | *@set display.format = docx*           |
| Cloud scope                      | *@get display.format*                  |

The **get**/**set** and **#get/#set** command can be used to retrieve Clarity configuration settings:

| **SCOPE**     | **example**                            |
| ------------- | -------------------------------------- |
| Session Scope | *set cloud.host= https://avbible.net/  |
| Session Scope | *get cloud.host                        |
| System scope  | *#set cloud.host= https://avbible.net/ |
| System scope  | *#get cloud*.host                      |

Macro definitions can utilize any of the three scopes:

| **SCOPE**     | **example**                 | **explanation**                                      |
| ------------- | --------------------------- | ---------------------------------------------------- |
| Session Scope | *{my macro} := find Jesus*  | *useful for temporary macro definitions*             |
| System scope  | *#{my macro} := find Jesus* | *macro stored on local file system or PC*            |
| Cloud scope   | *@{my macro} := find Jesus* | *macro stored in cloud using a Clarity cloud-driver* |
| Session Scope | *remove {my macro}*         | *removes session-defined macro*                      |
| System scope  | *#remove {my macro}*        | *removes macro stored on local file system  or PC*   |
| Cloud scope   | *@remove {my macro}*        | *removes macro stored in cloud*                      |

**STATUS directives:**

There are two status directives. One displays the current setting for the session:

**get format**

The other displays the current global setting:

**@get format**

 

**REMOVAL directives:**

Global control settings for SEARCH directives can be restored within the session:

**clear search.***         [All search control settings will be cleared with a single command]

Defaults for SEARCH directives can be globally restored (for this and any future session):

**@clear search.span**

**@clear display.format**

 

**ADDITIONAL NOTES:** 

In all cases, any number of spaces can be used between operators and terms. 

Also noteworthy: The reference Clarity implementation automatically adjusts the span of your to be inclusive of the number of search terms for the largest segment. So if you were to express:

**find span=1 + in the beginning (God Lord Jesus Christ Messiah)**

The minimum span has to be four(4). So the Clarity parser will adjust the search criteria as if the following command had been issued:

**find span=4 + in the beginning (God Lord Jesus Christ Messiah)**

**SPECIAL CHARACTERS & OPERATOR PRECEDENCE :**

​     The order for operator precedence is defined in AV Word as follows:

**{ }**

**:=**

**::**

**[+] or (+)** or plus delimited by white-space on left and right

**[-] or (-)** or hyphen delimited by white-space on left and right

**=**

**@**

**( )**

**[ ]**

**. . .**

**% %**

**" "**

**/ /**

**\ quote a reserved character**

**#**

**?**

**%**

**&**

**|**

 

**
**

 

**VERB segment**

**or**

**VERB segment + segment**

**or**

**VERB segment + segment + segment**

**etc.**

 

*Exactly one* **VERB** *is required.*

 

*At least one* **segment** *is required.*

 

*Directives refine the syntax generalized above.*

 

Macro definition is a command that does not automatically execute the macro once defined. If execution is also desired, the macro definition should use :: instead of := as follows to label the statement:



{my macro} :: @set span = 7



------

[[1\]](#_ftnref1) The *clear* verbs are supported as secondary directives, but not the *remove* verbs (The removal of labelled statements [aka macros] cannot be combined with any other segments).  The *remove* verb is always limited to a simple statement construction (simple statements contain only a single segment). Additionally, a statement with the *remove* verb cannot be used to define a newly labelled statement.

[[2\]](#_ftnref2) The *get* verbs are supported as secondary directives, but not the *expand* verbs (The expansion of labelled statements [aka macros] cannot be combined with any other segments).  The *expand* verb is always limited to a simple statement construction (simple statements contain only a single segment). Additionally, a statement with the *expand* verb cannot be used to define a newly labelled statement.