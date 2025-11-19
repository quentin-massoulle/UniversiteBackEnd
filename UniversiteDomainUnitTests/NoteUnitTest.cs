using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.NoteUseCase.Create;
using UniversiteDomain.Exceptions; 

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

        var parcours = new Parcours { Id = ParcoursId };
        var ueValide = new Ue { Id = UeId, EnseigneeDans = new List<Parcours> { parcours } }; 
        var etudiantValide = new Etudiant { Id = EtudiantId, ParcoursSuivi = parcours };
        var noteInput = new Note { Valeur = NoteValide, EtudiantId = EtudiantId, UeId = UeId };
        
        // Note simulée qui sera retournée par CreateAsync (avec ID=1)
        var noteCreee = new Note { Valeur = NoteValide, EtudiantId = EtudiantId, UeId = UeId }; 

        // 2. Simulation des règles (Doit passer pour le succès)
        mockUeRepository.Setup(repo => repo.GetByIdAsync(UeId)).ReturnsAsync(ueValide);
        mockEtudiantRepository.Setup(repo => repo.GetByIdAsync(EtudiantId)).ReturnsAsync(etudiantValide);
        
        // Pas de doublon existant
        mockNoteRepository
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note>()); 
        
        // 3. Simulation de la persistance
        mockNoteRepository.Setup(repo => repo.CreateAsync(noteInput)).ReturnsAsync(noteCreee);
        mockNoteRepository.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        // 4. Création du Use Case
        var useCase = new CreateNoteUseCase(
            mockNoteRepository.Object, 
            mockUeRepository.Object, 
            mockEtudiantRepository.Object);
        
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
        mockNoteRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    
    }
    
    [Test]
    public async Task CreateNoteUseCase_ExistingNote_ReturnsFailureWithErrorMessage()
    {
        var mockNoteRepository = new Mock<INoteRepository>();
        var mockUeRepository = new Mock<IUeRepository>();
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();

        var parcours = new Parcours { Id = ParcoursId };
        var ueValide = new Ue { Id = UeId, EnseigneeDans = new List<Parcours> { parcours } }; 
        var etudiantValide = new Etudiant { Id = EtudiantId, ParcoursSuivi = parcours };
        var noteInput = new Note { Valeur = NoteValide, EtudiantId = EtudiantId, UeId = UeId };
        var noteExistante = new Note {  Valeur = 10, EtudiantId = EtudiantId, UeId = UeId };

        // 2. Simulation de l'existence et de l'éligibilité (Succès)
        mockUeRepository.Setup(repo => repo.GetByIdAsync(UeId)).ReturnsAsync(ueValide);
        mockEtudiantRepository.Setup(repo => repo.GetByIdAsync(EtudiantId)).ReturnsAsync(etudiantValide);
        
        // 3. Simulation de l'existence d'un doublon (Règle d'échec)
        // FindByConditionAsync retourne UNE note existante
        mockNoteRepository
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note> { noteExistante });
        
        // 4. Création du Use Case
        var useCase = new CreateNoteUseCase(mockNoteRepository.Object, mockUeRepository.Object, mockEtudiantRepository.Object);
        
        // ACT & ASSERT
        
        
        // Vérifie que la persistance n'a PAS été appelée
        mockNoteRepository.Verify(repo => repo.CreateAsync(It.IsAny<Note>()), Times.Never);
        mockNoteRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}