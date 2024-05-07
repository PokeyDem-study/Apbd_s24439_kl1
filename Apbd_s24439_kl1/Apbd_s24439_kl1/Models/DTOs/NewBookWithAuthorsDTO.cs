namespace Apbd_s24439_kl1.Models.DTOs;

public class NewBookWithAuthorsDTO
{
    public string Title { get; set; }
    public List<NewBookAuthorDTO> Authors { get; set; } = null!;
}

public class NewBookAuthorDTO
{
    public string? FirstName { get; set; }
    public string LastName { get; set; }
}