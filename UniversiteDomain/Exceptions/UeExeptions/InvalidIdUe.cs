namespace UniversiteDomain.Exceptions.UeExeptions;

public class InvalidIdUe: Exception 
{
    public InvalidIdUe() : base() { }
    public InvalidIdUe(string message) : base(message) { }
    public InvalidIdUe(string message, Exception inner) : base(message, inner) { }
}