using Task_Management_API.Models.Enums;

namespace Task_Management_API.DTOs
{
    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
