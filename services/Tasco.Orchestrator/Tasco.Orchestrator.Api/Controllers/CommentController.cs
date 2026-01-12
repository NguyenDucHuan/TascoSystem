using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.CommentModel;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly CommentGrpcClientService _commentGrpcClient;
        private readonly ILogger<CommentController> _logger;

        public CommentController(
            CommentGrpcClientService commentGrpcClient,
            ILogger<CommentController> logger)
        {
            _commentGrpcClient = commentGrpcClient;
            _logger = logger;
        }

        /// <summary>
        /// Tạo comment mới
        /// </summary>
        /// <param name="request">Thông tin comment</param>
        /// <returns>Kết quả tạo comment</returns>
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Comment data is required");
                }

                if (string.IsNullOrEmpty(request.TaskId))
                {
                    return BadRequest("Task ID is required");
                }

                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Comment content is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Creating comment for task {TaskId} by user {UserId}",
                        request.TaskId, userId);

                    var grpcRequest = new CommentRequest
                    {
                        TaskId = request.TaskId,
                        Content = request.Content,
                        UserId = userId.ToString()
                    };

                    var response = await _commentGrpcClient.CreateCommentAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Comment created successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to create comment"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for task {TaskId}", request?.TaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách comment theo task ID
        /// </summary>
        /// <param name="taskId">ID của task</param>
        /// <param name="pageIndex">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách comment</returns>
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetCommentsByTaskId(
            string taskId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(taskId))
                {
                    return BadRequest("Task ID is required");
                }

                _logger.LogInformation("Getting comments for task {TaskId}, page {PageIndex}, size {PageSize}",
                    taskId, pageIndex, pageSize);

                var request = new CommentRequestByTaskId
                {
                    TaskId = taskId,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var response = await _commentGrpcClient.GetCommentsByTaskIdAsync(request);

                if (response != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Comments retrieved successfully",
                        data = response
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to get comments"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for task {TaskId}", taskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Cập nhật comment
        /// </summary>
        /// <param name="commentId">ID của comment</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(string commentId, [FromBody] UpdateCommentRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(commentId))
                {
                    return BadRequest("Comment ID is required");
                }

                if (request == null)
                {
                    return BadRequest("Comment data is required");
                }

                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Comment content is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating comment {CommentId} by user {UserId}",
                        commentId, userId);

                    var grpcRequest = new UpdateCommentRequest
                    {
                        Id = commentId,
                        Content = request.Content,
                        UserId = userId.ToString()
                    };

                    var response = await _commentGrpcClient.UpdateCommentAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Comment updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update comment"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Xóa comment
        /// </summary>
        /// <param name="commentId">ID của comment</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            try
            {
                if (string.IsNullOrEmpty(commentId))
                {
                    return BadRequest("Comment ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Deleting comment {CommentId} by user {UserId}",
                        commentId, userId);

                    var request = new CommentRequestById
                    {
                        Id = commentId,
                        UserId = userId.ToString()
                    };

                    var success = await _commentGrpcClient.DeleteCommentAsync(request);

                    if (success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Comment deleted successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to delete comment"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }


}