using BannerlordSearch.Application.Ports;
using BannerlordSearch.Application.Ports.Configuration;
using BannerlordSearch.Application.Ports.Repositories;
using BannerlordSearch.Application.UseCases;
using BannerlordSearch.Domain;
using BannerlordSearch.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BannerlordSearch.Application.Tests;

public class IndexBannerlordCodeUseCaseTests
{
    [Fact]
    public void CanBeInstantiated()
    {
        var useCase = new IndexBannerlordCodeUseCase(
            new Mock<ICodeIndex>().Object,
            new Mock<IBannerlordSourcePathProvider>().Object,
            NullLogger<IndexBannerlordCodeUseCase>.Instance);
        Assert.NotNull(useCase);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCodeIndexIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IndexBannerlordCodeUseCase(null!, new Mock<IBannerlordSourcePathProvider>().Object, NullLogger<IndexBannerlordCodeUseCase>.Instance));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPathProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IndexBannerlordCodeUseCase(new Mock<ICodeIndex>().Object, null!, NullLogger<IndexBannerlordCodeUseCase>.Instance));
    }

    [Fact]
    public void Execute_CallsGetBannerlordSourceFolderPath()
    {
        var mockIndex = new Mock<ICodeIndex>();
        mockIndex.Setup(ci => ci.Files).Returns(new List<IndexedFile>());
        var mockProvider = new Mock<IBannerlordSourcePathProvider>();
        mockProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("some_path");
        var useCase = new IndexBannerlordCodeUseCase(mockIndex.Object, mockProvider.Object, NullLogger<IndexBannerlordCodeUseCase>.Instance);

        useCase.Execute();

        mockProvider.Verify(p => p.GetBannerlordSourceFolderPath(), Times.Once);
    }

    [Fact]
    public void Execute_CallsEnsureBuilt_WithPathFromProvider()
    {
        var mockIndex = new Mock<ICodeIndex>();
        mockIndex.Setup(ci => ci.Files).Returns(new List<IndexedFile>());
        var mockProvider = new Mock<IBannerlordSourcePathProvider>();
        mockProvider.Setup(p => p.GetBannerlordSourceFolderPath()).Returns("my_root_path");
        var useCase = new IndexBannerlordCodeUseCase(mockIndex.Object, mockProvider.Object, NullLogger<IndexBannerlordCodeUseCase>.Instance);

        useCase.Execute();

        mockIndex.Verify(ci => ci.EnsureBuilt("my_root_path"), Times.Once);
    }
}
