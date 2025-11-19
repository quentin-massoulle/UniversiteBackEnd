namespace UniversiteDomain.Exceptions.NoteExeptions;

public class InvalidUeEtudiantExeption: Exception
{
    public InvalidUeEtudiantExeption() : base() { }
    public InvalidUeEtudiantExeption(string message) : base(message) { }
    public InvalidUeEtudiantExeption(string message, Exception inner) : base(message, inner) { }
}