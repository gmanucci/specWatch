using SpecWatch.Core.Reporting;

namespace SpecWatch.Core.Tests.Reporting;

public class ReportWriterTests
{
    [Fact]
    public void Serialize_ProducesCamelCaseJsonWithSummary()
    {
        var report = new UpdateReport
        {
            ManifestPath = "specwatch.yml",
            StartedAt = DateTimeOffset.Parse("2026-06-06T10:00:00Z"),
            FinishedAt = DateTimeOffset.Parse("2026-06-06T10:01:00Z"),
            Apis =
            [
                new ApiUpdateResult { Name = "payments", Changed = true, Generator = "kiota", GenerationSucceeded = true },
                new ApiUpdateResult { Name = "inventory", Changed = false, Generator = "nswag" },
            ],
        };
        report.RecomputeSummary();

        var json = new ReportWriter().Serialize(report);

        Assert.Contains("\"manifestPath\"", json);
        Assert.Contains("\"changedApis\": 1", json);
        Assert.Contains("\"generatedClients\": 1", json);
        Assert.Contains("\"totalApis\": 2", json);
    }

    [Fact]
    public void WriteToFile_CreatesFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "specwatch-report-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = Path.Combine(dir, "nested", "report.json");
            var report = new UpdateReport { ManifestPath = "specwatch.yml" };
            report.RecomputeSummary();

            new ReportWriter().WriteToFile(report, path);

            Assert.True(File.Exists(path));
            Assert.Contains("manifestPath", File.ReadAllText(path));
        }
        finally
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
