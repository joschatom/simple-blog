namespace backend.Helpers;

public class PasswordHasher
{
    private static readonly string salt = "@Q-iFwja7fPns$67wIl&ZB6lgjpVYBxE";
    private static readonly string[] pepperList = [
        "J/i4J/a7fPns$6~p|\"s&",
        "LT+H-`kCM.gdRV=~5k0P",
        "@Q-iFwj7wIl&gjpVYBxE",
        "Xi0Eyw+NFBwjHY,fNxu*",
        "#zZR21&t+QZnlK8G-i1\"",
        "DhWZI2'!sSgpEm=FP_r\\",
        "y-0kq+@ZB6l;9o2i.YQ;",
        "qaSb39B@LxII/t~S!2TX",
        "c3O5=h7Lt?jrc&jB0EGu",
        "7Ucj=nERMkSfD@T\\5r!m",
        "hWk|o8PbCe6R~4,$oQSG",
        "gDOHdwuZJEtiHX6Q=Gow",
        "XXyb'Ic1*=3!86%y4S!e",
        "tZTx\\*R8+vw&Bb#UL.\"I",
        "@nX:sCgB~2^'wXa6e0rN",
        "W|Hc=RT52;^J0^Hp?`Pg",
        "60krebE*rO-*!&Uw1zX$",
        "FpM**fA?VJJH$jDc5_A`",
        "G`Ec8r_s8iLMpi*mi?iK",
        "?'7s|'Ag:Q_UQr6Nz\\r:",
        "bqzhe7?NG1MEDSUC,JFi",
        "Fcn4-.EB3ThU,q!2fC/^",
        "A&UnK\\b^3N~ESq';2rWs",
        "KjiwxgI9Z10uI@8,3~&1",
        ",hbdY|K&7G^W1HmGy`RV",
        "n;mzgb#XDJ!5+e*m:7:^",
        "M2er*o\"j=D9p;#K6xz43",
        "/6XD/mJ7b4vpk57V_TeD",
        "/bO&N3np:-`k3Ve1%o=W",
        "tn.7qk`aqZ\"YH2Jfii2O"
    ];
    public static bool CompareHashAndPassword(string hash, string password)
    {
        foreach (string pepper in pepperList)
        {
            string passwordToVerfy = salt + password + pepper;

            if (BCrypt.Net.BCrypt.Verify(passwordToVerfy, hash))
            {
                return true;
            }

        }
        return false;
    }
    public static string HashPassword(string password)
    {
        int randomIndex = Helpers.GenerateRanInt(0, pepperList.Length - 1);
        string passwordToBeHashed = salt + password + pepperList[randomIndex];
        return BCrypt.Net.BCrypt.HashPassword(passwordToBeHashed);
    }
}