using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.ExceptionHandler;
using LibraryManagement.Services;
using LibraryManagement.Services.Implementation;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
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

        public BorrowController(IBorrowService borrowService,IMapper mapper,IDistributedCache distributedCache,IConfiguration configuration)
        {
            _borrowService = borrowService;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }


        [HttpGet("RetrieveBorrowedBooks", Name = "GetBorrowedBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowedBookResponse>>> RetrieveBorrowedBooks()
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
                return Ok(borrowedbooks);
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

            return Ok(response);
        }





        [HttpGet("{id:int}", Name = "RetrieveABorrowedBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BorrowedBookResponse>> RetrieveABorrwedBook(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var borrowedbook = await _borrowService.GetAsync(u => u.Id == id);
            if (borrowedbook == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<BorrowedBookResponse>(borrowedbook));
        }


        [HttpPost("BorrowBook")]
        public IActionResult BorrowBook([FromBody] BorrowBook request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var borrow = _borrowService.CreateAsync(request);
                return Ok(borrow);
            }
            catch (BookAlreadyBorrowedException ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

    }
}
