#!/bin/sh

mkdir Logs
msbuild ./CoreDll/CoreDll.csproj > ./Logs/buildCoreDll.log
msbuild ./NUnitTest/NUnitTest.csproj > ./Logs/buildNUnitTest.log
nunit-console4 ./NUnitTest/bin/Debug/NUnitTest.dll > ./Logs/runNUnitTest.log
