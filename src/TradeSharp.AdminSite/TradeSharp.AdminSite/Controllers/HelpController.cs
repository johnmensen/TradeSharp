using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.Models.AdminSiteHelp;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    /// <summary>
    /// Контроллер для страниц справки о сайте администратора
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// Содержит соответстивия "уникальный идентификатор квадратика на странице навигации" / "имя страници с названием этого квадратика"
        /// В этом списке не должно быть ключа в виде пустой стоки
        /// </summary>
        private readonly Dictionary<string, string> idPages = new Dictionary<string, string>
            {
                {"Accounts", "AccountManagePage.html"},
                {"NewAccount", "AccountManagePage.html"},
                {"Position", "PositionPage.html"},
                {"PositionEdit", "PositionListEditPage.html"},
                {"PositionDetails", "AccountManagePage.html"}, //TODO подумать как доделать, сейчас этой страници нет
                {"TradeSignal", "OptionsPage.html"},
                {"OwnerDetails", "AccountDetailsPage.html"},
                {"AccountDetails", "AccountDetailsPage.html"},
                {"AccountOwner", "AccountDetailsPage.html"}
            };

        /// <summary>
        /// Сохраняет имя 'pageName' что бы не передавать его в параметрах методов
        /// </summary>
        string imageName = string.Empty;

        public ActionResult AdminSiteHelp(string pageName)
        {
            if (string.IsNullOrEmpty(pageName)) pageName = "IntroductionPage.html";
            var bodyNode = GetBodyTagNodeFromPage(pageName);
            var model = new HelpModel
            {
                MainContent = bodyNode != null ? bodyNode.OuterXml : ""
            };

            if (Request.IsAjaxRequest())
                return PartialView("AdminSiteHelpPartial", model);
            return View(model);
        }

        /// <summary>
        /// Возвращает ВСЮ HTML разментку страници с описанием
        /// </summary>
        /// <param name="pageName">Имя страници, которую нужно достать</param>
        public ActionResult ElementDescriptionPageHTML(string pageName)
        {
            XmlNode html = null;
            if (idPages.ContainsKey(pageName))
                html = GetBodyTagNodeFromPage(idPages[pageName]);
            else
            {
                var node = idPages.FirstOrDefault(x => x.Value == pageName).Value;
                if (node != null) html = GetBodyTagNodeFromPage(node);
            }
            
            var model = new HelpModel
            {
                MainContent = html != null ? html.OuterXml : ""
            };
            return View(model);
        }

        /// <summary>
        /// Асинхронно возвращает на клиент описание элемента 
        /// </summary>
        /// <param name="imgName">Уникальный идентификатор элемента для которого нужно получить описание. 
        /// может быть типа xxxx-xxxx(идентификатор ссылки между страницами) или xxxxxxxxx(идентификатор страници)</param>
        /// <returns></returns>
        public ActionResult ElementDescription(string imgName)
        {
            imageName = imgName;


            string pageName = "AccountManagePage.html";
            if (imgName == "TradeSignal") pageName = "AccountManagePage.html";
            if (imgName == "Position") pageName = "PositionPage.html";
            if (imgName == "PositionEdit") pageName = "PositionListEditPage.html";
            //if (imgName == "PositionDetails") pageName = "PositionListEditPage.html"; //TODO подумать как доделать, сейчас этой страници нет
            if (imgName == "TradeSignal") pageName = "OptionsPage.html";
            if (imgName == "OwnerDetails") pageName = "AccountDetailsPage.html";
            if (imgName == "AccountDetails") pageName = "AccountDetailsPage.html";
            if (imgName == "AccountOwner") pageName = "AccountDetailsPage.html";


            if (imgName == "Accounts-NewAccount") pageName = "AccountManagePage.html";
            if (imgName == "Accounts-Position") pageName = "AccountManagePage.html";
            if (imgName == "Accounts-TradeSignal") pageName = "AccountManagePage.html";
            if (imgName == "Accounts-AccountDetails") pageName = "AccountManagePage.html";
            if (imgName == "Accounts-OwnerDetails") pageName = "AccountManagePage.html";
            if (imgName == "Accounts-AccountDetails") pageName = "AccountDetailsPage.html";
            if (imgName == "AccountDetails-AccountOwner") pageName = "AccountDetailsPage.html";
            if (imgName == "OwnerDetails-AccountOwner") pageName = "AccountDetailsPage.html";
            if (imgName == "AccountDetails-Position") pageName = "AccountDetailsPage.html";
            if (imgName == "Position-PositionEdit") pageName = "PositionPage.html";
            if (imgName == "Position-PositionDetails") pageName = "PositionPage.html";


            var bodyNode = GetBodyTagNodeFromPage(pageName);
            var result = TextSelector(bodyNode);

            return Content(result);
        }

        /// <summary>
        /// окно выбора отчета (не самого отчета!)
        /// </summary>
        public ActionResult BrokerReport()
        {
            return View("BrokerReport");
        }

        public ActionResult NavigationDiagram()
        {
            return View();
        }

        /// <summary>
        /// Обратная связь (Администратор может написать о пожеланиях, предложениях и обнаруженных ошибках)
        /// </summary>
        /// <returns></returns>
        public ActionResult Feedback()
        {
            return View();
        }

        /// <summary>
        /// Вытаскивает элемент Body из указанной html страници
        /// </summary>
        /// <param name="pageName">Имя страници, из которой нужно получить тег Body</param>
        /// <returns>таг Body в xml-е подобном виде</returns>
        private XmlNode GetBodyTagNodeFromPage(string pageName)
        {
            
            var path = Server.MapPath(string.Format("~/HelpPages/{0}/{1}", Resource.DatepickerRegional, pageName));

            XmlNode bodyNode = null;
            try
            {
                string line;
                using (var reader = new StreamReader(path)) line = reader.ReadToEnd();

                var htmlDocument = new XmlDocument();
                htmlDocument.Load(new StringReader(line));
                var bodyElement = htmlDocument.GetElementsByTagName("body");
                if (bodyElement.Count > 0)
                    bodyNode = bodyElement.Item(0);
            }
            catch (Exception ex)
            {
                Logger.Error("AdminSiteHelp", ex);
            }
            return bodyNode;
        }

        private string TextSelector(XmlNode node)
        {
            var result = new StringBuilder();

            if (node != null)
            {
                if (node.Attributes != null &&
                    node.Attributes.Cast<XmlAttribute>()
                        .Count(attr => attr.Name == "data-description" && attr.Value.Split(' ').Contains(imageName)) > 0)
                {
                    result.Append(node.OuterXml);
                    return result.ToString();
                }
                if (!node.HasChildNodes) return result.ToString();

                foreach (XmlNode childNode in node.ChildNodes)
                    result.Append(TextSelector(childNode));
            }

            return result.ToString();
        }
    }
}
