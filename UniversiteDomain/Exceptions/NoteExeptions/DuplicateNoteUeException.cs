namespace UniversiteDomain.Exceptions.NoteExeptions;

public class DuplicateNoteUeException: Exception
{
    public DuplicateNoteUeException() : base() { }
    public DuplicateNoteUeException(string message) : base(message) { }
    public DuplicateNoteUeException(string message, Exception inner) : base(message, inner) { }
}