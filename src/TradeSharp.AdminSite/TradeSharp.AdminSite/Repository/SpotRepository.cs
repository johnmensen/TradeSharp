using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class SpotRepository : ISpotRepository
    {
        public List<SPOT> GetAllSpotSymbols(TradeSharpConnection context)
        {
            var newContext = EnsureContext(ref context);
            try
            {
                return context.SPOT.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllSpotSymbols()", ex);
            }
            finally
            {
                if (newContext != null) newContext.Dispose();
            }
            return new List<SPOT>();
        }

        public string AddNewSpotItem(TradeSharpConnection context, SpotModel model)
        {
            var result = string.Empty;
            var newContext = EnsureContext(ref context);
            try
            {
                var spot = new SPOT
                {
                    ComBase = model.ComBase,
                    ComCounter = model.ComCounter,
                    Title = model.Title,
                    CodeFXI = model.CodeFXI,
                    MinVolume = model.MinVolume,
                    MinStepVolume = model.MinStepVolume,
                    Precise = model.Precise,
                    SwapBuy = model.SwapBuy,
                    SwapSell = model.SwapSell,
                    Description = model.Description
                };

                context.SPOT.Add(spot);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("AddNewSpotItem()", ex);
                result = ex.Message;
            }
            finally
            {
                if (newContext != null) newContext.Dispose();
            }
            return result;
        }

        public string EditSpotItem(TradeSharpConnection context, SpotModel model)
        {
            var result = string.Empty;
            var newContext = EnsureContext(ref context);
            try
            {
                var itemToEdit = context.SPOT.Single(x => x.Title == model.Title);

                itemToEdit.ComBase = model.ComBase;
                itemToEdit.ComCounter = model.ComCounter;
                itemToEdit.Title = model.Title;
                itemToEdit.CodeFXI = model.CodeFXI;
                itemToEdit.MinVolume = model.MinVolume;
                itemToEdit.MinStepVolume = model.MinStepVolume;
                itemToEdit.Precise = model.Precise;
                itemToEdit.SwapBuy = model.SwapBuy;
                itemToEdit.SwapSell = model.SwapSell;
                itemToEdit.Description = model.Description;

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("EditSpotItem()", ex);
                result = ex.Message;
            }
            finally
            {
                if (newContext != null) newContext.Dispose();
            }
            return result;
        }

        public string DeleteSpotItem(TradeSharpConnection context, string title)
        {
            var result = string.Empty;
            var newContext = EnsureContext(ref context);
            try
            {
                var itemToDel = context.SPOT.Single(x => x.Title == title);
                context.SPOT.Remove(itemToDel);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("DeleteSpotItem()", ex);
                result = ex.Message;
            }
            finally
            {
                if (newContext != null) newContext.Dispose();
            }
            return result;
        }

        private static TradeSharpConnection EnsureContext(ref TradeSharpConnection context)
        {
            if (context != null)
                return null;
            context = new TradeSharpConnection();
            return context;
        }
    }
}