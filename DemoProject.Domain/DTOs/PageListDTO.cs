namespace DemoProject.Domain.DTOs
{
    public class PageListDTO<T>
    {
        public int TotalPage { set; get; }

        public List<T> Items { set; get; } = new List<T>();

    }
}
