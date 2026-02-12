using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCases.Csv;

namespace UniversiteDomainUnitTests.UeUseCases.Csv;

public class GenerateCsvGlobalUseCaseTests
{
    private Mock<IRepositoryFactory> _mockFactory;
    private Mock<IUeRepository> _mockUeRepo;
    private GenerateCsvGlobalUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        _mockFactory = new Mock<IRepositoryFactory>();
        _mockUeRepo = new Mock<IUeRepository>();
        _mockFactory.Setup(f => f.UeRepository()).Returns(_mockUeRepo.Object);
        _useCase = new GenerateCsvGlobalUseCase(_mockFactory.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnCsv_WithAllUes()
    {
        // Arrange
        var etudiant = new Etudiant { Id = 1, NumEtud = "123", Nom = "Doe", Prenom = "John" };
        var parcours = new Parcours { Id = 1, Inscrits = new List<Etudiant> { etudiant } };
        var ue1 = new Ue 
        { 
            Id = 1, 
            NumeroUe = "UE01",
            Intitule = "Maths",
            EnseigneeDans = new List<Parcours> { parcours },
            Notes = new List<Note> { new Note { EtudiantId = 1, Valeur = 15 } }
        };
        var ue2 = new Ue 
        { 
            Id = 2, 
            NumeroUe = "UE02",
            Intitule = "Physics",
            EnseigneeDans = new List<Parcours> { parcours },
            Notes = new List<Note>()
        };

        _mockUeRepo.Setup(r => r.FindAllUesCompletAsync()).ReturnsAsync(new List<Ue> { ue1, ue2 });

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.That(result, Is.Not.Empty);
        var csvString = System.Text.Encoding.UTF8.GetString(result);
        Assert.That(csvString, Does.Contain("UE01;Maths;123;Doe;John;15"));
        Assert.That(csvString, Does.Contain("UE02;Physics;123;Doe;John;"));
    }
}
