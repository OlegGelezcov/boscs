using Bos.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UpgradesDB
{
    //public static List<SerializedUpgrade> Upgrades;
    //public static List<SerializedUpgrade> InvestorUpgrades;

/* 
    public static void Load(string filename)
    {
        //var upLines = File.ReadAllLines(filename);

        //LoadInternal(upLines);
    }
*/
    /* 
    public static void LoadFromTable()
    {
        Upgrades = GameData.instance.cashUpgrades;
        Upgrades.Sort();
        
        
        InvestorUpgrades = GameData.instance.investorsUpgrades;
        InvestorUpgrades.Sort();
    }*/

    /* 
    public static void LoadInternal(IEnumerable<string> upLines)
    {
        Upgrades = new List<SerializedUpgrade>();

        foreach (var line in upLines)
        {
            var split = line.Split('|');

            int id = int.Parse(split[0]);
            string name = split[1];
            double cost = double.Parse(split[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            UpgradeType ty = (UpgradeType)int.Parse(split[3], NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            int am = (int)double.Parse(split[4], NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            
            Upgrades.Add(new SerializedUpgrade()
            {
                Id = id,
                Name = name,
                Cost = cost,
                Type = ty,
                Ammount = am
            });
        }

        Upgrades.Sort();
    }*/

    /* 
    public static void LoadInternalInvestors(IEnumerable<string> upLines)
    {
        InvestorUpgrades = new List<SerializedUpgrade>();

        foreach (var line in upLines)
        {
            var split = line.Split('|');

            int id = int.Parse(split[0]);
            string name = split[1];
            double cost = double.Parse(split[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            UpgradeType ty = (UpgradeType)int.Parse(split[3], NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            int am = (int)double.Parse(split[4], NumberStyles.Float, NumberFormatInfo.InvariantInfo);

            InvestorUpgrades.Add(new SerializedUpgrade()
            {
                Id = id,
                Name = name,
                Cost = cost,
                Type = ty,
                Ammount = am
            });
        }
        InvestorUpgrades.Sort();
    }*/


/* 
    internal static void LoadInternalString()
    {
        List<string> strings = new List<string>();
        strings.Add("0|Quality Sneakers|250000|0|3");
        strings.Add("1|Bubble gum air freshners|500000|0|3");
        strings.Add("2|Optimized Routes|1000000|0|3");
        strings.Add("3|First Class Seating|5000000|0|3");
        strings.Add("4|Complementary Champagne|10000000|0|3");
        strings.Add("5|Elite Stewardesses|25000000|0|3");
        strings.Add("6|Deep sea charts|500000000|0|3");
        strings.Add("8|Titanium plating|10000000000|0|3");
        strings.Add("9|Rapid Re-materialization|250000000000|0|3");

        strings.Add("-1|Transport Mogul|1000000000000|0|3");
        strings.Add("0|High Quality Hats|20000000000000|0|3");
        strings.Add("1|Bengal Tiger Leather Seats|50000000000000|0|3");
        strings.Add("2|LCD Displays|100000000000000|0|3");
        strings.Add("3|Sleeping Wagons|500000000000000|0|3");
        strings.Add("4|Jacuzzi|1E+15|0|3");
        strings.Add("5|Private Terminals|2E+15|0|3");
        strings.Add("6|Souvenir Shop|5E+15|0|3");
        strings.Add("8|Solar Powered|7E+15|0|3");
        strings.Add("9|Warp Technology|2E+16|0|3");

        strings.Add("-1|Worldwide Investors|5E+16|0|3");
        strings.Add("0|Online Orders|2E+18|0|3");
        strings.Add("1|NOS Kits|5E+18|0|3");
        strings.Add("2|Memory Foam Seating|7E+18|0|3");
        strings.Add("3|Robot Conductors|1E+19|0|3");
        strings.Add("4|VIP Services|2E+19|0|3");
        strings.Add("5|Enhanced Engines|3.5E+19|0|3");
        strings.Add("6|Eye-patches|5E+19|0|3");
        strings.Add("8|Starboard observation deck|7.5E+19|0|3");
        strings.Add("9|Microfusion Cells|2E+20|0|3");

        strings.Add("-1|Worldwide Renoun |5E+20|0|3");
        strings.Add("0|Lightning stickers|2.5E+22|0|3");
        strings.Add("1|Black Cab|5E+22|0|3");
        strings.Add("2|Climatronic 2000|1E+23|0|3");
        strings.Add("3|Magnetic Lines|2E+23|0|3");
        strings.Add("4|Shrimp cocktails|3E+23|0|3");
        strings.Add("5|Steel beam coating|4E+23|0|3");
        strings.Add("6|Sea food cuisine|5E+23|0|3");
        strings.Add("8|Hyperdrive engine|6E+23|0|3");
        strings.Add("9|Chrono-porter|8E+23|0|3");

        strings.Add("-1|Mr. Worldwide|9E+23|0|3");
        strings.Add("0|Platinum Handles|1E+27|0|7");
        strings.Add("1|Drift king drivers|5E+27|0|7");
        strings.Add("2|Casino Royale|2.5E+28|0|7");
        strings.Add("3|Pinpoint Booking|1E+29|0|7");
        strings.Add("4|Celebrity Hotspot|2.5E+29|0|7");
        strings.Add("5|Inertial Dampeners|5E+29|0|7");
        strings.Add("6|Wireless periscope|1E+30|0|7");
        strings.Add("8|Cloaking Technology|5E+30|0|7");
        strings.Add("9|Tin foil hats|5E+31|0|7");

        strings.Add("-1|Fort Knox|1E+42|0|7");
        strings.Add("1|Flotation Device|5E+42|0|3");
        strings.Add("2|Burger Bus|2.5E+43|0|3");
        strings.Add("3|Soundproofed Wagons|5E+43|0|3");
        strings.Add("4|Bulletproof Windows|1E+44|0|3");
        strings.Add("5|Helium Gas Masks|2.5E+44|0|3");
        strings.Add("6|VR Exploration|5E+44|0|3");
        strings.Add("9|Reflective Plating|1E+45|0|3");
        strings.Add("9|Tardis|1E+46|0|3");

        strings.Add("0|Self Driving Rickshaws|2.5E+46|0|3");
        strings.Add("-1|Trust fund|1E+47|0|3");
        strings.Add("1|Automatic Payments|2.5E+47|0|3");
        strings.Add("2|Jet engine busses|5E+47|0|3");
        strings.Add("3|Tank engines|7.5E+47|0|3");
        strings.Add("4|Personal Masseuse|1E+48|0|3");
        strings.Add("5|Unmeltable steel beams|5E+48|0|3");
        strings.Add("6|Nemo's Tactics|1.5E+49|0|3");
        strings.Add("8|Post-Credit Neuralizer|5E+49|0|3");
        strings.Add("9|Spaghetti monster safari|2.5E+50|0|3");

        strings.Add("0|Helicopter attachments|5E+50|0|3");
        strings.Add("-1|I|1E+51|0|3");
        strings.Add("-1|Like|1E+54|0|5");
        strings.Add("-1|Upgrades|1E+60|0|7");
        strings.Add("1|Cuban cigars|1E+61|0|3");
        strings.Add("2|Bus Phone X|1E+62|0|3");
        strings.Add("-1|The matrix|1E+66|0|9");
        strings.Add("3|Toaster adaptor|1E+67|0|3");
        strings.Add("4|Jukebox|1E+68|0|3");
        strings.Add("-1|Robo Brain|1E+72|0|11");
        strings.Add("5|PI in the sky|1E+73|0|3.14");
        strings.Add("6|Whale translator|1E+74|0|3");
        strings.Add("-1|Intergalactic investors|1E+75|0|13");
        strings.Add("8|Time dilation device|1E+76|0|3");
        strings.Add("-1|Alien Investors|1E+78|0|15");
        strings.Add("9|Wormhole X-treme!|1E+79|0|3");

        strings.Add("0|Gin & Tonic Special|1E+80|0|3");
        strings.Add("-1|Private Island Headquarters|1E+84|0|3");
        strings.Add("-1|Des-pie-cito|3E+87|0|3.14");
        strings.Add("1|The fastest and the most furious|1E+90|0|3");
        strings.Add("2|Vegan friendly busses|5E+90|0|3");
        strings.Add("3|Intense employee train-ing|2.5E+91|0|3");
        strings.Add("4|Haunted Limo|5E+91|0|3");
        strings.Add("5|S-pie Plane|1E+92|0|3.14");
        strings.Add("6|Rubber Ducklings|2.5E+92|0|3");
        strings.Add("8|Alien Captain|5E+92|0|3");
        strings.Add("9|Interdimensional Teleporter|5E+93|0|3");

        strings.Add("0|Dragon reins|1E+94|0|3");
        strings.Add("-1|Mars expansion|5E+95|0|2");
        strings.Add("1|Uber |2E+96|0|2");
        strings.Add("2|Flying busses|1.1E+97|0|2");
        strings.Add("3|Relaxing SPA|6.6E+97|0|2");
        strings.Add("4|Celebrity drivers|2.3E+98|0|2");
        strings.Add("5|Kebap Plane|4E+98|0|2");
        strings.Add("6|Watersports special|7E+98|0|2");
        strings.Add("8|Event Horizon Exploration|4E+99|0|2");
        strings.Add("-1|Monopoly|1E+100|0|9");
        strings.Add("9|DNA Manipulation|1.45E+101|0|2");

        strings.Add("0|Dragon space suit|3E+101|0|2");
        strings.Add("-1|Emancipation|5E+101|0|2");
        strings.Add("2|Interdimensional rift creator|5E+102|0|3");
        strings.Add("2|Brogzog 5000 rift|1.5E+104|0|3");
        strings.Add("2|Amoeba Planet rift|4E+104|0|3");
        strings.Add("3|Swoosh|9E+104|0|3");
        strings.Add("3|Swish|6E+105|0|3");
        strings.Add("3|Butterscotch bonanza special|1.5E+106|0|3");
        strings.Add("4|Swimsuit models|6E+106|0|2");
        strings.Add("4|Baby alligator pets|1.85E+107|0|3");
        strings.Add("4|Disco lights|5E+107|0|3");
        strings.Add("-1|Intergalactic celebrity|6E+107|0|3");
        strings.Add("5|Manta ray diner|7.5E+107|0|2");
        strings.Add("5|Baby goat pets|5E+108|0|3");
        strings.Add("5|Hand made pillows|4.5E+109|0|3");
        strings.Add("6|Mariana Trench Expedition|1.25E+110|0|3");
        strings.Add("6|Origami decorations|3E+110|0|3");
        strings.Add("6|Mermaid chef|9E+110|0|3");
        strings.Add("-1|Artificial Islands|1E+111|0|3");
        strings.Add("8|Asgard technology|5E+111|0|2");
        strings.Add("8|Spaghetti monster chef|7E+112|0|3");
        strings.Add("8|Flamboyant Decorations|2.5E+113|0|3");
        strings.Add("9|Unlimited range|1.5E+115|0|9");
        strings.Add("9|Kinetic alleviation|7.5E+115|0|3");
        strings.Add("9|Pizza-portation|4E+116|0|3");

        strings.Add("-1|Transport Magnate|4.5E+116|0|3");
        strings.Add("0|Flower pots|5E+116|0|3");
        strings.Add("0|Sattelite television|7.5E+116|0|3");
        strings.Add("0|Kangaroo drivers|1E+117|0|3");
        strings.Add("1|Invisibility field|2E+117|0|3");
        strings.Add("1|Opera singers|2E+118|0|3");
        strings.Add("1|Fat cat|1.5E+119|0|3");
        strings.Add("-1|Galactic entrepreneur|3.5E+119|0|5");
        strings.Add("-1|Universe entrepreneur|5E+119|0|3");
        strings.Add("1|Walking on sunshine|7E+119|0|3");
        strings.Add("2|Stand-up-comedian|9.5E+119|0|3");
        strings.Add("3|Delorean Cars|4E+120|0|3");
        strings.Add("4|Chocolate chip cookies jar|9E+120|0|3");
        strings.Add("5|Snakes!|2.4E+121|0|3");
        strings.Add("6|Typhoon systems|1.11E+122|0|3");
        strings.Add("8|Space Alpaca|2.22E+122|0|3");
        strings.Add("9|Enhanced buffer|4.44E+122|0|3");

        strings.Add("0|Complementary corn flakes|5.55E+122|0|3");
        strings.Add("-1|Number of the beast|6.66E+122|0|6.66");
        strings.Add("-1|The illuminati|1E+123|0|3");
        strings.Add("1|Automated cars|3E+123|0|3");
        strings.Add("2|I can't believe it's not butter sandwiches|6E+123|0|3");
        strings.Add("3|Tram Force One|1.2E+124|0|3");
        strings.Add("4|Skittles dispensers|2.4E+124|0|3");
        strings.Add("5|Tactical plating|4.8E+124|0|3");
        strings.Add("6|Nuclear reactor|9.6E+124|0|3");
        strings.Add("8|Phasers|1.92E+125|0|3");
        strings.Add("9|The Doctor will see you now|7.68E+125|0|3");

        strings.Add("0|Gluten free rides|1E+126|0|3");
        strings.Add("-1|Moneygeddon|1E+127|0|5");
        strings.Add("2|Bus scarves|2E+129|0|3");
        strings.Add("3|Journey to the center of the earth|1.3E+130|0|3");
        strings.Add("8|Multiverse translocator|2.9E+130|0|3");
        strings.Add("0|Platinum Roads|7.1E+130|0|3");
        strings.Add("6|Captain Ahab's Diary|1.77E+131|0|3");
        strings.Add("1|Complementary apple pens|2.5E+131|0|3");
        strings.Add("7|Galactic colonies|3.1E+131|0|3");
        strings.Add("4|Unlimited availability|5.55E+131|0|3");
        strings.Add("5|Fighter pilots|7.36E+131|0|3");
        strings.Add("-1|Tax free since 93|9E+131|0|2");
        strings.Add("1|Complementary pineapple pens|5E+132|0|2");
        strings.Add("2|Tri-leveled busses|9.5E+133|0|2");
        strings.Add("3|Magnetic resonance|2.13E+134|0|2");
        strings.Add("4|Nightclub addon|4E+134|0|2");
        strings.Add("5|Gluten free water|9.85E+134|0|2");
        strings.Add("6|Kraken Defense System|8E+135|0|2");
        strings.Add("8|Space pirates protection|2.9E+136|0|2");
        strings.Add("9|Teleporter regulations|5E+137|0|2");

        strings.Add("0|Rickshaw regulations|9E+137|0|2");
        strings.Add("-1|Fidget spinner investor|5E+138|0|3");
        strings.Add("1|Taxi regulations|1.36E+140|0|3");
        strings.Add("2|Bus regulations|7E+140|0|3");
        strings.Add("-1|InSane management|3E+141|0|3");
        strings.Add("4|Limo regulations|2.1E+142|0|3");
        strings.Add("5|Plane regulations|5.5E+142|0|3");
        strings.Add("6|Submarine regulations|1.11E+143|0|3");
        strings.Add("7|Space shuttle regulations|2.23E+143|0|3");
        strings.Add("0|Pristine Rickshaws I|7.99E+143|0|3");
        strings.Add("-1|Black Market|2E+144|0|3");
        strings.Add("1|Pristine Taxis I|3E+144|0|3");
        strings.Add("2|Pristine Busses I|6E+144|0|3");
        strings.Add("3|Pristine Trams I|9E+144|0|3");
        strings.Add("4|Pristine Limos I|2.1E+145|0|3");
        strings.Add("5|Pristine Planes I|4.4E+145|0|3");
        strings.Add("6|Pristine Submarines I|8.9E+145|0|3");
        strings.Add("8|Pristine Space Shuttles I|1.29E+146|0|3");
        strings.Add("9|Time machine extravaganza|2.1E+146|0|3");

        strings.Add("0|Pristine Rickshaws II|3E+146|0|3");
        strings.Add("-1|Turtle souffle magnifique|4.5E+146|0|2.71828");
        strings.Add("5|Pristine Planes II|5E+147|0|5");
        strings.Add("1|Pristine Taxis II|3E+148|0|5");
        strings.Add("2|Pristine Busses II|1.8E+149|0|5");
        strings.Add("3|Pristine Trams II|5E+150|0|5");
        strings.Add("8|Pristine Teleporters II|2E+151|0|5");
        strings.Add("4|Pristine Limos II|8E+151|0|5");
        strings.Add("0|Pristine Rickshaws III|2.4E+152|0|5");
        strings.Add("6|Pristine Submarines II|7.2E+152|0|5");
        strings.Add("8|Pristine Space Shuttles II|2.1E+154|0|5");
        strings.Add("-1|Operator, may I help you?|5E+155|0|9.11");
        strings.Add("5|Pristine Planes III|7.77E+155|0|2");
        strings.Add("1|Pristine Taxis III|8.88E+155|0|2");
        strings.Add("2|Pristine Busses III|9.99E+155|0|2");
        strings.Add("3|Pristine Trams III|4E+156|0|2");
        strings.Add("9|Pristine Teleporters III|8E+156|0|2");

        strings.Add("4|Pristine Limos III|1.6E+157|0|2");
        strings.Add("0|Pristine Rickshaws Iv|3.2E+157|0|2");
        strings.Add("6|Pristine Submarines III|6.4E+157|0|2");
        strings.Add("8|Pristine Space Shuttles III|1.28E+158|0|2");
        strings.Add("-1|Profiteering|5.14E+158|0|2");
        strings.Add("5|Pristine Planes IV|1E+159|0|3");
        strings.Add("1|Pristine Taxis IV|1E+160|0|3");
        strings.Add("2|Pristine Busses IV|2.5E+160|0|3");
        strings.Add("3|Pristine Trams IV|7.5E+160|0|3");
        strings.Add("9|Pristine Teleporters IV|1E+161|0|3");

        strings.Add("4|Pristine Limos IV|1.5E+161|0|3");
        strings.Add("0|Pristine Rickshaws V|2E+161|0|3");
        strings.Add("6|Pristine Submarines IV|3E+161|0|3");
        strings.Add("8|Pristine Space Shuttles IV|4E+161|0|3");
        strings.Add("-1|Gatsby|9E+161|0|5");
        strings.Add("7|Pristine Space Shuttles V|1E+162|0|24");
        strings.Add("-1|The Philosopher's Stone|2.5E+164|0|2");
        strings.Add("1|Supreme Interdimensional Taxis|5E+164|0|22");
        strings.Add("-1|Interdimensional Taxation Policies|7.5E+164|0|2");
        strings.Add("2|Pristine Busses V|1E+165|0|20");
        strings.Add("-1|Divine Investors|2.5E+167|0|2");
        strings.Add("-1|Antibacterial Force Fields|7.5E+167|0|2");
        strings.Add("5|Pristine Planes V|1E+168|0|16");
        strings.Add("-1|Divine Investors II|2.5E+170|0|2");
        strings.Add("6|Pristine Submarines V|5E+170|0|14");
        strings.Add("-1|Liquified Assets|7.5E+170|0|2");
        strings.Add("9|Pristine teleporters V|1E+171|0|12");

        strings.Add("-1|Incredible profiteering|2.5E+173|0|2");
        strings.Add("0|Pristine Rickshaws VI|5E+173|0|10");
        strings.Add("-1|Angellic Alliance|7.5E+173|0|12");
        strings.Add("3|Pristine Trams V|1E+174|0|8");
        strings.Add("-1|Fiat Multipla Shares|2.5E+176|0|2");
        strings.Add("4|Pristine Limos V|5E+176|0|4");
        strings.Add("-1|Elite Intergalactic Society|5E+198|0|5");
        strings.Add("-1|High Society|2.7E+193|0|3");
        strings.Add("-1|Bussiness succession|2E+198|0|5");
        strings.Add("0|Pristine Rickshaws VII|1E+201|0|3");
        strings.Add("1|Pristine Taxis V|1.4E+202|0|3");
        strings.Add("2|Pristine Busses V|9.6E+202|0|3");
        strings.Add("3|Pristine Trams VI|1.98E+203|0|3");
        strings.Add("4|Pristine Limos VI|3.22E+203|0|3");
        strings.Add("5|Pristine Planes VI|6.79E+203|0|3");
        strings.Add("6|Pristine Submarines VI|8.88E+203|0|3");
        strings.Add("5|Pristine Planes VIII|2E+207|0|3");
        strings.Add("6|Pristine Submarines VII|9E+207|0|3");
        strings.Add("8|Pristine Space Shuttles VII|4.5E+208|0|3");
        strings.Add("9|Pristine Teleporters VII|3.28E+209|0|3");
        
        strings.Add("0|Pristine Rickshaws IX|1E+214|0|11");
        strings.Add("1|Pristine Taxis VII|1E+214|0|11");
        strings.Add("2|Pristine Busses VII|1E+214|0|11");
        strings.Add("3|Pristine Trams VIII|1E+214|0|11");
        strings.Add("4|Pristine Limos VIII|1E+214|0|11");
        strings.Add("5|Pristine Planes IX|1E+214|0|11");
        strings.Add("6|Pristine Submarines VIII|1E+214|0|11");
        strings.Add("8|Pristine Space Shuttles VIII|1E+214|0|11");
        strings.Add("9|Pristine Teleporters VIII|1E+214|0|11");
        
        strings.Add("0|Pristine Rickshaws X|1.5E+215|0|3");
        strings.Add("1|Pristine Taxis VIII|1.66E+215|0|3");
        strings.Add("2|Pristine Busses VIII|1.93E+215|0|3");
        strings.Add("3|Pristine Trams IX|4.1E+215|0|3");
        strings.Add("4|Pristine Limos  IX|6.78E+215|0|3");
        strings.Add("5|Pristine Planes X|9E+215|0|3");
        strings.Add("6|Pristine Submarines IX|1.2E+217|0|3");
        strings.Add("9|Pristine Teleporters IX|3.21E+218|0|3");
        
        strings.Add("-1|Surf'n'turf|5.55E+218|0|5");
        strings.Add("1|Pristine Taxis IX|8E+218|0|3");
        strings.Add("2|Pristine Busses IX|8E+218|0|3");
        strings.Add("3|Pristine Trams X|9E+218|0|3");
        strings.Add("4|Pristine Limos  X|3E+219|0|3");
        strings.Add("5|Maverick|4E+219|0|3");
        strings.Add("6|Pristine Submarines X|5E+219|0|3");
        strings.Add("8|Pristine Space Shuttles X|6E+219|0|3");
        strings.Add("9|Pristine Teleporters X|4.21E+221|0|3");
        
        strings.Add("0|Gluten superhero driver|6E+221|0|3");
        strings.Add("1|Pristine Taxis X|7.89E+221|0|3");
        strings.Add("2|Pristine Busses X|8.45E+221|0|3");
        strings.Add("3|Vomit Free Rides|2E+222|0|3");
        strings.Add("4|Omega Protocol|5E+222|0|3");
        strings.Add("5|Open Window Policy|1.4E+223|0|3");
        strings.Add("6|Wooden Legs|5.4E+223|0|3");
        strings.Add("8|Gluten dampeners|1.08E+224|0|3");
        strings.Add("9|Boneduck Carboncatch|4.68E+224|0|3");
        
        strings.Add("0|Unslippery Slippers|1E+228|0|7");
        strings.Add("1|Fake Taxis|1E+228|0|7");
        strings.Add("2|Sentry Tower Upgrade|1E+228|0|7");
        strings.Add("3|Wormhole Generator|1E+228|0|7");
        strings.Add("4|Mayan Drivers|1E+228|0|7");
        strings.Add("5|Pre-fired-up engines|1E+228|0|7");
        strings.Add("6|Hook Hands|1E+228|0|7");
        strings.Add("8|High density plating|1E+228|0|7");
        strings.Add("9|Strange Doctor|1E+228|0|7");
        
        strings.Add("0|AIR JORDANS 10000|3E+231|0|3");
        strings.Add("1|Gluten free air freshners|8E+231|0|3");
        strings.Add("2|Anti-rust coating|6.9E+232|0|3");
        strings.Add("3|Zorlakk Repellent|1.88E+233|0|3");
        strings.Add("4|Aztec Drivers|2.39E+233|0|3");
        strings.Add("5|Ultra Jets 2000|4.11E+233|0|3");
        strings.Add("6|Torpeedos|7E+233|0|3");
        strings.Add("8|Coup-Resistant Coating|9.12E+233|0|3");
        strings.Add("9|Sepia Filters|2.4E+235|0|3");
        
        strings.Add("0|Supersoldier Rickshaw Pullers|6.3E+235|0|3");
        strings.Add("1|VR Experience|1.99E+236|0|3");
        strings.Add("2|Pro-rust coating|3.98E+236|0|3");
        strings.Add("3|I like big Trams and I cannot lie|5.66E+236|0|3");
        strings.Add("4|Tyrannosaurus Rex Drivers|7E+236|0|3");
        strings.Add("5|Decorated Pilots|8E+236|0|3");
        strings.Add("6|Fishsticks|9E+236|0|3");
        strings.Add("8|Milky Way Avant-Garde|1.2E+238|0|3");
        strings.Add("9|Portal Guns|5E+238|0|3");
        
        strings.Add("0|Chuck Norris Training|1E+240|0|2");
        strings.Add("1|Blood payments|5E+240|0|2");
        strings.Add("2|Ninja Security|9E+240|0|2");
        strings.Add("3|Subway Sandwiches in the Subway|2.1E+241|0|2");
        strings.Add("4|Transformer Limousines|4.5E+241|0|2");
        strings.Add("5|Liquid metal casing|8.9E+241|0|2");
        strings.Add("6|Nuclear Capabilities|1.53E+242|0|2");
        strings.Add("8|Lint speed|2.99E+242|0|2");
        strings.Add("9|AAA Services|8.13E+242|0|2");
        
        strings.Add("0|Latino Music Boombox|2E+243|0|2");
        strings.Add("1|Hollywood actors as drivers|2.2E+244|0|2");
        strings.Add("2|Baby goat driver|4.4E+244|0|2");
        strings.Add("3|Screen cleaning gel|6.6E+244|0|2");
        strings.Add("4|Infinity Engines|8.8E+244|0|2");
        strings.Add("5|T-1000|1.11E+245|0|2");
        strings.Add("6|Sonar Exploration|2.22E+245|0|2");
        strings.Add("8|E = mc2|3.33E+245|0|2.99792458");
        strings.Add("9|Complementary potato skins|5.55E+245|0|2");
        
        strings.Add("0|Recycled paper chairs|1E+253|0|3");
        strings.Add("1|Elite Drifting|1E+253|0|3");
        strings.Add("2|Orbital drops|1E+253|0|3");
        strings.Add("3|Bug Free Zone|1E+253|0|3");
        strings.Add("4|Blazing Speed|1E+253|0|3");
        strings.Add("5|Skynet Inc.|1E+253|0|3");
        strings.Add("6|Pancake Creation Facility|1E+253|0|3");
        strings.Add("8|Space shufflin|1E+253|0|3");
        strings.Add("9|Portable teleporter pads|1E+253|0|3");
        
        strings.Add("0|Drive-yourself rickshaw|5E+253|0|9");
        strings.Add("1|Complementary Katanas|7.5E+253|0|9");
        strings.Add("2|Sloppy Joe advertising|1.25E+254|0|9");
        strings.Add("3|Tanning Beds|6.25E+254|0|9");
        strings.Add("4|Blazing-er Speed|3E+255|0|9");
        strings.Add("5|Backwards Flying Technology|1.5E+256|0|9");
        strings.Add("6|Cheesecake Research Facility|7.5E+256|0|9");
        strings.Add("8|Captain Jameson T. Cork|3.75E+257|0|9");
        strings.Add("9|Slurpees|8E+257|0|9");
        
        strings.Add("0|Argan Oil Vaporisator|6.4E+258|0|3");
        strings.Add("1|Fish & Chips dispenser|1.22E+259|0|3");
        strings.Add("2|No-nonsense policy|2.33E+260|0|3");
        strings.Add("3|Crustless Sandwiches|3.99E+260|0|3");
        strings.Add("4|420 Blaze it|7.66E+260|0|3");
        strings.Add("5|Malaysian Technology|1E+261|0|3");
        strings.Add("6|Whale Disguise|1.9E+262|0|3");
        strings.Add("8|The Wrath of Kohn|9.8E+262|0|3");
        strings.Add("9|Teleport is Life|5.44E+263|0|3");
        
        strings.Add("0|Puller Cybernetic Augmentations|7E+263|0|3");
        strings.Add("1|Fluffy Slippers|1E+264|0|3");
        strings.Add("2|Everything BUS|4.5E+265|0|3");
        strings.Add("3|Extra crust sandwiches|6.9E+265|0|3");
        strings.Add("4|Protozoan Augmentations|8.9E+265|0|3");
        strings.Add("5|Invisible Planes|1.89E+266|0|3");
        strings.Add("6|Polo Hats|2.89E+266|0|3");
        strings.Add("8|Solar Sails|4.48E+266|0|3");
        strings.Add("9|Turtle shell coating|5E+267|0|3");
        
        strings.Add("0|Pneumatic Wheels|1E+273|0|7");
        strings.Add("1|Technological Marvels|2E+273|0|7");
        strings.Add("2|Self-washing busses|3E+273|0|7");
        strings.Add("3|All crust sandwiches|6E+273|0|7");
        strings.Add("4|Jellybean Infinitum|2.5E+274|0|7");
        strings.Add("5|Even more invisible planes|2E+275|0|7");
        strings.Add("6|Sea Horse Exploatation|6E+275|0|7");
        strings.Add("8|Turbulent Juices|9.99E+275|0|7");
        strings.Add("9|De-materialization encapsulation|3E+277|0|7");
        
        strings.Add("5|I don't think we understand invisibility|1E+285|0|13");
        strings.Add("6|Jalapeno poppers|1E+285|0|13");
        strings.Add("8|Supercharged Engines|1E+285|0|13");
        strings.Add("9|Teleprompter in the teleporter|1E+285|0|13");

        strings.Add("7|Rigid plating|250000000000|0|3");
        strings.Add("7|Shiny gondola plating|2E+16|0|3");
        strings.Add("7|Stabilizer fins|2E+20|0|3");
        strings.Add("7|Ballonet air valves|8E+23|0|3");
        strings.Add("7|High quality aerostat|5E+31|0|7");
        strings.Add("7|Helium Compounds|1E+46|0|3");
        strings.Add("7|Enhanced catenary curtains|2.5E+50|0|3");
        strings.Add("7|Sturdy nose cone battens|1E+79|0|3");
        strings.Add("7|Semi-ridig framing|5E+93|0|3");
        strings.Add("7|Gondola Premium Cussions|1.45E+101|0|2");
        strings.Add("7|Vip lounge|1.5E+115|0|3");
        strings.Add("7|Gondola sauna|7.5E+115|0|3");
        strings.Add("7|Seat Warmers|4E+116|0|3");
        strings.Add("7|Personal Soundtrack|4.44E+122|0|3");
        strings.Add("7|Searchlights|7.68E+125|0|3");
        strings.Add("7|Blimp Inc.|2.9E+130|0|3");
        strings.Add("7|Blimp fleet|5E+137|0|2");
        strings.Add("7|Kirovk reporting techniques|2.1E+146|0|3");
        strings.Add("7|Special gas composition|2E+151|0|5");
        strings.Add("7|Seagull Repellant|8E+156|0|2");
        strings.Add("7|Steam boosters|1E+161|0|3");
        strings.Add("7|Alpha and Omega|1E+171|0|12");
        strings.Add("7|Dark Matter fuel system|3.28E+209|0|3");
        strings.Add("7|Plasmid manipulation|1E+214|0|11");
        strings.Add("7|Outworld Tours|3.21E+218|0|3");
        strings.Add("7|Temporal rift machine|4.21E+221|0|3");
        strings.Add("7|Gondola Jazz Singers|4.68E+224|0|3");
        strings.Add("7|Dirigible Reality Show|1E+228|0|7");
        strings.Add("7|Bond Villain Investor|2.4E+235|0|3");
        strings.Add("7|Extra-terestrial decor|5E+238|0|3");
        strings.Add("7|Executive Lounge|8.13E+242|0|2");
        strings.Add("7|Steampunk decor|5.55E+245|0|2");
        strings.Add("7|Staff Power Armor|1E+253|0|3");
        strings.Add("7|Gattling lasers|8E+257|0|9");
        strings.Add("7|Even more lasers|5.44E+263|0|3");
        strings.Add("7|Laser removal policy|5E+267|0|3");
        strings.Add("7|Trademark Oil|3E+277|0|7");
        strings.Add("7|Led's Zeppelin|1E+285|0|13");

        //LoadInternal(strings);
    }
    internal static void LoadInvestorStrings()
    {
        List<string> strings = new List<string>();
        strings.Add("-1 | Bag of plenty | 10000 | 0 | 3");
        strings.Add("-2|Business manuals|100000|2|2");
        strings.Add("-2|Talented entrepreneurs|100000000|2|2");
        strings.Add("-1|The business emancipation association|1000000000|0|5");
        strings.Add("1|Taxi Influx|25000000|3|10");
        strings.Add("2|Bus Influx|25000000|3|10");
        strings.Add("3|Tram Influx|25000000|3|10");
        strings.Add("4|Limo Influx|25000000|3|10");
        strings.Add("-1|Case of plenty|100000000000|0|5");
        
        strings.Add("1|Taxi Overflow|250000000|3|50");
        strings.Add("2|Bus Overflow|250000000|3|50");
        strings.Add("3|Tram Overflow|250000000|3|50");
        strings.Add("4|Limo Overflow|250000000|3|50");
        strings.Add("1|Taxi Cascade|25000000000|3|100");
        strings.Add("2|Bus Cascade|25000000000|3|100");
        strings.Add("3|Tram Cascade|25000000000|3|100");
        strings.Add("4|Limo Cascade|25000000000|3|100");
        strings.Add("-1|Welcome to Rapture|1000000000000|0|11");
        
        strings.Add("1|Age of the Taxi|250000000000000|0|3");
        strings.Add("2|Age of the Bus|750000000000000|0|3");
        strings.Add("3|Age of the Tram|2E+15|0|3");
        strings.Add("4|Age of the Limo|5E+15|0|3");
        strings.Add("5|Age of the Plane|1E+16|0|3");
        strings.Add("6|Age of the Submarine|2.5E+16|0|3");
        strings.Add("8|Age of the Space Shuttle|7.5E+16|0|3");
        strings.Add("9|Age of the teleporter|2E+17|0|3");
        strings.Add("7|Age of the Zeppelin|4E+17|0|3");
        
        strings.Add("0|Age of the Rickshaw|1E+18|0|3");
        strings.Add("-1|The galactic investment conundrum|1E+21|0|15");
        strings.Add("1|Taxicus Superbus|1E+22|3|75");
        strings.Add("2|Bussicus Supremus|1E+22|3|75");
        strings.Add("3|Tramus Bonus|1E+22|3|75");
        strings.Add("4|Limos Laetus|1E+22|3|75");
        strings.Add("5|Planus Multus|1E+22|3|75");
        strings.Add("1|Swift Taxi Delivery|1E+23|3|75");
        strings.Add("2|Swift Bus Delivery|1E+23|3|75");
        strings.Add("3|Swift Tram Delivery|1E+23|3|75");
        strings.Add("4|Swift Limo Delivery|1E+23|3|75");
        strings.Add("5|Swift Plane Delivery|1E+23|3|75");
        strings.Add("1|First Amen-dment|1E+31|3|100");
        strings.Add("2|Unrefusable Offer|1E+32|3|100");
        strings.Add("-2|20.000 leagues above the sea|1E+33|2|2");
        strings.Add("-1|Acer obsidio|1E+34|0|3");
        strings.Add("-1|2+2 is 4 -1 that's 3|1E+36|0|3");
        strings.Add("-1|Businesses theme song|1E+40|0|5");
        strings.Add("-1|Manbearpig investors|1E+42|0|5");
        strings.Add("1|Technological taxi advancement|2E+42|3|50");
        strings.Add("2|Keanu's bus|1E+47|0|4");
        strings.Add("3|Tramwma|2E+47|0|6");
        strings.Add("4|Stretch|7E+47|0|3");
        strings.Add("5|Snakeless planes|2E+48|0|3");
        strings.Add("6|Replicator-proof submarines|2.5E+49|0|3");
        strings.Add("8|Replicator-proof spaceships|5E+50|0|3");
        strings.Add("9|What is a teleporter anyway?|2E+52|0|3");
        strings.Add("7|Zeppelinus Magnus|8E+52|0|3");
        
        strings.Add("0|Hiroshi's Rickshaw|1.5E+53|0|3");
        strings.Add("1|Mobile app orders|3E+53|0|3");
        strings.Add("-2|Intelligent investor investments|5E+53|2|2");
        strings.Add("1|Clever taxi|1E+54|0|3");
        strings.Add("2|Pistachio colored busses|4E+54|0|3");
        strings.Add("3|Black Mesa Transit System|9E+54|0|3");
        strings.Add("4|Ranjit's Limo|2.5E+55|0|3");
        strings.Add("5|Messerschmitt plating|7.5E+55|0|3");
        strings.Add("6|Ultramarine Blue|1.77E+56|0|3");
        strings.Add("8|Deflector shields|3E+56|0|3");
        strings.Add("9|Anti-liquefiation measurements|5E+56|0|3");
        strings.Add("7|Blue bombs|8E+56|0|3");
        
        strings.Add("0|Foldable Rickshaws|1E+57|0|3");
        strings.Add("1|Blazing Taxis|3E+61|3|30");
        strings.Add("2|Blazing Busses|3E+61|3|30");
        strings.Add("3|Blazing Trams|3E+61|3|30");
        strings.Add("4|Blazing Limos|3E+61|3|30");
        strings.Add("6|Blazing Submarines|3E+61|3|30");
        strings.Add("-1|The war on coal|1E+62|0|5");
        strings.Add("1|Tesla Taxi|2E+63|0|3");
        strings.Add("2|Tesla Bus|2E+63|0|3");
        strings.Add("3|Tesla Tram|2E+63|0|3");
        strings.Add("4|Tesla Limo|2E+63|0|3");
        strings.Add("5|Tesla Plane|2E+63|0|3");
        strings.Add("6|Tesla Submarine|2E+63|0|3");
        strings.Add("8|Tesla Space Shuttle|2E+63|0|3");
        strings.Add("9|Can a teleporter be Tesla?|2E+63|0|3");
        strings.Add("7|Tesla Zeppelin|2E+63|0|3");
        
        strings.Add("0|Timmy's Handy Rickshaw|2E+63|0|3");
        strings.Add("-1|Tenorman's Chilly Recipe|1E+65|0|7");
        strings.Add("1|Taxi Extravaganza|1E+66|0|3");
        strings.Add("2|Bus Extravaganza|4E+66|0|3");
        strings.Add("3|Tram Extravaganza|1.3E+67|0|3");
        strings.Add("4|Limo Extravaganza|2E+67|0|3");
        strings.Add("5|Plane Extravaganza|2.9E+67|0|3");
        strings.Add("6|Submarine Extravaganza|3.8E+67|0|3");
        strings.Add("8|Space Shuttle Extravaganza|5.2E+67|0|3");
        strings.Add("9|Teleporter Extravaganza|6.7E+67|0|3");
        strings.Add("7|Zeppelin Extravaganza|7.2E+67|0|3");
        
        strings.Add("0|Unbalanced Rickshaw|9.6E+67|0|3");
        strings.Add("1|Beercules the driver|1.25E+68|3|50");
        strings.Add("-1|Pie in the sky makes me wonder why|7.77E+68|0|3.14");
        strings.Add("1|Taxi Supremacy|5E+69|3|10");
        strings.Add("2|Bus Supremacy|5E+69|3|10");
        strings.Add("3|Tram Supremacy|5E+69|3|10");
        strings.Add("4|Limo Supremacy|5E+69|3|10");
        strings.Add("5|Plane Supremacy|5E+69|3|10");
        strings.Add("6|Submarine Supremacy|5E+69|3|10");
        strings.Add("8|Space Shuttle Supremacy|5E+69|3|10");
        strings.Add("9|Teleporter Supremacy|5E+69|3|10");
        strings.Add("7|Zeppelin Supremacy|5E+69|3|10");
        
        strings.Add("0|Rickshaw Supremacy|5E+69|3|10");
        strings.Add("1|Taxi éclat|1E+72|0|3");
        strings.Add("2|Bus éclat|5E+72|0|3");
        strings.Add("3|Tram éclat|2.2E+73|0|3");
        strings.Add("4|Limo éclat|4.4E+73|0|3");
        strings.Add("5|Plane éclat|1.11E+74|0|3");
        strings.Add("6|Submarine éclat|2.22E+74|0|3");
        strings.Add("8|Space Shuttle éclat|3.33E+74|0|3");
        strings.Add("9|Teleporter éclat|4.44E+74|0|3");
        strings.Add("7|Zeppelin éclat|5.55E+74|0|3");
        
        strings.Add("0|Never|6.66E+74|0|3");
        strings.Add("2|Going|2.5E+76|3|30");
        strings.Add("1|To|2.5E+76|3|30");
        strings.Add("3|Give|2.5E+76|3|30");
        strings.Add("4|You |2.5E+76|3|30");
        strings.Add("5|Up And Coming Capitalists|2.5E+76|3|30");
        strings.Add("6|Never|2.5E+76|3|30");
        strings.Add("8|Going|2.5E+76|3|30");
        strings.Add("9|To|2.5E+76|3|30");
        strings.Add("7|Let|2.5E+76|3|30");
        
        strings.Add("0|You |2.5E+76|3|30");
        strings.Add("1|Down|1.1E+79|0|3");
        strings.Add("2|And|2.7E+79|0|3");
        strings.Add("3|Hurt |4.3E+79|0|3");
        strings.Add("4|You |8.7E+79|0|3");
        strings.Add("5|57 seats Plane|1.9E+80|0|3");
        strings.Add("6|Black Sea dwelver|3.21E+80|0|3");
        strings.Add("8|Armageddon|4.95E+80|0|3");
        strings.Add("9|Fly experiments|6E+80|0|3");
        strings.Add("7|Ol' Bomby|7.25E+80|0|3");
        
        strings.Add("0|Green energy Rickshaw|8.98E+80|0|3");
        strings.Add("-1|Ecclesiastes|3E+84|0|5");
        strings.Add("-1|The Night's dichotomy|1.3E+88|0|5");
        strings.Add("-1|Tree of Knowledge|3E+90|0|3");
        strings.Add("-1|The Source|1.3E+94|0|4");
        strings.Add("-1|Light blue magical liquid|2.4E+97|0|5");
        strings.Add("1|D.C. Cabs|1E+102|3|30");
        strings.Add("2|Zombie proof school bus|1E+102|3|30");
        strings.Add("3|In Tram pizza shop|1E+102|3|30");
        strings.Add("4|The transporter|1E+102|3|30");
        strings.Add("5|Malaysian tracking device|1E+102|3|30");
        strings.Add("6|Acme Torpedoes|1E+102|3|30");
        strings.Add("8|Phasers|1E+102|3|30");
        strings.Add("9|Inexpensive materials|1E+102|3|30");
        strings.Add("7|Loudspeakers|1E+102|3|30");
        
        strings.Add("0|Wall escalating capabilities|1E+102|3|30");
        strings.Add("-1|Ice queen|3.33E+110|0|3");
        strings.Add("1|T-1000 driver|1E+114|0|3");
        strings.Add("2|Fitness coach driver|2E+115|0|3");
        strings.Add("3|Tranvía especial|5E+115|0|3");
        strings.Add("4|Lim-bo|1E+116|0|3");
        strings.Add("5|Long range missiles|2E+116|0|3");
        strings.Add("6|Water recycler|3E+116|0|3");
        strings.Add("8|Free toast|4E+116|0|3");
        strings.Add("9|Remote teleportation|5E+116|0|3");
        strings.Add("7|Lead Zeppelin|7.5E+116|0|3");
        
        strings.Add("0|Iconic Rickshaw|2E+117|0|3");
        strings.Add("1|Iconic Taxi|1E+129|3|35");
        strings.Add("2|Iconic Bus|1E+129|3|35");
        strings.Add("3|Iconic Tram|1E+129|3|35");
        strings.Add("4|Iconic Limo|1E+129|3|35");
        strings.Add("5|Iconic Plane|1E+129|3|35");
        strings.Add("6|Iconic Submarine|1E+129|3|35");
        strings.Add("8|Iconic Space Shuttle|1E+129|3|35");
        strings.Add("9|Iconic Teleporter|1E+129|3|35");
        strings.Add("7|Iconic Zeppelin|1E+129|3|35");
        
        strings.Add("0|Revered Rickshaw|1E+129|3|35");
        strings.Add("1|Revered Taxi|1E+138|0|3");
        strings.Add("2|Revered Bus|1E+138|0|3");
        strings.Add("3|Revered Tram|1E+138|0|3");
        strings.Add("4|Revered Limo|1E+138|0|3");
        strings.Add("5|Revered Plane|1E+138|0|3");
        strings.Add("6|Revered Submarine|1E+138|0|3");
        strings.Add("8|Revered Space Shuttle|1E+138|0|3");
        strings.Add("9|Revered teleporter|1E+138|0|3");
        
        strings.Add("0|Ultimate Rickshaw|1E+138|0|3");
        strings.Add("7|Ultimate Zeppelin|1E+138|0|3");
        strings.Add("-1|Thunderstruck|2E+138|0|20");
        strings.Add("2|Busicus Maximus|2E+138|3|75");
        strings.Add("5|Planus Maximus|2E+138|3|75");
        strings.Add("6|Submarinicus Maximus|2E+138|3|75");
        strings.Add("9|Teleporterus Maximus|2E+138|3|75");
        strings.Add("3|Tramus Maximus|2E+138|3|75");
        strings.Add("0|Rickshawicus Maximus|2E+138|3|75");
        strings.Add("1|Taxicus Maximus|3E+138|3|75");
        strings.Add("4|Limos Maximus|3E+138|3|75");
        strings.Add("8|Space-us Shuttelitus Maximus|3E+138|3|75");
        strings.Add("7|Zepplenius Maximus|4E+138|3|75");

        //LoadInternalInvestors(strings);

    }*/
    
}

/* 
[Serializable]
public class SerializedUpgrade : IComparable<SerializedUpgrade>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Cost { get; set; }
    public UpgradeType Type { get; set; }
    public double Ammount { get; set; }

    public int CompareTo(SerializedUpgrade other)
    {
        return Cost.CompareTo(other.Cost);
    }

    public void TransformToNewFormat(int id, out UpgradeJsonData newUpgradeData, out LocalizationStringData newStringData ) {
        string stringId = $"upgrade_desc_{id}";
        newStringData = new LocalizationStringData {
            id = stringId,
            en = Name,
            ru = string.Empty
        };

        newUpgradeData = new UpgradeJsonData {
            id = id,
            generatorId = Id,
            name = stringId,
            price = Cost,
            upgradeType = Type,
            value = Ammount
        };
    }
}*/
