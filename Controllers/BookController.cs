using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net;

namespace LibraryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class BookController : ControllerBase
    {
        private IBookService _bookService;
        private IMapper _mapper;
        private IDistributedCache _distributedCache;
        private IConfiguration _configuration;

        public BookController(IBookService bookService, IMapper mapper, IDistributedCache distributedCache, IConfiguration configuration)
        {
            _bookService = bookService;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }

        
        [HttpGet("RetrieveBookCollections")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> RetrieveAllBooks()
        {
            var cacheKey = _configuration["CacheSettings:RetriveAllBookCacheKey"];
            var cachedResult = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResult))
            {
                var cachedBooks = JsonConvert.DeserializeObject<IEnumerable<GetAllBooks>>(cachedResult);
                return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = cachedBooks };
            }

            var bookList = await _bookService.GetAllAsync();
            var mappedBooks = _mapper.Map<IEnumerable<GetAllBooks>>(bookList);

            var cacheDurationMinutes = double.Parse(_configuration["CacheSettings:CacheDurationMinutes"]);
            var slidingExpirationMinutes = double.Parse(_configuration["CacheSettings:SlidingExpirationMinutes"]);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheDurationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpirationMinutes)
            };

            await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(mappedBooks), cacheOptions);

            return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = mappedBooks };
        }


        [HttpGet("GetAllBooksAvailableForBorrow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetAllBooksAvailableForBorrowAsync()
        {
            var cacheKey = _configuration["CacheSettings:AllBooksAvailableForBorrowCacheKey"];
            var cachedResult = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResult))
            {
                var cachedBooks = JsonConvert.DeserializeObject<IEnumerable<GetAllBooks>>(cachedResult);
                return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = cachedBooks };
            }

            var bookList = await _bookService.GetAllBooksAvailableForBorrowAsync();
            var mappedBooks = _mapper.Map<IEnumerable<GetAllBooks>>(bookList);

            var cacheDurationMinutes = double.Parse(_configuration["CacheSettings:CacheDurationMinutes"]);
            var slidingExpirationMinutes = double.Parse(_configuration["CacheSettings:SlidingExpirationMinutes"]);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheDurationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpirationMinutes)
            };

            await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(mappedBooks), cacheOptions);

            return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = mappedBooks };
        }



        [HttpGet("{id:int}", Name = "GetBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetBook(int id)
        {
            if (id == 0)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Invalid Id" } };
            }
            var book = await _bookService.GetAsync(id);
            if (book == null)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.NotFound, IsSuccess = false, ErrorMessages = new List<string> { "Book not found" } };
            }
            var mappedBook = _mapper.Map<GetAllBooks>(book);
            return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = mappedBook };
        }




        [HttpPost("AddBookToCollection")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AddABook([FromBody] AddNewBook addNewBook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Invalid request data" } });
            }
            if (await _bookService.GetByIsbn(addNewBook.Isbn) != null)
            {
                return BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Book already part of our Library Collection" } });
            }
            if (addNewBook == null)
            {
                return BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Invalid request data" } });
            }
            Book book = _mapper.Map<Book>(addNewBook);
            await _bookService.CreateAsync(book);
            var mappedBook = _mapper.Map<GetAllBooks>(book);

            return CreatedAtRoute("GetBook", new { id = book.Id }, new ApiResponse { StatusCode = HttpStatusCode.Created, IsSuccess = true, Result = mappedBook });

        }



        [HttpPut("UpdateReturnedBooks")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Unauthorized)]
        public async Task<ActionResult<ApiResponse>> UpdateReturnedBookAvailabilityStatus(int id)
        {
            if (id == 0)
            {
                return BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Invalid Id" } });
            }
            var book = await _bookService.GetAsync(id);
            if (book == null)
            {
                return NotFound(new ApiResponse { StatusCode = HttpStatusCode.NotFound, IsSuccess = false, ErrorMessages = new List<string> { "Book not found" } });
            }
            await _bookService.UpdateIsAvailableStatusAsync(id);
            await _bookService.SaveAsync();
            return Ok(new ApiResponse { StatusCode = HttpStatusCode.NoContent, IsSuccess = true });

        }

        [HttpDelete("RemoveBook")]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveBook(int id)
        {
            if (id == 0)
            {
                return BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Invalid Id" } });
            }
            var book = await _bookService.GetAsync(id);
            if (book == null)
            {
                return NotFound(new ApiResponse { StatusCode = HttpStatusCode.NotFound, IsSuccess = false, ErrorMessages = new List<string> { "Book not found" } });
            }
            await _bookService.RemoveAsync(book);
            return Ok(new ApiResponse { StatusCode = HttpStatusCode.NoContent, IsSuccess = true });

        }

    }
}
