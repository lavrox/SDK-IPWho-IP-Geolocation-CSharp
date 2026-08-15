using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPWho;

class TestMain
{
    static int pass = 0, fail = 0;
    static void Ok(bool c, string m)
    {
        if (c) { pass++; Console.WriteLine("  PASS " + m); }
        else   { fail++; Console.WriteLine("  FAIL " + m); }
    }

    static async Task<int> Main()
    {
        string key = Environment.GetEnvironmentVariable("IPWHO_API_KEY");
        var c = new IpWhoClient(key);

        // 1. lookup
        var r = await c.LookupAsync("8.8.8.8");
        var d = r.Data;
        var gl = d.GeoLocation; var tz = d.Timezone; var fl = d.Flag; var cu = d.Currency; var cn = d.Connection;
        Ok(d.Ip == "8.8.8.8", "lookup ip == 8.8.8.8");
        Ok(gl.Country == "United States", $"country == United States (got {gl.Country})");
        Ok(cn.AsnNumber.HasValue && (int)cn.AsnNumber.Value == 15169, $"asn_number == 15169 (got {cn.AsnNumber})");
        Ok(gl.DialCode != null, $"dial_code captured ({gl.DialCode})");
        Ok(gl.IsInEu.HasValue, "is_in_eu captured");
        Ok(tz.TimeZone != null, $"time_zone captured ({tz.TimeZone})");
        Ok(fl.FlagIcon != null, $"flag_Icon captured ({fl.FlagIcon})");
        Ok(fl.FlagUnicode != null, $"flag_unicode captured ({fl.FlagUnicode})");
        Ok(cu.NamePlural != null, $"name_plural captured ({cu.NamePlural})");
        Ok(cn.AsnOrg != null, $"asn_org captured ({cn.AsnOrg})");
        Ok(cn.ConnectionType != null, $"connection_type captured ({cn.ConnectionType})");

        // 2. me
        var me = await c.MeAsync();
        Ok(me.Data != null && !string.IsNullOrEmpty(me.Data.Ip), $"me ip captured ({me.Data?.Ip})");

        // 3. bulk
        var b = await c.BulkAsync(new List<string> { "8.8.8.8", "1.1.1.1" });
        Ok(b != null && b.Count == 2, $"bulk returns 2 (got {b?.Count})");

        // 4. bad key
        try
        {
            await new IpWhoClient("sk.invalid_test_key").LookupAsync("8.8.8.8");
            Ok(false, "bad key should raise");
        }
        catch (IpWhoException e) { Ok(true, "bad key raised " + e.GetType().Name); }
        catch (Exception e) { Ok(true, "bad key raised " + e.GetType().Name); }

        Console.WriteLine($"\nCSHARP RESULT: {pass} passed, {fail} failed");
        return fail > 0 ? 1 : 0;
    }
}
