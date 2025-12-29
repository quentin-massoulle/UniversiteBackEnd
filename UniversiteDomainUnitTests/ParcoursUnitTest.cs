using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.UseCases.EtudiantUseCases;
using UniversiteDomain.UseCases.ParcoursUseCase;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;

namespace UniversiteDomainUnitTests;

public class ParcoursUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateParcoursUseCase()
    {
        long idParcours = 1;
        String nomParcours = "Ue 1";
        int anneFormation = 2;
        
        // On crée le parcours qui doit être ajouté en base
        Parcours parcoursAvant = new Parcours{NomParcours = nomParcours, AnneeFormation = anneFormation};
        
        // On initialise une fausse datasource qui va simuler un EtudiantRepository
        var mockParcours = new Mock<IParcoursRepository>();
        
        // Il faut ensuite aller dans le use case pour simuler les appels des fonctions vers la datasource
        // Nous devons simuler FindByCondition et Create
        // On dit à ce mock que le parcours n'existe pas déjà
        mockParcours
            .Setup(repo=>repo.FindByConditionAsync(p=>p.Id.Equals(idParcours)))
            .ReturnsAsync((List<Parcours>)null);
        // On lui dit que l'ajout d'un étudiant renvoie un étudiant avec l'Id 1
        Parcours parcoursFinal =new Parcours{Id=idParcours,NomParcours= nomParcours, AnneeFormation = anneFormation};
        mockParcours.Setup(repo=>repo.CreateAsync(parcoursAvant)).ReturnsAsync(parcoursFinal);
        
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto=>facto.ParcoursRepository()).Returns(mockParcours.Object);
        
        // Création du use case en utilisant le mock comme datasource
        CreateParcoursUseCase useCase=new CreateParcoursUseCase(mockFactory.Object);
        
        // Appel du use case
        var parcoursTeste=await useCase.ExecuteAsync(parcoursAvant);
        
        // Vérification du résultat
        Assert.That(parcoursTeste.Id, Is.EqualTo(parcoursFinal.Id));
        Assert.That(parcoursTeste.NomParcours, Is.EqualTo(parcoursFinal.NomParcours));
        Assert.That(parcoursTeste.AnneeFormation, Is.EqualTo(parcoursFinal.AnneeFormation));
    }
    
    [Test]
    public async Task AddEtudiantDansParcoursUseCase()
    {
        long idEtudiant = 1;
        long idParcours = 3;
        Etudiant etudiant= new Etudiant { Id = 1, NumEtud = "1", Nom = "nom1", Prenom = "prenom1", Email = "1" };
        Parcours parcours = new Parcours{Id=3, NomParcours = "Ue 3", AnneeFormation = 1};
        
        // On initialise des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockParcours = new Mock<IParcoursRepository>();
        List<Etudiant> etudiants = new List<Etudiant>();
        etudiants.Add(new Etudiant{Id=1});
        mockEtudiant
            .Setup(repo=>repo.FindByConditionAsync(e=>e.Id.Equals(idEtudiant)))
            .ReturnsAsync(etudiants);

        List<Parcours> parcourses = new List<Parcours>();
        parcourses.Add(parcours);
        
        List<Parcours> parcoursFinaux = new List<Parcours>();
        Parcours parcoursFinal = parcours;
        parcoursFinal.Inscrits.Add(etudiant);
        parcoursFinaux.Add(parcours);
        
        mockParcours
            .Setup(repo=>repo.FindByConditionAsync(e=>e.Id.Equals(idParcours)))
            .ReturnsAsync(parcourses);
        mockParcours
            .Setup(repo => repo.AddEtudiantAsync(idParcours, idEtudiant))
            .ReturnsAsync(parcoursFinal);
        
        // Création d'une fausse factory qui contient les faux repositories
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto=>facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        mockFactory.Setup(facto=>facto.ParcoursRepository()).Returns(mockParcours.Object);
        
        // Création du use case en utilisant le mock comme datasource
        AddEtudiantDansParcoursUseCase useCase=new AddEtudiantDansParcoursUseCase(mockFactory.Object);
        
        // Appel du use case
        var parcoursTest=await useCase.ExecuteAsync(idParcours, idEtudiant);
        // Vérification du résultat
        Assert.That(parcoursTest.Id, Is.EqualTo(parcoursFinal.Id));
        Assert.That(parcoursTest.Inscrits, Is.Not.Null);
        Assert.That(parcoursTest.Inscrits.Count, Is.EqualTo(1));
        Assert.That(parcoursTest.Inscrits[0].Id, Is.EqualTo(idEtudiant));
    }
    [Test]
    public async Task AddEtudiantDansParcoursUseCase_List()
    {
        long idParcours = 3;
        Parcours parcours = new Parcours{Id=3, NomParcours = "Ue 3", AnneeFormation = 1};
        List<Etudiant> etudiantsToAdd = new List<Etudiant>
        {
            new Etudiant { Id = 1, NumEtud = "1", Nom = "nom1", Prenom = "prenom1", Email = "1" },
            new Etudiant { Id = 2, NumEtud = "2", Nom = "nom2", Prenom = "prenom2", Email = "2" }
        };
        long[] idEtudiants = etudiantsToAdd.Select(e => e.Id).ToArray();

        // On initialise des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockParcours = new Mock<IParcoursRepository>();
        
        // Setup FindByCondition for each student
        foreach (var etudiant in etudiantsToAdd)
        {
            mockEtudiant
                .Setup(repo => repo.FindByConditionAsync(e => e.Id.Equals(etudiant.Id)))
                .ReturnsAsync(new List<Etudiant> { etudiant });
            
            // Setup verification that student is not already in parcours
            mockEtudiant
                .Setup(repo => repo.FindByConditionAsync(e => e.Id.Equals(etudiant.Id) && e.ParcoursSuivi.Id.Equals(idParcours)))
                .ReturnsAsync(new List<Etudiant>());
        }

        List<Parcours> parcourses = new List<Parcours> { parcours };
        
        mockParcours
            .Setup(repo => repo.FindByConditionAsync(p => p.Id.Equals(idParcours)))
            .ReturnsAsync(parcourses);

        Parcours parcoursFinal = parcours;
        parcoursFinal.Inscrits.AddRange(etudiantsToAdd);

        mockParcours
            .Setup(repo => repo.AddEtudiantAsync(idParcours, idEtudiants))
            .ReturnsAsync(parcoursFinal);
        
        // Création d'une fausse factory qui contient les faux repositories
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        mockFactory.Setup(facto => facto.ParcoursRepository()).Returns(mockParcours.Object);
        
        // Création du use case en utilisant le mock comme datasource
        AddEtudiantDansParcoursUseCase useCase = new AddEtudiantDansParcoursUseCase(mockFactory.Object);
        
        // Appel du use case
        var parcoursTest = await useCase.ExecuteAsync(idParcours, idEtudiants);
        
        // Vérification du résultat
        Assert.That(parcoursTest.Id, Is.EqualTo(parcoursFinal.Id));
        Assert.That(parcoursTest.Inscrits, Is.Not.Null);
        Assert.That(parcoursTest.Inscrits.Count, Is.EqualTo(2));
        
        // Verify AddEtudiantAsync was called with the array
        mockParcours.Verify(repo => repo.AddEtudiantAsync(idParcours, idEtudiants), Times.Once);
    }

    [Test]
    public async Task GetParcoursUseCase_ById_Success()
    {
        long id = 1;
        Parcours parcours = new Parcours { Id = id, NomParcours = "P1", AnneeFormation = 1 };
        
        var mockParcoursRepository = new Mock<IParcoursRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockParcoursRepository.Setup(repo => repo.FindAsync(id)).ReturnsAsync(parcours);
        mockFactory.Setup(f => f.ParcoursRepository()).Returns(mockParcoursRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.ParcoursUseCase.Get.GetParcoursUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(id);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task GetParcoursUseCase_All_Success()
    {
        var mockParcoursRepository = new Mock<IParcoursRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var parcoursList = new List<Parcours> { new Parcours { Id = 1, NomParcours = "P1", AnneeFormation = 1 } };
        
        mockParcoursRepository.Setup(repo => repo.FindAllAsync()).ReturnsAsync(parcoursList);
        mockFactory.Setup(f => f.ParcoursRepository()).Returns(mockParcoursRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.ParcoursUseCase.Get.GetParcoursUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteParcoursUseCase_Success()
    {
        long id = 1;
        
        var mockParcoursRepository = new Mock<IParcoursRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockFactory.Setup(f => f.ParcoursRepository()).Returns(mockParcoursRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.ParcoursUseCase.Delete.DeleteParcoursUseCase(mockFactory.Object);
        
        await useCase.ExecuteAsync(id);
        
        mockParcoursRepository.Verify(repo => repo.DeleteAsync(id), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateParcoursUseCase_Success()
    {
        long id = 1;
        Parcours parcours = new Parcours { Id = id, NomParcours = "Old", AnneeFormation = 1 };
        
        var mockParcoursRepository = new Mock<IParcoursRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockParcoursRepository.Setup(repo => repo.FindAsync(id)).ReturnsAsync(parcours);
        mockFactory.Setup(f => f.ParcoursRepository()).Returns(mockParcoursRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.ParcoursUseCase.Update.UpdateParcoursUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(id, "New", 2);
        
        Assert.That(result.NomParcours, Is.EqualTo("New"));
        Assert.That(result.AnneeFormation, Is.EqualTo(2));
        mockParcoursRepository.Verify(repo => repo.UpdateAsync(parcours), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }
}