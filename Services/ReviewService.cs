using API.Model;
using API.Model.DB;
using API.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    public class ReviewService
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddReviewAsync(ReviewDTO reviewDTO)
        {
            try
            {
                // Validate input
                if (reviewDTO == null)
                {
                    _logger.LogWarning("Attempted to add null review");
                    return false;
                }

                if (reviewDTO.Rating < 1 || reviewDTO.Rating > 5)
                {
                    _logger.LogWarning("Invalid rating value: {Rating}", reviewDTO.Rating);
                    return false;
                }

                // Check if customer has already reviewed this product
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == reviewDTO.ProductId &&
                                            r.CustomerId == reviewDTO.CustomerId);

                if (existingReview != null)
                {
                    _logger.LogInformation("Customer {CustomerId} already reviewed product {ProductId}",
                        reviewDTO.CustomerId, reviewDTO.ProductId);
                    return false;
                }

                var review = new Review
                {
                    ProductId = reviewDTO.ProductId,
                    CustomerId = reviewDTO.CustomerId,
                    Rating = reviewDTO.Rating,
                    Comment = reviewDTO.Comment?.Trim(),
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow, // Use UTC for consistency
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Review added successfully for product {ProductId} by customer {CustomerId}",
                    reviewDTO.ProductId, reviewDTO.CustomerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for product {ProductId} by customer {CustomerId}",
                    reviewDTO.ProductId, reviewDTO.CustomerId);
                return false;
            }
        }

        public async Task<List<ReviewDTO>> GetProductReviewsAsync(int productId, bool approvedOnly = true)
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.Product)
                    .Include(r => r.Customer)
                    .Where(r => r.ProductId == productId && (!approvedOnly || r.IsApproved))
                    .OrderByDescending(r => r.CreatedAt) // Most recent first
                    .Select(r => new ReviewDTO
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        ProductName = r.Product.NamaProduct,
                        CustomerId = r.CustomerId,
                        CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        IsApproved = r.IsApproved,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", productId);
                return new List<ReviewDTO>();
            }
        }

        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    _logger.LogWarning("Review {ReviewId} not found for approval", reviewId);
                    return false;
                }

                review.IsApproved = true;
                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Review {ReviewId} approved successfully", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
                return false;
            }
        }

        public async Task<bool> UpdateReviewAsync(int reviewId, ReviewDTO reviewDTO)
        {
            try
            {
                var existingReview = await _context.Reviews.FindAsync(reviewId);
                if (existingReview == null)
                {
                    _logger.LogWarning("Review {ReviewId} not found for update", reviewId);
                    return false;
                }

                // Validate rating
                if (reviewDTO.Rating < 1 || reviewDTO.Rating > 5)
                {
                    _logger.LogWarning("Invalid rating value: {Rating}", reviewDTO.Rating);
                    return false;
                }

                existingReview.Rating = reviewDTO.Rating;
                existingReview.Comment = reviewDTO.Comment?.Trim();
                existingReview.IsApproved = false; // Reset approval status on update
                existingReview.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Review {ReviewId} updated successfully", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
                return false;
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    _logger.LogWarning("Review {ReviewId} not found for deletion", reviewId);
                    return false;
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Review {ReviewId} deleted successfully", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
                return false;
            }
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            try
            {
                var averageRating = await _context.Reviews
                    .Where(r => r.ProductId == productId && r.IsApproved)
                    .AverageAsync(r => (double?)r.Rating) ?? 0.0;

                return Math.Round(averageRating, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for product {ProductId}", productId);
                return 0.0;
            }
        }

        public async Task<int> GetProductReviewCountAsync(int productId, bool approvedOnly = true)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.ProductId == productId && (!approvedOnly || r.IsApproved));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting reviews for product {ProductId}", productId);
                return 0;
            }
        }

        public async Task<List<ReviewDTO>> GetPendingReviewsAsync()
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.Product)
                    .Include(r => r.Customer)
                    .Where(r => !r.IsApproved)
                    .OrderBy(r => r.CreatedAt) // Oldest first for admin review
                    .Select(r => new ReviewDTO
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        ProductName = r.Product.NamaProduct,
                        CustomerId = r.CustomerId,
                        CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        IsApproved = r.IsApproved,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reviews");
                return new List<ReviewDTO>();
            }
        }
    }
}