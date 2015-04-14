using System;
using System.Collections.Generic;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Contract;
using TradeSharp.Hub.BL.Model;
using TradeSharp.Util;
using System.Linq;
using System.Data.Entity;

namespace TradeSharp.Hub.BL.Repository
{
    public class TickerRepository : EntityRepository<Ticker>, ITickerRepository
    {
        public PagedListResult<Ticker> GetAllTickers(string sortBy, bool ascending, int skip, int take)
        {
            return GetItemsPaged(speciman.Property(s => s.BaseCurrency), sortBy, ascending, skip, take);
        }

        public Ticker GetTicker(string name)
        {
            return context.Ticker.Include(t => t.BaseCurrency).FirstOrDefault(t => t.Name == name);
        }

        public IEnumerable<string> GetTickerNames()
        {
            try
            {
                return context.Ticker.Select(x => x.Name);
            }
            catch (Exception ex)
            {
                Logger.Error("GetTickerNames", ex);
                return null;
            }
        }

        public bool DeleteTicker(string name, out string errorString)
        {
            errorString = string.Empty;

            var ticker = context.Ticker.FirstOrDefault(t => t.Name == name);
            if (ticker == null)
            {
                errorString = "\"" + name + "\" не найден";
                return false;
            }
            try
            {
                context.Ticker.Remove(ticker);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в DeleteTicker " + name, ex);
                errorString = ex.Message;
                return false;
            }
        }

        public bool AddOrUpdateTicker(Ticker ticker, out string errorString)
        {
            if (ticker == null)
                throw new ArgumentException("");

            errorString = string.Empty;
            var existOne = context.Ticker.FirstOrDefault(t => t.Name == ticker.Name);
            
            try
            {
                var baseCurrency = context.Currency.First(c => c.Code == ticker.BaseCurrency.Code);

                if (existOne != null)
                {
                    existOne.BaseCurrency = baseCurrency;
                }
                else
                {
                    context.Ticker.Add(new Ticker
                        {
                            Name = ticker.Name,
                            BaseCurrency = baseCurrency
                        });
                }
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка добавления / редактирования тикера " + ticker.Name, ex);
                errorString = ex.Message;
                return false;
            }
        }

        #region Работа с псевдонимами
        /// <summary>
        /// обновляет только псевдонимы, но за то все скопом
        /// </summary>
        public void UpdateListTickerAlias(List<TickerAlias> newTickerAliasList)
        {
            try
            {
                var dbTikerAlias = context.TickerAlias.ToList();

                foreach (var newTickerAlias in newTickerAliasList)
                {
                    var currentTikerAlias =
                        dbTikerAlias.FirstOrDefault(
                            x => x.Ticker == newTickerAlias.Ticker && x.Server == newTickerAlias.Server);

                    if (currentTikerAlias == null || currentTikerAlias.Alias == newTickerAlias.Alias) continue;
                    currentTikerAlias.Alias = newTickerAlias.Alias;
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Debug("UpdateListTickerAllias", ex);
            }
        }

        public IEnumerable<TickerAlias> GetAllTickerAlias()
        {
            try
            {
                return context.TickerAlias;
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllTickerAllias", ex);
                return null;
            }
        }

        public TickerAlias GetTickerAlias(string server, string ticker)
        {
            try
            {
                var alias = context.TickerAlias.SingleOrDefault(x => x.Server == server && x.Ticker == ticker);
                if (alias == null) 
                    Logger.Error(string.Format("GetTickerAlias - в таблице 'TickerAlias' не найдела запись с идентификаторами '{0}' '{1}'", server, ticker));
                return alias;
            }
            catch (Exception ex)
            {
                Logger.Error("GetTickerAllias", ex);
                return null;
            }
        }

        public bool RemoveTickerAlias(string server, string ticker)
        {
            try
            {
                var alias = context.TickerAlias.SingleOrDefault(x => x.Server == server && x.Ticker == ticker);
                if (alias == null)
                {
                    Logger.Error(string.Format("RemoveTickerAlias - в таблице 'TickerAlias' не найдела запись с идентификаторами '{0}' '{1}'", server, ticker));
                    return false;
                }

                context.TickerAlias.Remove(alias);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("RemoveTickerAlias", ex);
                return false;
            }
        }

        public bool AddOrUpdateTickerAlias(TickerAlias newTickerAlias)
        {
            try
            {
                if (newTickerAlias == null)
                {
                    Logger.Error("newTickerAlias не может быть null");
                    return false;
                }

                var dbTikerAlias =
                    context.TickerAlias.FirstOrDefault(
                        x => x.Ticker == newTickerAlias.Ticker && x.Server == newTickerAlias.Server);

                if (dbTikerAlias != null)
                {
                    dbTikerAlias.Ticker = newTickerAlias.Ticker;
                    dbTikerAlias.Server = newTickerAlias.Server;
                    dbTikerAlias.Alias = newTickerAlias.Alias;
                }
                else
                {
                    context.TickerAlias.Add(newTickerAlias);
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("AddOrUpdateTickerAllias", ex);
                return false;
            }

            return true;
        }
        #endregion
    }
}
