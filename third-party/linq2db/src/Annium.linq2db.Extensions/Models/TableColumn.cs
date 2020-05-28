using System.Reflection;
using LinqToDB.Extensions;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Models
{
    public class TableColumn
    {
        public string Name => Attribute.Name ?? Member.Name;
        public MemberInfo Member { get; }
        public ColumnAttribute Attribute { get; }
        public DataTypeAttribute? DataType { get; }
        public NullableAttribute? Nullable { get; }
        public PrimaryKeyAttribute? PrimaryKey { get; }
        public AssociationAttribute? Association { get; }

        public TableColumn(
            MemberInfo member,
            ColumnAttribute attribute,
            DataTypeAttribute? dataType,
            NullableAttribute? nullable,
            PrimaryKeyAttribute? primaryKey,
            AssociationAttribute? association
        )
        {
            Member = member;
            Attribute = attribute;
            DataType = dataType;
            Nullable = nullable;
            PrimaryKey = primaryKey;
            Association = association;
        }

        public override string ToString() => $"{Member.GetMemberType().Name} {Name}";
    }
}