using System;
using TradeSharp.Util;

namespace TradeSharp.Linq
{
    public class DatabaseContext
    {
        private static TradeSharpConnection fakeDbConnection;

        #region Singletone

        private static readonly Lazy<DatabaseContext> instance = new Lazy<DatabaseContext>(() => new DatabaseContext());

        public static DatabaseContext Instance
        {
            get { return instance.Value; }
        }

        #endregion

        public TradeSharpConnection Make()
        {
            try
            {
                var dbContext = fakeDbConnection ??
                       new TradeSharpConnection(LiveConnectionString.DefaultConnectionString);

                if (dbContext.TestException != null) throw dbContext.TestException; //Это нужно, что бы в тестовом проекте генерировать исключения
                return dbContext;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в DatabaseContext.Make():", ex);
                throw;
            }
        }
        public static void InitializeFake(TradeSharpConnection connection)
        {
            fakeDbConnection = connection;
        }

        private DatabaseContext()
        {
        }
    }
}
