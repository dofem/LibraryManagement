using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Dto.Request
{
    public class BorrowBook
    {
        [ForeignKey("Book")]
        public int BookId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }
    }
}
