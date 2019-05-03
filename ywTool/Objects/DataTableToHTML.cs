using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ywTool.Objects
{
    public class DataTableToHTML
    {
        public static string Convert(string sectionHeader, DataTable dt, bool includeColumnName = true, string format="Table")
        {
            int colnum = dt.Columns.Count;
            StringBuilder html = new StringBuilder();
            html.AppendLine($"<table width=100%><tr><td align=center>");
            html.AppendLine($"<table  style=\"background-color:#cecece;width:780px\">");

            if (!string.IsNullOrWhiteSpace(sectionHeader))
                html.AppendLine($"<tr><td colspan='{colnum}' style='background-color:#ccff00;font-size:20px; font-weight:bold; padding-left:6px;color:blue'>{sectionHeader}</td></tr>");
            if (includeColumnName)
            {
                html.AppendLine("<tr style='background-color:#bfff00;font-size:16px;'>");
                foreach (DataColumn c in dt.Columns)
                    html.AppendLine($"<td>{c.ColumnName}</td>");
                html.AppendLine("</tr>");
            }
            int i = 0;
            if(colnum==2)
            {
                html.AppendLine("<tr><td width='30%'></td><td></td></tr>");
            }
            foreach(DataRow r in dt.Rows)
            {
                html.AppendLine("<tr>");
                var style = i % 2 == 0 ? "style='padding-left:8px; background-color:#ccffcc'" : "style='padding-left:8px; background-color:#ffffcc'";
                foreach (DataColumn c in dt.Columns)
                    html.AppendLine($"<td {style}>" + (System.Convert.IsDBNull (r[c.ColumnName]) ? "" :  r[c.ColumnName].ToString())+"</td>");
                html.AppendLine("</tr>");
                i++;
            }

            html.AppendLine($"</table>");
            html.AppendLine($"</td></tr></table>");

            return html.ToString();
        }

        private static string GeneratePlot(IList<DataPoint> series)
        {
            using (var ch = new Chart())
            {
                ch.ChartAreas.Add(new ChartArea());
                var s = new Series();
                foreach (var pnt in series) s.Points.Add(pnt);
                ch.Series.Add(s);
                var filename = Path.Combine(GlobalSettings.OutputTmp, Guid.NewGuid().ToString() + ".png");
                ch.SaveImage(filename, ChartImageFormat.Png);
                return filename;
            }
        }
    }
}
