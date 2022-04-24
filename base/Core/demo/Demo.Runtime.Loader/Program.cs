using System;
using Annium.Core.Entrypoint;
using Demo.Runtime.Loader;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

Console.WriteLine("Make use of https://github.com/HavenDV/H.Dependencies to resolve from deps.json");