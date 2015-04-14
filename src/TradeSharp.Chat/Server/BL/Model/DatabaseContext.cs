using System;

namespace TradeSharp.Chat.Server.BL.Model
{
    public class DatabaseContext
    {
        #region Singletone

        private static readonly Lazy<DatabaseContext> instance = new Lazy<DatabaseContext>(() => new DatabaseContext());

        public static DatabaseContext Instance
        {
            get { return instance.Value; }
        }

        #endregion

        // TODO: use TradeSharp.Linq
        public MTS_LIVEEntities MakeTerminal()
        {
            try
            {
                return new MTS_LIVEEntities();
            }
            catch(Exception ex)
            {
                //Logger.Error("Ошибка в DatabaseContext.Make():", ex);
                throw;
            }
        }

        public TS_ChatEntities MakeChat()
        {
            try
            {
                return new TS_ChatEntities();
            }
            catch (Exception ex)
            {
                //Logger.Error("Ошибка в DatabaseContext.Make():", ex);
                throw;
            }
        }

        private DatabaseContext()
        {
        }
    }
}
