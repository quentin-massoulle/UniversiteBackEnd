using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.UeUseCases.Csv;

public class SaisirNotesUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long idUe, Stream csvStream)
    {
        var ueRepository = repositoryFactory.UeRepository();
        var noteRepository = repositoryFactory.NoteRepository();
        
        var ue = await ueRepository.FindUeCompletAsync(idUe);
        if (ue == null) throw new UeNotFoundException();

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            MissingFieldFound = null,
            HeaderValidated = null
        });

        var records = csv.GetRecords<NoteCsvDto>().ToList();
        
        // Tous les étudiants valides de l'UE (pour validation)
        // Map NumEtud -> EtudiantId pour lookup rapide
        var etudiantsMap = ue.EnseigneeDans?
            .SelectMany(p => p.Inscrits ?? new List<Etudiant>())
            .GroupBy(e => e.NumEtud) // Au cas où un étudiant est dans plusieurs parcours (ne devrait pas arriver, mais safe)
            .ToDictionary(g => g.Key, g => g.First().Id);

        if (etudiantsMap == null) etudiantsMap = new Dictionary<string, long>();

        foreach (var record in records)
        {
            // Validation basique
            if (!etudiantsMap.ContainsKey(record.NumEtud))
            {
               throw new ArgumentException($"L'étudiant {record.NumEtud} n'est pas inscrit dans un parcours contenant cette UE.");
            }

            if (record.Note.HasValue)
            {
                if (record.Note < 0 || record.Note > 20)
                {
                    throw new ArgumentException($"La note pour l'étudiant {record.NumEtud} doit être comprise entre 0 et 20.");
                }

                var studentId = etudiantsMap[record.NumEtud];

                // Vérifier si note existe déjà (mise à jour) ou nouvelle
                var existingNote = ue.Notes?.FirstOrDefault(n => n.EtudiantId == studentId);
                
                if (existingNote != null)
                {
                    existingNote.Valeur = record.Note.Value;
                    // Entity Framework tracks this change via 'ue' object
                }
                else
                {
                    var newNote = new Note
                    {
                        UeId = idUe,
                        EtudiantId = studentId,
                        Valeur = record.Note.Value
                    };
                    // We need to add this new note. Since we have the repository factory, we can use the repo.
                    // But standard way is context.Notes.Add(newNote). Repository usually hides this.
                    // INoteRepository should have CreateAsync or similar? 
                    // Let's rely on INoteRepository.SaveNotesAsync for batch, or just add to keys.
                    // Actually, if we add it to ue.Notes collection, EF might track it if relationship is set up.
                    // But 'ue.Notes' is ICollection. 
                    if (ue.Notes == null) ue.Notes = new List<Note>();
                    ue.Notes.Add(newNote);
                }
            }
        }
        
        // Commit all changes (Updates to existing notes and New notes added to collection)
        await repositoryFactory.SaveChangesAsync();
    }
}
