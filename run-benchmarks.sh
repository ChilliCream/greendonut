#!/usr/bin/env bash

dotnet build src -c release
sudo dotnet "src/Benchmarks/bin/Release/netcoreapp2.1/Benchmarks.dll"
