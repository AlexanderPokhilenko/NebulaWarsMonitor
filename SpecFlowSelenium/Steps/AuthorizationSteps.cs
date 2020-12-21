using BoDi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Steps
{
    [Binding]
    public class AuthorizationSteps
    {
        private readonly IWebDriver _webDriver;

        public AuthorizationSteps(IObjectContainer objectContainer)
        {
            _webDriver = objectContainer.Resolve<IWebDriver>();
        }

        [Given(@"that user is on the ""(.*)"" page")]
        public void GivenThatUserIsOnThePage(string page)
        {
            _webDriver.Url = Globals.GetUrl(page);
        }

        [Given(@"that user is authorized")]
        public void GivenThatUserIsAuthorized()
        {
            GivenThatUserIsOnThePage("Monitor/LogIn");
            WhenUserEntersCorrectPassword();
            WhenUserClickTheButton("send");
        }

        [When(@"user enters wrong password ""(.*)""")]
        public void WhenUserEntersWrongPassword(string password)
        {
            EnterPassword(password);
        }

        [When(@"user enters correct password")]
        public void WhenUserEntersCorrectPassword()
        {
            EnterPassword(Globals.PasswordForTesting);
        }

        private void EnterPassword(string password)
        {
            var passwordInput = _webDriver.FindElement(By.Id("password"));
            passwordInput.SendKeys(password);
        }

        [When(@"user click the ""(.*)"" button")]
        public void WhenUserClickTheButton(string buttonId)
        {
            var button = _webDriver.FindElement(By.Id(buttonId));
            button.Click();
        }
        
        [Then(@"the ""(.*)"" message should be shown")]
        public void ThenTheMessageShouldBeShown(string messageId)
        {
            var message = _webDriver.FindElement(By.Id(messageId));
            Assert.IsNotNull(message);
        }

        [Then(@"the user will be redirected to ""(.*)"" page")]
        public void ThenTheUserWillBeRedirectedToPage(string page)
        {
            var currentUrl = _webDriver.Url;
            var expectedUrl = Globals.GetUrl(page);
            Assert.AreEqual(expectedUrl, currentUrl);
        }

        [AfterScenario]
        public void CloseBrowser()
        {
            _webDriver.Quit();
        }

    }
}
