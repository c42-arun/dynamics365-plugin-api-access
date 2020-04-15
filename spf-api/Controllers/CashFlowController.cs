using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using spf_api.Helpers;
using SpreadsheetGear;

namespace spf_api.Controllers
{
    public class CashFlowController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Calculate(string loanAmount, string interestRate, string term)
        {
            //return Ok(new { Message = "Call success"});

            string calcSpreadSheet = new ResourcesHelper().GetAmotizationSpreadSheetFileName();

            IWorkbook workbook = Factory.GetWorkbook(calcSpreadSheet, System.Globalization.CultureInfo.CurrentCulture);
            IWorksheet worksheet = workbook.Worksheets["Schedule"];
            IRange cells = worksheet.Cells;

            cells["D6"].Formula = loanAmount;
            cells["D7"].Formula = interestRate;
            cells["D8"].Formula = term;

            string totalPayments = cells["H7"].Text;
            string totalInterest = cells["H8"].Text;
            string monthlyPayment = cells["D21"].Text;

            decimal.TryParse(totalPayments, out var totalPaymentsParsed);
            decimal.TryParse(totalInterest, out var totalInterestParsed);
            decimal.TryParse(monthlyPayment, out var monthlyPaymentParsed);

            File.Delete(calcSpreadSheet);

            return Ok(new { TotalPayments = totalPaymentsParsed, TotalInterest = totalInterestParsed, MonthlyPayment = monthlyPaymentParsed });
        }

        [HttpGet]
        public IHttpActionResult CalculateSpf(string acquisitionCosts, string loanType)
        {
            //return Ok(new { Message = "Call success"});

            string calcSpreadSheet = new ResourcesHelper().GetSpfSpreadSheetFileName();

            IWorkbookSet workbookSet = Factory.GetWorkbookSet();
            workbookSet.Calculation = Calculation.Manual;
            workbookSet.BackgroundCalculation = true;

            IWorkbook workbook = workbookSet.Workbooks.Open(calcSpreadSheet);
            //IWorkbook workbook = Factory.GetWorkbook(calcSpreadSheet, System.Globalization.CultureInfo.CurrentCulture);
            IWorksheet worksheet = workbook.Worksheets["Inputs"];
            IRange cells = worksheet.Cells;

            cells["B2"].Formula = acquisitionCosts;
            cells["E10"].Formula = loanType.ToUpper() == "I" ? "Interest Only" : "Amortising";

            workbookSet.Calculate();

            string iru = cells["H4"].Text;
            string non_utilisation = cells["H5"].Text;
            string arrangementfee = cells["H6"].Text;
            string totalCosts = cells["H7"].Text;

            decimal.TryParse(totalCosts, out var totalPaymentsParsed);

            File.Delete(calcSpreadSheet);

            return Ok(new { acquisitionCosts, iru, non_utilisation, arrangementfee, totalCosts });
        }

        [HttpGet]
        public IHttpActionResult Test()
        {
            return Ok("Call success YES!!!");
        }
    }
}
