namespace UniversiteDomain.Exceptions.NoteExeptions;

public class InvalidValeurNoteException : Exception
{
    public InvalidValeurNoteException() : base() { }
    public InvalidValeurNoteException(string message) : base(message) { }
    public InvalidValeurNoteException(string message, Exception inner) : base(message, inner) { }
}