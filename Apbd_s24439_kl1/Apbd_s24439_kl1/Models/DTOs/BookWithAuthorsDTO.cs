namespace Apbd_s24439_kl1.Models.DTOs;

public class BookWithAuthorsDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<AuthorDTO> Authors { get; set; } = null!;
}

public class AuthorDTO
{
    public string? FirstName { get; set; }
    public string LastName { get; set; }
}