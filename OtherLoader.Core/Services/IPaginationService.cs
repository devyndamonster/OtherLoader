using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Services
{
    public interface IPaginationService
    {
        public int GetNumberOfPages(int pageSize, int itemCount);

        public int HasNextPage(int pageSize, int itemCount, int currentPage);

        public int HasPrevPage(int currentPage);
    }
}
