using System.Data;
using ECM7.Migrator.Framework;
using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace TradeSharp.DbMigration.TradeSharp
{
    static class Schema
    {
        public const string Name = "dbo";
    }

    // Описание, как создавать миграции
    // https://code.google.com/p/ecm7migrator/wiki/WritingMigrations
    // каждая миграция - новый класс, его название - имя миграции

    // начальную миграцию не трогать
    [Migration(1)]
    public class InitMigration : Migration
    {
        public override void Apply()
        {
        }

        public override void Revert()
        {
        }
    }

    [Migration(2)]
    public class AccountShareHistoryTable : Migration
    {
        private const string TableName = "ACCOUNT_SHARE_HISTORY";

        public override void Apply()
        {
            Database.AddTable(TableName,
                              new Column("Account", DbType.Int32, ColumnProperty.NotNull),
                              new Column("ShareOwner", DbType.Int32, ColumnProperty.NotNull),
                              new Column("Date", DbType.DateTime, ColumnProperty.NotNull),
                              new Column("OldShare", DbType.Decimal.WithSize(12, 9), ColumnProperty.Null),
                              new Column("NewShare", DbType.Decimal.WithSize(12, 9), ColumnProperty.NotNull),
                              new Column("OldHWM", DbType.Decimal.WithSize(12, 2), ColumnProperty.Null),
                              new Column("NewHWM", DbType.Decimal.WithSize(12, 2), ColumnProperty.NotNull)); 
            
            Database.AddPrimaryKey("Pk" + TableName,
                new SchemaQualifiedObjectName
                {
                    Name = TableName,
                    Schema = Schema.Name
                }, "Account", "ShareOwner", "Date");

            // sic! сначала идет Foreign Key Table, потом - PK Table
            Database.AddForeignKey("Fk" + TableName + "Account",
                new SchemaQualifiedObjectName
                {
                    Name = TableName,
                    Schema = Schema.Name
                }, "Account",
                new SchemaQualifiedObjectName
                {
                    Name = "ACCOUNT",
                    Schema = Schema.Name
                }, "ID",
                ForeignKeyConstraint.Cascade,
                ForeignKeyConstraint.Cascade);

            Database.AddForeignKey("Fk" + TableName + "User",
                new SchemaQualifiedObjectName
                {
                    Name = TableName,
                    Schema = Schema.Name
                }, "ShareOwner",
                new SchemaQualifiedObjectName
                {
                    Name = "PLATFORM_USER",
                    Schema = Schema.Name
                }, "ID",
                ForeignKeyConstraint.Cascade,
                ForeignKeyConstraint.Cascade);
            // в БД у ключей вроде нет никаких действий на обновление / удаление
        }

        public override void Revert()
        {
            Database.RemoveConstraint(new SchemaQualifiedObjectName
            {
                Name = TableName,
                Schema = Schema.Name
            }, "Fk" + TableName);
            Database.RemoveTable(TableName);
        }
    }

    [Migration(3)]
    public class ExtendAccountShareHistoryTable : Migration
    {
        private const string TableName = "ACCOUNT_SHARE_HISTORY";
        private const string NewColumnName = "ShareAmount";

        public override void Apply()
        {
            Database.AddColumn(TableName,
                new Column(NewColumnName, DbType.Decimal.WithSize(12, 2), ColumnProperty.NotNull, 0M));
        }

        public override void Revert()
        {
            Database.RemoveColumn(TableName, NewColumnName);
        }
    }
}
