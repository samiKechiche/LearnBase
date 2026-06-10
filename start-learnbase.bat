@echo off

echo Starting LearnBase Backend...
start cmd /k "cd /d C:\Users\kechi\Desktop\Epi\PFA\LearnBase\backend\LearnBase.API && dotnet build && dotnet run --urls http://localhost:5077"

echo Starting LearnBase Frontend...
start cmd /k "cd /d C:\Users\kechi\Desktop\Epi\PFA\LearnBase\frontend && npm start"

echo LearnBase is starting...


