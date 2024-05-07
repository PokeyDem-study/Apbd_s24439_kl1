using Apbd_s24439_kl1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Apbd_s24439_kl1.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExists(int id)
    {
        var query = "SELECT 1 FROM Books WHERE PK = @Id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<BookWithAuthorsDTO> GetBookAuthors(int id)
    {
        var query =
            "SELECT b.PK AS BookId, b.title AS BookTitle, a.first_name AS AuthorFirstName, a.last_name AS AuthorLastName " +
            "FROM books b " +
            "JOIN books_authors ba ON b.PK = ba.FK_book " +
            "JOIN authors a ON ba.FK_author = a.PK " +
            "WHERE b.PK = @Id";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var bookIdOrdinal = reader.GetOrdinal("BookId");
        var bookTitleOrdinal = reader.GetOrdinal("BookTitle");
        var authorFirstNameOrdinal = reader.GetOrdinal("AuthorFirstName");
        var authorLastNameOrdinal = reader.GetOrdinal("AuthorLastName");

        BookWithAuthorsDTO bookWithAuthorsDto = null;

        while (await reader.ReadAsync())
        {
            if (bookWithAuthorsDto is not null)
            {
                bookWithAuthorsDto.Authors.Add(new AuthorDTO()
                {
                    FirstName = reader.GetString(authorFirstNameOrdinal),
                    LastName = reader.GetString(authorLastNameOrdinal)
                });
            }
            else
            {
                bookWithAuthorsDto = new BookWithAuthorsDTO()
                {
                    Id = reader.GetInt32(bookIdOrdinal),
                    Title = reader.GetString(bookTitleOrdinal),
                    Authors = new List<AuthorDTO>()
                    {
                        new AuthorDTO()
                        {
                            FirstName = reader.GetString(authorFirstNameOrdinal),
                            LastName = reader.GetString(authorLastNameOrdinal)
                        }
                    }
                };
            }
        }

        return bookWithAuthorsDto;
    }

    public async Task AddBookWithAuthors(NewBookWithAuthorsDTO newBookWithAuthorsDto)
    {
        var query = "INSERT INTO books VALUES (@Title) SELECT @@IDENTITY AS BookId";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;

        command.Parameters.AddWithValue("@Title", newBookWithAuthorsDto.Title);

        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            var bookId = await command.ExecuteScalarAsync();

            foreach (var author in newBookWithAuthorsDto.Authors)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO authors VALUES(@FirstName, @LastName) SELECT @@IDENTITY AS AuthorId";
                command.Parameters.AddWithValue("@FirstName", author.FirstName);
                command.Parameters.AddWithValue("@LastName", author.LastName);

                var authorId = await command.ExecuteScalarAsync();

                command.Parameters.Clear();
                command.CommandText = "INSERT INTO books_authors VALUES(@BookId, @AuthorId)";
                command.Parameters.AddWithValue("@BookId", bookId);
                command.Parameters.AddWithValue("@AuthorId", authorId);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}