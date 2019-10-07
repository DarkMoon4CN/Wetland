using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Wetland.Models;

namespace Wetland
{
    interface IDBContext
    {
        bool AddList<T>(List<T> entitys);

        bool Add<T>(T entity);

        List<T> Get<T>(Expression<Func<T, bool>> where, DBPageBase page);
    }
}
