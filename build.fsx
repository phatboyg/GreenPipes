#r @"src/packages/FAKE/tools/FakeLib.dll"
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.Git.Information
open Fake.SemVerHelper

let buildOutputPath = FullName "./build_output"
let buildArtifactPath = FullName "./build_artifacts"
let packagesPath = FullName "./src/packages"
let keyFile = FullName "./GreenPipes.snk"

let assemblyVersion = "1.0.0.0"
let baseVersion = "1.0.9"

let semVersion : SemVerInfo = parse baseVersion

let Version = semVersion.ToString()

let branch = (fun _ ->
  (environVarOrDefault "APPVEYOR_REPO_BRANCH" (getBranchName "."))
)

let FileVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (Version + "." + "0"))

let informationalVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else " (" + branchName + "/" + (getCurrentSHA1 ".").[0..7] + ")"
  (FileVersion + label)
)

let nugetVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else "-" + (if branchName="mt3" then "beta" else branchName)
  let version = if branchName="master" then Version else FileVersion
  (Version + label)
)

let InfoVersion = informationalVersion()
let NuGetVersion = nugetVersion()


printfn "Using version: %s" Version

Target "Clean" (fun _ ->
  ensureDirectory buildOutputPath
  ensureDirectory buildArtifactPath

  CleanDir buildOutputPath
  CleanDir buildArtifactPath
)

Target "RestorePackages" (fun _ -> 
  DotNetCli.Restore (fun p -> { p with Project = "./src/" } )
)

Target "Build" (fun _ ->

  CreateCSharpAssemblyInfo @".\src\SolutionVersion.cs"
    [ Attribute.Title "GreenPipes"
      Attribute.Description "GreenPipes, a pipes and filters library for the Task Parallel Library"
      Attribute.Product "GreenPipes"
      Attribute.Version assemblyVersion
      Attribute.FileVersion FileVersion
      Attribute.InformationalVersion InfoVersion
    ]
  DotNetCli.Build (fun p-> { p with Project = @".\src\GreenPipes"
                                    Configuration= "Release"
                                    Output = buildArtifactPath})
)

Target "Package" (fun _ ->
  DotNetCli.Pack (fun p-> { p with 
                                Project = @".\src\GreenPipes"
                                Configuration= "Release"
                                OutputPath= buildArtifactPath })
)

Target "Default" (fun _ ->
  trace "Build starting..."
)

"Clean"
  ==> "RestorePackages"
  ==> "Build"
  ==> "Package"
  ==> "Default"

RunTargetOrDefault "Default"