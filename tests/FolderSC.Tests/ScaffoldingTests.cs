using FolderSC.Core;

namespace FolderSC.Tests;

public class ScaffoldingTests
{
    [Fact]
    public void SolutionAndProjects_ShouldBeCorrectlyConfigured()
    {
        Assert.Equal("FolderSC", FolderSCConstants.ApplicationName);
        Assert.Equal("1.0.0", FolderSCConstants.Version);
    }
}
