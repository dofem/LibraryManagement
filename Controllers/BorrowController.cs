using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.ExceptionHandler;
using LibraryManagement.Services;
using LibraryManagement.Services.Implementation;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private IBorrowService _borrowService;
        private IMapper _mapper;

        public BorrowController(IBorrowService borrowService,IMapper mapper)
        {
            _borrowService = borrowService;
            _mapper = mapper;
        }

        
        [HttpGet("RetrieveBorrowedBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowedBookResponse>>> RetrieveBorrowedBooks()
        {
            var books = await _borrowService.GetAllAsync();
            return Ok(_mapper.Map<BorrowedBookResponse>(books));          
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
