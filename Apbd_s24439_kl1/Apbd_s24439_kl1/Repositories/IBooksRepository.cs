using Apbd_s24439_kl1.Models.DTOs;

namespace Apbd_s24439_kl1.Repositories;

public interface IBooksRepository
{
    Task<bool> DoesBookExists(int id);
    Task<BookWithAuthorsDTO> GetBookAuthors(int id);

    Task AddBookWithAuthors(NewBookWithAuthorsDTO newBookWithAuthorsDto);
}