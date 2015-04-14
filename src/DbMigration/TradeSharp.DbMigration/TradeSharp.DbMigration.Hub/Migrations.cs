using System.Data;
using ECM7.Migrator.Framework;
using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace TradeSharp.DbMigration.Hub
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
    public class InitialStructureMigration : Migration
    {
        private const string TableNameCurrency = "Currency";

        private const string TableNameTicker = "Ticker";
        
        private const string TableNameServerInstance = "ServerInstance";
        
        public override void Apply()
        {
            Database.AddTable(TableNameCurrency,
                              new Column("Code", DbType.AnsiString.WithSize(3), ColumnProperty.PrimaryKey),
                              new Column("CurrencyIndex", DbType.Int32, ColumnProperty.NotNull));

            Database.AddTable(TableNameTicker,
                              new Column("Name", DbType.AnsiString.WithSize(8), ColumnProperty.PrimaryKey),
                              new Column("Currency_Code", DbType.AnsiString.WithSize(3), ColumnProperty.NotNull));

            Database.AddTable(TableNameServerInstance,
                              new Column("Code", DbType.AnsiString.WithSize(5), ColumnProperty.PrimaryKey),
                              new Column("Country", DbType.AnsiString.WithSize(20), ColumnProperty.NotNull),
                              new Column("Title", DbType.String.WithSize(64), ColumnProperty.NotNull),
                              new Column("PrimaryIP", DbType.AnsiString.WithSize(15), ColumnProperty.NotNull),
                              new Column("Password", DbType.AnsiString.WithSize(15), ColumnProperty.NotNull),
                              new Column("Allowed", DbType.Boolean, ColumnProperty.NotNull),
                              new Column("Currency_Code", DbType.AnsiString.WithSize(3), ColumnProperty.NotNull),
                              new Column("MarkupPerMillionTrader", DbType.Decimal.WithSize(6, 2), ColumnProperty.NotNull),
                              new Column("MarkupPerMillionInvestor", DbType.Decimal.WithSize(6, 2), ColumnProperty.NotNull));

            // sic! сначала идет Foreign Key Table, потом - PK Table
            Database.AddForeignKey("Fk" + TableNameServerInstance + "Currency",
                new SchemaQualifiedObjectName
                {
                    Name = TableNameServerInstance,
                    Schema = Schema.Name
                }, "Currency_Code",
                new SchemaQualifiedObjectName
                {
                    Name = TableNameCurrency,
                    Schema = Schema.Name
                }, "Code",
                ForeignKeyConstraint.Cascade,
                ForeignKeyConstraint.Cascade);

            Database.AddForeignKey("Fk" + TableNameTicker + "Currency",
                new SchemaQualifiedObjectName
                {
                    Name = TableNameTicker,
                    Schema = Schema.Name
                }, "Currency_Code",
                new SchemaQualifiedObjectName
                {
                    Name = TableNameCurrency,
                    Schema = Schema.Name
                }, "Code",
                ForeignKeyConstraint.Cascade,
                ForeignKeyConstraint.Cascade);
        }

        public override void Revert()
        {
            Database.RemoveTable(TableNameServerInstance);
            Database.RemoveTable(TableNameTicker);
            Database.RemoveTable(TableNameCurrency);
        }
    }

    [Migration(3)]
    public class BrokerVolumeAndTickerAllias : Migration
    {
        private const string TableNameBrokerVolume = "BrokerVolume";

        private const string TableNameTickerAlias = "TickerAlias";

        private const string TableNameServerInstance = "ServerInstance";

        private const string TableNameTicker = "Ticker";

        private const string NewColumnNameGmt = "GMT";

        public override void Apply()
        {
            Database.AddColumn(TableNameServerInstance,
                new Column(NewColumnNameGmt, DbType.Int16, ColumnProperty.Null));

            Database.ExecuteNonQuery("update " + Schema.Name + "." + TableNameServerInstance + " set " + NewColumnNameGmt + " = 4 where GMT is null");

            Database.ChangeColumn(
                new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameServerInstance,
                                           Schema = Schema.Name
                                       }, NewColumnNameGmt, DbType.Int16, true);

            Database.AddTable(TableNameBrokerVolume,
                              new Column("Date", DbType.Date, ColumnProperty.PrimaryKey),
                              new Column("Server", DbType.AnsiString.WithSize(5), ColumnProperty.PrimaryKey),
                              new Column("Ticker", DbType.AnsiString.WithSize(8), ColumnProperty.PrimaryKey),
                              new Column("AccountCategory", DbType.Int32, ColumnProperty.PrimaryKey),
                              new Column("Volume", DbType.Int64, ColumnProperty.NotNull));

            Database.AddTable(TableNameTickerAlias,
                              new Column("Server", DbType.AnsiString.WithSize(5), ColumnProperty.PrimaryKey),
                              new Column("Ticker", DbType.AnsiString.WithSize(8), ColumnProperty.PrimaryKey),
                              new Column("Alias", DbType.AnsiString.WithSize(12), ColumnProperty.NotNull));

            #region 

            // BrokerVolume <-> Server
            Database.AddForeignKey("Fk" + TableNameBrokerVolume + "Server",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameBrokerVolume,
                                           Schema = Schema.Name
                                       }, "Server",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameServerInstance,
                                           Schema = Schema.Name
                                       }, "Code",
                                   ForeignKeyConstraint.NoAction,
                                   ForeignKeyConstraint.NoAction);

            // BrokerVolume <-> Ticker
            Database.AddForeignKey("Fk" + TableNameBrokerVolume + "Ticker",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameBrokerVolume,
                                           Schema = Schema.Name
                                       }, "Ticker",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameTicker,
                                           Schema = Schema.Name
                                       }, "Name",
                                   ForeignKeyConstraint.NoAction,
                                   ForeignKeyConstraint.NoAction);


            // TickerAlias <-> Server
            Database.AddForeignKey("Fk" + TableNameTickerAlias + "Server",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameTickerAlias,
                                           Schema = Schema.Name
                                       }, "Server",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameServerInstance,
                                           Schema = Schema.Name
                                       }, "Code",
                                   ForeignKeyConstraint.NoAction,
                                   ForeignKeyConstraint.NoAction);

            // TickerAlias <-> Ticker
            Database.AddForeignKey("Fk" + TableNameTickerAlias + "Ticker",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameTickerAlias,
                                           Schema = Schema.Name
                                       }, "Ticker",
                                   new SchemaQualifiedObjectName
                                       {
                                           Name = TableNameTicker,
                                           Schema = Schema.Name
                                       }, "Name",
                                   ForeignKeyConstraint.NoAction,
                                   ForeignKeyConstraint.NoAction);

            #endregion

        }

        public override void Revert()
        {
            Database.RemoveColumn(TableNameServerInstance, NewColumnNameGmt);
            Database.RemoveTable(TableNameBrokerVolume);
            Database.RemoveTable(TableNameTickerAlias);            
        }
    }
}
