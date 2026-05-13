using System.Diagnostics;

// AegisLoop V1 — Lanceur racine (.NET 10)
var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "api";
var (project, label) = mode switch
{
    "worker" => ("src\\AegisLoop.Worker\\AegisLoop.Worker.csproj", "Worker (IngestionWorker)"),
    _ => ("src\\AegisLoop.Api\\AegisLoop.Api.csproj", "API (port 5100)"),
};
Console.WriteLine($"AegisLoop V1 — Lancement {label}...");
var psi = new ProcessStartInfo("dotnet", $"run --project {project}") { UseShellExecute = false };
Process.Start(psi)?.WaitForExit();