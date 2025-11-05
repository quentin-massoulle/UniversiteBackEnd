using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.ParcoursExeptions;
using UniversiteDomain.Exceptions.UeExeptions;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

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
}
