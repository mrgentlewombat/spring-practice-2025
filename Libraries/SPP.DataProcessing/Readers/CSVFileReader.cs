using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;
using System.Text.RegularExpressions;
using SPP.DataProcessing.Models;

namespace SPP.DataProcessing.Readers;

public class CsvAgentReader
{
    public List<string> Errors { get; } = new();

    public List<Agent> ReadAgents(string filePath)
    {
        var agents = new List<Agent>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = context =>
            {
                Errors.Add($"[Line {context.Context.Parser.RawRow}] Invalid data found: {context.RawRecord}");
            }
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<AgentMap>();

        try
        {
            foreach (var agent in csv.GetRecords<Agent>())
            {
                if (!IsValidEmail(agent.Contact))
                {
                    Errors.Add($"[Line {csv.Context.Parser.RawRow}] Invalid email: {agent.Contact}");
                    continue;
                }

                agents.Add(agent);
            }
        }
        catch (Exception ex)
        {
            Errors.Add($"CSV parsing error: {ex.Message}");
        }

        return agents;
    }

    private bool IsValidEmail(string email)
    {
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}

public sealed class AgentMap : ClassMap<Agent>
{
    public AgentMap()
    {
        Map(m => m.AgentID).Index(0);
        Map(m => m.Name).Index(1);
        Map(m => m.CodeName).Index(2);
        Map(m => m.Location).Index(3);
        Map(m => m.DateOfBirth).Index(4).TypeConverter(new DateOnlyConverter());
        Map(m => m.Contact).Index(5);
        Map(m => m.LastMission).Index(6);
        Map(m => m.Status).Index(7);
        Map(m => m.ClearenceLevel).Index(8);
        Map(m => m.ArchiveNote).Index(9);
    }
}

public class DateOnlyConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (DateOnly.TryParse(text, out var date))
            return date;
        throw new TypeConverterException(this, memberMapData, text, row.Context, $"Invalid date format: {text}");
    }
}
