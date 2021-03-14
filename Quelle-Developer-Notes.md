# Quelle Developer Notes

##### version 1.0.1.3D

1. SEARCH
   - find
2. CONTROL
   - set
   - clear
   - show
3. LABEL
   - save
   - delete
   - review
4. DISPLAY
   - print
5. SYSTEM
   - help
   - status
   - generate
   - exit

| Verb        | Action Type | Syntax Category | Required Parameters     | Required Operators | Optional Operators | Provider or Driver |
| ----------- | :---------: | --------------- | ----------------------- | :----------------: | :----------------: | ------------------ |
| *find*      |  implicit   | SEARCH          | **1**: *search spec*    |                    |  **" " [ ] ( )**   | provider           |
| *set*       |  implicit   | CONTROL         | **2**: *name* = *value* |       **=**        |                    | driver             |
| *clear*     |  implicit   | CONTROL         | **1**: *control_name*   |       **=@**       |                    | driver             |
| **@show**   | independent | CONTROL         | **0+**: *control_names* |                    |                    | driver             |
| **@print**  |  dependent  | DISPLAY         | **0+**: *identifiers*   |                    |      **[ ]**       | provider           |
| **@save**   |  dependent  | LABEL           | **1**: *macro_label*    |      **{ }**       |                    | driver             |
| **@delete** | independent | LABEL           | **1+**: *macro_label*s  |      **{ }**       |                    | driver             |
| **@review** | independent | LABEL           | **0+**: *macro_labels*  |                    |      **{ }**       | driver             |

Macros are yaml files which include values for all controls, indented as

search:

​	span: 7

macro:

​	My Macro: foo

Can YAML contain spaces?

| Verb          | Action Type | Clause Type | Required Arguments |
| ------------- | ----------- | ----------- | ------------------ |
| **@help**     | independent | SYSTEM      | 0 or 1             |
| **@generate** | independent | SYSTEM      | 2 or 4             |
| **@status**   | independent | SYSTEM      | 0 or 1             |
| **@exit**     | independent | SYSTEM      | 0                  |



| Fully Specified Name | Short Name | Meaning                              | Values     | Visibility |
| -------------------- | ---------- | ------------------------------------ | ---------- | ---------- |
| search.span          | span       | proximity                            | 0 to 1000  | normal     |
| search.domain        | domain     | the domain of the search             | string     | normal     |
| search.exact         | exact      | exact match vs liberal/fuzzy         | true/false | normal     |
| display.heading      | heading    | heading of results                   | string     | normal     |
| display.record       | record     | annotation of results                | string     | normal     |
| display.format       | format     | display format of results            | Table 7-1  | normal     |
| display.output       | output     | ability to redirect output to a file | filename   | normal     |

| Fully Specified Name | Short Name  | Meaning                                                     | Values                                 | Visibility |
| -------------------- | ----------- | ----------------------------------------------------------- | -------------------------------------- | ---------- |
| system.host          | host        | URL of driver                                               | string                                 | normal     |
| system.indentation   | indentation | specifies tabs or spaces on when invoking @generate command | tab, spaces:2, spaces:3, spaces:4, ... | *hidden*   |

### 
