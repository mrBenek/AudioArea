using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Packt.Shared
{
    public class Pagination
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int StartRecord { get; set; }
        public int EndRecord { get; set; }

        public Pagination() { }

        public Pagination(int totalItems, int page, int pageSize = 10)
        {

            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 2;
            int endPage = currentPage + 2;

            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 5)
                {
                    startPage = endPage - 4;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;

            StartRecord = (CurrentPage - 1) * PageSize + 1;
            EndRecord = StartRecord - 1 + PageSize;
        }
    }
}
