# Gatekeeper Roles-Based Access Control

Roles-Based Access Control Library in C#

Gatekeeper is a simple library for implementing roles-based access control to control access to resources by users given a specified operation type.  Gatekeeper uses Sqlite3 and requires that sqlite3.dll be placed in the project and output directory.  

With Gatekeeper, you can define users, roles, and permissions, then authorize access attempts to resources (by resource name and operation).

## Help, Feedback, and Disclaimer

First things first - do you need help or have feedback?  Contact me at joel at maraudersoftware.com dot com or file an issue here!  We would love to get your feedback to help make our products better.  Also, there may be bugs or issues that we have yet to encounter!  

## Sample App

Refer to the ```Test``` project for a working example.

## Running in Mono

Gatekeeper works well in Mono environments to the extent that we have tested it. It is recommended that when running under Mono, you execute the containing EXE using --server and after using the Mono Ahead-of-Time Compiler (AOT).  Mono.Data.Sqlite was used to ensure Mono compatibility.
```
mono --aot=nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048 --server myapp.exe
mono --server myapp.exe
```