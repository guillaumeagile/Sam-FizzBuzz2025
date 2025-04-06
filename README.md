# README #
FizzBuzz implementation in C#, continuation of a training session the 22/12.

## Interesting Points ##
This implementation has some interesting points that were discussed during the training session. 

### Dependency Injection ###
The rules are externalized and injected into the engine. 

### Open/Close principle ###
Clients can create and inject their own rules into the engine, using the defined interfaces and base class methods.

### Sorting and IComparable ###
The rules are ordered by priority

### HashSets ###
The usage of the SortedSet allows us to insert the rules in any order and even insert them multple time without duplication.

## Technical Aspects ##
This solution was originally developed on Windows 10 with Visual Studio 2015 Community Edition. It was then adapted to Monodevelop 5.10 and Mono 4.6.2 and all of the further modifications were developed with Monodevelop on Debian 8. The solution was also adapted to NUnit from the original MSTest implementation.