#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text.RegularExpressions;
using System.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<GebizTable> outTable, TraceWriter log)
{
    string html = await req.Content.ReadAsStringAsync();

    string result = "";

    string category = "";
    string type = "";
    string title = "";
    string tenderType = "";
    string tenderURL = "";
    string tenderNo = "";
    string callingTitle = "";
    string callingEntity = "";
    string publicationDate = "";
    string submissionDate = "";
    string submissionTime = "";
    string submissionMethod = "";

    //trim html first
    var output = Regex.Split(html, "<tbody>");
    result = output.Last();
    output = Regex.Split(result, "</tbody>");
    result = output.First();
    result.Replace("\r\n", "");

    //trim different column
    output = Regex.Split(result, "<tr ");

    int ending = output.Count();

    //start the loop from 2 to end
    //this is done on analyzing html string
    //the information is extracted accordingly
    for(int i = 2; i < ending;i++)
    {
        result = output[i];

        int pfrom = result.IndexOf("<b>") + "<b>".Length;
        category = result.Substring(pfrom, result.IndexOf("</b>") - pfrom);

        result = result.Remove(0, pfrom + category.Length + 4);

        pfrom = result.IndexOf("<b>") + "<b>".Length;
        type = result.Substring(pfrom, result.IndexOf("</b>") - pfrom);

        result = result.Remove(0, pfrom + type.Length + 4);

        pfrom = result.IndexOf(":") + 1;
        title = result.Substring(pfrom, result.IndexOf("<b>") - pfrom);

        result = result.Remove(0, pfrom + title.Length + 3);

        pfrom = 0;
        tenderType = result.Substring(pfrom, result.IndexOf("</b>") - pfrom);

        result = result.Remove(0, pfrom + tenderType.Length + 4);

        pfrom = result.IndexOf("\"") + 1;
        tenderURL = result.Substring(pfrom, result.IndexOf("\" ") - pfrom);

        result = result.Remove(0, pfrom + tenderURL.Length + 1);
        result = result.Remove(0, result.IndexOf("<span") + 5);

        pfrom = result.IndexOf(">") + 1;
        tenderNo = result.Substring(pfrom, result.IndexOf("<") - pfrom);

        result = result.Remove(0, pfrom + tenderNo.Length + 13);

        pfrom = result.IndexOf("<b>") + 3;
        callingTitle = result.Substring(pfrom, result.IndexOf("</b>") - pfrom);

        result = result.Remove(0, pfrom + callingTitle.Length + 4);

        pfrom = result.IndexOf(":") + 1;
        callingEntity = result.Substring(pfrom, result.IndexOf("</td>") - pfrom);

        result = result.Remove(0, pfrom + callingEntity.Length);
        result = result.Remove(0, result.IndexOf("sans-serif") + 10);

        pfrom = result.IndexOf(">") + 1;
        publicationDate = result.Substring(pfrom, result.IndexOf("<") - pfrom);

        result = result.Remove(0, pfrom + publicationDate.Length + 20);
        result = result.Remove(0, result.IndexOf("color") + 3);

        pfrom = result.IndexOf(">") + 1;
        submissionDate = result.Substring(pfrom, result.IndexOf(" ") - pfrom);

        result = result.Remove(0, pfrom + submissionDate.Length);

        pfrom = 0;
        submissionTime = result.Substring(pfrom, result.IndexOf("S") - pfrom);

        result = result.Remove(0, pfrom + submissionTime.Length);

        pfrom = result.IndexOf(": ") + 2;
        submissionMethod = result.Substring(pfrom, result.IndexOf("<") - pfrom);   

        outTable.Add(new GebizTable()
        {
            PartitionKey = "Functions",
            RowKey = Guid.NewGuid().ToString(),
            Category = category,
            Type = type,
            Title = title,
            TenderType = tenderType,
            TenderURL = tenderURL,
            TenderNo = tenderNo,
            CallingTitle = callingTitle,
            CallingEntity = callingEntity,
            PublicationDate = publicationDate,
            SubmissionDate = submissionDate,
            SubmissionTime = submissionTime,
            SubmissionMethod = submissionMethod
        });
    }
    return req.CreateResponse(HttpStatusCode.Created);
}

public class GebizTable : TableEntity
{
    public string Category { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string TenderType { get; set; }
    public string TenderURL { get; set; }
    public string TenderNo { get; set; }
    public string CallingTitle { get; set; }
    public string CallingEntity { get; set; }
    public string PublicationDate { get; set; }
    public string SubmissionDate { get; set; }
    public string SubmissionTime { get; set; }
    public string SubmissionMethod { get; set; }
}
