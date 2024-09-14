using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace QueryLogAPI.Controllers
{
    [ApiController]
    [Route("queries/count")]
    public class QueryCountController : ControllerBase
    {
        [HttpGet("{datePrefix}")]
        public IActionResult GetDistinctQueryCount(string datePrefix)
        {
            
            List<QueryLogEntry> logEntries = ReadLogEntries();

            /*QueryLogEntrY nesnelerinin Timestamp bilgilerini parametre olarak alınan datePrefix'in uzunluğuna göre formatlar ve datePrefix değeri ile başlayan kayıtları filtreler.
             Daha sonra filtrelenmiş kayıtlar arasından Distinct ile tekrar eden Query değerlerini kaldırarak benzersiz kayıtları seçer.*/

            var distinctQueryies = logEntries.Where(e=>
                e.Time.ToString(GetDateFormat(datePrefix.Length), CultureInfo.InvariantCulture).StartsWith(datePrefix)
            ).Select(entry => entry.Query).Distinct();

            return Ok(new { count = distinctQueryies.Count() });
        }

        
        private string GetDateFormat(int datePrefixLength)
        {
            return datePrefixLength switch
            {
                4 => "yyyy",                 
                7 => "yyyy-MM",             
                10 => "yyyy-MM-dd",          
                13 => "yyyy-MM-dd HH",       
                16 => "yyyy-MM-dd HH:mm",    
                _ => "yyyy-MM-dd HH:mm:ss",  
            };
        }

        //tsv dosyasını okumak için yazıldı
        private List<QueryLogEntry> ReadLogEntries()
        {
            var logEntries = new List<QueryLogEntry>();

            if (System.IO.File.Exists("hn_logs.tsv"))
            {
                string[] logLines = System.IO.File.ReadAllLines("hn_logs.tsv"); //dosyadaki tüm satırlar okunur ve logLines arrayine eklenir.

                foreach (var line in logLines)
                {
                    var parts = line.Split('\t'); // tab tuşuyla satırdaki time ve query ayrılır.

                    if (parts.Length >= 2) 
                    {
                        logEntries.Add(new QueryLogEntry
                        {
                            Time = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), // tarih bilgisinin istenilen şekilde parse edilir.
                            Query = parts[1]                     
                        });
                    }
                }
            }

            return logEntries;
        }
    }

    
}
