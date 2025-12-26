using System.Globalization;
using CsvHelper;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ThikaResQNet.Data;
using ThikaResQNet.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ThikaResQNet.Services
{
    public class ReportExportService : IReportExportService
    {
        private readonly AppDbContext _db;

        public ReportExportService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> ExportIncidentsToCsvAsync(DateTime? start = null, DateTime? end = null)
        {
            var query = _db.Incidents.AsQueryable();
            if (start.HasValue) query = query.Where(i => i.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(i => i.CreatedAt <= end.Value);
            var list = await query.ToListAsync();

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, leaveOpen: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(list.Select(i => new
            {
                i.IncidentId,
                i.ReporterId,
                i.Description,
                i.Latitude,
                i.Longitude,
                i.AddressText,
                i.SeverityScore,
                Status = i.Status.ToString(),
                i.CreatedAt,
                i.AssignedResponderId,
                i.AssignedAt
            }));
            writer.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        public async Task<byte[]> ExportIncidentsToPdfAsync(DateTime? start = null, DateTime? end = null)
        {
            var query = _db.Incidents.AsQueryable();
            if (start.HasValue) query = query.Where(i => i.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(i => i.CreatedAt <= end.Value);
            var list = await query.ToListAsync();

            using var ms = new MemoryStream();
            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12);

            double y = 20;
            foreach (var i in list)
            {
                var line = $"[{i.CreatedAt:yyyy-MM-dd HH:mm}] Id:{i.IncidentId} Severity:{i.SeverityScore} Status:{i.Status} Desc:{(i.Description?.Length>80? i.Description.Substring(0,80)+"...": i.Description)}";
                gfx.DrawString(line, font, XBrushes.Black, new XRect(20, y, page.Width - 40, 20), XStringFormats.TopLeft);
                y += 20;
                if (y > page.Height - 40)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 20;
                }
            }

            doc.Save(ms);
            ms.Position = 0;
            return ms.ToArray();
        }
    }
}