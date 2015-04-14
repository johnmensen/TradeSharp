using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.CommonClass
{
    /// <summary>
    /// Вспомогательный класс утилит
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Получает с сервера из какой либо таблици список пригодный для привязки к DropDownListFor
        /// </summary>
        public static List<SelectListItem> SelectAllValuesFromTable<T>(Expression<Func<T, SelectListItem>> titleValueSelector) where T : class
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    return ctx.Set<T>().Select(titleValueSelector).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SelectAllValuesFromTable", ex);
            }   
            return new List<SelectListItem>();
        }

        /// <summary>
        /// Получает с сервера из какой либо таблици список строк
        /// </summary>
        public static List<string> SelectAllValuesFromTableToList<T>(Expression<Func<T, string>> titleValueSelector) where T : class
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    return ctx.Set<T>().Select(titleValueSelector).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SelectAllValuesFromTableToList", ex);
            }
            return new List<string>();
        }

        /// <summary>
        /// Проверка наличия в таблици БД записей, удовлетворяющих указанным условиям
        /// </summary>
        public static bool CheckExistValuesFromTable<T>(Expression<Func<T, bool>> titleValueSelector) where T : class
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    return ctx.Set<T>().Any(titleValueSelector);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CheckExistValuesFromTable()", ex);
            }
            return false;
        }

        /// <summary>
        /// Мапит коллекцию, полученную из хранимой процедуры GetPositionList в список из PositionItem
        /// </summary>
        /// <param name="allPositions"></param>
        public static List<PositionItem> DecoratPositionItems(List<GetPositionList_Result> allPositions)
        {
            var result =  new List<PositionItem>();
            if (allPositions == null || !allPositions.Any()) return result;
            try
            {
                result = allPositions.Select(marketOrder => new PositionItem
                {
                    ID = marketOrder.ID,
                    AccountID =  marketOrder.AccountID,
                    State = (PositionState)marketOrder.IsClosed,
                    PriceEnter = marketOrder.PriceEnter,
                    PriceExit = marketOrder.PriceExit,
                    TimeEnter = marketOrder.TimeEnter,
                    TimeExit = marketOrder.TimeExit,
                    ResultDepo = marketOrder.ResultDepo,
                    ResultPoints = marketOrder.ResultPoints,
                    Symbol = marketOrder.Symbol,
                    Side = marketOrder.Side
                }).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось преобразовать коллекцию, полученную из хранимой процедуры GetPositionList в список из PositionItem.", ex);
            }
            
            return result;
        }

        /// <summary>
        /// Формирует список из заголовков допустимых в Converter типов + тип String
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMetadataTypeTitle()
        {
            var result = Converter.GetSupportedTypeNames();
            result.Add("string");
            return result;
        }

        /// <summary>
        /// Формирует данные (в виде словаря) для выпадающих списков, часто используемых на сайте
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetDataToFillDropDownListForAccountView()
        {
            var result = new Dictionary<string, object>
                {
                    {
                        "listTickers",
                        SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem {Text = x.Title, Value = x.Title})
                    },
                    {
                        "listGroups",
                        SelectAllValuesFromTable<ACCOUNT_GROUP>(x => new SelectListItem {Text = x.Code, Value = x.Code})
                    },
                    {
                        "listUserRights",
                        Enum.GetValues(typeof (UserAccountRights))
                            .Cast<UserAccountRights>()
                            .Select(x => new SelectListItem
                                {
                                    Text = EnumFriendlyName<UserAccountRights>.GetString(x),
                                    Value = x.ToString()
                                }).ToList()
                    },
                    {
                        "listUserRoles",
                        Enum.GetValues(typeof (UserRole)).Cast<UserRole>().Select(x => new SelectListItem
                            {
                                Text = EnumFriendlyName<UserRole>.GetString(x),
                                Value = x.ToString()
                            }).ToList()
                    }
                };

            return result;
        }

        /// <summary>
        /// Генерируе человеко-понятное описание какого либо типа
        /// </summary>
        /// <param name="type">Тип, для которого генерируется описание</param>
        /// <returns>Массив, содержащий в первом элементе тип свойства в виде строки, во втором - описание свойства</returns>
        public static Tuple<string, string> GetTypeDescription(Type type)
        {
            try
            {
                if (Converter.IsNullable(type))
                {
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    if (type == typeof(string)) underlyingType = typeof(string);
                    return typeDescriptions[underlyingType];
                }
                return typeDescriptions[type];
            }
            catch (Exception ex)
            {
                Logger.Error("GetTypeDescription : неизвестный тип данных "  + type.Name, ex);
                return new Tuple<string, string>(type.Name, string.Format("{0}, {1} {2}", Resource.TextUnknownDataType, Resource.TextFailedGetDescriptionFor, type.Name));
            }
        }


        /// <summary>
        /// словарь поясняющих комментариев к типам данных (используется в представлении редактирования сделок)
        /// </summary>
        private static readonly Dictionary<Type, Tuple<string, string>> typeDescriptions = new Dictionary<Type, Tuple<string, string>>
            {
                {typeof (decimal), new Tuple<string, string>("decimal",Resource.TypeDescriptionDecimal)},
                {typeof (DateTime), new Tuple<string, string>("dateTime", Resource.TypeDescriptionDateTime)},
                {typeof (bool), new Tuple<string, string>("bool", Resource.TypeDescriptionBool)},
                {typeof (int), new Tuple<string, string>("int", Resource.TypeDescriptionInt)},
                {typeof (double), new Tuple<string, string>("double", Resource.TypeDescriptionDouble)},
                {typeof (float), new Tuple<string, string>("float", Resource.TypeDescriptionFloat)},
                {typeof (short), new Tuple<string, string>("short", Resource.TypeDescriptionShort)},
                {typeof (long), new Tuple<string, string>("long", Resource.TypeDescriptionLong)},
                {typeof (byte), new Tuple<string, string>("byte", Resource.TypeDescriptionByte)},
                {typeof (uint), new Tuple<string, string>("uint", Resource.TypeDescriptionUint)},
                {typeof (ulong), new Tuple<string, string>("ulong", Resource.TypeDescriptionUlong)},
                {typeof (String), new Tuple<string, string>("string", Resource.TypeDescriptionString)},
                {typeof (PositionExitReason), new Tuple<string, string>("PositionExitReason", Resource.TypeDescriptionEnum + " (0, 1, 2...)")},
                {typeof (PositionState), new Tuple<string, string>("PositionState", Resource.TypeDescriptionEnum + " (0, 1, 2...)")}
            };
        

        public static Dictionary<string, int> dealSide = new Dictionary<string, int>
            {
                {"Sell", -1},
                {"Buy", 1},
            };

        public static AutoTradeSettings GetDefaultPortfolioTradeSettings()
        {
            return new AutoTradeSettings
            {
                TradeAuto = true,
                PercentLeverage = 100,
                MaxLeverage = 15,
                MinVolume = 10000,
                StepVolume = 10000
            };
        }
    }

    public static class PropertyExtensions
    {
        public static string GetPropertyName<TObject>(this TObject type,
                                                       Expression<Func<TObject, object>> propertyRefExpr)
        {
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        public static string GetName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
        {
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        private static string GetPropertyNameCore(Expression propertyRefExpr)
        {
            if (propertyRefExpr == null)
                throw new ArgumentNullException("propertyRefExpr", "propertyRefExpr is null.");

            var memberExpr = propertyRefExpr as MemberExpression;
            if (memberExpr == null)
            {
                var unaryExpr = propertyRefExpr as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                    memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
                return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.",
                             "propertyRefExpr");
        }
    }
}