using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Calculators;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class CalculatorsTests
    {
        // FinancialCalculator
        [TestMethod]
        public void Financial_CompoundInterest_Works()
        {
            // 1000 principal, 5%, 12 times/year, 10 years -> ~1647.01
            double result = FinancialCalculator.CalculateCompoundInterest(1000, 5, 12, 10);
            Assert.AreEqual(1647.01, result, 0.01);
        }

        [TestMethod]
        public void Financial_LoanPayment_CalculatesCorrectly()
        {
            // 100,000 loan, 5% annual, 360 months (30 years) -> ~536.82
            double payment = FinancialCalculator.CalculateLoanPayment(100000, 5, 360);
            Assert.AreEqual(536.82, payment, 0.01);
        }

        [TestMethod]
        public void Financial_Roi_CalculatesCorrectly()
        {
            // Invest 100, return 150 -> 50% ROI
            double roi = FinancialCalculator.CalculateRoi(100, 150);
            Assert.AreEqual(50.0, roi, 0.01);
        }
    }
}
