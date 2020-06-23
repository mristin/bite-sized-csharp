# bite-sized

![Check](
https://github.com/mristin/bite-sized-csharp/workflows/Check/badge.svg
) [![Coverage Status](
https://coveralls.io/repos/github/mristin/bite-sized-csharp/badge.svg)](
https://coveralls.io/github/mristin/bite-sized-csharp
) [![Nuget](
https://img.shields.io/nuget/v/BiteSized)](
https://www.nuget.org/packages/BiteSized
)


Bite-sized is a dotnet tool to check that your source code is not too long 
(number of lines) and not too wide (number of characters per line).

## Motivation

It would be great if we could decouple our source code from its visual 
representation and stop treating it like text (*e.g.*, diff'ing two files 
*as code* regardless of formatting). Unfortunately, our development
environments are not there yet and we still rely heavily on tools that treat
the source code as *text*. Hence, we need to use tools that are unable to 
automatically wrap and reformat the code on the spot. This forces us to deal
with issues unless we enforce line and character limits.
 
**Maximum line width** is important so that:
* You can compare 2 or 3 files in parallel or view them without tedious 
  horizontal scrolls. For example, Github layout will expect all source code to 
  be within a certain width (and force you to horizontally scroll otherwise).
* Furthermore, the developers using laptops or people who need to display or 
  print your code will really appreciate it.
* Finally, short lines force you to be succinct to a certain degree and 
  long lines might signal that your code is too stuffed. Humans need width 
  limits for parsing (hence the layout of the books!). 

The limit of 120 characters is perhaps a nice compromise between wide-enough 
lines (no unnecessary line breaks) and reading experience.

**Maximum number of lines** is a good proxy of complexity. Once you pass a
certain threshold, problems start to surface:
* It is difficult to find yourself around in a long file.
* The set of symbols that you have to keep in your head grows. (Think of all
  the `using`s in your long C# file that will clutter the symbol space).
* Long files are a potential indicator that the level of abstraction is not
  adequate anymore.
* Your tests will be conceptually blurry if you relate them to long files 
  (*i.e.* many tests lumped together).
* Your commit log is loosing precision as file changes will not be that 
  informative any more.
* ... and many, many more problems.

Beware that this limit should not be too strict lest it gives you fragmented 
code (which is equally bad for the readability!). A reasonable maximum length
might be about 2000 lines. 

## Related Tools

Surprisingly, at the time of the writing (2020-06-27), we couldn't find a simple
tool to check the length and width of (source) files in dotnet:

* The widely used code inspection tool [Resharper CLI](
https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html
) does not have a rule for maximum number of line and characters.

* Another code inspector, [StyleCop](
https://github.com/StyleCop/StyleCop
) supports the maximum number of lines and characters, but the tool is not 
actively maintained anymore (see [this section in their readme](
https://github.com/StyleCop/StyleCop#considerations
)).

* The successor to StyleCop, [StyleCopAnalyzers](
https://github.com/DotNetAnalyzers/StyleCopAnalyzers) seem reluctant to support
 a rule to check for the maximum width and length (see [this issue](
https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/782
) and [the related comment](
https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/782#issuecomment-140106659
)).

* There is a code inspector [Menees.Analyzers](
https://github.com/menees/Analyzers) that can verify the line number and length.
Unfortunately, it is not a dotnet tool (it requires .NET Framework, see
[this comment on the Github issue](
https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/782) and 
[this comment](
https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/782#issuecomment-243275140
)).

## Installation

Bite-sized is a dotnet tool. You can either install it globally:

```bash
dotnet tool install -g BiteSized
```
or locally (if you use tool manifests, see [this Microsoft tutorial](
https://docs.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use
)):

```bash
dotnet tool install BiteSized
```

## Usage

You run bite-sized through `dotnet`.

To obtain help:

```bash
dotnet bite-sized --help
```

You specify which files need to be checked using glob patterns in `--inputs`:

```bash
dotnet bite-sized --inputs "ProjectX/**/*.cs"
```

You exclude by passing `--excludes` with glob patterns  
*(we use '\' for line continuation here)*:

```bash
dotnet bite-sized \
    --inputs "ProjectX/**/*.cs" \
    --exclude "**/obj/**"
``` 

The limits can be adjusted if you are not happy with the defaults:

```bash
dotnet bite-sized \
    --inputs "ProjectX/**/*.cs" \
    --max-line-length 160 \
    --max-lines-in-file 5000
```

## Contributing

Feature requests, bug reports *etc.* are highly welcome! Please [submit
a new issue](https://github.com/mristin/bite-sized-csharp/issues/new).

If you want to contribute in code, please see
[CONTRIBUTING.md](CONTRIBUTING.md).

## Versioning

We follow [Semantic Versioning](http://semver.org/spec/v1.0.0.html).
The version X.Y.Z indicates:

* X is the major version (backward-incompatible w.r.t. command-line arguments),
* Y is the minor version (backward-compatible), and
* Z is the patch version (backward-compatible bug fix).