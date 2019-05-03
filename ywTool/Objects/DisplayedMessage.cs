using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using ywTool.BaseObjects;

namespace ywTool.Objects
{
    public class DisplayedMessageOption
    {
        public int SkipHead { get; set; } = 0;
        public int RemoveTail { get; set; } = 0;
        public bool IncludeInstruction { get; set; } = false;
        public bool IncludeStatusImage { get; set; } = true;
        public bool IncludeCmd { get; set; } = true;
        public bool IncludeTaskName { get; set; } = true;
        public bool IncludeTimeStamp { get; set; } = true;
    }

    public class DisplayedMessage
    {
        public string MessageId;
        public string ThreadId;
        public DateTime Start;
        public DateTime? End;
        public string Cmd;
        public CmdRunState State;
        public List<MessageStructure> MessageList;

        public string GetHTMLMessage(DisplayedMessageOption dmo)
        {
            var image = "yellow.png";
            switch (State)
            {
                case CmdRunState.RUNNING:
                case CmdRunState.STARTING:
                    image = "running.gif";
                    break;
                case CmdRunState.FINISHED:
                    image = "green.png";
                    break;
                case CmdRunState.EXITWITHERROR:
                    image = "yellow.png";
                    break;
                default:
                    break;
            }
            var statusImage = Path.Combine(GlobalSettings.ToolResourceDir, image);
            var ts = Start.ToString("HH:mm:ss") + (End.HasValue ? ("&nbsp;-&nbsp;" + End.Value.ToString("HH:mm:ss")) : "") + (End.HasValue ? ("&nbsp;&nbsp;&nbsp;&nbsp;cost:&nbsp;" + (End.Value - Start).TotalSeconds.ToString("#0.00") + " Seconds.") : "");
            var ret = "";
            if (dmo.IncludeCmd)
                ret = $"<tr><td class='Cmd'>" + (dmo.IncludeStatusImage ? $"<img src='{statusImage}' />" : "") + $"&nbsp;{Cmd} &nbsp;&nbsp;&nbsp;" + 
                    (dmo.IncludeTimeStamp ? $"<span class='TS'>[{ts}]</span>":"") + "</td></tr>";
            if (MessageList != null)
            {
                foreach (var m in MessageList)
                {
                    if (m.MessageFormat == MessageFormat.IMAGE)
                    {
                        ret += $"<tr><td class='ImageMessage'><img class=\"GeneratedImage\" src=\"data:image/png;base64,{m.Message}\"></td></tr>";
                    }
                    else
                    {
                        bool? result = null;
                        if ((m.MessageType & (int)MessageType.SUCCESS) > 0 || State == CmdRunState.FINISHED)
                            result = true;
                        else if ((m.MessageType & (int)MessageType.ERROR) > 0 || State != CmdRunState.FINISHED)
                            result = false;

                        if (m.MessageType == (int)MessageType.JOBNAME && dmo.IncludeTaskName)
                            ret += @"<tr><td class=""JobName"">" + m.Message + "</td></tr>";
                        else if (!result.HasValue)
                            ret += @"<tr><td class=""NormalMessage"">" + m.Message + "</td></tr>";
                        else
                            ret += @"<tr><td class=""" + (result.Value ? "SuccessMessage" : "FailMessage") + @""">" + m.Message + $"&nbsp;&nbsp;&nbsp;" + (dmo.IncludeTimeStamp ? $"<span class='TS'>[{m.TimeStamp.ToString("HH:mm:ss")}]</span>" : "") + "</td></tr>";
                    }
                }
            }
            return ret;
        }

        public string GetEmailMessage(DisplayedMessageOption dmo , List<Attachment> attachments , string imageprefix )
        {
            dmo.IncludeStatusImage = false;
            var image = "yellow.png";
            switch (State)
            {
                case CmdRunState.RUNNING:
                case CmdRunState.STARTING:
                    image = "running.gif";
                    break;
                case CmdRunState.FINISHED:
                    image = "green.png";
                    break;
                case CmdRunState.EXITWITHERROR:
                    image = "yellow.png";
                    break;
                default:
                    break;
            }
            var statusImage = Path.Combine(GlobalSettings.ToolResourceDir, image);
            var ts = Start.ToString("HH:mm:ss") + (End.HasValue ? ("&nbsp;-&nbsp;" + End.Value.ToString("HH:mm:ss")) : "") + (End.HasValue ? ("&nbsp;&nbsp;&nbsp;&nbsp;cost:&nbsp;" + (End.Value - Start).TotalSeconds.ToString("#0.00") + " Seconds.") : "");
            var ret = "";
            if (dmo.IncludeCmd)
                ret = $"<tr><td class='Cmd'>" + (dmo.IncludeStatusImage ? $"<img src='{statusImage}' />" : "") + $"&nbsp;{Cmd} &nbsp;&nbsp;&nbsp;" +
                    (dmo.IncludeTimeStamp ? $"<span class='TS'>[{ts}]</span>" : "") + "</td></tr>";
            if (MessageList != null)
            {
                foreach (var m in MessageList)
                {
                    System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[\\'\\\"]{1}data:image\\/png;base64,[A-Za-z0-9+/=]+[\\'\\\"]{1}");
                    var tmpmessage = m.Message;
                    if (rgx.IsMatch(m.Message))
                    {
                        var matchs = rgx.Matches(m.Message);
                        foreach(System.Text.RegularExpressions.Match im in matchs)
                        {
                            string cid = Guid.NewGuid().ToString();
                            string filename = Path.Combine(GlobalSettings.OutputTmp, $"{imageprefix}{cid}.png");
                            var imagedata = im.Value.Substring("'data:image/png;base64,".Length);
                            imagedata = imagedata.Substring(0, imagedata.Length - 1);
                            File.WriteAllBytes(filename, Convert.FromBase64String(imagedata));
                            Attachment tmp = new Attachment(filename, MediaTypeNames.Image.Jpeg);
                            tmp.ContentId = cid;
                            tmp.ContentDisposition.Inline = true;
                            tmp.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                            attachments.Add(tmp);
                            tmpmessage = tmpmessage.Replace(im.Value, $"'cid:{cid}'");
                        }
                    }

                    if (m.MessageFormat == MessageFormat.IMAGE)
                    {
                        string cid = Guid.NewGuid().ToString();
                        string filename = Path.Combine(GlobalSettings.OutputTmp, $"{imageprefix}{cid}.png");
                        File.WriteAllBytes(filename, Convert.FromBase64String(m.Message));
                        Attachment tmp = new Attachment(filename, MediaTypeNames.Image.Jpeg);
                        tmp.ContentId = cid;
                        tmp.ContentDisposition.Inline = true;
                        tmp.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                        attachments.Add(tmp);
                        ret += $"<tr><td class='ImageMessage'><img class=\"GeneratedImage\" src=\"cid:{cid}\"></td></tr>";
                    }
                    else
                    {
                        bool? result = null;
                        if ((m.MessageType & (int)MessageType.SUCCESS) > 0 || State == CmdRunState.FINISHED)
                            result = true;
                        else if ((m.MessageType & (int)MessageType.ERROR) > 0 || State != CmdRunState.FINISHED)
                            result = false;

                        if (m.MessageType == (int)MessageType.JOBNAME)
                        {
                            if (dmo.IncludeTaskName)
                                ret += @"<tr><td class=""JobName"">" + tmpmessage + "</td></tr>";
                        }
                        else if (!result.HasValue)
                            ret += @"<tr><td class=""NormalMessage"">" + tmpmessage + "</td></tr>";
                        else
                            ret += @"<tr><td class=""" + (result.Value ? "SuccessMessage" : "FailMessage") + @""">" + tmpmessage + $"&nbsp;&nbsp;&nbsp;" + (dmo.IncludeTimeStamp ? $"<span class='TS'>[{m.TimeStamp.ToString("HH:mm:ss")}]</span>" : "") + "</td></tr>";
                    }
                }
            }
            return ret;
        }

        public static string GetMessage(DisplayedMessageOption dmo, bool isForEmail=false,  string tmpfileprefix="~tmp_", List<Attachment> attachments = null)
        {
            List<DisplayedMessage> tmp = GlobalSettings.Message.Skip(dmo.SkipHead).ToList();
            tmp = tmp.Take(tmp.Count - dmo.RemoveTail).ToList();

            var msg = "";
            if(isForEmail)
                msg = string.Join("\r\n", tmp.Select(a => a.GetEmailMessage(dmo, attachments, tmpfileprefix)).ToArray());
            else
                msg = string.Join("\r\n", tmp.Select(a => a.GetHTMLMessage(dmo)).ToArray());
            var ret = GlobalSettings.MessageTemplate.Replace("{{DETAILMESSAGE}}", msg);
            ret = ret.Replace("{{INSTRUCTION}}", dmo.IncludeInstruction ? GlobalSettings.InstructionTemplate :  "");
            return ret;
        }

    }
}
