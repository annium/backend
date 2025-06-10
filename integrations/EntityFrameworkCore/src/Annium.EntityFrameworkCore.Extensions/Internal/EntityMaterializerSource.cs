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

/// <summary>
/// Custom entity materializer source that supports materialization hooks for entities implementing IMaterializable
/// </summary>
internal class EntityMaterializerSource : EntityMaterializerSourceBase
{
    /// <summary>
    /// Initializes a new instance of the EntityMaterializerSource class
    /// </summary>
    /// <param name="dependencies">The dependencies required by the entity materializer source</param>
    public EntityMaterializerSource(EntityMaterializerSourceDependencies dependencies)
        : base(dependencies) { }

    /// <summary>
    /// Creates a materialization expression that includes calling OnMaterialized for entities implementing IMaterializable
    /// </summary>
    /// <param name="entityType">The entity type being materialized</param>
    /// <param name="entityInstanceName">The name of the entity instance variable</param>
    /// <param name="materializationContextExpression">The materialization context expression</param>
    /// <returns>An expression that materializes the entity and calls OnMaterialized if applicable</returns>
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
