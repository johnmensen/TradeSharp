using System.Collections.Generic;
using System.Linq;

namespace Entity
{
    /// <summary>
    /// содержит методы-расширения для работы со множествами
    /// например, проверить: все ли элементы множества равны между собой
    /// </summary>
    public static class SetManageExtensions
    {
        #region AreEqual
        
        /// <summary>
        /// равны ли списки между собой?
        /// все либо null, либо содержат одинаковое количество
        /// одинаковых элементов
        /// </summary>
        public static bool AreEqual(this IEnumerable<IEnumerable<int>> lst)
        {
            if (lst == null) return true;

            IEnumerable<int> lastItem = null;
            bool firstCompare = true;

            foreach (var innerList in lst)
            {
                if (firstCompare)
                {
                    firstCompare = false;
                    lastItem = innerList;
                    continue;
                }
                if (lastItem == null && innerList != null) return false;                
                if (lastItem != null && innerList == null) return false;
                if (lastItem == null && innerList == null) continue;
                if (!lastItem.SequenceEqual(innerList)) return false;
                lastItem = innerList;
            }
            return true;
        }

        /// <summary>
        /// если параметров нет - проверить, равны ли между собой
        /// иначе проверить, равен ли каждый элемент любому из переданных параметров
        /// </summary>        
        public static bool AreEqual(this IEnumerable<bool> lst)
        {
            bool? compradant = null;
            foreach (var i in lst)
            {
                if (compradant.HasValue == false)
                {
                    compradant = i;
                    continue;
                }
                if (compradant != i) return false;
            }
            return true;
        }

        /// <summary>
        /// если параметров нет - проверить, равны ли между собой
        /// иначе проверить, равен ли каждый элемент любому из переданных параметров
        /// </summary>        
        public static bool AreEqual(this IEnumerable<int> lst, params int[] comparers)
        {
            int? compradant = null;
            foreach (var i in lst)
            {
                if (comparers.Length == 0)
                {
                    if (compradant.HasValue == false)
                    {
                        compradant = i;
                        continue;
                    }
                    if (compradant != i) return false;
                    continue;
                }
                var hasSame = false;
                foreach (var cr in comparers)
                {
                    if (i == cr) 
                    {
                        hasSame = true;
                        break;
                    }
                }
                if (!hasSame) return false;
            }
            return true;
        }

        /// <summary>
        /// если параметров нет - проверить, равны ли между собой
        /// иначе проверить, равен ли каждый элемент любому из переданных параметров
        /// </summary>        
        public static bool AreEqual(this IEnumerable<decimal> lst, params decimal[] comparers)
        {
            decimal? compradant = null;
            foreach (var i in lst)
            {
                if (comparers.Length == 0)
                {
                    if (compradant.HasValue == false)
                    {
                        compradant = i;
                        continue;
                    }
                    if (compradant != i) return false;
                    continue;
                }
                var hasSame = false;
                foreach (var cr in comparers)
                {
                    if (i == cr)
                    {
                        hasSame = true;
                        break;
                    }
                }
                if (!hasSame) return false;
            }
            return true;
        }

        /// <summary>
        /// если параметров нет - проверить, равны ли между собой
        /// иначе проверить, равен ли каждый элемент любому из переданных параметров
        /// </summary>        
        public static bool AreEqual(this IEnumerable<double> lst, params double[] comparers)
        {
            double? compradant = null;
            foreach (var i in lst)
            {
                if (comparers.Length == 0)
                {
                    if (compradant.HasValue == false)
                    {
                        compradant = i;
                        continue;
                    }
                    if (compradant != i) return false;
                    continue;
                }
                var hasSame = false;
                foreach (var cr in comparers)
                {
                    if (i == cr)
                    {
                        hasSame = true;
                        break;
                    }
                }
                if (!hasSame) return false;
            }
            return true;
        }

        /// <summary>
        /// если параметров нет - проверить, равны ли между собой
        /// иначе проверить, равен ли каждый элемент любому из переданных параметров
        /// </summary>        
        public static bool AreEqual(this IEnumerable<string> lst, params string[] comparers)
        {
            string compradant = null;
            var compInitiated = false;

            foreach (var i in lst)
            {
                if (comparers.Length == 0)
                {
                    if (!compInitiated)
                    {
                        compradant = i;
                        compInitiated = true;
                        continue;
                    }
                    if (compradant != i) return false;
                    continue;
                }
                var hasSame = false;
                foreach (var cr in comparers)
                {
                    if (i == cr)
                    {
                        hasSame = true;
                        break;
                    }
                }
                if (!hasSame) return false;
            }
            return true;
        }
        #endregion
    }
}
