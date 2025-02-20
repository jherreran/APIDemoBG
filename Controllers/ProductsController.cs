using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductSearchAPI.Data;
using ProductSearchAPI.Models;

namespace ProductSearchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;

        public ProductsController(ProductContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Product>>> GetProducts(
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Products.AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Product>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<Product>>> SearchProducts(
            [FromQuery] string? query,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var searchQuery = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchQuery = searchQuery.Where(p =>
                    EF.Functions.Like(p.Name, $"%{query}%") ||
                    EF.Functions.Like(p.Description, $"%{query}%")
                );
            }

            var totalItems = await searchQuery.CountAsync();

            var items = await searchQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPost("addWishlist/{id}")]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsDesired = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("removeWishlist/{id}")]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsDesired = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("Wishlist")]
        public async Task<ActionResult<IEnumerable<Product>>> GetDesiredProducts()
        {
            return await _context.Products
                                 .Where(p => p.IsDesired)
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
