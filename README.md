NSoft.Log
=========

Simple, flexible and easily extendable library for various logging tasks.

## Strong sides of this library are:
## Flexibility
This logger supports writing of the any data that might be represented as an array of strings. In addition, any combination of data writers could be used.

##Performance
The library has a tiny overhead. It depends on writer that you use. For example, FileWriter can write more than 80000 records per second.

##Reliability
Each category of log channels might be linked with one active and any amount of the reserved writers. Active writer is an enabled writer with the highest priority. Writer is switched to another one when it can't write data. For example, when logs can not be sent to the WCF service, they are written to the file system.

##Extensibility
New writers could be developed and referenced in the log manager configuration.
