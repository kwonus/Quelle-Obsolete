# Quelle Developer Notes

##### version 1.0.1.3E

1. SEARCH
   - find
2. CONTROL
   - set
   - clear
   - get
3. LABEL
   - save
   - delete
   - show
4. DISPLAY
   - print
5. - HISTORY
   - review
   - undo
   - redo
6. SYSTEM
   - help
   - status
   - generate
   - exit

| Verb          | Action Type | Syntax Category | Required Parameters     | Required Operators | Optional Operators | Provider or Driver |
| ------------- | :---------: | --------------- | ----------------------- | :----------------: | :----------------: | ------------------ |
| *find*        |  implicit   | SEARCH          | **1**: *search spec*    |                    |  **" " [ ] ( )**   | provider           |
| *set*         |  implicit   | CONTROL         | **2**: *name* = *value* |       **=**        |                    | driver             |
| *clear*       |  implicit   | CONTROL         | **1**: *control_name*   |       **=@**       |                    | driver             |
| **@get**      | independent | CONTROL         | **0+**: *control_names* |                    |                    | driver             |
| **@print**    |  dependent  | DISPLAY         | **0+**: *identifiers*   |                    |      **[ ]**       | provider           |
| **@save**     |  dependent  | LABEL           | **1**: *macro_label*    |                    |                    | driver             |
| **@delete**   | independent | LABEL           | **1+**: *macro_label*s  |      **{ }**       |                    | driver             |
| **@show**     | independent | LABEL           | **0+**: *macro_labels*  |                    |      **{ }**       | driver             |
| **@review**   | independent | HISTORY         | 0 or 1                  |                    |                    | driver             |
| **@undo**     | independent | HISTORY         | 0                       |                    |                    | driver             |
| **@redo**     | independent | HISTORY         | 0                       |                    |                    | driver             |
| **@help**     | independent | SYSTEM          | 0 or 1                  |                    |                    | driver             |
| **@generate** | independent | SYSTEM          | 2 or 4                  |                    |      **! >**       | driver             |
| **@status**   | independent | SYSTEM          | 0 or 1                  |                    |                    | provider           |
| **@exit**     | independent | SYSTEM          | 0                       |                    |                    | driver             |



| Long Name          | Short Name  | Meaning                                                     | Values                                 | Passed to search provider | Notes                                                   |
| ------------------ | ----------- | ----------------------------------------------------------- | -------------------------------------- | ------------------------- | ------------------------------------------------------- |
| search.span        | span        | proximity                                                   | 0 to 1000                              | yes                       |                                                         |
| search.domain      | domain      | search domain                                               | string                                 | yes                       |                                                         |
| search.exact       | exact       | exact vs liberal/fuzzy                                      | true/false                             | yes                       |                                                         |
| display.heading    | heading     | heading of results                                          | string                                 | no                        |                                                         |
| display.record     | record      | fetch result annotations                                    | string                                 | no                        |                                                         |
| display.format     | format      | page result format                                          | Table 7-1                              | yes                       |                                                         |
| display.output     | output      | save page result to file                                    | filename                               | no                        |                                                         |
| system.host        | host        | URL of driver                                               | string                                 | no                        | define the search provider to use                       |
| system.indentation | indentation | specifies tabs or spaces on when invoking @generate command | tab, spaces:2, spaces:3, spaces:4, ... | *hidden*                  | hidden, do not expose value with wildcard @get requests |

Macros are yaml files which include values for all controls.  In the example below, the yaml file would be *named my-macro-label.yaml*.  Macros are always case-insensitive.  And hyphens and spaces are synonymous when naming the macro.

```yaml
search:
​	span: 7
​	domain: kjv
​	extact: false
display:
​	heading: !!null
​	record: !!null
#	format: html    # need not be included, because it has no effect on macros
#	output: !!null	# need not be included, because it has no effect on macros
system:
​	host: http://avbible.net
#	indentation: 4  # need not be included, because it has no effect on macros
definition:
​	label:	My Macro Label
​	macro:	eternal power godhead ; Jehova
```

Control variable share this common format.  However, they are split across three distinct files and occur without the yaml section headings.



search.yaml

```yaml
span: 7
domain: kjv
extact: false
```



display.yaml

```yaml
heading: !!null
record: !!null
format: html
output: !!null
```



system.yaml

```yaml
host: http://avbible.net
indentation: 4
```



Terse & concise parsing notes (these test-cases need to be included in unit-tests):

```
a b (c d) // "(e f) g [h i] j" // "[k l]...m...(n o)" // "[(p q) r s] t" //"*men boy*"
segment 1: (unquoted)		/ NEGATIVE
	fragment 1.1:	 a 	/ position = none / SINGLETON
	fragment 1.2:	 b 	/ position = none / SINGLETON
	fragment 1.3:	(c d) 	/ position = none / SET
segment 2: (quoted)
	fragment 2.1:	(e f) 	/ t1: position = 1 / SET
	fragment 2.2:	 g 	/ position = t1+1 / SINGLETON
	fragment 2.3:	 h 	/ g+1 >= position <= g+2 / SINGLETON
	fragment 2.3:	 i 	/ g+1 >= position <= g+2 / SINGLETON
	fragment 2.4:	 j 	/ position = g + 3 / SINGLETON
segment 3: (quoted)
	fragment 3.1:	 k 	/ 1 >= position <= 2 / SINGLETON
	fragment 3.2:	 l 	/ 1 >= position <= 2 / SINGLETON
	fragment 3.3:	 m 	/ t3: 3 <= position <= span/ SINGLETON
	fragment 3.4:	(n o) 	/ t3 > position <= span / SET
segment 4: (quoted)
	fragment 4.1:	(p q)	/ 1 >= position <= 3 / SET
	fragment 4.2:	 r 	/ 1 >= position <= 3 / SINGLETON
	fragment 4.3:	 s 	/ 1 >= position <= 3 / SINGLETON
	fragment 4.4:	 t 	/ position = 4 /SINGLETON
segment 5: quoted
	fragment 5.1:	(men women) 		 / position = 1 / SET
	fragment 5.2:	(boy boys boycott) 	 / position = 2 / SET


NOTE ([p q] r s) == (p q r s) ... Therefore delete/ignore square-braces within parens

We will also add the & symbol to allow matching upon a single token (like PN and stem).
For parsing purposes spaces WILL NOT be allowed around the &. Example:
/pronoun#2PS/&/BOV/ #run&/v/
^^^
(This segment has two fragments; each fragment has two tokens)
By restricting the tokenfgroup above, we do not need specialized parsing for AVX, the
execute method will desipher meaning of tokens.

Unrelated to any of this, we shouild be able to set the span to "Verse Scope".
We could represent this with a span = 0.

STEP 1
======
break command into macro (left-side) and segment-array

The first byte is the type:
 AV | AVX | AV+AVX | AV != AVX (diff) | SYM | Eo? | Lemma(AV) | Lemma(AVX) | Part-Of-Speech | WordClass | Person-Number
If Specialized, then the next 3 bytes are used to identify the the type of symbol (zero-bits means any symbol for \SYM\)
If AV and/or AVX, the the last 16-bit word is the word-key


STEP 2
======
for each SEGMENT
extract fragment-array

ADD specialized fragments to Quelle 2.0
========================================
/SYMbol/	(any punctuation or symbol / i.e non-alpha-numeric)
/PUNCtuation/	(any punctuation or symbol / i.e non-alpha-numeric)
/,./	(comma or period)
/EoV/	(end of verse)
/BoV/	(beginning of verse)
/EoC/	(end of chapter)
/BoC/	(beginning of chapter)
/EoB/	(end of book)
/BoB/	(beginning of book)

EACH SEGMENT HAS AN ANCHOR. AN ANCHOR is either a normal fragment or a specialized
fragment of BoV/BoC/BoB or Open-Paren Bits will no longer apply to tokens, but to
segments. We will limit bits to either 32, with overflow reusing the 1-bit in
round-robin fashion. This bit will be primarily used for display and will mark all
words in the matching span. All words constituting a match on the span will me marked.

For each anchor-candidate, forward-scan to completion of a match within the span, and
marking bits only upon success. As soon as a failure is noticed, call-out, but do not
advance the cursor until all segments are forward scanned and conditionally marked
(matching words that do not consitute a matching segment won't be highlighted).

This should be no slower than earlier implementation and way more straightforward.

NEGATIVE POLARITY APPLIES TO VERSES.
THIS MIGHT BE A LITTLE TRICKY OR COUNTER INTUITIVE

Parse()
Validate()
Execute()
```

(Notes above exemplify how punctuation is used to represent Boolean expressions)  