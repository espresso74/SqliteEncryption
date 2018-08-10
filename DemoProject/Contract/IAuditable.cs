namespace DemoProject.Contract
{
    public interface IAuditable
    {
        string CreatedBy { get; }
        string CreatedOn { get; }
        string ModifiedBy { get; }
        string ModifiedOn { get; }
    }
}
