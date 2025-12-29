using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;

namespace UniversiteDomainUnitTests;

public class EtudiantUnitTest
{
    [SetUp]
    public void Setup()
    {
    }
    [Test]
    public async Task CreateEtudiantUseCase_Success()
    {
        // ARRANGE
        long id = 1;
        String numEtud = "et1";
        string nom = "Durant";
        string prenom = "Jean";
        string email = "jean.durant@etud.u-picardie.fr";
        
        // On crée l'étudiant qui doit être ajouté en base
        Etudiant etudiantSansId = new Etudiant{NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        
        // 1. Définition des Mocks
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();

        // Données de réponse attendues
        var reponseVierge = new List<Etudiant>();
        Etudiant etudiantCree = new Etudiant{Id=id, NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        
        // 2. Simulation de la fonction FindByConditionAsync (Deux appels sont faits dans le Use Case)
        // On dit au mock que l'étudiant n'existe ni par NumEtud ni par Email.
        // Quel que soit le paramètre de la fonction, on renvoie la liste vide.
        mockEtudiantRepository
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(reponseVierge);
        
        // 3. Simulation de la fonction CreateAsync
        mockEtudiantRepository
            .Setup(repoEtudiant => repoEtudiant.CreateAsync(etudiantSansId))
            .ReturnsAsync(etudiantCree);
        
        // 4. Simulation de la sauvegarde
        // CORRECTION : Supprimer l'anti-pattern .Wait() du Use Case et simuler la tâche terminée.
        mockFactory.Setup(facto => facto.SaveChangesAsync()).Returns(Task.CompletedTask); 

        // 5. Configuration de la Factory (Le cœur de la correction pour ce style de code)
        // On dit à la Factory de retourner le mock EtudiantRepository quand il est demandé.
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiantRepository.Object);
        
        // Création du use case en injectant la Factory
        CreateEtudiantUseCase useCase = new CreateEtudiantUseCase(mockFactory.Object);
        
        // ACT
        // Appel du use case
        var etudiantTeste = await useCase.ExecuteAsync(etudiantSansId);
        
        // ASSERT
        
        // Vérification que les Repositories ont été utilisés comme attendu
        
        // On doit vérifier que FindByCondition a été appelé deux fois (une fois pour NumEtud, une fois pour Email)
        mockEtudiantRepository.Verify(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()), Times.Exactly(2), "La vérification d'unicité (NumEtud et Email) doit être faite deux fois.");
        
        // On vérifie que CreateAsync a été appelé une seule fois
        mockEtudiantRepository.Verify(repo => repo.CreateAsync(etudiantSansId), Times.Once, "La création doit être appelée une fois.");
        
        // Vérification du résultat (L'entité retournée doit correspondre à la simulation)
        Assert.That(etudiantTeste.Id, Is.EqualTo(etudiantCree.Id));
        Assert.That(etudiantTeste.NumEtud, Is.EqualTo(etudiantCree.NumEtud));
        Assert.That(etudiantTeste.Nom, Is.EqualTo(etudiantCree.Nom));
        Assert.That(etudiantTeste.Prenom, Is.EqualTo(etudiantCree.Prenom));
        Assert.That(etudiantTeste.Email, Is.EqualTo(etudiantCree.Email));
    }

    [Test]
    public async Task GetEtudiantUseCase_ById_Success()
    {
        long id = 1;
        Etudiant etudiant = new Etudiant { Id = id, NumEtud = "e1", Nom = "Nom", Prenom = "Prenom", Email = "email@test.com" };
        
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockEtudiantRepository.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(etudiant);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.EtudiantUseCases.Get.GetEtudiantUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(id);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task GetEtudiantUseCase_ById_NotFound()
    {
        long id = 1;
        
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockEtudiantRepository.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync((Etudiant?)null);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.EtudiantUseCases.Get.GetEtudiantUseCase(mockFactory.Object);
        
        Assert.ThrowsAsync<UniversiteDomain.Exceptions.EtudiantExceptions.EtudiantNotFoundException>(() => useCase.ExecuteAsync(id));
    }

    [Test]
    public async Task DeleteEtudiantUseCase_Success()
    {
        long id = 1;
        Etudiant etudiant = new Etudiant { Id = id };
        
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockEtudiantRepository.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(etudiant);
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.EtudiantUseCases.Delete.DeleteEtudiantUseCase(mockFactory.Object);
        
        await useCase.ExecuteAsync(id);
        
        mockEtudiantRepository.Verify(repo => repo.DeleteAsync(id), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateEtudiantUseCase_Success()
    {
        long id = 1;
        Etudiant etudiant = new Etudiant { Id = id, NumEtud = "old", Nom = "Old", Prenom = "Old", Email = "old@test.com" };
        
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockEtudiantRepository.Setup(repo => repo.FindAsync(id)).ReturnsAsync(etudiant);
        mockEtudiantRepository.Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant>()); // No duplicates
            
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.EtudiantUseCases.Update.UpdateEtudiantUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(id, "new", "NewNom", "NewPrenom", "new@test.com");
        
        Assert.That(result.NumEtud, Is.EqualTo("new"));
        mockEtudiantRepository.Verify(repo => repo.UpdateAsync(etudiant), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }
}