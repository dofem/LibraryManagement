using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Dto.Response
{
    public class BorrowedBookResponse
    {
        [ForeignKey("Book")]
        public int BookId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public DateTime BorrowDate { get; set; } 
        public DateTime DueDate { get; set; } 
    }
}
