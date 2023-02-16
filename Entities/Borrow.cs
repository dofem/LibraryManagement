namespace LibraryManagement.Entities
{
    public class Borrow : BaseEntity
    {
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
    }
}
