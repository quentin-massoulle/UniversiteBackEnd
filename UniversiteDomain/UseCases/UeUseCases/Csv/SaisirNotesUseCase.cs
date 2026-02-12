using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entites;
using UniversiteDomain.Exceptions.UeExeptions;

namespace UniversiteDomain.UseCases.UeUseCases.Csv;

public class SaisirNotesUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(Stream csvStream)
    {
        var ueRepository = repositoryFactory.UeRepository();
        var noteRepository = repositoryFactory.NoteRepository();
        
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            MissingFieldFound = null,
            HeaderValidated = null
        });

        // Lecture des records en mémoire pour traitement
        var records = csv.GetRecords<NoteCsvDto>().ToList();
        
        if (!records.Any()) return; // Rien à faire

        // On prend le NumeroUe du premier enregistrement (on suppose que tout le fichier concerne la même UE)
        var numeroUe = records.First().NumeroUe;
        if (string.IsNullOrEmpty(numeroUe)) throw new ArgumentException("Le Numéro UE est manquant dans le fichier CSV.");

        // Vérification de cohérence (optionnel : on peut jeter une erreur si le fichier contient plusieurs UEs différentes)
        if (records.Any(r => r.NumeroUe != numeroUe))
        {
            throw new ArgumentException("Le fichier CSV doit contenir des notes pour une seule et même UE.");
        }

        var ue = await ueRepository.FindUeCompletByNumeroAsync(numeroUe);
        if (ue == null) throw new UeNotFoundException();

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
               throw new ArgumentException($"L'étudiant {record.NumEtud} n'est pas inscrit dans un parcours contenant cette UE ({numeroUe}).");
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
                        UeId = ue.Id,
                        EtudiantId = studentId,
                        Valeur = record.Note.Value
                    };
                    
                    if (ue.Notes == null) ue.Notes = new List<Note>();
                    ue.Notes.Add(newNote);
                }
            }
        }
        
        // Commit all changes (Updates to existing notes and New notes added to collection)
        await repositoryFactory.SaveChangesAsync();
    }
}
