using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCases.Csv;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomainUnitTests.UeUseCases.Csv;

public class GenerateCsvNotesUseCaseTests
{
    private Mock<IRepositoryFactory> _mockFactory;
    private Mock<IUeRepository> _mockUeRepo;
    private GenerateCsvNotesUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        _mockFactory = new Mock<IRepositoryFactory>();
        _mockUeRepo = new Mock<IUeRepository>();
        _mockFactory.Setup(f => f.UeRepository()).Returns(_mockUeRepo.Object);
        _useCase = new GenerateCsvNotesUseCase(_mockFactory.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnCsv_WhenUeExists()
    {
        // Arrange
        var ueId = 1L;
        var etudiant = new Etudiant { Id = 1, NumEtud = "123", Nom = "Doe", Prenom = "John" };
        var parcours = new Parcours { Id = 1, Inscrits = new List<Etudiant> { etudiant } };
        var ue = new Ue 
        { 
            Id = ueId, 
            EnseigneeDans = new List<Parcours> { parcours },
            Notes = new List<Note>()
        };

        _mockUeRepo.Setup(r => r.FindUeCompletAsync(ueId)).ReturnsAsync(ue);

        // Act
        var result = await _useCase.ExecuteAsync(ueId);

        // Assert
        Assert.That(result, Is.Not.Empty);
        var csvString = System.Text.Encoding.UTF8.GetString(result);
        Assert.That(csvString, Does.Contain("123;Doe;John;"));
    }

    [Test]
    public void ExecuteAsync_ShouldThrow_WhenUeNotFound()
    {
        _mockUeRepo.Setup(r => r.FindUeCompletAsync(It.IsAny<long>())).ReturnsAsync((Ue?)null);
        Assert.ThrowsAsync<UeNotFoundException>(() => _useCase.ExecuteAsync(1));
    }
}
