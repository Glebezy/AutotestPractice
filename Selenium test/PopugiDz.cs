using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Selenium_test
{
    public class PopugiDz
    {
        public ChromeDriver driver;
        public WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }
        
        private const string resulttext = "Хорошо, мы пришлём имя ";
        private string girlcall = resulttext + "для вашей девочки на e-mail:";
        private string boycall = resulttext + "для вашего мальчика на e-mail:";
        private string testUrl = "https://qa-course.kontur.host/selenium-practice/";
        
        private By emailInputLocator = By.Name("email");
        private By formError = By.ClassName("form-error");
        private By buttonLocator = By.Id("sendMe");
        private By emailResultLocator = By.ClassName("your-email");
        private By anotherEmailLinkLocator = By.Id("anotherEmail");
        private By resultblock = By.CssSelector("#resultTextBlock"); 
        private By radioBoyLocator = By.Id("boy");
        private By radioGirlLocator = By.Id("girl");
        private By resulttextLocator = By.ClassName("result-text");

        public class AssertsAccumulator
        {
            private StringBuilder Errors { get; set; }
            private bool AssertsPassed { get; set; }

            private string AccumulatedErrorMessage
            {
                get
                {
                    return Errors.ToString();
                }
            }

            public AssertsAccumulator()
            {
                Errors = new StringBuilder();
                AssertsPassed = true;
            }

            private void RegisterError(string exceptionMessage)
            {
                AssertsPassed = false;
                Errors.AppendLine(exceptionMessage);
            }

            public void Accumulate(Action assert)
            {
                try
                {
                    assert.Invoke();
                }
                catch (Exception exception)
                {
                    RegisterError(exception.Message);
                }
            }

            public void Release()
            {
                if (!AssertsPassed)
                {
                    throw new AssertionException(AccumulatedErrorMessage);
                }
            }
        }

        public void SendForm()
        {
            driver.FindElement(emailInputLocator).SendKeys("test@test.com");
            driver.FindElement(buttonLocator).Click();
        }

        [TestCase("te.st@test.com", TestName = "EngMail")]
        [TestCase("Йцивет1@домен.рф", TestName = "RuMail")]
        [TestCase("test/test@test.com", TestName = "SlashMail")]
        [TestCase("x@example.com", TestName = "1charLocalMail")]
        [TestCase("exampl-ed@e-example.com", TestName = "hyphenMail")]
        [TestCase("test@tes2t.com", TestName = "NumberDomainMail")]
        [TestCase("tes2t@test.com", TestName = "NumberLocalMail")]
        public void PopugSite_FillFormEmail_Success(string TextMail)
        {
            driver.Navigate().GoToUrl(testUrl);
            driver.FindElement(emailInputLocator).SendKeys(TextMail);
            driver.FindElement(buttonLocator).Click();

            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.AreEqual(TextMail, driver.FindElement(emailResultLocator).Text,"Емейлы не совпали"));
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(resulttextLocator).Displayed, "Текст результата отображается"));
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(anotherEmailLinkLocator).Displayed, "Ссылка для другого емейла не отображается"));
            assertsAccumulator.Release();
        }

        [TestCase("Abc.example.com", TestName = "w/o@Mail")]
        [TestCase("A@b@c@example.com", TestName = "w/@@Mail")]
        [TestCase("1234567890123456789012345678901234567890123456789012345678901234x@example.com", TestName = "Max64charLocalMail")]
        [TestCase("ae@.", TestName = "Min2charLabelDomainMail")]
        [TestCase("ae@12.1234567890123456789012345678901234567890123456789012345678901234", TestName = "Max63charLabelDomainMail")]
        [TestCase("@example.com", TestName = "EmptyLocalMail")]
        [TestCase("Abc@", TestName = "EmptyDomainMail")]
        [TestCase("John..Doe@example.com", TestName = "doubledotLocalMail")]
        [TestCase(".re@example.com", TestName = "FirstDotLocalMail")]
        [TestCase("re.@example.com", TestName = "LastDotLocalMail")]
        [TestCase("c@examplecom", TestName = "w/oDotDomainMail")]
        [TestCase("test@test.c om", TestName = "SpaceDomainMail")]
        [TestCase("te st@test.cm", TestName = "SpaceLocalMail")]
        public void PopugSite_InvalidFillFormEmail_Errors(string TextMail)
        {
            driver.Navigate().GoToUrl(testUrl);
            driver.FindElement(emailInputLocator).SendKeys(TextMail);
            driver.FindElement(buttonLocator).Click();

            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.AreEqual("error", driver.FindElement(emailInputLocator).GetAttribute("class"), "поле не подсвечивается"));
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(formError).Displayed, "Текст ошибки не отображается"));
            assertsAccumulator.Accumulate(() => Assert.AreEqual("Некорректный email", driver.FindElement(formError).Text, "Неверный текст ошибки"));
            assertsAccumulator.Release();
        }

        [Test]
        public void PopugSite_EmptyFormEmail_Error()
        {
            driver.Navigate().GoToUrl(testUrl);
            driver.FindElement(buttonLocator).Click();

            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.AreEqual("error", driver.FindElement(emailInputLocator).GetAttribute("class"), "поле не подсвечивается"));
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(formError).Displayed, "Текст ошибки не отображается"));
            assertsAccumulator.Accumulate(() => Assert.AreEqual("Введите email", driver.FindElement(formError).Text, "Неверный текст ошибки"));
            assertsAccumulator.Release();
        }

        [Test]
        public void PopugSite_AnotherEmailLink_FreeFormEmail()
        {
            driver.Navigate().GoToUrl(testUrl);
            SendForm();
            driver.FindElement(anotherEmailLinkLocator).Click();

            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.AreEqual(string.Empty,driver.FindElement(emailInputLocator).Text, "строка емейла не отчистилась"));
            assertsAccumulator.Accumulate(() => Assert.IsEmpty(driver.FindElements(resultblock), "Поле с сообщением осталось"));
            assertsAccumulator.Accumulate(() => Assert.IsFalse(driver.FindElement(anotherEmailLinkLocator).Displayed, "Не исчезла ссылка"));
            assertsAccumulator.Release();
        }

        [Test]
        public void PopugSite_RadioGender_Preselected()
        {
            driver.Navigate().GoToUrl(testUrl);
            SendForm();

            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(radioBoyLocator).Selected, "Мальчик не выбран по умолчанию"));
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(resulttextLocator).Text.Contains(boycall), "Предварительное радио не влияет на пол"));
            assertsAccumulator.Release();
        }

        [Test]
        public void PopugSite_RadioGender_Enabled()
        {
            driver.Navigate().GoToUrl(testUrl);

            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(radioBoyLocator).Enabled, "Радиокнопка мальчика отключена"));
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(radioGirlLocator).Enabled, "Радиокнопка девочки отключена"));
            assertsAccumulator.Release();
        }

        [Test]
        public void PopugSite_RadioGender_GirlResultText()
        {
            driver.Navigate().GoToUrl(testUrl);
            driver.FindElement(radioGirlLocator).Click();
            SendForm();
            
            Assert.IsTrue(driver.FindElement(resulttextLocator).Text.Contains(girlcall), "Радиокнопка не сменила пол c М на Ж");
        }

        [Test]
        public void PopugSite_RadioGender_SwitchResultText()
        {
            driver.Navigate().GoToUrl(testUrl);
            driver.FindElement(radioGirlLocator).Click();
            SendForm();
            driver.FindElement(anotherEmailLinkLocator).Click();
            driver.FindElement(radioBoyLocator).Click();
            SendForm();

            Assert.IsTrue(driver.FindElement(resulttextLocator).Text.Contains(boycall), "Радиокнопка не сменила пол c Ж на М");
        }

        [Test]
        public void PopugSite_F5afterSubmit_EmptyForm()
        {
            driver.Navigate().GoToUrl(testUrl);
            SendForm();
            driver.Navigate().Refresh();
            
            var assertsAccumulator = new AssertsAccumulator();
            assertsAccumulator.Accumulate(() => Assert.IsTrue(driver.FindElement(radioBoyLocator).Selected, "Радиокнопка не предвыбрана по умолчанию"));
            assertsAccumulator.Accumulate(() => Assert.AreEqual(string.Empty, driver.FindElement(emailInputLocator).Text, "строка емейла не отчистилась"));
            assertsAccumulator.Accumulate(() => Assert.IsEmpty(driver.FindElements(resultblock), "Поле с сообщением осталось"));
            assertsAccumulator.Accumulate(() => Assert.IsFalse(driver.FindElement(anotherEmailLinkLocator).Displayed, "Не исчезла ссылка"));
            assertsAccumulator.Release();
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }

    }

}
