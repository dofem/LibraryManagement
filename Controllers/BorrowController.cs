using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.ExceptionHandler;
using LibraryManagement.Services;
using LibraryManagement.Services.Implementation;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace LibraryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private IBorrowService _borrowService;
        private IMapper _mapper;
        private IDistributedCache _distributedCache;
        private IConfiguration _configuration;

        public BorrowController(IBorrowService borrowService, IMapper mapper, IDistributedCache distributedCache, IConfiguration configuration)
        {
            _borrowService = borrowService;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }


        [HttpGet("RetrieveBorrowedBooks", Name = "GetBorrowedBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> RetrieveBorrowedBooks()
        {
            // Try to get the result from cache
            var cacheKey = _configuration["CacheSettings:BorrowedBooksCacheKey"];
            //string serializedBookList;
            List<BorrowedBookResponse> borrowedbooks;
            var cachedResult = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResult) && cachedResult.Length > 0)
            {
                //serializedBookList = Encoding.UTF8.GetString(cachedResult);
                borrowedbooks = JsonConvert.DeserializeObject<List<BorrowedBookResponse>>(cachedResult);
                return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = borrowedbooks };
            }

            // If not in cache, get the result from the service
            var borrowedbook = await _borrowService.GetAllAsync();
            var response = _mapper.Map<List<BorrowedBookResponse>>(borrowedbook);

            // Set the result in cache
            var cacheDurationMinutes = double.Parse(_configuration["CacheSettings:CacheDurationMinutes"]);
            var slidingExpirationMinutes = double.Parse(_configuration["CacheSettings:SlidingExpirationMinutes"]);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheDurationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpirationMinutes)
            };
            await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(response), options);

            return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = response };
        }


        [HttpGet("{id:int}", Name = "RetrieveABorrowedBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> RetrieveABorrwedBook(int id)
        {
            if (id == 0)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { "Invalid Id" } };
            }
            var borrowedbook = await _borrowService.GetAsync(id);
            if (borrowedbook == null)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.NotFound, IsSuccess = false, ErrorMessages = new List<string> { "Borrowed book not found" } };
            }
            return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = _mapper.Map<BorrowedBookResponse>(borrowedbook) };
        }

        [HttpPost("BorrowBook")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> BorrowBook([FromBody] BorrowBook request)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = ModelState.Values.SelectMany(x => x.Errors.Select(e => e.ErrorMessage)).ToList() };
            }
            try
            {
                var borrow = await _borrowService.CreateAsync(request);
                var mappedBorrow = _mapper.Map<BorrowedBookResponse>(borrow);
                return CreatedAtRoute("RetrieveABorrowedBook", new { id = borrow.BookId}, new ApiResponse { StatusCode = HttpStatusCode.Created, IsSuccess = true, Result = mappedBorrow });
            }
            catch (BookAlreadyBorrowedException ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = new List<string> { ex.Message } };
            }

        }

    }
}
