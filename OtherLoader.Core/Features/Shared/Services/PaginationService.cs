using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Services
{
    public class PaginationService : IPaginationService
    {
        public int GetNumberOfPages(int pageSize, int itemCount)
        {
            var pagesRoundedUp = (int)Math.Ceiling((double)itemCount / pageSize);

            return Math.Max(pagesRoundedUp, 1);
        }

        public bool HasNextPage(int pageSize, int itemCount, int currentPage)
        {
            return currentPage < GetNumberOfPages(pageSize, itemCount) - 1;
        }

        public bool HasPrevPage(int currentPage)
        {
            return currentPage > 0;
        }
    }
}
