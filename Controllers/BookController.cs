using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

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

        public BookController(IBookService bookService,IMapper mapper,IDistributedCache distributedCache,IConfiguration configuration)
        {
            _bookService = bookService;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }

        [HttpGet("RetrieveBookCollections")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetAllBooks>>> RetrieveAllBooks()
        {
            var cacheKey = _configuration["CacheSettings:RetriveAllBookCacheKey"];
            var cachedResult = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResult))
            {
                var cachedBooks = JsonConvert.DeserializeObject<IEnumerable<GetAllBooks>>(cachedResult);
                return Ok(cachedBooks);
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

            return Ok(mappedBooks);
        }


        [HttpGet("GetAllBooksAvailableForBorrow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetAllBooks>>> GetAllBooksAvailableForBorrowAsync()
        {
            var cacheKey = _configuration["CacheSettings:AllBooksAvailableForBorrowCacheKey"];
            var cachedResult = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResult))
            {
                var cachedBooks = JsonConvert.DeserializeObject<IEnumerable<GetAllBooks>>(cachedResult);
                return Ok(cachedBooks);
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

            return Ok(mappedBooks);
        }



        [HttpGet("{id:int}",Name ="GetBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetAllBooks>> GetBook(int id)
        {
            if (id == 0) 
            {
                return BadRequest();
            }
            var book = await _bookService.GetAsync(u => u.Id == id);
            if (book == null) 
            {
                return NotFound();
            }
            return Ok(_mapper.Map<GetAllBooks>(book));
        }

        [HttpPost("AddBookToCollection")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetAllBooks>> AddABook([FromBody] AddNewBook addNewBook)
        {
            if(!ModelState.IsValid) 
            { 
                return BadRequest(ModelState);
            }
            if (await _bookService.GetAsync(u => u.Title.ToLower() == addNewBook.Title.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Book already part of our Library Collection");
                return BadRequest(ModelState);
            }
            if (addNewBook == null)
            {
                return BadRequest(addNewBook);
            }
            Book book = _mapper.Map<Book>(addNewBook);
            await _bookService.CreateAsync(book);
            return CreatedAtRoute("GetBook",new {id = book.Id},book);
        }


        [HttpPut("UpdateReturnedBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> UpdateReturnedBookAvailabilityStatus(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var book = await _bookService.GetAsync(u => u.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            await _bookService.UpdateIsAvailableStatusAsync(id);
            await _bookService.SaveAsync();
            return NoContent();
        }



        [HttpDelete("RemoveBook")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveBook(int id)
        {
            if(id == 0)
            {
                return BadRequest();
            }
            var book = await _bookService.GetAsync(u =>u.Id == id);
            if (book == null)
            { 
                return NotFound();
            }
            await _bookService.RemoveAsync(book);
            return NoContent();
        }

    }
}
