using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.Helper;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Модель для представления редактирования позиций 
    /// </summary>
    public class PositionsEditModel
    {
        public List<SystemProperty> OpenPositionValues { get; set; }
        public List<SystemProperty> ClosePositionValues { get; set; }

        //Вспомогательный списки. Хранят Id всех сделок из positions. Используются в представлении. 
        public List<int> OpenIdList { get; set; }
        public List<int> CloseIdList { get; set; }

        /// <summary>
        /// Список ошибок валидации. Формат: Поле - коментарий к ошибке 
        /// </summary>
        public List<Tuple<string, string>> validationErrorList = new List<Tuple<string, string>>();

        public PositionsEditModel()
        {
        }

        public PositionsEditModel(List<PositionItem> positions)
        {
            //Вспомогательный списки. Хранят Id всех сделок из positions. Используются в представлении. 
            OpenIdList = positions.Where(x => !x.IsClosed).Select(x => x.ID).ToList();
            CloseIdList = positions.Where(x => x.IsClosed).Select(x => x.ID).ToList();
            //////////////////////////////////////////////////////////////////////////
             
            
            OpenPositionValues = new List<SystemProperty>();
            ClosePositionValues = new List<SystemProperty>();

            var attrsMeta = typeof(PositionItem).GetCustomAttribute<MetadataTypeAttribute>();

            //Проверям свойства на уровне класса PositionItem
            foreach (var prop in typeof(PositionItem).GetProperties())
            {
                var positionInfo = attrsMeta.MetadataClassType.GetProperty(prop.Name);
                if (positionInfo == null || !Attribute.IsDefined(positionInfo, typeof(EditableAttribute), true)) continue;

                //Определяем 'опасено' ли это поле для редактирования
                var isDanger = Attribute.IsDefined(positionInfo, typeof(DanderRangeAttribute), true);
                var isDealStateDefined = Attribute.IsDefined(positionInfo, typeof(DealStateAttribute), true);

                
                if (!isDealStateDefined)
                {
                    FillProrertyList(positions.Where(x => !x.IsClosed), prop, isDanger, OpenPositionValues);
                    FillProrertyList(positions.Where(x => x.IsClosed), prop, isDanger, ClosePositionValues);
                }
                else
                {
                    var attrDealState = positionInfo.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(DealStateAttribute));
                    if (attrDealState != null && (bool) attrDealState.ConstructorArguments[0].Value)
                        FillProrertyList(positions.Where(x => !x.IsClosed), prop, isDanger, OpenPositionValues);
                    if (attrDealState != null && !(bool)attrDealState.ConstructorArguments[0].Value)
                        FillProrertyList(positions.Where(x => x.IsClosed), prop, isDanger, ClosePositionValues);
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод заполнения кортежей OpenPositionValues, ClosePositionValues и OtherPositionValues данными
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="currentProp"></param>
        /// <param name="isDanger">Дополнительный параметр, указывает 'опасено' ли это поле для редактирования</param>
        /// <param name="valueList">Список свойст из таблици представления 'SafePositionEdit'. В этом методе в 'valueList' добавляется всегда
        /// однин элемент (новая строка в таблице представления 'SafePositionEdit'). Но при этом считается статистика</param>
        private static void FillProrertyList(IEnumerable<PositionItem> positions, PropertyInfo currentProp, bool isDanger,  List<SystemProperty> valueList)
        {
            var positionItems = positions as PositionItem[] ?? positions.ToArray();

            //Содержит человеко-понятное имя свойства из атрибута
            var propTitle = currentProp.Name;
            try
            {
                var attrDisplName = currentProp.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof (DisplayNameAttribute));
                if (attrDisplName != null) propTitle = (string) attrDisplName.ConstructorArguments[0].Value;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("FillProrertyList() - {0} {1}", Resource.TextUnableReadAttributeDisplayName, propTitle), ex);
            }

                     
            if (positionItems.Count() == 1)
            {
                var propObj = currentProp.GetValue(positionItems.Single());
                var propObjStr = propObj == null ? string.Empty : Converter.GetStringFromObject(propObj);

                var newRow = new SystemProperty
                    {
                        #region 
                        PropertyType = currentProp.PropertyType,
                        SystemName = currentProp.Name,
                        PropertyTypeName = Utils.GetTypeDescription(currentProp.PropertyType).Item1,
                        PropertyTypeComment = Utils.GetTypeDescription(currentProp.PropertyType).Item2,
                        Title = propTitle,
                        IsDanger = isDanger,
                        Value = propObjStr,
                        Comment = Resource.TextSameInAllSelectedItems,
                        Tag = propObjStr
                        #endregion
                    };

                newRow.SetDescription();
                valueList.Add(newRow);
                return;
            }

            //=============   В случае, если для редактирования было выбрано больше двух строк  ==============
            //сюда будем складывать статистику
            var differentValues = new List<string>(); // примеры значений текущего свойства
            const string strNull = "null";
            const string strEmpty = " - ";

            // собираем статистику по текущему свойству
            foreach (var item in positionItems)
            {
                var propObj = currentProp.GetValue(item);
                var currentValues = propObj == null ? strNull : string.IsNullOrEmpty(propObj.ToString()) ? strEmpty : Converter.GetStringFromObject(propObj);
                if (differentValues.Contains(currentValues)) continue;
                differentValues.Add(currentValues);
            }

            var newMultiRow = new SystemProperty
            {
                #region
                PropertyType = currentProp.PropertyType,
                SystemName = currentProp.Name,
                PropertyTypeName = Utils.GetTypeDescription(currentProp.PropertyType).Item1,
                PropertyTypeComment = Utils.GetTypeDescription(currentProp.PropertyType).Item2,
                Title = propTitle,
                IsDanger = isDanger,
                Value = string.Format("{0}", string.Join(", ", differentValues)),
                Comment = (differentValues.Count < 2) ? Resource.TextSameInAllSelectedItems : string.Format("{0} {1}", Resource.TextNumberDistinctValues, +differentValues.Count),
                Tag = (differentValues.Count == 1) ? differentValues[0].Replace(strNull, string.Empty).Replace(strEmpty, string.Empty) : string.Empty
                #endregion
            };

            newMultiRow.SetDescription();
            valueList.Add(newMultiRow);
        }
    }
}