using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.ParcoursExeptions;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;
using UniversiteDomain.UseCases.UeUseCase.Update;

namespace UniversiteDomainUnitTests;

public class UeUnitTest
{
    private Mock<IRepositoryFactory> _repositoryFactoryMock;
    private Mock<IParcoursRepository> _parcoursRepositoryMock;
    private Mock<IUeRepository> _ueRepositoryMock;
    private AddUeDansParcoursUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        _repositoryFactoryMock = new Mock<IRepositoryFactory>();
        _parcoursRepositoryMock = new Mock<IParcoursRepository>();
        _ueRepositoryMock = new Mock<IUeRepository>();

        _repositoryFactoryMock.Setup(f => f.ParcoursRepository()).Returns(_parcoursRepositoryMock.Object);
        _repositoryFactoryMock.Setup(f => f.UeRepository()).Returns(_ueRepositoryMock.Object);

        _useCase = new AddUeDansParcoursUseCase(_repositoryFactoryMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldAddUe_WhenValidData()
    {
        // Arrange
        var parcours = new Parcours { Id = 1, NomParcours = "Informatique", AnneeFormation = 2 };
        var ue = new Ue { Id = 10, NumeroUe = "UE101", Intitule = "Programmation" };

        _ueRepositoryMock.Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        _parcoursRepositoryMock.Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcours });

        _parcoursRepositoryMock.Setup(r => r.AddUeAsync(parcours.Id, ue.Id))
            .ReturnsAsync(parcours);

        // Act
        var result = await _useCase.ExecuteAsync(parcours.Id, ue.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(parcours.Id));
        _parcoursRepositoryMock.Verify(r => r.AddUeAsync(parcours.Id, ue.Id), Times.Once);
    }
  

    [Test]
    public void ExecuteAsync_ShouldThrow_DuplicateUeDansParcoursException_WhenUeAlreadyInParcours()
    {
        // Arrange
        var ue = new Ue { Id = 10, NumeroUe = "UE101", Intitule = "Programmation" };
        var parcours = new Parcours
        {
            Id = 1,
            NomParcours = "Informatique",
            AnneeFormation = 2,
            UesEnseignees = new List<Ue> { new Ue { Id = 10 } } // déjà présente
        };

        _ueRepositoryMock.Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        _parcoursRepositoryMock.Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcours });

        // Act + Assert
        Assert.ThrowsAsync<DuplicateUeDansParcoursException>(() => _useCase.ExecuteAsync(parcours.Id, ue.Id));
    }
    [Test]
    public async Task ExecuteAsync_ShouldAddUeList_WhenValidData()
    {
        // Arrange
        var parcours = new Parcours { Id = 1, NomParcours = "Informatique", AnneeFormation = 2 };
        var ues = new List<Ue>
        {
            new Ue { Id = 10, NumeroUe = "UE101", Intitule = "Programmation" },
            new Ue { Id = 11, NumeroUe = "UE102", Intitule = "Base de données" }
        };
        long[] ueIds = ues.Select(u => u.Id).ToArray();

        foreach (var ue in ues)
        {
            _ueRepositoryMock.Setup(r => r.FindByConditionAsync(It.Is<Expression<Func<Ue, bool>>>(expr => expr.Compile()(ue))))
                .ReturnsAsync(new List<Ue> { ue });
        }

        _parcoursRepositoryMock.Setup(r => r.FindByConditionAsync(It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours> { parcours });

        _parcoursRepositoryMock.Setup(r => r.AddUeAsync(parcours.Id, ueIds))
            .ReturnsAsync(parcours);

        // Act
        var result = await _useCase.ExecuteAsync(parcours.Id, ueIds);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(parcours.Id));
        _parcoursRepositoryMock.Verify(r => r.AddUeAsync(parcours.Id, ueIds), Times.Once);
    }
}

public class AddParcoursToUeUnitTest
{
    private Mock<IRepositoryFactory> _repositoryFactoryMock;
    private Mock<IUeRepository> _ueRepositoryMock;
    private AddParcoursToUeUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        _repositoryFactoryMock = new Mock<IRepositoryFactory>();
        _ueRepositoryMock = new Mock<IUeRepository>();

        _repositoryFactoryMock.Setup(f => f.UeRepository()).Returns(_ueRepositoryMock.Object);

        _useCase = new AddParcoursToUeUseCase(_repositoryFactoryMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldAddParcours_WhenValidData()
    {
        // Arrange
        long idUe = 1;
        long idParcours = 10;
        var ue = new Ue { Id = idUe };

        _ueRepositoryMock.Setup(r => r.AddParcoursAsync(idUe, idParcours))
            .ReturnsAsync(ue);

        // Act
        var result = await _useCase.ExecuteAsync(idUe, idParcours);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(idUe));
        _ueRepositoryMock.Verify(r => r.AddParcoursAsync(idUe, idParcours), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldAddParcoursList_WhenValidData()
    {
        // Arrange
        long idUe = 1;
        long[] idParcours = { 10, 11 };
        var ue = new Ue { Id = idUe };

        _ueRepositoryMock.Setup(r => r.AddParcoursAsync(idUe, idParcours))
            .ReturnsAsync(ue);

        // Act
        var result = await _useCase.ExecuteAsync(idUe, idParcours);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(idUe));
        _ueRepositoryMock.Verify(r => r.AddParcoursAsync(idUe, idParcours), Times.Once);
    }
}

public class CreateUeUnitTest
{
    [Test]
    public async Task CreateUeUseCase_Success()
    {
        var mockUeRepository = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        var ue = new Ue { NumeroUe = "UE1", Intitule = "Intitule" };
        
        mockUeRepository.Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());
        mockUeRepository.Setup(repo => repo.CreateAsync(It.IsAny<Ue>())).ReturnsAsync(ue);
        
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.UeUseCases.Create.CreateUeUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync("UE1", "Intitule");
        
        Assert.That(result, Is.Not.Null);
        mockUeRepository.Verify(repo => repo.CreateAsync(It.IsAny<Ue>()), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }
}

public class GetUeUnitTest
{
    [Test]
    public async Task GetUeUseCase_ById_Success()
    {
        long id = 1;
        var ue = new Ue { Id = id, NumeroUe = "UE1", Intitule = "Intitule" };
        
        var mockUeRepository = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockUeRepository.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(ue);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepository.Object);
        
        var useCase = new UniversiteDomain.UseCases.UeUseCase.Get.GetUeUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(id);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
    }
}

public class DeleteUeUnitTest
{
    [Test]
    public async Task DeleteUeUseCase_Success()
    {
        long id = 1;
        
        var mockUeRepository = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.UeUseCase.Delete.DeleteUeUseCase(mockFactory.Object);
        
        await useCase.ExecuteAsync(id);
        
        mockUeRepository.Verify(repo => repo.DeleteAsync(id), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }
}

public class UpdateUeUnitTest
{
    [Test]
    public async Task UpdateUeUseCase_Success()
    {
        long id = 1;
        var ue = new Ue { Id = id, NumeroUe = "Old", Intitule = "Old" };
        
        var mockUeRepository = new Mock<IUeRepository>();
        var mockFactory = new Mock<IRepositoryFactory>();
        
        mockUeRepository.Setup(repo => repo.FindAsync(id)).ReturnsAsync(ue);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepository.Object);
        mockFactory.Setup(f => f.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var useCase = new UniversiteDomain.UseCases.UeUseCase.Update.UpdateUeUseCase(mockFactory.Object);
        
        var result = await useCase.ExecuteAsync(id, "New", "NewIntitule");
        
        Assert.That(result.NumeroUe, Is.EqualTo("New"));
        mockUeRepository.Verify(repo => repo.UpdateAsync(ue), Times.Once);
        mockFactory.Verify(f => f.SaveChangesAsync(), Times.Once);
    }
}
