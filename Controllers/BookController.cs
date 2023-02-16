using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private IBookService _bookService;
        private IMapper _mapper;

        public BookController(IBookService bookService,IMapper mapper)
        {
            _bookService = bookService;
            _mapper = mapper;
        }

        [HttpGet("RetrieveBookCollections")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetAllBooks>>> RetrieveAllBooks()
        {
            var bookList = await _bookService.GetAllAsync();
            return Ok(_mapper.Map<GetAllBooks>(bookList));
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
