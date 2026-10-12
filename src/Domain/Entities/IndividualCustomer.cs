namespace Domain.Entities;

public sealed class IndividualCustomer : Customer
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalId { get; set; } = default!; // Tc kimlik numarası
    public DateOnly DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MotherName { get; set; }
    public string? FatherName { get; set; }
}
