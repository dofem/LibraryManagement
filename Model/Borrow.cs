using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Entities
{
    public class Borrow : BaseEntity
    {
        [ForeignKey("Book")]
        public int BookId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);
        public DateTime? ReturnDate { get; set; }
        public string PhoneNumber { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
    }
}
