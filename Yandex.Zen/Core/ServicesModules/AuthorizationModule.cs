﻿using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.Enums.Http;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.ServicesModules
{
    public class AuthorizationModule : IAuthorizationModule
    {
        private readonly Random _rnd = new Random();
        private BrowserBusySettingsModel _settingsMode;
        private bool _endOfAuthProcess;
        private bool _isSuccess;

        public AuthorizationModule(DataManager manager)
        {
            Manager = manager;
        }

        public bool IsSuccess { get => _isSuccess; }
        private DataManager Manager { get; set; }

        /// <summary>
        /// Авторизация.
        /// </summary>
        public void Authorization() => Authorization(out _);

        /// <summary>
        /// Авторизация.
        /// </summary>
        /// <param name="isSuccess"></param>
        public void Authorization(out bool isSuccess)
        {
            _settingsMode = Manager.Browser.BrowserGetCurrentBusySettings();

            HE xAvatar = new HE("//div[contains(@class, 'desk-notif-card')]/descendant::a[contains(@class, 'avatar')]", "Аватар пользователя");
            HE xFieldLogin = new HE("//input[@name='login']", "Логин");
            HE xFieldPass = new HE("//input[contains(@type, 'password')]", "Пароль");
            HE xButtonSubmit = new HE("//button[@type='submit']", "Подтвердить вход");
            HE xFormChangePass = new HE("//div[contains(@class, 'change-password-page') and contains(@class, 'passp-auth-screen')]", "Доступ к аккаунту ограничен");
            HE xButtonChangePassNext = new HE("//div[contains(@data-t, 'submit-change-pwd')]/descendant::button[contains(@data-t, 'action')]", "Подтвердить смену пароля");
            HE xFieldAnswer = new HE("//input[contains(@name, 'answer')]", "Ответа на контрольный вопрос");
            HE xButtonAnswer = new HE("//div[contains(@data-t, 'submit-check-answer')]/button", "Подтвердить ввод ответа на контрольный вопрос");

            var browser = Manager.Browser;
            var account = (IAccount)Manager.CurrentResourceObject;

            var log = new LogSettings(false, true, true);
            var firstStart = true;
            var counterAttempts = 0;

            while (true)
            {
                if (firstStart)
                {
                    browser.ActiveTab.Navigate("https://yandex.ru/", true);

                    if (xAvatar.TryFindElement(3))
                    {
                        Logger.Write("Аккаунт уже авторизирован", LoggerType.Info, true, false, false);
                        isSuccess = _isSuccess = true;
                        break;
                    }
                    else firstStart = false;
                }

                if (++counterAttempts > 3)
                {
                    Logger.Write("Слишком много ошибок в время авторизации", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string> { browser.ActiveTab.URL });
                    isSuccess = _isSuccess = false;
                    return;
                }

                browser.ActiveTab.Navigate("https://passport.yandex.ru/auth?origin=home_yandexid&retpath=https%3A%2F%2Fyandex.ru&backpath=https%3A%2F%2Fyandex.ru", "https://yandex.ru/", true);

                if (!xFieldLogin.TryFindElement(3, log)) continue;
                else xFieldLogin.SetValue(account.Login, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));

                if (!xButtonSubmit.TryFindElement(3, log)) continue;
                else xButtonSubmit.Click(_rnd.Next(250, 500));

                if (!xFieldPass.TryFindElement(3, log)) continue;
                else xFieldPass.SetValue(account.Password, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));

                if (!xButtonSubmit.TryFindElement(3, log)) continue;
                else xButtonSubmit.Click(_rnd.Next(250, 500));

                #region ====[ОГРАНИЧЕНИЕ ДОСТУПА И СМЕНА ПАРОЛЯ]===============================
                if (xFormChangePass.TryFindElement(3, null))
                {
                    Logger.Write("Восстановление доступа", LoggerType.Info, true, false, true);

                    if (!xButtonChangePassNext.TryFindElement(3, log)) continue;
                    else xButtonChangePassNext.Click(_rnd.Next(250, 500));

                    #region ====[РАСПОЗНАВАНИЕ КАПЧИ]==================
                    browser.UseTrafficMonitoring = true;
                    if (!TryRecognizeCaptcha()) continue;
                    browser.UseTrafficMonitoring = false;

                    if (!xFieldAnswer.TryFindElement(3, log)) continue;
                    else xFieldAnswer.SetValue(account.AnswerQuestion, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));

                    if (!xButtonAnswer.TryFindElement(3, log)) continue;
                    else xButtonAnswer.Click(_rnd.Next(250, 500));
                    #endregion ========================================

                    #region ====[ПРИВЯЗКА НОМЕРА К АККАУНТУ]===========
                    if (!TryBindNumberPhone())
                    {
                        if (_endOfAuthProcess is false) continue;
                        isSuccess = _isSuccess = false;
                        return;
                    }
                    else
                    {
                        isSuccess = _isSuccess = true;
                        break;
                    }
                    #endregion ========================================
                }
                else
                {
                    /*
                     * TODO: Доработать гуд авторизацию, если нет формы восстановления доступа
                     */
                }
                #endregion ==================================================================
            }

            /* TODO: Добавить условие, если пароль был только что привязан
             *       Если есть номер в таблице, то не проверять его в настройках
             */
            var phoneBilded = default(bool);

            if (string.IsNullOrWhiteSpace(account.PhoneNumber) && !(phoneBilded = CheckPhoneNumberBinding()))
            {
                Logger.Write("К аккаунту не привязан номер", LoggerType.Info, true, false, true, LogColor.Yellow);
                isSuccess = _isSuccess = false;

                /* 
                 * TODO: -Временно помечать аккаунты, что они авторизированы, но требуется привязка номера.
                 *      - вносить в таблицу: 'AuthYesPhoneNo'
                 * 
                 *       -Идти привязывать номер
                 * 
                 *       !(возможно нужно проверять номер в дзен)
                 * 
                 *       Task level: very-low
                 */
            }
            else if (string.IsNullOrWhiteSpace(account.PhoneNumber) && phoneBilded)
            {
                Logger.Write("К аккаунту привязан номер, но сам номер отсутствует в таблице", LoggerType.Info, true, false, true);
                isSuccess = _isSuccess = false;
                /* 
                 * TODO: Нужно реализовать поиск по файлу лога аккаунта и внести этот номер в таблицу
                 *       Пока мы не реализовали эту логику - вносить в таблицу: 'Search'
                 */
            }
            else
            {
                Logger.Write("К аккаунту привязан номер", LoggerType.Info, true, false, true);
            }

            browser.BrowserSetBusySettings(_settingsMode);
        }

        /// <summary>
        /// Разгадывание капчи.
        /// </summary>
        /// <returns></returns>
        private bool TryRecognizeCaptcha()
        {
            HE xFieldCaptcha = new HE("//input[contains(@name, 'captcha_answer')]", "Поле капчи");
            HE xImgCaptcha = new HE("//div[@class='captcha__container']/descendant::img[@src!='']", "Изображение капчи");
            HE xButtonCaptchaNext = new HE("//div[contains(@data-t, 'submit-captcha')]/button", "Подтвердить ввод капчи");

            var browser = Manager.Browser;
            var captchaService = Manager.CurrentResourceObject.CaptchaService;

            var log = new LogSettings(false, true, true);
            var attempts = 0;
            _ = browser.ActiveTab.GetTraffic();

            while (true)
            {
                if (++attempts > 3)
                {
                    Logger.Write("Слишком много ошибок во время разгадывания капчи", LoggerType.Warning, true, false, true, LogColor.Yellow);
                    return false;
                }

                if (!xImgCaptcha.TryFindElement(3, log)) continue;
                if (!xFieldCaptcha.TryFindElement(3, log)) continue;
                if (!xButtonCaptchaNext.TryFindElement(3, log)) continue;

                // Разгадывание и ввод капчи
                if (captchaService.TryRecognize(xImgCaptcha.Element, out var result))
                {
                    Logger.Write(captchaService.LogMessage, LoggerType.Info, true, false, true);
                    xFieldCaptcha.SetValue(result, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));
                    xButtonCaptchaNext.Click(_rnd.Next(250, 500));
                }
                else continue;

                // Проверка ответа разгадывания
                var btBody = browser.ActiveTab.GetTraffic()
                    .Where(x => x.Url.Contains("registration-validations/checkHuman"))
                    .LastOrDefault()?.ResponseBody;

                if (btBody != null)
                {
                    var responseBody = Encoding.UTF8.GetString(btBody, 0, btBody.Length);
                    var body = Regex.Match(responseBody, "(?<=\"status\":\").*?(?=\")").Value;

                    switch (body)
                    {
                        case string b when "ok".Equals(b, StringComparison.OrdinalIgnoreCase):
                        {
                            Logger.Write("Капча успешно разгадана", LoggerType.Info, true, false, true);
                        }
                        return true;

                        case string b when "error".Equals(b, StringComparison.OrdinalIgnoreCase):
                        {
                            Logger.Write("Капча разгадана неверно", LoggerType.Warning, true, false, true);
                        }
                        continue;

                        default:
                        {
                            Logger.Write($"Неизвестная ошибка: '{nameof(responseBody)}:{responseBody}'", LoggerType.Warning, true, false, true);
                        }
                        return false;
                    }

                    //if (body.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    Logger.Write("Капча успешно разгадана", LoggerType.Info, true, false, true);
                    //    return true;
                    //}
                    //else if (body.Equals("error", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    Logger.Write("Капча разгадана неверно", LoggerType.Warning, true, false, true);
                    //    continue;
                    //}
                    //else
                    //{
                    //    Logger.Write($"Неизвестная ошибка: '{nameof(responseBody)}:{responseBody}'", LoggerType.Warning, true, false, true);
                    //    return false;
                    //}
                }
                else
                {
                    Logger.Write("В трафике не найден ответ от 'yandex' на разгадывание капчи", LoggerType.Warning, true, false, true, LogColor.Yellow);
                    return false;
                }
            }
        }

        /// <summary>
        /// Привязка номера.
        /// </summary>
        /// <returns></returns>
        private bool TryBindNumberPhone()
        {
            HE xFieldPhone = new HE("//input[contains(@name, 'phone')]", "Номер телефона");
            HE xButtonPhoneNext = new HE("//div[contains(@data-t, 'submit-send-code')]/button", "Подтвердить ввод телефона");

            HE xFieldSmsCode = new HE("//input[contains(@data-t, 'phoneCode')]", "SMS Код");
            HE xButtonSmsCodeNext = new HE("//div[contains(@class, 'PhoneConfirmationCode')]/button[contains(@data-t, 'action')]", "Подтвердить ввод кода");
            HE xButtonSmsCodeReSend = new HE("//button[contains(@data-t, 'retry-to-request-code')]", "Отправить ещё sms код");

            HE xFieldNewPass = new HE("//input[contains(@data-t, 'input-password') and not(contains(@data-t, 'confirm'))]", "Новый пароль");
            HE xFieldNewPassConfirm = new HE("//input[contains(@data-t, 'input-password_confirm')]", "Новый пароль");
            HE xButtonNewPassNext = new HE("//div[contains(@data-t, 'commit-password')]/descendant::button[contains(@data-t, 'action')]", "Пдтвердить новый пароль");
            HE xButtonFinish = new HE("//div[contains(@data-t, 'submit-finish')]/descendant::*[contains(@data-t, 'action')]", "Финиш");
            HE xButtonAccountDetailsConfirm = new HE("//div[contains(@data-t, 'check-data-submit')]/descendant::*[contains(@data-t, 'action')]", "Подтвердить данные аккаунта");

            var account = (IAccount)Manager.CurrentResourceObject;
            var smsService = account.SmsService;

            _endOfAuthProcess = false;
            var log = new LogSettings(false, true, true);

            if (!xFieldPhone.TryFindElement(3, log) || !xButtonPhoneNext.TryFindElement(3, log)) return false;

            #region ====[ПОЛУЧЕНИЕ И ВВОД НОМЕРА + ОТПРАВКА КОДА]============
            if (!smsService.TryGetPhoneNumber())
            {
                Logger.Write(smsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                _endOfAuthProcess = true;
                return false;
            }
            else Logger.Write(smsService.LogMessage, LoggerType.Info, true, false, true, LogColor.Blue);

            xFieldPhone.SetValue(smsService.Data.NumberPhone, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));
            xButtonPhoneNext.Click(_rnd.Next(250, 500));

            if (!xFieldSmsCode.TryFindElement(3, log) ||
                !xButtonSmsCodeNext.TryFindElement(3, log) ||
                !xButtonSmsCodeReSend.TryFindElement(3, log))
            {
                smsService.CancelPhoneNumber();
                Logger.Write(smsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                _endOfAuthProcess = true;
                return false;
            }
            #endregion =======================================================

            #region ====[ПОЛУЧЕНИЕ И ВВОД КОДА]===============================
            if (!smsService.TryGetSmsCode(false))
            {
                Logger.Write(smsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                smsService.CancelPhoneNumber();
                Logger.Write(smsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                _endOfAuthProcess = true;
                return false;
            }
            xFieldSmsCode.SetValue(smsService.Data.SmsCodeOrStatus, LevelEmulation.SuperEmulation, _rnd.Next(1500, 3000));
            #endregion =======================================================

            #region ====[ГЕНЕРАЦИЯ И УСТАНОВКА НОВОГО ПАРОЛЯ]=================
            if (!xFieldNewPass.TryFindElement(3, log) ||
                !xFieldNewPassConfirm.TryFindElement(3, log) ||
                !xButtonNewPassNext.TryFindElement(3, log))
            {
                _endOfAuthProcess = true;
                return false;
            }

            account.GenerateNewPassword();
            xFieldNewPass.SetValue(account.Password, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));
            xFieldNewPassConfirm.SetValue(account.Password, LevelEmulation.SuperEmulation, _rnd.Next(250, 500));
            xButtonNewPassNext.Click(_rnd.Next(1500, 3000));

            if (!xButtonFinish.TryFindElement(3, log))
            {
                account.Profile.Save();
                _endOfAuthProcess = true;
                Logger.Write("Не удалось определить успешность завершения смены пароля", LoggerType.Info, true, false, true, LogColor.Yellow);
                return false;
            }
            else
            {
                xButtonFinish.Click(_rnd.Next(4000, 5000));
                if (xButtonAccountDetailsConfirm.TryFindElement(3, log))
                    xButtonAccountDetailsConfirm.Click(_rnd.Next(1500, 3000));
                account.Profile.Save();
                /*
                 * TODO: Сохранить номер аккаунта в таблицу и новый пароль
                 */
                Logger.Write("Пароль успешно изменен/номер успешно привязан к аккаунту", LoggerType.Info, true, false, true, LogColor.Green);
            }
            return true;
            #endregion ========================================================
        }

        /// <summary>
        /// Проверка привязки номера к аккаунту.
        /// </summary>
        /// <returns>true - номер привязан; иначе - false.</returns>
        public bool CheckPhoneNumberBinding()
        {
            var userAgent = Manager.Zenno.Profile.UserAgent;
            var proxy = Manager.Browser.GetProxy();
            var cookies = Manager.Zenno.Profile.CookieContainer;

            var httpResponse = ZennoPoster.HTTP.Request
            (
                HttpMethod.GET, "https://passport.yandex.ru/profile",
                UserAgent: userAgent,
                proxy: proxy,
                respType: ResponceType.BodyOnly,
                cookieContainer: cookies
            );
            return !string.IsNullOrWhiteSpace(Regex.Match(httpResponse, "(?<=\"number\":\").*?(?=\")").Value);
        }
    }
}