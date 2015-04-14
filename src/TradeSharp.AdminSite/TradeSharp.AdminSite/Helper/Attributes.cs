using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Helper
{
    // Содержаться самописные атрибуты, которых нет среди стандартных. Стандартные атрибуты, переопределённые для локалихации
    // находятся в папке "\BL\Localisation\"

    /// <summary>
    /// Массив Не допустимых значений
    /// </summary>
    public class ValidationStringArrayUnallowableAttribute : ValidationAttribute
    {
        private const string ErrorMessageText = "Error";

        /// <summary>
        /// Множество не допустимых значений
        /// </summary>
        public string[] unallowableValue = null;

        public override bool IsValid(object value)
        {
            ErrorMessage = ErrorMessageText;
            if (value == null) return true;

            try
            {
                ErrorMessage = Resource.ErrorMessageInvalidFieldValue;

                var testVal = value.ToString();
                if (unallowableValue != null && !unallowableValue.Select(x => x.ToLower()).Contains(testVal.ToLower())) return true;
            }
            catch (Exception ex)
            {
                Logger.Error("ValidationStringArrayUnallowableAttribute", ex);
                return false;
            }
            return false;
        }
    }

    /// <summary>
    /// Допустимые целочисленные значения
    /// </summary>
    public class ValidationArrayAllowAttribute : ValidationAttribute
    {
        private const string ErrorMessageText = "Error";

        /// <summary>
        /// Множество допустимых значений
        /// </summary>
        public int[] allowValue = null;

        public override bool IsValid(object value)
        {
            ErrorMessage = ErrorMessageText;
            if (value == null) return true;

            try
            {
                ErrorMessage = Resource.ErrorMessageInvalidFieldValue;
                var testVal = (int)value;
                if (allowValue != null && allowValue.Contains(testVal)) return true;
            }
            catch (Exception ex)
            {
                Logger.Error("ValidationArrayAllowAttribute", ex);
                return false;
            }
            return false;
        }
    }

    public class ValidationVectorAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return true;

            try
            {
                var testVal = (float)value;
                if (testVal > 0) return true;
            }
            catch (Exception ex)
            {
                Logger.Error("ValidationVectorAttribute", ex);
                return false;
            }
            return false;
        }
    }

    /// <summary>
    /// Хранит цвет, которым описывается степень опасности изменения свойства.
    /// </summary>
    public class DanderRangeAttribute : Attribute
    {
        public Tuple<bool, Color, string> DangerRanger { get; private set; }

        /// <summary>
        /// Конструктор аттрибута
        /// </summary>
        /// <param name="dangerRanger">Опасно ли свойство для изменения (true - значит опасно)</param>
        /// <param name="tooltipResourceName">Комментарий, что бидет происходить, при изменении этого свойства</param>
        public DanderRangeAttribute(bool dangerRanger, string tooltipResourceName)
        {
            var color = Color.Blue;
            if (dangerRanger) color = Color.Red;

            var localisationTooltip = string.Empty;
            try
            {
                localisationTooltip = Resource.ResourceManager.GetString(tooltipResourceName, CultureInfo.CurrentUICulture);
            }
            catch (Exception ex)
            {
                Logger.Error("DanderRangeAttribute", ex);
            }
            DangerRanger = new Tuple<bool, Color, string>(dangerRanger, color, localisationTooltip);
        }
    }

    /// <summary>
    /// Аттрибут статуса сделки - открыта, закрыта. 
    /// </summary>
    public class DealStateAttribute : Attribute
    {
        public bool IsOpen { get; private set; }

        public DealStateAttribute(bool isOpen)
        {
            IsOpen = isOpen;
        }
    }
}