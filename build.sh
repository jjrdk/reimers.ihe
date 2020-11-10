#!/usr/bin/env bash

dotnet new tool-manifest --force
dotnet tool install Cake.Tool --version 0.38.5
dotnet tool restore
dotnet cake build.cake
