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

 the solution is implemented in C# using .NET core 8.0.
Can build and run the solution using `dotnet build` and `dotnet run`, under any OS (Windows, Linux, Mac)
Can run on CI/CD: a docker file is provided in the test project, so that it can run on its own

