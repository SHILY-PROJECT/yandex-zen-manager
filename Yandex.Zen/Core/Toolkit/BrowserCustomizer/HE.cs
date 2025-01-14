﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool.Models;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer
{
    public class HE
    {
        [ThreadStatic] private static Instance _browser;
        [ThreadStatic] private static DataManager _dataManager;

        public HE(string xpath) => XPath = xpath;
        public HE(string xpath, string descriptionXPath) : this(xpath) => Description = descriptionXPath;
        public HE(string xpath, string descriptionXPath, HtmlElement htmlElement) : this(xpath, descriptionXPath) => Element = htmlElement;
        public HE(string xpath, string descriptionXPath, List<HtmlElement> htmlElementCollection)
            : this(xpath, descriptionXPath) => Collection = htmlElementCollection;

        /// <summary>
        /// XPath путь к элементу.
        /// </summary>
        public string XPath { get; set; } = string.Empty;

        /// <summary>
        /// Информация об элементе (название).
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Экземпляр  html элемента.
        /// </summary>
        public HtmlElement Element { get; set; }

        /// <summary>
        /// Экземпляр списка html элементов.
        /// </summary>
        public List<HtmlElement> Collection { get; set; }

        /// <summary>
        /// Информация для логирования. 'XPath' и 'Info' в форматированном виде.
        /// </summary>
        public string InformationForLog
        {
            get
            {
                var text = "Element not found.";

                text += !string.IsNullOrWhiteSpace(XPath) ? $"{nameof(XPath)}: {XPath}" : "";
                text += !string.IsNullOrWhiteSpace(Description) ? $"{nameof(Description)}: {Description}" : "";

                return text;
            }
        }

        public static void ConfigureGlobalBrowse(DataManager manager)
        {
            _dataManager = manager;
            _browser = manager.Browser;
        }

        /// <summary>
        /// Установить значение.
        /// </summary>
        public void SetValue(string value, LevelEmulation levelEmulation, int msTimeoutAfterAction,
            bool autoFindElement = true, int attemptsFindElement = 3, bool exceptionIfNotFind = true, LogSettings log = null)
        {
            if (Element.IsNullOrVoid() && autoFindElement)
                FindElement(attemptsFindElement, exceptionIfNotFind, log);

            Element.SetValue(_browser.ActiveTab, value, levelEmulation, msTimeoutAfterAction);
        }

        /// <summary>
        /// Клик по элементу.
        /// </summary>
        public void Click(int msTimeoutAfterAction = 0, bool waitPageLoad = true,
            bool autoFindElement = true, int attemptsFindElement = 3, bool exceptionIfNotFind = true, LogSettings log = null)
        {
            if (Element.IsNullOrVoid() && autoFindElement)
                FindElement(attemptsFindElement, exceptionIfNotFind, log);

            Element.Click(_browser.ActiveTab, msTimeoutAfterAction, waitPageLoad);
        }

        /// <summary>
        /// Пытаться найти элемент.
        /// </summary>
        /// <param name="attemptsFindElement"></param>
        /// <param name="logError"></param>
        /// <returns></returns>
        public bool TryFindElement(int attemptsFindElement = 3, LogSettings log = null)
        {
            Element = _browser.FindFirstElement(this, false, false, attemptsFindElement);

            if (Element.IsNullOrVoid() is false) return true;

            if (log != null && log.IsOn)
                Logger.Write(InformationForLog, LoggerType.Warning, log.Resource, log.General, log.ZennoPoster, LogColor.Yellow);

            return false;
        }

        /// <summary>
        /// Пытаться найти элементы.
        /// </summary>
        /// <param name="attemptsFindElement"></param>
        /// <param name="logError"></param>
        /// <returns></returns>
        public bool TryFindElements(int attemptsFindElement = 3, LogSettings log = null)
        {
            Collection = _browser.FindElements(this, false, false, attemptsFindElement).ToList();

            if (Collection.Any() is false) return true;

            if (log != null && log.IsOn)
                Logger.Write(InformationForLog, LoggerType.Warning, log.Resource, log.General, log.ZennoPoster, LogColor.Yellow);

            return false;
        }

        /// <summary>
        /// Поиск элемента.
        /// </summary>
        /// <param name="attemptsFindElement"></param>
        /// <param name="throwExceptionIfNotFind"></param>
        /// <param name="logError"></param>
        public void FindElement(int attemptsFindElement = 3, bool throwExceptionIfNotFind = true, LogSettings log = null)
        {
            Element = _browser.FindFirstElement(this, false, false, attemptsFindElement);

            if (Element.IsNullOrVoid())
            {
                if (log != null && log.IsOn)
                    Logger.Write(InformationForLog, LoggerType.Warning, log.Resource, log.General, log.ZennoPoster, LogColor.Yellow);

                if (throwExceptionIfNotFind)
                    throw new Exception(InformationForLog);
            }
        }

        /// <summary>
        /// Поиск элементов.
        /// </summary>
        /// <param name="attemptsFindElement"></param>
        /// <param name="throwExceptionIfNotFind"></param>
        /// <param name="logError"></param>
        public void FindElements(int attemptsFindElement = 3, bool throwExceptionIfNotFind = true, LogSettings log = null)
        {
            Collection = _browser.FindElements(this, false, false, attemptsFindElement).ToList();

            if (Collection.Any() is false)
            {
                if (log != null && log.IsOn)
                    Logger.Write(InformationForLog, LoggerType.Warning, log.Resource, log.General, log.ZennoPoster, LogColor.Yellow);

                if (throwExceptionIfNotFind)
                    throw new Exception(InformationForLog);
            }
        }
    }
}
