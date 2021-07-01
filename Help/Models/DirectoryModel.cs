using LNF.Data;
using LNF.Help;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Help.Models
{
    public enum WorkHoursPart
    {
        MorningStart = 1,
        MorningEnd = 2,
        AfternoonStart = 3,
        AfternoonEnd = 4
    }

    public class StaffDirectoryNotFoundException : Exception
    {
        public int StaffDirectoryID { get; }

        public StaffDirectoryNotFoundException(int staffDirectoryId)
        {
            StaffDirectoryID = staffDirectoryId;
        }

        public override string Message
        {
            get
            {
                return string.Format("Cannot find record with StaffDirectoryID {0}", StaffDirectoryID);
            }
        }
    }

    public struct PhoneNumber
    {
        private bool isInvalid;
        private bool isEmpty;

        public readonly string AreaCode;
        public readonly string Prefix;
        public readonly string LineNumber;
        public readonly string Format;

        public static PhoneNumber Parse(string phoneNumber, string format = null)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return new PhoneNumber() { isInvalid = false, isEmpty = true };

            string defaultAreaCode = "734";
            string areaCode = string.Empty;
            string prefix = string.Empty;
            string lineNumber = string.Empty;

            var temp = phoneNumber.Trim().Replace("(", string.Empty).Replace(")", string.Empty).Replace(" ", "-");
            var splitter = temp.Split('-');

            if (splitter.Length > 2)
            {
                areaCode = splitter[0];
                prefix = splitter[1];
                lineNumber = splitter[2];
            }
            else if (splitter.Length > 1)
            {
                areaCode = defaultAreaCode;
                prefix = splitter[0];
                lineNumber = splitter[1];
            }
            else
                return new PhoneNumber() { isInvalid = true };

            return new PhoneNumber(areaCode, prefix, lineNumber, format);
        }

        public PhoneNumber(string areaCode, string prefix, string lineNumber, string format = null)
        {
            isInvalid = false;
            isEmpty = false;

            if (string.IsNullOrEmpty(areaCode + prefix + lineNumber))
                isEmpty = true;
            else
            {
                if (areaCode == null || !Regex.IsMatch(areaCode, @"\b\d\d\d\b"))
                    isInvalid = true;

                if (prefix == null || !Regex.IsMatch(prefix, @"\b\d\d\d\b"))
                    isInvalid = true;

                if (lineNumber == null || !Regex.IsMatch(lineNumber, @"\b\d\d\d\d\b"))
                    isInvalid = true;
            }

            AreaCode = areaCode;
            Prefix = prefix;
            LineNumber = lineNumber;
            Format = format;
        }

        public override string ToString()
        {
            if (!IsValid())
                return "Invalid";
            else if (IsEmpty())
                return string.Empty;
            else
            {
                string fmt = string.IsNullOrEmpty(Format) ? "{0}-{1}-{2}" : Format;
                return string.Format(fmt, AreaCode, Prefix, LineNumber);
            }
        }

        public bool IsValid()
        {
            return !isInvalid;
        }

        public bool IsEmpty()
        {
            return isEmpty;
        }
    }

    public class DirectoryModel
    {
        public string Command { get; set; }
        public int StaffDirectoryID { get; set; }
        public int ClientID { get; set; }
        public bool ViewDeleted { get; set; }
        public string HoursText { get; set; }
        public string PhoneAreaCode { get; set; }
        public string PhonePrefix { get; set; }
        public string PhoneLineNumber { get; set; }
        public string Office { get; set; }
        public string HoursXML { get; set; }
        public bool Deleted { get; set; }
        public IClient CurrentUser { get; set; }
        public IStaffDirectory StaffDirectory { get; set; }
        public IEnumerable<IStaffDirectory> DirectoryItems { get; set; }
        public SelectListItem[] StaffSelectListItems { get; set; }

        private StaffTimeInfoCollection staffTime;
        private string message;
        private bool fatalError;

        public IHtmlString GetMessage()
        {
            return new HtmlString(message);
        }

        public bool IsFatalError()
        {
            return fatalError;
        }

        protected void AppendAlert(string text, bool fatal = false)
        {
            TagBuilder builder = new TagBuilder("div");
            builder.AddCssClass("alert alert-danger");
            builder.Attributes.Add("role", "alert");
            builder.SetInnerText(text);
            message += builder.ToString();

            fatalError = fatalError || fatal;
        }

        public string GetDisplayName()
        {
            NotFoundCheck();
            return StaffDirectory.DisplayName;
        }

        public int GetClientID()
        {
            NotFoundCheck();
            return StaffDirectory.ClientID;
        }

        public string GetHoursHtml(IStaffDirectory sd)
        {
            StaffTimeInfoCollection staffTime = new StaffTimeInfoCollection(sd.HoursXML);
            string[] lines = staffTime.GetHoursText();
            string result = string.Empty;
            if (lines.Length > 0)
                result = "<div>" + string.Join("</div><div>", lines) + "</div>";
            return result;
        }

        public string[] GetHoursText()
        {
            NotFoundCheck();
            StaffTimeInfoCollection staffTime = new StaffTimeInfoCollection(StaffDirectory.HoursXML);
            string[] result = staffTime.GetHoursText();
            return result;
        }

        public void Load()
        {
            if (StaffDirectoryID == 0 && !CanAdd())
            {
                AppendAlert("You do not have permission to add.", true);
                return;
            }

            if (StaffDirectoryID == 0)
                return;

            NotFoundCheck();

            if (StaffDirectory.Deleted)
            {
                if (!CanDelete())
                {
                    AppendAlert("You do not have permission to access a deleted record.", true);
                    return;
                }
            }

            PhoneNumber phoneNumber = PhoneNumber.Parse(StaffDirectory.ContactPhone);
            PhoneAreaCode = phoneNumber.AreaCode;
            PhonePrefix = phoneNumber.Prefix;
            PhoneLineNumber = phoneNumber.LineNumber;
            Office = StaffDirectory.Office;
            Deleted = StaffDirectory.Deleted;
        }

        public bool Validate()
        {
            NotFoundCheck();

            message = string.Empty;
            int errors = 0;

            PhoneNumber phoneNumber = new PhoneNumber(PhoneAreaCode, PhonePrefix, PhoneLineNumber);
            if (!phoneNumber.IsValid())
            {
                AppendAlert("Invalid phone number.");
                errors++;
            }

            if (errors > 0)
                return false;

            StaffDirectory.ContactPhone = phoneNumber.ToString();
            StaffDirectory.Office = Office;

            return true;
        }

        public bool Save(NameValueCollection formData)
        {
            if (StaffDirectoryID == 0)
            {
                if (CanAdd())
                {
                    //Client c = DataSession.Single<Client>(ClientID);

                    if (ClientID == 0)
                    {
                        AppendAlert("You must select a client.");
                        return false;
                    }

                    StaffDirectory = new StaffDirectoryItem() { ClientID = ClientID };

                    if (!Validate())
                        return false;
                }
                else
                {
                    AppendAlert("You do not have permission to add.", true);
                    return false;
                }
            }
            else
            {
                NotFoundCheck();

                if (IsReadOnly(StaffDirectory))
                {
                    AppendAlert("You do not have permission to edit.", true);
                    return false;
                }
                else
                {
                    if (!Validate())
                        return false;
                }
            }

            if (SetHoursXML(formData))
            {
                if (StaffDirectory.Deleted != Deleted)
                {
                    if (CanDelete())
                        StaffDirectory.Deleted = Deleted;
                    else
                    {
                        AppendAlert("You do not have permission to delete.");
                        return false;
                    }
                }
            }
            else
                return false;

            return true;
        }

        public string GetWorkHoursPartText(WorkHoursPart part)
        {
            switch (part)
            {
                case WorkHoursPart.MorningStart:
                    return "Morning Start";
                case WorkHoursPart.MorningEnd:
                    return "Morning End";
                case WorkHoursPart.AfternoonStart:
                    return "Afternoon Start";
                case WorkHoursPart.AfternoonEnd:
                    return "Afternoon End";
                default:
                    throw new ArgumentException("part");
            }
        }

        public bool SetHoursXML(NameValueCollection formData)
        {
            NotFoundCheck();
            var staffTime = new StaffTimeInfoCollection(StaffDirectory.HoursXML);
            var lines = HoursText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            staffTime.SetHoursText(lines);
            StaffDirectory.HoursXML = staffTime.ToXML().ToString();

            //int errors = 0;

            //WorkHoursPart[] parts = { WorkHoursPart.MorningStart, WorkHoursPart.MorningEnd, WorkHoursPart.AfternoonStart, WorkHoursPart.AfternoonEnd };

            //foreach (var timeInfo in staffTime)
            //{
            //    string chkName = string.Format("{0}Checked", timeInfo.WorkHoursDayName());
            //    timeInfo.Value.Checked = formData[chkName] == "on";

            //    foreach (WorkHoursPart part in parts)
            //    {
            //        string txtName = timeInfo.WorkHoursTimeTextBoxName(part);

            //        string val = formData[txtName];

            //        if (timeInfo.Value.Checked && string.IsNullOrEmpty(val))
            //        {
            //            AppendAlert(string.Format("Missing time for {0}: {1}", timeInfo.WorkHoursDayName(), GetWorkHoursPartText(part)));
            //            errors++;
            //        }
            //        else
            //        {
            //            timeInfo.SetValue(val, part);
            //        }
            //    }
            //}

            //sd.HoursXML = staffTime.ToXML().ToString();

            return true;
        }

        public string GetPhone(IStaffDirectory item, string format = null)
        {
            PhoneNumber phoneNumber = PhoneNumber.Parse(item.ContactPhone, format);
            return phoneNumber.ToString();
        }

        public StaffTimeInfoCollection GetStaffTime()
        {
            if (staffTime == null)
            {
                if (StaffDirectoryID == 0)
                    staffTime = new StaffTimeInfoCollection();
                else
                {
                    NotFoundCheck();
                    staffTime = new StaffTimeInfoCollection(StaffDirectory.HoursXML);
                }
            }

            return staffTime;
        }

        public string GetWorkHoursTime(DayOfWeek day, WorkHoursPart part)
        {
            if (staffTime == null)
            {
                NotFoundCheck();
                staffTime = new StaffTimeInfoCollection(StaffDirectory.HoursXML);
            }

            StaffTimeInfo timeInfo = staffTime[day];
            StaffTimeValue timeValue = null;

            switch (part)
            {
                case WorkHoursPart.MorningStart:
                    timeValue = timeInfo.AM.Start;
                    break;
                case WorkHoursPart.MorningEnd:
                    timeValue = timeInfo.AM.End;
                    break;
                case WorkHoursPart.AfternoonStart:
                    timeValue = timeInfo.PM.Start;
                    break;
                case WorkHoursPart.AfternoonEnd:
                    timeValue = timeInfo.PM.End;
                    break;
                default:
                    throw new ArgumentException("part");
            }

            return timeValue.Value.ToString();
        }

        public bool IsReadOnly(IStaffDirectory item)
        {
            if (CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return false;

            if (item.ClientID == CurrentUser.ClientID)
                return false;

            return true;
        }

        public bool CanViewDeleted()
        {
            return CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        public bool CanAdd()
        {
            return CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        public bool CanDelete()
        {
            return CurrentUser.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        public void NotFoundCheck()
        {
            if (StaffDirectory == null)
                throw new StaffDirectoryNotFoundException(StaffDirectoryID);
        }
    }

    public static class StaffTimeInfoHelper
    {
        public static string WorkHoursDayName(this KeyValuePair<DayOfWeek, StaffTimeInfo> kvp)
        {
            return Enum.GetName(typeof(DayOfWeek), kvp.Key);
        }

        public static IHtmlString WorkHoursDayCheckBox(this HtmlHelper helper, KeyValuePair<DayOfWeek, StaffTimeInfo> kvp)
        {
            string name = string.Format("{0}Checked", kvp.WorkHoursDayName());
            TagBuilder builder = new TagBuilder("input");
            builder.Attributes.Add("type", "checkbox");
            builder.Attributes.Add("name", name);
            builder.Attributes.Add("id", name);
            if (kvp.Value.Checked)
                builder.Attributes.Add("checked", "checked");
            builder.AddCssClass("day-checkbox");
            return new HtmlString(builder.ToString());
        }

        public static string WorkHoursTimeTextBoxName(this KeyValuePair<DayOfWeek, StaffTimeInfo> kvp, WorkHoursPart part)
        {
            return string.Format("{0}{1}", kvp.WorkHoursDayName(), Enum.GetName(typeof(WorkHoursPart), part));
        }

        public static IHtmlString WorkHoursTimeTextBox(this HtmlHelper helper, KeyValuePair<DayOfWeek, StaffTimeInfo> kvp, WorkHoursPart part)
        {
            string name = kvp.WorkHoursTimeTextBoxName(part);

            switch (part)
            {
                case WorkHoursPart.MorningStart:
                    return helper.TextBox(name, kvp.Value.AM.Start, new { @class = "time-text form-control" });
                case WorkHoursPart.MorningEnd:
                    return helper.TextBox(name, kvp.Value.AM.End, new { @class = "time-text form-control", @data_toggle = "popover" });
                case WorkHoursPart.AfternoonStart:
                    return helper.TextBox(name, kvp.Value.PM.Start, new { @class = "time-text form-control", @data_toggle = "popover" });
                case WorkHoursPart.AfternoonEnd:
                    return helper.TextBox(name, kvp.Value.PM.End, new { @class = "time-text form-control", @data_toggle = "popover" });
                default:
                    throw new ArgumentException("part");
            }
        }

        public static StaffTimeValue TimeValue(this KeyValuePair<DayOfWeek, StaffTimeInfo> kvp, WorkHoursPart part)
        {
            switch (part)
            {
                case WorkHoursPart.MorningStart:
                    return kvp.Value.AM.Start;
                case WorkHoursPart.MorningEnd:
                    return kvp.Value.AM.End;
                case WorkHoursPart.AfternoonStart:
                    return kvp.Value.PM.Start;
                case WorkHoursPart.AfternoonEnd:
                    return kvp.Value.PM.End;
                default:
                    throw new ArgumentException("part");
            }
        }

        public static TimeRange TimeRange(this KeyValuePair<DayOfWeek, StaffTimeInfo> kvp, WorkHoursPart part)
        {
            switch (part)
            {
                case WorkHoursPart.MorningStart:
                case WorkHoursPart.MorningEnd:
                    return kvp.Value.AM;
                case WorkHoursPart.AfternoonStart:
                case WorkHoursPart.AfternoonEnd:
                    return kvp.Value.PM;
                default:
                    throw new ArgumentException("part");
            }
        }

        public static void SetValue(this KeyValuePair<DayOfWeek, StaffTimeInfo> kvp, string value, WorkHoursPart part)
        {
            StaffTimeValue timeValue = kvp.TimeValue(part);
            timeValue.Value = StaffTimeValue.Parse(value);
        }
    }
}