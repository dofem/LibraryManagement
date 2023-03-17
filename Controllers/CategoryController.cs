using AutoMapper;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        public async Task<ActionResult<ApiResponse>> RetrieveAllCategories()
        {
            try
            {
                var categoryList = await _categoryService.GetAllAsync();
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = _mapper.Map<IEnumerable<GetCategories>>(categoryList)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        public async Task<ActionResult<ApiResponse>> GetCategory(int id)
        {
            try
            {
                if (id == 0)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Invalid ID" }
                    };
                }
                var category = await _categoryService.GetAsync(id);
                if (category == null)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Category not found" }
                    };
                }
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = _mapper.Map<GetCategories>(category)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

        [HttpPost("AddCategory")]
        public async Task<ActionResult<ApiResponse>> AddACategory([FromBody] AddNewCategory addNewCategory)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList()
                    };
                }
                if (await _categoryService.GetByName(addNewCategory.Name) != null)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Category already exists" }
                    };
                }
                if (addNewCategory == null)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Invalid request" }
                    };
                }
                Category category = _mapper.Map<Category>(addNewCategory);
                await _categoryService.CreateAsync(category);
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.Created,
                    IsSuccess = true,
                    Result = _mapper.Map<GetCategories>(category)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

        [HttpDelete("RemoveCategory")]
        public async Task<ActionResult<ApiResponse>> RemoveCategory(int id)
        {
            try
            {
                if (id == 0)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Invalid ID" }
                    };
                }
                var category = await _categoryService.GetAsync(id);
                if (category == null)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Category not Found" }
                    };
                }
                await _categoryService.RemoveAsync(category);
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.NoContent,
                    IsSuccess = true,
                    Result = "Deleted Successfully",
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

    }
}
