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
        public IHttpActionResult Test()
        {
            return Ok("Call success YES!!!");
        }
    }
}
