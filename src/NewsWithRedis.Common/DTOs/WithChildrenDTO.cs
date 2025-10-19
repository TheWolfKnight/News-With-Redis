using System.Collections.Generic;

namespace NewsWithRedis.Common.Dtos;

public record WithChildrenDto<TParent, TChild>(TParent Parent, List<TChild> Children);
