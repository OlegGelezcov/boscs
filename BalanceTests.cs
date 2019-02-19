using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Moq;

public class BalanceTests {

    [Test]
    public void TestInvestorPercent() {
        // Use the Assert class to test conditions.
        //var mock = new Mock<IInvestorInfo>();
        //mock.Setup(i => i.GetCurrentInvestorCount()).Returns(5.76e30);
        //mock.Setup(i => i.GetInvestorEffectiveness()).Returns(0.01f);
        //mock.Setup(i => i.GetMinInvestorCountForBonus()).Returns(1000);

        //InvestorCalculator investorCalculator = new InvestorCalculator();
        //double actual = investorCalculator.GetInvestorsPercent(mock.Object);
        //Assert.AreEqual(actual / 5.76E+25, 1, 0.1);
    }

    [Test]
    public void TestIncome() {
        //var mock = new Mock<IInvestorInfo>();
        //mock.Setup(i => i.GetCurrentInvestorCount()).Returns(5.76e30);
        //mock.Setup(i => i.GetInvestorEffectiveness()).Returns(0.01f);
        //mock.Setup(i => i.GetMinInvestorCountForBonus()).Returns(1000);

        //InvestorCalculator investorCalculator = new InvestorCalculator();

        //double profit = 271E+20;
        //double profit1 = ModifyByInvestors(profit, investorCalculator.GetInvestorsPercent(mock.Object));

        //mock = new Mock<IInvestorInfo>();
        //mock.Setup(i => i.GetCurrentInvestorCount()).Returns(5.71e30);
        //mock.Setup(i => i.GetInvestorEffectiveness()).Returns(0.01f);
        //mock.Setup(i => i.GetMinInvestorCountForBonus()).Returns(1000);
        //double profit2 = ModifyByInvestors(profit, investorCalculator.GetInvestorsPercent(mock.Object));

        //Debug.Log($"profit1 => {profit1}, profit2 => {profit2}");
    }

    private double ModifyByInvestors(double profit, double investorKoefficient) {
        profit += profit * investorKoefficient;
        return profit;
    }

    //// A UnityTest behaves like a coroutine in PlayMode
    //// and allows you to yield null to skip a frame in EditMode
    //[UnityTest]
    //public IEnumerator NewTestScriptWithEnumeratorPasses() {
    //    // Use the Assert class to test conditions.
    //    // yield to skip a frame
    //    yield return null;
    //}
}


