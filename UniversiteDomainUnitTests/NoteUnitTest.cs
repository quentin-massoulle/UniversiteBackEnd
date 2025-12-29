using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.NoteUseCase.Create;
using UniversiteDomain.Exceptions;
using UniversiteDomain.Exceptions.NoteExeptions;

namespace UniversiteDomainUnitTests;

public class NoteUnitTest
{
    [SetUp]
    public void Setup()
    {
    }
    
    private const long EtudiantId = 100;
    private const long UeId = 200;
    private const long ParcoursId = 300;
    private const float NoteValide = 15;

    [Test]
    public async Task CreateNoteUseCase_NewValidNote_ReturnsSuccessWithCreatedNote()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockUeRepository = new Mock<IUeRepository>();
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepository.Object);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepository.Object);

        var parcours = new Parcours { Id = ParcoursId };
        var ueValide = new Ue { Id = UeId, EnseigneeDans = new List<Parcours> { parcours } }; 
        var etudiantValide = new Etudiant { Id = EtudiantId, ParcoursSuivi = parcours };
        var noteInput = new Note { Valeur = NoteValide, EtudiantId = EtudiantId, UeId = UeId };
        
        // Note simulée qui sera retournée par CreateAsync (avec ID=1)
        var noteCreee = new Note { Valeur = NoteValide, EtudiantId = EtudiantId, UeId = UeId }; 

        // 2. Simulation des règles (Doit passer pour le succès)
        mockUeRepository.Setup(repo => repo.FindAsync(UeId)).ReturnsAsync(ueValide);
        mockEtudiantRepository.Setup(repo => repo.FindAsync(EtudiantId)).ReturnsAsync(etudiantValide);
        
        // Pas de doublon existant
        mockNoteRepository
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note>()); 
        
        // 3. Simulation de la persistance
        mockNoteRepository.Setup(repo => repo.CreateAsync(noteInput)).ReturnsAsync(noteCreee);
        mockFactory.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        // 4. Création du Use Case
        var useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // ACT
        // La méthode ExecuteAsync retourne l'entité Note directement
        var noteTeste = await useCase.ExecuteAsync(noteInput);
        
        // ASSERT
        
        // 1. Vérification que la Note retournée est bien celle qui a été simulée
        Assert.That(noteTeste, Is.Not.Null);
        Assert.That(noteTeste.Valeur, Is.EqualTo(noteCreee.Valeur));
        Assert.That(noteTeste.EtudiantId, Is.EqualTo(noteCreee.EtudiantId));
        
        // 2. Vérification que les méthodes de persistance ont été appelées
        mockNoteRepository.Verify(repo => repo.CreateAsync(It.IsAny<Note>()), Times.Once);
        mockFactory.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Test]
    public async Task CreateNoteUseCase_ExistingNote_ReturnsFailureWithErrorMessage()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockUeRepository = new Mock<IUeRepository>();
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepository.Object);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepository.Object);

        var parcours = new Parcours { Id = ParcoursId };
        var ueValide = new Ue { Id = UeId, EnseigneeDans = new List<Parcours> { parcours } }; 
        var etudiantValide = new Etudiant { Id = EtudiantId, ParcoursSuivi = parcours };
        var noteInput = new Note { Valeur = NoteValide, EtudiantId = EtudiantId, UeId = UeId };
        var noteExistante = new Note {  Valeur = 10, EtudiantId = EtudiantId, UeId = UeId };

        // 2. Simulation de l'existence et de l'éligibilité (Succès)
        mockUeRepository.Setup(repo => repo.FindAsync(UeId)).ReturnsAsync(ueValide);
        mockEtudiantRepository.Setup(repo => repo.FindAsync(EtudiantId)).ReturnsAsync(etudiantValide);
        
        // 3. Simulation de l'existence d'un doublon (Règle d'échec)
        // FindByConditionAsync retourne UNE note existante
        mockNoteRepository
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note> { noteExistante });
        
        // 4. Création du Use Case
        var useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // ACT & ASSERT
        Assert.ThrowsAsync<DuplicateNoteUeException>(() => useCase.ExecuteAsync(noteInput));
        
        // Vérifie que la persistance n'a PAS été appelée
        mockNoteRepository.Verify(repo => repo.CreateAsync(It.IsAny<Note>()), Times.Never);
        mockFactory.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task GetNoteUseCase_ById_Success()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var note = new Note { EtudiantId = EtudiantId, UeId = UeId, Valeur = NoteValide };
        
        mockNoteRepository.Setup(repo => repo.FindAsync(EtudiantId, UeId)).ReturnsAsync(note);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.NoteUseCase.Get.GetNoteUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(EtudiantId, UeId);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Valeur, Is.EqualTo(NoteValide));
    }

    [Test]
    public async Task GetNoteUseCase_All_Success()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var notes = new List<Note> { new Note { EtudiantId = EtudiantId, UeId = UeId, Valeur = NoteValide } };
        
        mockNoteRepository.Setup(repo => repo.FindAllAsync()).ReturnsAsync(notes);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.NoteUseCase.Get.GetNoteUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteNoteUseCase_Success()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var note = new Note { EtudiantId = EtudiantId, UeId = UeId, Valeur = NoteValide };
        
        mockNoteRepository.Setup(repo => repo.FindAsync(EtudiantId, UeId)).ReturnsAsync(note);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.NoteUseCase.Delete.DeleteNoteUseCase(mockFactory.Object);
        
        await useCase.ExecuteAsync(EtudiantId, UeId);
        
        mockNoteRepository.Verify(repo => repo.DeleteAsync(note), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateNoteUseCase_Success()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var note = new Note { EtudiantId = EtudiantId, UeId = UeId, Valeur = 10 };
        
        mockNoteRepository.Setup(repo => repo.FindAsync(EtudiantId, UeId)).ReturnsAsync(note);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.NoteUseCase.Update.UpdateNoteUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(EtudiantId, UeId, 18);
        
        Assert.That(result.Valeur, Is.EqualTo(18));
        mockNoteRepository.Verify(repo => repo.UpdateAsync(note), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateNoteUseCase_InvalidValue_ThrowsException()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var note = new Note { EtudiantId = EtudiantId, UeId = UeId, Valeur = 10 };
        
        mockNoteRepository.Setup(repo => repo.FindAsync(EtudiantId, UeId)).ReturnsAsync(note);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.NoteUseCase.Update.UpdateNoteUseCase(mockFactory.Object);
        
        Assert.ThrowsAsync<InvalidValeurNoteException>(() => useCase.ExecuteAsync(EtudiantId, UeId, 25));
    }
}