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
    public class CategoryController : ControllerBase
    {
        private ICategoryService _categoryService;
        private IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        [HttpGet("RetrieveAvailableCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetCategories>>> RetrieveAllBooks()
        {
            var categoryList = await _categoryService.GetAllAsync();
            return Ok(_mapper.Map<GetCategories>(categoryList));
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCategories>> GetCategory(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var category = await _categoryService.GetAsync(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<GetCategories>(category));
        }

        [HttpPost("AddCategory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetCategories>> AddACategory([FromBody] AddNewCategory addNewCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await _categoryService.GetAsync(u => u.Name.ToLower() == addNewCategory.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Book Category already created");
                return BadRequest(ModelState);
            }
            if (addNewCategory == null)
            {
                return BadRequest(addNewCategory);
            }
            Category category = _mapper.Map<Category>(addNewCategory);
            await _categoryService.CreateAsync(category);
            return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
        }

        [HttpDelete("RemoveCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveCategory(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var category = await _categoryService.GetAsync(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            await _categoryService.RemoveAsync(category);
            return NoContent();
        }

    }
}
