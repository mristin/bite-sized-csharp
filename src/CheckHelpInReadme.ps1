<#
.DESCRIPTION
This script checks that the help output from the program and the message
documented in the Readme coincide.
#>

function GenerateDiffReport($helpLines, $endMarkerAt, $startMarkerAt, $readmeLines, $diffAt)
{
    $reportLines = @("The captured output:")
    for($i = 0; $i -lt $helpLines.Length; $i++)
    {
        if (($null -ne $diffAt) -and ($diffAt -eq $i))
        {
            $prefix = ">>> " + "$( $i + 1 )".PadLeft(5)
        }
        else
        {
            $prefix = "$( $i + 1 )".PadLeft(9)
        }

        $reportLines += "${prefix}: $( $helpLines[$i] )"
    }

    $reportLines += ""
    $reportLines += "The help in the Readme:"

    $lineCount = $endMarkerAt - $startMarkerAt - 1
    for($i = 0; $i -lt $lineCount; $i++)
    {
        if (($null -ne $diffAt) -and ($diffAt -eq $i))
        {
            $prefix = ">>> $( $i + $startMarkerAt + 1 ) ($($i + 1))".PadLeft(5)
        }
        else
        {
            $prefix = "$( $i + $startMarkerAt + 1 ) ($($i + 1))".PadLeft(9)
        }

        $reportLines += "${prefix}: $( $readmeLines[$i + $startMarkerAt + 1] )"
    }

    return $reportLines
}

function Main
{
    $repoRoot = Split-Path -Parent $PSScriptRoot

    $buildDir = Join-Path $repoRoot "out"

    if (!(Test-Path $buildDir))
    {
        throw ("The build directory does not exist: $buildDir; " +
                "did you `dotnet publish -c Release` to it?")
    }

    $program = Join-Path $buildDir "BiteSized"
    if (!(Test-Path $program))
    {
        $program = Join-Path $buildDir "BiteSized.exe"

        if (!(Test-Path $program))
        {
            throw ("The program could not be found " +
                    "in the build directory: $buildDir; " +
                    "did you `dotnet publish -c Release` to it?")
        }
    }

    $helpLines = & $program --help
    if (!($helpLines -is [array]))
    {
        throw ("Expected the output to be an array, but got $($helpLines.GetType() ): $helpLines")
    }

    $prefixEmptyLines = 0
    for($i = 0; $i -lt $helpLines.Length; $i++)
    {
        if ($helpLines[$i] -eq "")
        {
            $prefixEmptyLines++
        }
        break;
    }

    $suffixEmptyLines = 0
    for($i = $helpLines.Length - 1; $i -ge 0; $i--)
    {
        if ($helpLines[$i] -eq "")
        {
            $suffixEmptyLines++
        }
        break;
    }

    $helpLines = $helpLines[$prefixEmptyLines .. ($helpLines.Length - $suffixEmptyLines - 1)]

    # Make help lines valid markdown to make the comparison against Readme
    # a bit more concise
    $helpLines = (
    @("``````") +
            $helpLines +
            @("``````")
    )

    $readmePath = Join-Path $repoRoot "README.md"
    if (!(Test-Path $readmePath))
    {
        throw "The readme could not be found: $readmePath"
    }
    $readme = Get-Content $readmePath

    $readmeLines = $readme.Split(
            @("`r`n", "`r", "`n"), [StringSplitOptions]::None)

    $startMarker = "<!--- Help starts. -->"
    $endMarker = "<!--- Help ends. -->"

    $startMarkerAt = -1
    $endMarkerAt = -1
    for($i = 0; $i -lt $readmeLines.Length; $i++)
    {
        $trimmed = $readmeLines[$i].Trim()
        if ($trimmed -eq $startMarker)
        {
            $startMarkerAt = $i
        }

        if ($trimmed -eq $endMarker)
        {
            $endMarkerAt = $i
        }
    }

    if ($startMarkerAt -eq -1)
    {
        throw "The start marker $startMarker could not be found in: $readmePath"
    }
    if ($endMarkerAt -eq -1)
    {
        throw "The end marker $endMarker could not be found in: $readmePath"
    }

    $lineCount = $endMarkerAt - $startMarkerAt - 1

    if ($lineCount -ne $helpLines.Length)
    {
        $errorLines = @(
            "--help gave $( $helpLines.Length ) line(s)."
            "The Readme contained $lineCount line(s)."
        )

        $errorLines = $errorLines + (
            GenerateDiffReport `
                    -helpLines $helpLines `
                    -endMarkerAt $endMarkerAt `
                    -startMarkerAt $startMarkerAt `
                    -readmeLines $readmeLines `
                    -diffAt $null)
        $error = $errorLines -Join [Environment]::NewLine
        throw $error
    }

    for($i = $startMarkerAt + 1; $i -lt $endMarkerAt; $i++)
    {
        $helpLineIdx = $i - $startMarkerAt - 1
        $helpLine = $helpLines[$helpLineIdx]
        $readmeLine = $readmeLines[$i]
        if ($helpLine -ne $readmeLine)
        {
            $errorLines = @(
                "The line $( $i + 1 ) in $readmePath does not " +
                    "coincide with the line $( $helpLineIdx + 1 ) of --help: ",
                "$( $readmeLine|ConvertTo-Json ) != " +
                    $( $helpLine|ConvertTo-Json )
            )

            $errorLines = $errorLines + (
                GenerateDiffReport `
                    -helpLines $helpLines `
                    -endMarkerAt $endMarkerAt `
                    -startMarkerAt $startMarkerAt `
                    -readmeLines $readmeLines `
                    -diffAt $helpLineIdx)
            $error = $errorLines -Join [Environment]::NewLine
            throw $error
        }
    }
    Write-Host "The --help message coincides with the Readme."
}

Main