param($installPath, $toolsPath, $package, $project)

$p = Get-Project

$analyzerPath = join-path $toolsPath "analyzers"
$analyzerFilePath = join-path $analyzerPath "BlackFox.Roslyn.Diagnostics.dll"

$p.Object.AnalyzerReferences.Remove("$analyzerFilePath")