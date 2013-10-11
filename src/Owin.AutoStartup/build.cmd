@echo off
%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe Owin.AutoStartup.sln /t:Rebuild /p:Configuration=Release
cd Owin.AutoStartup 
nuget pack Owin.AutoStartup.nuspec
cd ..
