using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.UeUseCases.Csv;

public class GenerateCsvNotesUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<byte[]> ExecuteAsync(long idUe)
    {
        var ueRepository = repositoryFactory.UeRepository();
        var ue = await ueRepository.FindUeCompletAsync(idUe);
        if (ue == null) throw new UeNotFoundException();

        var records = new List<NoteCsvDto>();

        // Récupérer tous les étudiants inscrits dans les parcours où l'UE est enseignée
        if (ue.EnseigneeDans != null)
        {
            var etudiantsTries = ue.EnseigneeDans
                .Where(p => p.Inscrits != null)
                .SelectMany(p => p.Inscrits!)
                .DistinctBy(e => e.Id)
                .OrderBy(e => e.Nom)
                .ThenBy(e => e.Prenom)
                .ToList();

            foreach (var etudiant in etudiantsTries)
            {
                // Chercher si une note existe déjà pour cet étudiant dans cette UE
                var noteExistante = ue.Notes?.FirstOrDefault(n => n.EtudiantId == etudiant.Id);
                
                records.Add(new NoteCsvDto
                {
                    NumEtud = etudiant.NumEtud,
                    Nom = etudiant.Nom,
                    Prenom = etudiant.Prenom,
                    Note = noteExistante?.Valeur
                });
            }
        }

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        });

        await csv.WriteRecordsAsync(records);
        await writer.FlushAsync();
        
        return memoryStream.ToArray();
    }
}

public class NoteCsvDto
{
    public string NumEtud { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public float? Note { get; set; }
}
