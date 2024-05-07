using Apbd_s24439_kl1.Models.DTOs;
using Apbd_s24439_kl1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Apbd_s24439_kl1.Controllers;

[Route("api/books")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;

    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }

    [HttpGet("{id}/authors")]
    public async Task<IActionResult> GetBookAuthors(int id)
    {
        if (!await _booksRepository.DoesBookExists(id))
            return NotFound($"Book with id: {id} not found");

        var bookAuthors = await _booksRepository.GetBookAuthors(id);

        return Ok(bookAuthors);
    }

    [HttpPost]
    public async Task<IActionResult> AddBookWithAuthors(NewBookWithAuthorsDTO newBookWithAuthorsDto)
    {
        await _booksRepository.AddBookWithAuthors(newBookWithAuthorsDto);

        return Created("",newBookWithAuthorsDto);
    }
}