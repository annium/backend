#pragma warning disable EF1001
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using EntityMaterializerSourceBase = Microsoft.EntityFrameworkCore.Query.Internal.EntityMaterializerSource;

namespace Annium.EntityFrameworkCore.Extensions.Internal;

internal class EntityMaterializerSource : EntityMaterializerSourceBase
{
    public EntityMaterializerSource(EntityMaterializerSourceDependencies dependencies)
        : base(dependencies) { }

    [Obsolete("Use the overload that accepts an EntityMaterializerSourceParameters object.")]
    public override Expression CreateMaterializeExpression(
        IEntityType entityType,
        string entityInstanceName,
        Expression materializationContextExpression
    )
    {
        var parameters = new EntityMaterializerSourceParameters(entityType, entityInstanceName, null);
        var baseExpression = base.CreateMaterializeExpression(parameters, materializationContextExpression);
        var clrType = entityType.ClrType;
        if (!clrType.GetInterfaces().Contains(typeof(IMaterializable)))
            return baseExpression;

        var onMaterializedMethod = clrType.GetMethod(nameof(IMaterializable.OnMaterialized))!;

        var blockExpressions = new List<Expression>(((BlockExpression)baseExpression).Expressions);
        var instanceVariable = (ParameterExpression)blockExpressions.Last();

        var onMaterializedExpression = Expression.Call(instanceVariable, onMaterializedMethod);

        blockExpressions.Insert(blockExpressions.Count - 1, onMaterializedExpression);

        return Expression.Block(new[] { instanceVariable }, blockExpressions);
    }
}
#pragma warning restore EF1001
