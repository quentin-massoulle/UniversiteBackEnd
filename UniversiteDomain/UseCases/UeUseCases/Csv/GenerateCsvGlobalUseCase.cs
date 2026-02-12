using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entites;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.UeUseCases.Csv;

public class GenerateCsvGlobalUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<byte[]> ExecuteAsync()
    {
        var ueRepository = repositoryFactory.UeRepository();
        var allUes = await ueRepository.FindAllUesCompletAsync();
        
        var records = new List<NoteCsvDto>();

        foreach (var ue in allUes)
        {
            if (ue.EnseigneeDans == null) continue;

            // Pour éviter les doublons d'étudiants (si inscrit dans plusieurs parcours de la même UE - rare mais possible)
            var etudiantsTraites = new HashSet<long>();

            foreach (var parcours in ue.EnseigneeDans)
            {
                if (parcours.Inscrits == null) continue;

                foreach (var etudiant in parcours.Inscrits)
                {
                    if (etudiantsTraites.Contains(etudiant.Id)) continue;
                    etudiantsTraites.Add(etudiant.Id);

                    // Chercher si une note existe déjà pour cet étudiant dans cette UE
                    var noteExistante = ue.Notes?.FirstOrDefault(n => n.EtudiantId == etudiant.Id);
                    
                    records.Add(new NoteCsvDto()
                    {
                        NumeroUe = ue.NumeroUe,
                        Intitule = ue.Intitule,
                        NumEtud = etudiant.NumEtud,
                        Nom = etudiant.Nom,
                        Prenom = etudiant.Prenom,
                        Note = noteExistante?.Valeur
                    });
                }
            }
        }
        
        // Trier par UE puis par Nom/Prénom
        records = records.OrderBy(r => r.NumeroUe).ThenBy(r => r.Nom).ThenBy(r => r.Prenom).ToList();

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });

        await csv.WriteRecordsAsync(records);
        await writer.FlushAsync();
        
        return memoryStream.ToArray();
    }
}
