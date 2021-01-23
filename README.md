# Quelle-HMI

#### Searching sources

With the C# code contained herein, and companion github/kwonus projects, the simplicity of creating a
Quelle driver using this Quelle HMI library is exemplified. This code can be used as a template to create your own driver, or it can be subclassed to extend behavior: Specifically, the Cloud() methods need implementations in the subclass to provide the actual search functionality.
<br/></br>
A tandem project in github provides a standard Quelle driver and a standard Quelle interpreter.
Using the standard Quelle Driver projects (with this Quelle HMI library), there are less than 500 
lines of code to extend and/or modify to customize your own Quelle compliant driver/interpreter.
The value proposition here is that parsing is tedious. And starting your search CLI with a concise syntax
with an easy to digest parsing library could easily save your team a person-year in design-time and coding.
Quelle source code is licensed with a liberal MIT license.
<br/></br>
The design incentive for Quelle HMI and the standard driver/interpreter is to support the broader Digital-AV
effort: That project [Digital-AV] provides a command-line interface for searching and publishing the KJV bible.
Every attempt has been made to keep the Quelle syntax agnostic about the search domain, yet the Quelle
user documentation itself is heavily biased in its syntax examples. Still, the search domain of the
StandardQuelleDriver remains unbiased.

Concise notes on how punctuation is used to represent Boolean expressions.  More complete documentation is soon to be forthcoming.

```
a b (c d) // "(e f) g [h i] j" // "[k l] ... m ... (n o)" + "[(p q) r s] t" // "*men boy*"
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
By restricting the tokenfgroup above, we do not need specialized parsing for AVX, the execute method will desipher meaning of tokens.

Unrelated to any of this, we shouild be able to set the span to "Verse Scope".  We could represent this with a span = 0.

STEP 1
======
break command into macro (left-side) and segment-array

The first byte is the type: AV | AVX | AV+AVX | AV != AVX (diff) | SYM | Eo? | Lemma(AV) | Lemma(AVX) | Part-Of-Speech | WordClass | Person-Number
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

EACH SEGMENT HAS AN ANCHOR. AN ANCHOR is either a normal fragment or a specialized fragment of BoV/BoC/BoB or Open-Paren
Bits will no longer apply to tokens, but to segments. We will limit bits to either 32, with overflow reusing the 1-bit
in round-robin fashion. This bit will be primarily used for display and will mark all words in the matching span. All words
that constitute a match on the span will me marked.

for each anchor-candidate, forward-scan to completion of a match within the span, and marking bits only upon success.
As soon as a failure is noticed, call-out, but do not advance the cursor until all segments are forward scanned and
conditionally marked (a matching word that does not consitute a matching segment will no longer be highlighted).

This should be no slower than earlier implementation and way more straightforward.

NEGATIVE POLARITY APPLIES TO VERSES. THIS MIGHT BE A LITTLE TRICKY OR COUNTER INTUITIVE

Parse()
Validate()
Execute()

```