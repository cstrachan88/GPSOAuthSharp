# GPSOAuthSharp

A Portable .NET client library for Google Play Services OAuth written in C#.

This is a C# port of https://github.com/simon-weber/gpsoauth

## Updates
Project has been bumped to v1.0.0 due to some updates. Upgraded the library to a PCL, allowing a wider range of targets. Also, the authentication now happens asynchronously.

I also added back the MIT license since that was the license on the original project.

## NuGet package
You can find the old NuGetPackage [here](https://www.nuget.org/packages/GPSOAuthSharp/). A newer package has been created by [Nimgoble](https://github.com/Nimgoble), however this repo contains the latest changes.

## Usage
Construct a `GPSOAuthSharp.GPSOAuthClient(email, password)`.

Use `await PerformMasterLogin()` or `await PerformOAuth()` to retrieve a `Dictionary<string, string>` of response values. 

Demo response values: 

![](http://i.imgur.com/v5PqdKe.png)

Wrong credentials:

![](http://i.imgur.com/ubakOF3.png)

You can download an executable for the Demo on the [Releases page](https://github.com/vemacs/GPSOAuthSharp/releases/). 

The source for the Demo is located in the `GPSOAuthDemo` directory. The [main class](https://github.com/vemacs/GPSOAuthSharp/blob/master/GPSOAuthDemo/GPSOAuthDemo/Program.cs) is here.

Python result (for comparison): 

![](http://i.imgur.com/JyLnAK5.png)

## Goals
This project intends to follow the Google-specific parts of the Python implementation extremely carefully, so that any changes made to the Python implementation can be easily applied to this.
