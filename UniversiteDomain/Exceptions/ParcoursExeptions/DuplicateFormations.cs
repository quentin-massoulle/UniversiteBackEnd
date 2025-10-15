namespace UniversiteDomain.Exceptions.ParcoursExeptions;

public class DuplicateFormations : Exception
{
    public DuplicateFormations() : base() { }
    public DuplicateFormations(string message) : base(message) { }
    public DuplicateFormations(string message, Exception inner) : base(message, inner) { }
}