namespace LibraryManagement.ExceptionHandler
{
    public class BookAlreadyBorrowedException : Exception
    {
        public BookAlreadyBorrowedException(string message) : base(message)
        {
        }
    }
}
