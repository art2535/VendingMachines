using System.IO;
using System.Text;
using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.Desktop.Services
{
    public static class ExportService
    {
        public static void ExportToCsv(string fileName, List<DeviceListItem> data)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID;Модель;Компания;Модем;Адрес");

            foreach (var item in data)
            {
                var line = $"{item.Id};{item.Model};{item.Company};{item.Modem};{item.Address}";
                csv.AppendLine(line);
            }

            File.WriteAllText(fileName, csv.ToString(), Encoding.UTF8);
        }

        public static void ExportToHtml(string fileName, List<DeviceListItem> data)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='utf-8'><style>table{border-collapse:collapse;width:100%;} th,td{border:1px solid #ccc;padding:8px;text-align:left;} th{background-color:#f2f2f2;}</style></head><body>");
            html.AppendLine("<h2>Список торговых автоматов</h2>");
            html.AppendLine("<table><thead><tr><th>ID</th><th>Модель</th><th>Компания</th><th>Модем</th><th>Адрес</th></tr></thead><tbody>");

            foreach (var item in data)
            {
                html.AppendLine($"<tr><td>{item.Id}</td><td>{item.Model}</td><td>{item.Company}</td><td>{item.Modem}</td><td>{item.Address}</td></tr>");
            }

            html.AppendLine("</tbody></table></body></html>");
            File.WriteAllText(fileName, html.ToString(), Encoding.UTF8);
        }
    }
}