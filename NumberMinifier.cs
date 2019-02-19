using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberMinifier {
    private static readonly uint[] orderNameHashes = new uint[102] {
        "Uncentillion".ToJenkins(),
        "Centillion".ToJenkins(),
        "Novemnonagintillion".ToJenkins(),
        "Onctononagintillion".ToJenkins(),
        "Septnonagintillion".ToJenkins(),
        "Sexnonagintillion".ToJenkins(),
        "Quinnonagintillion".ToJenkins(),
        "Quattuornonagintillion".ToJenkins(),
        "Trenonagintillion".ToJenkins(),
        "Duononagintillion".ToJenkins(),
        "Unnonagintillion".ToJenkins(),
        "Nonagintillion".ToJenkins(),
        "Novemoctogintillion".ToJenkins(),
        "Octooctogintillion".ToJenkins(),
        "Septoctogintillion".ToJenkins(),
        "Sexoctogintillion".ToJenkins(),
        "Quinoctogintillion".ToJenkins(),
        "Quattuoroctogintillion".ToJenkins(),
        "Treoctogintillion".ToJenkins(),
        "Duooctogintillion".ToJenkins(),
        "Unoctogintillion".ToJenkins(),
        "Octogintillion".ToJenkins(),
        "Novemseptuagintillion".ToJenkins(),
        "Octoseptuagintillion".ToJenkins(),
        "Septseptuagintillion".ToJenkins(),
        "Sexseptuagintillion".ToJenkins(),
        "Quinseptuagintillion".ToJenkins(),
        "Quattuorseptuagintillion".ToJenkins(),
        "Treseptuagintillion".ToJenkins(),
        "Duoseptuagintillion".ToJenkins(),
        "Unseptuagintillion".ToJenkins(),
        "Septuagintillion".ToJenkins(),
        "Novemsexagintillion".ToJenkins(),
        "Octosexagintillion".ToJenkins(),
        "Septsexagintillion".ToJenkins(),
        "Sexsexagintillion".ToJenkins(),
        "Quinsexagintillion".ToJenkins(),
        "Quattuorsexagintillion".ToJenkins(),
        "Tresexagintillion".ToJenkins(),
        "Duosexagintillion".ToJenkins(),
        "Unsexagintillion".ToJenkins(),
        "Sexagintillion".ToJenkins(),
        "Novemquinquagintillion".ToJenkins(),
        "Octoquinquagintillion".ToJenkins(),
        "Septquinquagintillion".ToJenkins(),
        "Sexquinquagintillion".ToJenkins(),
        "Quinquinquagintillion".ToJenkins(),
        "Quattuorquinquagintillion".ToJenkins(),
        "Trequinquagintillion".ToJenkins(),
        "Duoquinquagintillion".ToJenkins(),
        "Unquinquagintillion".ToJenkins(),
        "Quinquagintillion".ToJenkins(),
        "Novemquadragintillion".ToJenkins(),
        "Octoquadragintillion".ToJenkins(),
        "Septquadragintillion".ToJenkins(),
        "Sexquadragintillion".ToJenkins(),
        "Quinquadragintillion".ToJenkins(),
        "Quattuorquadragintillion".ToJenkins(),
        "Trequadragintillion".ToJenkins(),
        "Duoquadragintillion".ToJenkins(),
        "Unquadragintillion".ToJenkins(),
        "Quadragintillion".ToJenkins(),
        "Novemtrigintillion".ToJenkins(),
        "Octotrigintillion".ToJenkins(),
        "Septentrigintillion".ToJenkins(),
        "Sextrigintillion".ToJenkins(),
        "Quintrigintillion".ToJenkins(),
        "Quattuortrigintillion".ToJenkins(),
        "Tretrigintillion".ToJenkins(),
        "Duotrigintillion".ToJenkins(),
        "Untrigintillion".ToJenkins(),
        "Trigintillion".ToJenkins(),
        "Novemvigintillion".ToJenkins(),
        "Octovigintillion".ToJenkins(),
        "Septenvigintillion".ToJenkins(),
        "Sexvigintillion".ToJenkins(),
        "Quinvigintillion".ToJenkins(),
        "Quattuorvigintillion".ToJenkins(),
        "Tresvigintillion".ToJenkins(),
        "Duovigintillion".ToJenkins(),
        "Unvigintillion".ToJenkins(),
        "Vigintillion".ToJenkins(),
        "Novemdecillion".ToJenkins(),
        "Octodecillion".ToJenkins(),
        "Septendecillion".ToJenkins(),
        "Sexdecillion".ToJenkins(),
        "Quindecillion".ToJenkins(),
        "Quattuordecillion".ToJenkins(),
        "Tredecillion".ToJenkins(),
        "Duodecillion".ToJenkins(),
        "Undecillion".ToJenkins(),
        "Decillion".ToJenkins(),
        "Nonillion".ToJenkins(),
        "Octillion".ToJenkins(),
        "Septillion".ToJenkins(),
        "Sextillion".ToJenkins(),
        "Quintillion".ToJenkins(),
        "Quadrillion".ToJenkins(),
        "Trillion".ToJenkins(),
        "Billion".ToJenkins(),
        "Million".ToJenkins(),
        "Thousand".ToJenkins()
    };


    public static Dictionary<int, string> FillAbbreviations()
    {
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
        var charIndex = 0;
        var doubleCharIndex = 0;

        var result = new Dictionary<int, string>();
        result.Add(3, "K");
        result.Add(6, "M");
        result.Add(9, "B");
        
        for (int i = 12; i < 1000; i++)
        {
            if (i % 3 == 0)
            {

                if (charIndex == alpha.Length)
                {
                    charIndex = 0;
                    doubleCharIndex++;
                }

                if (doubleCharIndex == alpha.Length)
                {
                    charIndex++;
                    doubleCharIndex = 0;
                }

                result.Add(i, $"{alpha[charIndex]}{alpha[doubleCharIndex]}");
                doubleCharIndex++;
            }
        }

        return result;
    }




    private static readonly Dictionary<int, string> abbreviations = FillAbbreviations();/*= new Dictionary<int, string> {
        [3] = "K",
        [6] = "M",
        [9] = "B",
        [12] = "t",
        [15] = "q",
        [18] = "Q",
        [21] = "s",
        [24] = "S",
        [27] = "o",
        [30] = "n",
        [33] = "d",
        [36] = "U",
        [39] = "D",
        [42] = "T",
        [45] = "Qt",
        [48] = "Qd",
        [51] = "Sd",
        [54] = "St",
        [57] = "O",
        [60] = "N",
        [63] = "v",
        [66] = "c"
    };
    */

    public static string[] PrettyAbbreviationValueSeparated(double value) {
        int digitCount = (int)Math.Ceiling(Math.Log10(value));
        string formatFirst = "{0:F1}";
        string formatSecond = "{0}";
        if(digitCount  >= 1000 ) {
            int rem = digitCount % 3;
            digitCount -= rem;
            string powerOf10 = "1e" + digitCount.ToString();
            double doublePower = double.Parse(powerOf10);
            return new string[] { string.Format(formatFirst, value / doublePower), string.Format(formatSecond, powerOf10) };
        } else if(digitCount >= 3 ) {
            int rem = digitCount % 3;
            digitCount -= rem;

            if (value / Math.Pow(10, digitCount) < 1.0) {
                digitCount -= 3;
            }

            if(abbreviations.ContainsKey(digitCount)) {
                string powerOf10 = "1e" + digitCount.ToString();
                double doublePower = double.Parse(powerOf10);
                return new string[] { string.Format(formatFirst, value / doublePower), string.Format(formatSecond, abbreviations[digitCount]) };
            } else {
                return new string[] { string.Format(formatFirst, value), string.Empty };
            }
        } else {
            if (value < 1)
            {

                return new string[] { string.Format(formatFirst, value), string.Empty };
            }
            return new string[] { ((int)value).ToString(), string.Empty };
        }
    }

    public static string PrettyAbbreviatedValue(double value, bool endlOnWord = false, string customSep = "", bool useDecimals = true) {
        int digitCount = (int)Math.Ceiling(Math.Log10(value));
        
        string format = "{0:F2}{1}{2}";
        if (!useDecimals)
        {
            format = "{0:0}{1}{2}";
        }
        var endl = (endlOnWord) ? "\r\n" : customSep;    
            
        if (digitCount >= 1000) {
            int rem = digitCount % 3;
            digitCount -= rem;
            string powerOf10 = "1e" + digitCount.ToString();
            double doublePower = double.Parse(powerOf10);
            return string.Format(format, value / doublePower, endl, powerOf10);
        } else if (digitCount >= 3) {
            int rem = digitCount % 3;
            digitCount -= rem;

            if(value / Math.Pow(10, digitCount) < 1.0 ) {
                digitCount -= 3;
            }

            if (abbreviations.ContainsKey(digitCount)) {
                
                string powerOf10 = "1e" + digitCount.ToString();
                double doublePower = double.Parse(powerOf10);
                return string.Format(format, value / doublePower, endl, abbreviations[digitCount]);
            } else {
                //throw new UnityException($"invalid digit count value => {digitCount}");
                return string.Format(format, value, endl, string.Empty);
            }
        } else {
            return string.Format("{0:F1}", value);
        }        
    }


    private static string[] PrettyRepresentation(double value) {
        var digitCount = (int)Math.Ceiling(Math.Log10(value));
        var localization = GameServices.Instance?.ResourceService?.Localization ?? null;

        string format = "{0:F1}";

        if (localization == null) {
            return new string[] { value.Format(format), string.Empty };
        }


        if (digitCount > 306) return new string[] { (value / 1e306).Format(format), localization.GetString(orderNameHashes[0]) };
        if (digitCount > 303) return new string[] { (value / 1e303).Format(format), localization.GetString(orderNameHashes[1]) };
        if (digitCount > 300) return new string[] { (value / 1e300).Format(format), localization.GetString(orderNameHashes[2]) };
        if (digitCount > 297) return new string[] { (value / 1e297).Format(format), localization.GetString(orderNameHashes[3]) };
        if (digitCount > 294) return new string[] { (value / 1e294).Format(format), localization.GetString(orderNameHashes[4]) };
        if (digitCount > 291) return new string[] { (value / 1e291).Format(format), localization.GetString(orderNameHashes[5]) };
        if (digitCount > 288) return new string[] { (value / 1e288).Format(format), localization.GetString(orderNameHashes[6]) };
        if (digitCount > 285) return new string[] { (value / 1e285).Format(format), localization.GetString(orderNameHashes[7]) };
        if (digitCount > 282) return new string[] { (value / 1e282).Format(format), localization.GetString(orderNameHashes[8]) };
        if (digitCount > 279) return new string[] { (value / 1e279).Format(format), localization.GetString(orderNameHashes[9]) };
        if (digitCount > 276) return new string[] { (value / 1e276).Format(format), localization.GetString(orderNameHashes[10]) };
        if (digitCount > 273) return new string[] { (value / 1e273).Format(format), localization.GetString(orderNameHashes[11]) };
        if (digitCount > 270) return new string[] { (value / 1e270).Format(format), localization.GetString(orderNameHashes[12]) };
        if (digitCount > 267) return new string[] { (value / 1e267).Format(format), localization.GetString(orderNameHashes[13]) };
        if (digitCount > 264) return new string[] { (value / 1e264).Format(format), localization.GetString(orderNameHashes[14]) };
        if (digitCount > 261) return new string[] { (value / 1e261).Format(format), localization.GetString(orderNameHashes[15]) };
        if (digitCount > 258) return new string[] { (value / 1e258).Format(format), localization.GetString(orderNameHashes[16]) };
        if (digitCount > 255) return new string[] { (value / 1e255).Format(format), localization.GetString(orderNameHashes[17]) };
        if (digitCount > 252) return new string[] { (value / 1e252).Format(format), localization.GetString(orderNameHashes[18]) };
        if (digitCount > 249) return new string[] { (value / 1e249).Format(format), localization.GetString(orderNameHashes[19]) };
        if (digitCount > 246) return new string[] { (value / 1e246).Format(format), localization.GetString(orderNameHashes[20]) };
        if (digitCount > 243) return new string[] { (value / 1e243).Format(format), localization.GetString(orderNameHashes[21]) };
        if (digitCount > 240) return new string[] { (value / 1e240).Format(format), localization.GetString(orderNameHashes[22]) };
        if (digitCount > 237) return new string[] { (value / 1e237).Format(format), localization.GetString(orderNameHashes[23]) };
        if (digitCount > 234) return new string[] { (value / 1e234).Format(format), localization.GetString(orderNameHashes[24]) };
        if (digitCount > 231) return new string[] { (value / 1e231).Format(format), localization.GetString(orderNameHashes[25]) };
        if (digitCount > 228) return new string[] { (value / 1e228).Format(format), localization.GetString(orderNameHashes[26]) };
        if (digitCount > 225) return new string[] { (value / 1e225).Format(format), localization.GetString(orderNameHashes[27]) };
        if (digitCount > 222) return new string[] { (value / 1e222).Format(format), localization.GetString(orderNameHashes[28]) };
        if (digitCount > 219) return new string[] { (value / 1e219).Format(format), localization.GetString(orderNameHashes[29]) };
        if (digitCount > 216) return new string[] { (value / 1e216).Format(format), localization.GetString(orderNameHashes[30]) };
        if (digitCount > 213) return new string[] { (value / 1e213).Format(format), localization.GetString(orderNameHashes[31]) };
        if (digitCount > 210) return new string[] { (value / 1e210).Format(format), localization.GetString(orderNameHashes[32]) };
        if (digitCount > 207) return new string[] { (value / 1e207).Format(format), localization.GetString(orderNameHashes[33]) };
        if (digitCount > 204) return new string[] { (value / 1e204).Format(format), localization.GetString(orderNameHashes[34]) };
        if (digitCount > 201) return new string[] { (value / 1e201).Format(format), localization.GetString(orderNameHashes[35]) };
        if (digitCount > 198) return new string[] { (value / 1e198).Format(format), localization.GetString(orderNameHashes[36]) };
        if (digitCount > 195) return new string[] { (value / 1e195).Format(format), localization.GetString(orderNameHashes[37]) };
        if (digitCount > 192) return new string[] { (value / 1e192).Format(format), localization.GetString(orderNameHashes[38]) };
        if (digitCount > 189) return new string[] { (value / 1e189).Format(format), localization.GetString(orderNameHashes[39]) };
        if (digitCount > 186) return new string[] { (value / 1e186).Format(format), localization.GetString(orderNameHashes[40]) };
        if (digitCount > 183) return new string[] { (value / 1e183).Format(format), localization.GetString(orderNameHashes[41]) };
        if (digitCount > 180) return new string[] { (value / 1e180).Format(format), localization.GetString(orderNameHashes[42]) };
        if (digitCount > 177) return new string[] { (value / 1e177).Format(format), localization.GetString(orderNameHashes[43]) };
        if (digitCount > 174) return new string[] { (value / 1e174).Format(format), localization.GetString(orderNameHashes[44]) };
        if (digitCount > 171) return new string[] { (value / 1e171).Format(format), localization.GetString(orderNameHashes[45]) };
        if (digitCount > 168) return new string[] { (value / 1e168).Format(format), localization.GetString(orderNameHashes[46]) };
        if (digitCount > 165) return new string[] { (value / 1e165).Format(format), localization.GetString(orderNameHashes[47]) };
        if (digitCount > 162) return new string[] { (value / 1e162).Format(format), localization.GetString(orderNameHashes[48]) };
        if (digitCount > 159) return new string[] { (value / 1e159).Format(format), localization.GetString(orderNameHashes[49]) };
        if (digitCount > 156) return new string[] { (value / 1e156).Format(format), localization.GetString(orderNameHashes[50]) };
        if (digitCount > 153) return new string[] { (value / 1e153).Format(format), localization.GetString(orderNameHashes[51]) };
        if (digitCount > 150) return new string[] { (value / 1e150).Format(format), localization.GetString(orderNameHashes[52]) };
        if (digitCount > 147) return new string[] { (value / 1e147).Format(format), localization.GetString(orderNameHashes[53]) };
        if (digitCount > 144) return new string[] { (value / 1e144).Format(format), localization.GetString(orderNameHashes[54]) };
        if (digitCount > 141) return new string[] { (value / 1e141).Format(format), localization.GetString(orderNameHashes[55]) };
        if (digitCount > 138) return new string[] { (value / 1e138).Format(format), localization.GetString(orderNameHashes[56]) };
        if (digitCount > 135) return new string[] { (value / 1e135).Format(format), localization.GetString(orderNameHashes[57]) };
        if (digitCount > 132) return new string[] { (value / 1e132).Format(format), localization.GetString(orderNameHashes[58]) };
        if (digitCount > 129) return new string[] { (value / 1e129).Format(format), localization.GetString(orderNameHashes[59]) };
        if (digitCount > 126) return new string[] { (value / 1e126).Format(format), localization.GetString(orderNameHashes[60]) };
        if (digitCount > 123) return new string[] { (value / 1e123).Format(format), localization.GetString(orderNameHashes[61]) };
        if (digitCount > 120) return new string[] { (value / 1e120).Format(format), localization.GetString(orderNameHashes[62]) };
        if (digitCount > 117) return new string[] { (value / 1e117).Format(format), localization.GetString(orderNameHashes[63]) };
        if (digitCount > 114) return new string[] { (value / 1e114).Format(format), localization.GetString(orderNameHashes[64]) };
        if (digitCount > 111) return new string[] { (value / 1e111).Format(format), localization.GetString(orderNameHashes[65]) };
        if (digitCount > 108) return new string[] { (value / 1e108).Format(format), localization.GetString(orderNameHashes[66]) };
        if (digitCount > 105) return new string[] { (value / 1e105).Format(format), localization.GetString(orderNameHashes[67]) };
        if (digitCount > 102) return new string[] { (value / 1e102).Format(format), localization.GetString(orderNameHashes[68]) };
        if (digitCount > 99) return new string[] { (value / 1e99).Format(format), localization.GetString(orderNameHashes[69]) };
        if (digitCount > 96) return new string[] { (value / 1e96).Format(format), localization.GetString(orderNameHashes[70]) };
        if (digitCount > 93) return new string[] { (value / 1e93).Format(format), localization.GetString(orderNameHashes[71]) };
        if (digitCount > 90) return new string[] { (value / 1e90).Format(format), localization.GetString(orderNameHashes[72]) };
        if (digitCount > 87) return new string[] { (value / 1e87).Format(format), localization.GetString(orderNameHashes[73]) };
        if (digitCount > 84) return new string[] { (value / 1e84).Format(format), localization.GetString(orderNameHashes[74]) };
        if (digitCount > 81) return new string[] { (value / 1e81).Format(format), localization.GetString(orderNameHashes[75]) };
        if (digitCount > 78) return new string[] { (value / 1e78).Format(format), localization.GetString(orderNameHashes[76]) };
        if (digitCount > 75) return new string[] { (value / 1e75).Format(format), localization.GetString(orderNameHashes[77]) };
        if (digitCount > 72) return new string[] { (value / 1e72).Format(format), localization.GetString(orderNameHashes[78]) };
        if (digitCount > 69) return new string[] { (value / 1e69).Format(format), localization.GetString(orderNameHashes[79]) };
        if (digitCount > 66) return new string[] { (value / 1e66).Format(format), localization.GetString(orderNameHashes[80]) };
        if (digitCount > 63) return new string[] { (value / 1e63).Format(format), localization.GetString(orderNameHashes[81]) };
        if (digitCount > 60) return new string[] { (value / 1e60).Format(format), localization.GetString(orderNameHashes[82]) };
        if (digitCount > 57) return new string[] { (value / 1e57).Format(format), localization.GetString(orderNameHashes[83]) };
        if (digitCount > 54) return new string[] { (value / 1e54).Format(format), localization.GetString(orderNameHashes[84]) };
        if (digitCount > 51) return new string[] { (value / 1e51).Format(format), localization.GetString(orderNameHashes[85]) };
        if (digitCount > 48) return new string[] { (value / 1e48).Format(format), localization.GetString(orderNameHashes[86]) };
        if (digitCount > 45) return new string[] { (value / 1e45).Format(format), localization.GetString(orderNameHashes[87]) };
        if (digitCount > 42) return new string[] { (value / 1e42).Format(format), localization.GetString(orderNameHashes[88]) };
        if (digitCount > 39) return new string[] { (value / 1e39).Format(format), localization.GetString(orderNameHashes[89]) };
        if (digitCount > 36) return new string[] { (value / 1e36).Format(format), localization.GetString(orderNameHashes[90]) };
        if (digitCount > 33) return new string[] { (value / 1e33).Format(format), localization.GetString(orderNameHashes[91]) };
        if (digitCount > 30) return new string[] { (value / 1e30).Format(format), localization.GetString(orderNameHashes[92]) };
        if (digitCount > 27) return new string[] { (value / 1e27).Format(format), localization.GetString(orderNameHashes[93]) };
        if (digitCount > 24) return new string[] { (value / 1e24).Format(format), localization.GetString(orderNameHashes[94]) };
        if (digitCount > 21) return new string[] { (value / 1e21).Format(format), localization.GetString(orderNameHashes[95]) };
        if (digitCount > 18) return new string[] { (value / 1e18).Format(format), localization.GetString(orderNameHashes[96]) };
        if (digitCount > 15) return new string[] { (value / 1e15).Format(format), localization.GetString(orderNameHashes[97]) };
        if (digitCount > 12) return new string[] { (value / 1e12).Format(format), localization.GetString(orderNameHashes[98]) };
        if (digitCount > 9) return new string[] { (value / 1e9).Format(format), localization.GetString(orderNameHashes[99]) };
        if (digitCount > 6) return new string[] { (value / 1e6).Format(format), localization.GetString(orderNameHashes[100]) };
        if (digitCount > 3) return new string[] { (value / 1e3).Format(format), localization.GetString(orderNameHashes[101]) };
        return new string[] { value.Format(format), string.Empty };
    }

    private static string MinifyBigInt(double value, bool endlOnWord, string customSep = "", bool useDecimals = true) {
        var digitCount = Math.Ceiling(Math.Log10(value));

        string format = "{0:F2}{1}{2}";
        if (!useDecimals) {
            format = "{0:0}{1}{2}";
        }
        var endl = (endlOnWord) ? "\r\n" : customSep;

        var localization = GameServices.Instance.ResourceService.Localization;

        if (digitCount > 306) return string.Format(format, value / 1e306, endl, localization.GetString(orderNameHashes[0]));
        if (digitCount > 303) return string.Format(format, value / 1e303, endl, localization.GetString(orderNameHashes[1]));
        if (digitCount > 300) return string.Format(format, value / 1e300, endl, localization.GetString(orderNameHashes[2]));
        if (digitCount > 297) return string.Format(format, value / 1e297, endl, localization.GetString(orderNameHashes[3]));
        if (digitCount > 294) return string.Format(format, value / 1e294, endl, localization.GetString(orderNameHashes[4]));
        if (digitCount > 291) return string.Format(format, value / 1e291, endl, localization.GetString(orderNameHashes[5]));
        if (digitCount > 288) return string.Format(format, value / 1e288, endl, localization.GetString(orderNameHashes[6]));
        if (digitCount > 285) return string.Format(format, value / 1e285, endl, localization.GetString(orderNameHashes[7]));
        if (digitCount > 282) return string.Format(format, value / 1e282, endl, localization.GetString(orderNameHashes[8]));
        if (digitCount > 279) return string.Format(format, value / 1e279, endl, localization.GetString(orderNameHashes[9]));
        if (digitCount > 276) return string.Format(format, value / 1e276, endl, localization.GetString(orderNameHashes[10]));
        if (digitCount > 273) return string.Format(format, value / 1e273, endl, localization.GetString(orderNameHashes[11]));
        if (digitCount > 270) return string.Format(format, value / 1e270, endl, localization.GetString(orderNameHashes[12]));
        if (digitCount > 267) return string.Format(format, value / 1e267, endl, localization.GetString(orderNameHashes[13]));
        if (digitCount > 264) return string.Format(format, value / 1e264, endl, localization.GetString(orderNameHashes[14]));
        if (digitCount > 261) return string.Format(format, value / 1e261, endl, localization.GetString(orderNameHashes[15]));
        if (digitCount > 258) return string.Format(format, value / 1e258, endl, localization.GetString(orderNameHashes[16]));
        if (digitCount > 255) return string.Format(format, value / 1e255, endl, localization.GetString(orderNameHashes[17]));
        if (digitCount > 252) return string.Format(format, value / 1e252, endl, localization.GetString(orderNameHashes[18]));
        if (digitCount > 249) return string.Format(format, value / 1e249, endl, localization.GetString(orderNameHashes[19]));
        if (digitCount > 246) return string.Format(format, value / 1e246, endl, localization.GetString(orderNameHashes[20]));
        if (digitCount > 243) return string.Format(format, value / 1e243, endl, localization.GetString(orderNameHashes[21]));
        if (digitCount > 240) return string.Format(format, value / 1e240, endl, localization.GetString(orderNameHashes[22]));
        if (digitCount > 237) return string.Format(format, value / 1e237, endl, localization.GetString(orderNameHashes[23]));
        if (digitCount > 234) return string.Format(format, value / 1e234, endl, localization.GetString(orderNameHashes[24]));
        if (digitCount > 231) return string.Format(format, value / 1e231, endl, localization.GetString(orderNameHashes[25]));
        if (digitCount > 228) return string.Format(format, value / 1e228, endl, localization.GetString(orderNameHashes[26]));
        if (digitCount > 225) return string.Format(format, value / 1e225, endl, localization.GetString(orderNameHashes[27]));
        if (digitCount > 222) return string.Format(format, value / 1e222, endl, localization.GetString(orderNameHashes[28]));
        if (digitCount > 219) return string.Format(format, value / 1e219, endl, localization.GetString(orderNameHashes[29]));
        if (digitCount > 216) return string.Format(format, value / 1e216, endl, localization.GetString(orderNameHashes[30]));
        if (digitCount > 213) return string.Format(format, value / 1e213, endl, localization.GetString(orderNameHashes[31]));
        if (digitCount > 210) return string.Format(format, value / 1e210, endl, localization.GetString(orderNameHashes[32]));
        if (digitCount > 207) return string.Format(format, value / 1e207, endl, localization.GetString(orderNameHashes[33]));
        if (digitCount > 204) return string.Format(format, value / 1e204, endl, localization.GetString(orderNameHashes[34]));
        if (digitCount > 201) return string.Format(format, value / 1e201, endl, localization.GetString(orderNameHashes[35]));
        if (digitCount > 198) return string.Format(format, value / 1e198, endl, localization.GetString(orderNameHashes[36]));
        if (digitCount > 195) return string.Format(format, value / 1e195, endl, localization.GetString(orderNameHashes[37]));
        if (digitCount > 192) return string.Format(format, value / 1e192, endl, localization.GetString(orderNameHashes[38]));
        if (digitCount > 189) return string.Format(format, value / 1e189, endl, localization.GetString(orderNameHashes[39]));
        if (digitCount > 186) return string.Format(format, value / 1e186, endl, localization.GetString(orderNameHashes[40]));
        if (digitCount > 183) return string.Format(format, value / 1e183, endl, localization.GetString(orderNameHashes[41]));
        if (digitCount > 180) return string.Format(format, value / 1e180, endl, localization.GetString(orderNameHashes[42]));
        if (digitCount > 177) return string.Format(format, value / 1e177, endl, localization.GetString(orderNameHashes[43]));
        if (digitCount > 174) return string.Format(format, value / 1e174, endl, localization.GetString(orderNameHashes[44]));
        if (digitCount > 171) return string.Format(format, value / 1e171, endl, localization.GetString(orderNameHashes[45]));
        if (digitCount > 168) return string.Format(format, value / 1e168, endl, localization.GetString(orderNameHashes[46]));
        if (digitCount > 165) return string.Format(format, value / 1e165, endl, localization.GetString(orderNameHashes[47]));
        if (digitCount > 162) return string.Format(format, value / 1e162, endl, localization.GetString(orderNameHashes[48]));
        if (digitCount > 159) return string.Format(format, value / 1e159, endl, localization.GetString(orderNameHashes[49]));
        if (digitCount > 156) return string.Format(format, value / 1e156, endl, localization.GetString(orderNameHashes[50]));
        if (digitCount > 153) return string.Format(format, value / 1e153, endl, localization.GetString(orderNameHashes[51]));
        if (digitCount > 150) return string.Format(format, value / 1e150, endl, localization.GetString(orderNameHashes[52]));
        if (digitCount > 147) return string.Format(format, value / 1e147, endl, localization.GetString(orderNameHashes[53]));
        if (digitCount > 144) return string.Format(format, value / 1e144, endl, localization.GetString(orderNameHashes[54]));
        if (digitCount > 141) return string.Format(format, value / 1e141, endl, localization.GetString(orderNameHashes[55]));
        if (digitCount > 138) return string.Format(format, value / 1e138, endl, localization.GetString(orderNameHashes[56]));
        if (digitCount > 135) return string.Format(format, value / 1e135, endl, localization.GetString(orderNameHashes[57]));
        if (digitCount > 132) return string.Format(format, value / 1e132, endl, localization.GetString(orderNameHashes[58]));
        if (digitCount > 129) return string.Format(format, value / 1e129, endl, localization.GetString(orderNameHashes[59]));
        if (digitCount > 126) return string.Format(format, value / 1e126, endl, localization.GetString(orderNameHashes[60]));
        if (digitCount > 123) return string.Format(format, value / 1e123, endl, localization.GetString(orderNameHashes[61]));
        if (digitCount > 120) return string.Format(format, value / 1e120, endl, localization.GetString(orderNameHashes[62]));
        if (digitCount > 117) return string.Format(format, value / 1e117, endl, localization.GetString(orderNameHashes[63]));
        if (digitCount > 114) return string.Format(format, value / 1e114, endl, localization.GetString(orderNameHashes[64]));
        if (digitCount > 111) return string.Format(format, value / 1e111, endl, localization.GetString(orderNameHashes[65]));
        if (digitCount > 108) return string.Format(format, value / 1e108, endl, localization.GetString(orderNameHashes[66]));
        if (digitCount > 105) return string.Format(format, value / 1e105, endl, localization.GetString(orderNameHashes[67]));
        if (digitCount > 102) return string.Format(format, value / 1e102, endl, localization.GetString(orderNameHashes[68]));
        if (digitCount > 99) return string.Format(format, value / 1e99, endl, localization.GetString(orderNameHashes[69]));
        if (digitCount > 96) return string.Format(format, value / 1e96, endl, localization.GetString(orderNameHashes[70]));
        if (digitCount > 93) return string.Format(format, value / 1e93, endl, localization.GetString(orderNameHashes[71]));
        if (digitCount > 90) return string.Format(format, value / 1e90, endl, localization.GetString(orderNameHashes[72]));
        if (digitCount > 87) return string.Format(format, value / 1e87, endl, localization.GetString(orderNameHashes[73]));
        if (digitCount > 84) return string.Format(format, value / 1e84, endl, localization.GetString(orderNameHashes[74]));
        if (digitCount > 81) return string.Format(format, value / 1e81, endl, localization.GetString(orderNameHashes[75]));
        if (digitCount > 78) return string.Format(format, value / 1e78, endl, localization.GetString(orderNameHashes[76]));
        if (digitCount > 75) return string.Format(format, value / 1e75, endl, localization.GetString(orderNameHashes[77]));
        if (digitCount > 72) return string.Format(format, value / 1e72, endl, localization.GetString(orderNameHashes[78]));
        if (digitCount > 69) return string.Format(format, value / 1e69, endl, localization.GetString(orderNameHashes[79]));
        if (digitCount > 66) return string.Format(format, value / 1e66, endl, localization.GetString(orderNameHashes[80]));
        if (digitCount > 63) return string.Format(format, value / 1e63, endl, localization.GetString(orderNameHashes[81]));
        if (digitCount > 60) return string.Format(format, value / 1e60, endl, localization.GetString(orderNameHashes[82]));
        if (digitCount > 57) return string.Format(format, value / 1e57, endl, localization.GetString(orderNameHashes[83]));
        if (digitCount > 54) return string.Format(format, value / 1e54, endl, localization.GetString(orderNameHashes[84]));
        if (digitCount > 51) return string.Format(format, value / 1e51, endl, localization.GetString(orderNameHashes[85]));
        if (digitCount > 48) return string.Format(format, value / 1e48, endl, localization.GetString(orderNameHashes[86]));
        if (digitCount > 45) return string.Format(format, value / 1e45, endl, localization.GetString(orderNameHashes[87]));
        if (digitCount > 42) return string.Format(format, value / 1e42, endl, localization.GetString(orderNameHashes[88]));
        if (digitCount > 39) return string.Format(format, value / 1e39, endl, localization.GetString(orderNameHashes[89]));
        if (digitCount > 36) return string.Format(format, value / 1e36, endl, localization.GetString(orderNameHashes[90]));
        if (digitCount > 33) return string.Format(format, value / 1e33, endl, localization.GetString(orderNameHashes[91]));
        if (digitCount > 30) return string.Format(format, value / 1e30, endl, localization.GetString(orderNameHashes[92]));
        if (digitCount > 27) return string.Format(format, value / 1e27, endl, localization.GetString(orderNameHashes[93]));
        if (digitCount > 24) return string.Format(format, value / 1e24, endl, localization.GetString(orderNameHashes[94]));
        if (digitCount > 21) return string.Format(format, value / 1e21, endl, localization.GetString(orderNameHashes[95]));
        if (digitCount > 18) return string.Format(format, value / 1e18, endl, localization.GetString(orderNameHashes[96]));
        if (digitCount > 15) return string.Format(format, value / 1e15, endl, localization.GetString(orderNameHashes[97]));
        if (digitCount > 12) return string.Format(format, value / 1e12, endl, localization.GetString(orderNameHashes[98]));
        if (digitCount > 9) return string.Format(format, value / 1e9, endl, localization.GetString(orderNameHashes[99]));
        if (digitCount > 6) return string.Format(format, value / 1e6, endl, localization.GetString(orderNameHashes[100]));
        if (digitCount > 3) return string.Format(format, value / 1e3, endl, localization.GetString(orderNameHashes[101]));
        if (digitCount <= 3) return value.ToString("F2");

        return null;
    }
}
