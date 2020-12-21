using System.Linq;
using BoDi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Steps
{
    [Binding]
    public class TableEditingSteps
    {
        private readonly IWebDriver _webDriver;

        public TableEditingSteps(IObjectContainer objectContainer)
        {
            _webDriver = objectContainer.Resolve<IWebDriver>();
        }

        [Then(@"the row with class ""(.*)"" will (.*) in the table ""(.*)""")]
        public void ThenTheRowWithClassWillBeInTheTable(string rowClass, bool contains, string tableId)
        {
            var table = _webDriver.FindElement(By.Id(tableId));
            var rows = table.FindElements(By.TagName("tr"));
            var lastRow = rows.Last();
            var classes = lastRow.GetAttribute("class").Split(' ');
            Assert.AreEqual(contains, classes.Contains(rowClass));
        }

        [StepArgumentTransformation(@"(be|not be)")]
        public bool BeToBool(string value) => value == "be";

        [When(@"user click the button with class ""(.*)"" at the last row of table ""(.*)""")]
        public void WhenUserClickTheButtonWithClassAtTheLastRowOfTable(string buttonClass, string tableId)
        {
            var table = _webDriver.FindElement(By.Id(tableId));
            var rows = table.FindElements(By.TagName("tr"));
            var lastRow = rows.Last();
            var button = lastRow.FindElement(By.ClassName(buttonClass));
            button.Click();
        }

        [When(@"user enters ""(.*)"" into first input of last row of table ""(.*)""")]
        public void WhenUserEntersIntoFirstInputOfFirstRowOfTable(string text, string tableId)
        {
            var table = _webDriver.FindElement(By.Id(tableId));
            var rows = table.FindElements(By.TagName("tr"));
            var lastRow = rows.Last();
            var inputs = lastRow.FindElements(By.TagName("input"));
            var firstInput = inputs.First();
            firstInput.Clear();
            firstInput.SendKeys(text);
            firstInput.Click();
        }

    }
}
