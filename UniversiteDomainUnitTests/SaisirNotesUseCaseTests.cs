using System.Text;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.UeUseCases.Csv;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomainUnitTests.UeUseCases.Csv;

public class SaisirNotesUseCaseTests
{
    private Mock<IRepositoryFactory> _mockFactory;
    private Mock<IUeRepository> _mockUeRepo;
    private Mock<INoteRepository> _mockNoteRepo;
    private SaisirNotesUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        _mockFactory = new Mock<IRepositoryFactory>();
        _mockUeRepo = new Mock<IUeRepository>();
        _mockNoteRepo = new Mock<INoteRepository>();
        
        _mockFactory.Setup(f => f.UeRepository()).Returns(_mockUeRepo.Object);
        _mockFactory.Setup(f => f.NoteRepository()).Returns(_mockNoteRepo.Object);
        _mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);

        _useCase = new SaisirNotesUseCase(_mockFactory.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSaveNotes_WhenValidCsv()
    {
        // Arrange
        var ueNumero = "UE01";
        var etudiant = new Etudiant { Id = 10, NumEtud = "E123", Nom = "Doe", Prenom = "John" };
        var parcours = new Parcours { Id = 1, Inscrits = new List<Etudiant> { etudiant } };
        var ue = new Ue 
        { 
            Id = 1,
            NumeroUe = ueNumero,
            Intitule = "Maths", 
            EnseigneeDans = new List<Parcours> { parcours },
            Notes = new List<Note>() 
        };

        _mockUeRepo.Setup(r => r.FindUeCompletByNumeroAsync(ueNumero)).ReturnsAsync(ue);

        var csvContent = "NumeroUe;Intitule;NumEtud;Nom;Prenom;Note\nUE01;Maths;E123;Doe;John;15";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        await _useCase.ExecuteAsync(stream);

        // Assert
        Assert.That(ue.Notes.Count, Is.EqualTo(1));
        Assert.That(ue.Notes.First().Valeur, Is.EqualTo(15));
        _mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void ExecuteAsync_ShouldThrow_WhenNoteInvalid()
    {
        // Arrange
        var ueNumero = "UE01";
        var etudiant = new Etudiant { Id = 10, NumEtud = "E123" };
        var parcours = new Parcours { Id = 1, Inscrits = new List<Etudiant> { etudiant } };
        var ue = new Ue { Id = 1, NumeroUe = ueNumero, EnseigneeDans = new List<Parcours> { parcours } };

        _mockUeRepo.Setup(r => r.FindUeCompletByNumeroAsync(ueNumero)).ReturnsAsync(ue);

        var csvContent = "NumeroUe;Intitule;NumEtud;Nom;Prenom;Note\nUE01;Maths;E123;Doe;John;25"; // Note > 20
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(stream));
        Assert.That(ex.Message, Does.Contain("0 et 20"));
    }

    [Test]
    public void ExecuteAsync_ShouldThrow_WhenUeMismatch()
    {
         // Arrange
        var ueNumero = "UE01";
        var ue = new Ue { Id = 1, NumeroUe = ueNumero };

        _mockUeRepo.Setup(r => r.FindUeCompletByNumeroAsync(ueNumero)).ReturnsAsync(ue);

        // CSV asks for UE02 but we loaded UE01 (simulated via repository mismatch or pure logic check)
        // Actually the logic is: read CSV -> get UE02 -> load UE02.
        // If CSV contains mixed UEs, it throws.
        var csvContent = "NumeroUe;Intitule;NumEtud;Nom;Prenom;Note\nUE01;Maths;E123;Doe;John;15\nUE02;Phys;E124;B;A;10";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(stream));
        Assert.That(ex.Message, Does.Contain("une seule et même UE"));
    }
}
