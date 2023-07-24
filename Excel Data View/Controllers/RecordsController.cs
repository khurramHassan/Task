using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ExcelDataReader;
using ExcelDataView.Models;
using System.Collections.Generic;
using System.IO;
using System;
using ExcelDataView.Context;
using ExcelDataView.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;

namespace ExcelDataView.Controllers
{
    public class RecordsController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;

        public RecordsController(IWebHostEnvironment hostingEnvironment, ApplicationDbContext context)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(List<Models.ExcelRecord> excelrecord = null)
        {
            excelrecord = excelrecord == null ? new List<Models.ExcelRecord>() : excelrecord;
            return View(excelrecord);
        }

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            List<Models.ExcelRecord> excelRecord  = new List<Models.ExcelRecord>();
            try
            {
                string fileName = $"{_hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
                using (FileStream fileStream = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                excelRecord = this.GetRecord(file.FileName);


                saveExcelData(excelRecord);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", ex.InnerException.Message);
                }
                else
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(excelRecord);
        }

        private void saveExcelData(List<Models.ExcelRecord> excelrecord)
        {
            foreach (var record in excelrecord)
            {
                _context.ExcelRecords.Add(new Entity.ExcelRecord()
                {
                    FirstName = record.FirstName,
                    //LastName = record.LastName,
                    Email = record.Email,
                    //DOB = record.DOB,
                    //FatherName = record.FatherName,

                });
            }
        }

        [HttpGet]
        public IActionResult Download()
        {

            string fileName = "Record.xlsx";
            string filePath = $"{_hostingEnvironment.WebRootPath}\\files\\{fileName}";

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private List<Models.ExcelRecord> GetRecord(string fName)
        {
            List<Models.ExcelRecord> excel_records = new List<Models.ExcelRecord>();
            var fileName = $"{_hostingEnvironment.WebRootPath}\\files\\{fName}";
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        if (string.IsNullOrEmpty(reader.GetValue(0)?.ToString()) || string.IsNullOrEmpty(reader.GetValue(1)?.ToString()) || string.IsNullOrEmpty(reader.GetValue(2)?.ToString()))
                        {
                            throw new Exception("Value can not be empty in required columns");
                        }

                        excel_records.Add(new Models.ExcelRecord()
                        {
                            FirstName = reader.GetValue(0)?.ToString(),
                            Id = reader.GetValue(1)?.ToString(),
                            Email = reader.GetValue(2)?.ToString(),
                            

                        });
                    }
                }
            }

            return excel_records; // No error message, return null.
        }
    }
}