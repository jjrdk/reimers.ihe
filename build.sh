#!/usr/bin/env bash

dotnet new tool-manifest
dotnet tool install Cake.Tool
dotnet tool restore
dotnet cake build.cake
