using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsWithRedis.Common.DTOs
{
    public record WithChildrenDTO<TParent, TChild>
        (TParent Parent, List<TChild> Children);
}
