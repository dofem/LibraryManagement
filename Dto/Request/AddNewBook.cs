namespace LibraryManagement.Dto.Request
{
    public class AddNewBook
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public int CategoryId { get; set; }
    }
}
