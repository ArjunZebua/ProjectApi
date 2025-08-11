using Microsoft.AspNetCore.Mvc;
using API.Model.DB;
using Microsoft.EntityFrameworkCore;
using API.Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ReviewController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetAllReviews()
        {
            return await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReviewById(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound();

            return review;
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByProductId(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Include(r => r.Customer)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Review>> CreateReview(Review review)
        {
            review.CreatedAt = DateTime.Now;
            review.UpdatedAt = DateTime.Now;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, Review updatedReview)
        {
            if (id != updatedReview.Id)
                return BadRequest();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            review.Rating = updatedReview.Rating;
            review.Comment = updatedReview.Comment;
            review.IsApproved = updatedReview.IsApproved;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            review.IsApproved = true;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}