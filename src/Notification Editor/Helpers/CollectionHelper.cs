using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireBlade.NotificationEditor.Helpers
{
    public static class CollectionHelper
    {
        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> e, Func<T, Task<bool>> f)
        {
            foreach (var item in e)
                if (await f(item))
                    return item;

            return default(T);
        }
    }
}
