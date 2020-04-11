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

            Dictionary<string, string> retVal = new Dictionary<string, string>();
            retVal.Add("TotalPayments", totalPayments);
            retVal.Add("TotalInterest", totalInterest);


            File.Delete(calcSpreadSheet);

            return Ok(retVal);
        }
    }
}
