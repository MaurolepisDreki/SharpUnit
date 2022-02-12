&#0169;<time datetime="2022">MMXXII</time> Maurolepis Dreki; All Rights Reserved

# SharpUnit
> ... a *good* Unit Tester for C#...

C# is an OOP language, where "OO" can be taken to mean "Object-Obsessed" rather than the usual meaning of 
"Object-Oriented".  Granted that this programmer has refused to touch C# for the better part of a decade, xi has yet to
find a Testing Framework that is to xir liking, "Unit" style or otherwise.  The closest such Framework has been 
[BCUnit](https://github.com/BelledonneCommunications/bcunit), which I regularly use in my prefered language (C/C++).
It is important to note that *this* framework (SharpUnit) is not a port of BCUnit to C#, rather SharpUnit is it's own 
testing framework with a nostalgic flavor reminiscent of BCUnit; if writting in C/C++, I highly recomend using that 
tool.

## What does it mean to be "good"?
I've thought long and hard on this and decided that to be good, a framework must match three criteria:
 1. A framework **must** be simple to use, even to being semi-intiuitive.
 2. A framework **must** simplify the task at hand through structure and automation.
 3. A framework **must** be abusable, such that it's only true limitation is it's user's inginuity.

For the most part, I beleive SharpDevelop does exactly as described, and that where it falls short is a failure of the
language as opposed to the design; E. g., C# reflection does not penitrate object instances thereby limiting our
ability to automate test assertations via traversing the callstack and thereby imposing further limitations on the user
such as the inability to reuse test methods.

## Using SharpUnit
At the moment, the only way to use the library is to download it and compile it with your code:

```shell-session
$ git submodule add https://github.com/MaurolepisDreki/SharpUnit.git SharpUnit
$ dotnet sln add SharpUnit
$ dotnet add <yourprojectTester> reference SharpUnit
```

