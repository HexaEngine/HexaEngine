// See https://aka.ms/new-console-template for more information
using HexaEngine.Core;
using HexaEngine.Tests;

Console.WriteLine("Hello, World!");

BasicDrawingInstanced tests = new();
SdlWindow window = new();
window.Show();
tests.Setup(window);
Task.Run(() => tests.DrawSphere());
Application.Run(window);
tests.Teardown();