using System;
using System.Collections.Generic;

namespace AirlineFlightManagement.Services.Helpers
{
    public static class LocationMapper
    {
        //dictionary for common city and country names to their corresponding IATA airport codes, with case-insensitive keys
        public static readonly Dictionary<string, string> NameToCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // ================================================================
            //  ROMÂNIA
            // ================================================================
            { "Bucuresti",                  "OTP" },
            { "București",                  "OTP" },
            { "Bucharest",                  "OTP" },
            { "Romania",                    "OTP" },
            { "România",                    "OTP" },
            { "Cluj",                       "CLJ" },
            { "Cluj-Napoca",                "CLJ" },
            { "Timisoara",                  "TSR" },
            { "Timișoara",                  "TSR" },
            { "Iasi",                       "IAS" },
            { "Iași",                       "IAS" },
            { "Sibiu",                      "SBZ" },
            { "Targu Mures",                "TGM" },
            { "Târgu Mureș",                "TGM" },
            { "Craiova",                    "CRA" },
            { "Bacau",                      "BCM" },
            { "Bacău",                      "BCM" },
            { "Oradea",                     "OMR" },
            { "Arad",                       "ARW" },
            { "Suceava",                    "SCV" },
            { "Constanta",                  "CND" },
            { "Constanța",                  "CND" },

            // ================================================================
            //  EUROPA DE VEST
            // ================================================================

            // Marea Britanie
            { "London",                     "LHR" },
            { "Londra",                     "LHR" },
            { "Marea Britanie",             "LHR" },
            { "United Kingdom",             "LHR" },
            { "UK",                         "LHR" },
            { "Anglia",                     "LHR" },
            { "England",                    "LHR" },
            { "Manchester",                 "MAN" },
            { "Birmingham",                 "BHX" },
            { "Edinburgh",                  "EDI" },
            { "Edinburgh",                  "EDI" },
            { "Glasgow",                    "GLA" },
            { "Bristol",                    "BRS" },
            { "Scotland",                   "EDI" },
            { "Scotlanda",                  "EDI" },

            // Franta
            { "Paris",                      "CDG" },
            { "Franta",                     "CDG" },
            { "Franța",                     "CDG" },
            { "France",                     "CDG" },
            { "Nice",                       "NCE" },
            { "Lyon",                       "LYS" },
            { "Marseille",                  "MRS" },
            { "Marsilia",                   "MRS" },
            { "Bordeaux",                   "BOD" },
            { "Toulouse",                   "TLS" },
            { "Strasbourg",                 "SXB" },
            { "Nantes",                     "NTE" },

            // Germania
            { "Germany",                    "FRA" },
            { "Germania",                   "FRA" },
            { "Frankfurt",                  "FRA" },
            { "Berlin",                     "BER" },
            { "Munich",                     "MUC" },
            { "Munchen",                    "MUC" },
            { "München",                    "MUC" },
            { "Hamburg",                    "HAM" },
            { "Dusseldorf",                 "DUS" },
            { "Düsseldorf",                 "DUS" },
            { "Cologne",                    "CGN" },
            { "Koln",                       "CGN" },
            { "Köln",                       "CGN" },
            { "Stuttgart",                  "STR" },
            { "Nuremberg",                  "NUE" },
            { "Nurnberg",                   "NUE" },
            { "Nürnberg",                   "NUE" },
            { "Leipzig",                    "LEJ" },
            { "Bremen",                     "BRE" },
            { "Hanover",                    "HAJ" },
            { "Hannover",                   "HAJ" },

            // Italia
            { "Italy",                      "FCO" },
            { "Italia",                     "FCO" },
            { "Rome",                       "FCO" },
            { "Roma",                       "FCO" },
            { "Milan",                      "MXP" },
            { "Milano",                     "MXP" },
            { "Venice",                     "VCE" },
            { "Venetia",                    "VCE" },
            { "Veneția",                    "VCE" },
            { "Naples",                     "NAP" },
            { "Napoli",                     "NAP" },
            { "Florence",                   "FLR" },
            { "Florenta",                   "FLR" },
            { "Florența",                   "FLR" },
            { "Firenze",                    "FLR" },
            { "Catania",                    "CTA" },
            { "Palermo",                    "PMO" },
            { "Bologna",                    "BLQ" },
            { "Turin",                      "TRN" },
            { "Torino",                     "TRN" },
            { "Bari",                       "BRI" },

            // Spania
            { "Spain",                      "MAD" },
            { "Spania",                     "MAD" },
            { "Madrid",                     "MAD" },
            { "Barcelona",                  "BCN" },
            { "Seville",                    "SVQ" },
            { "Sevilla",                    "SVQ" },
            { "Malaga",                     "AGP" },
            { "Málaga",                     "AGP" },
            { "Valencia",                   "VLC" },
            { "Bilbao",                     "BIO" },
            { "Alicante",                   "ALC" },
            { "Palma",                      "PMI" },
            { "Palma de Mallorca",          "PMI" },
            { "Tenerife",                   "TFN" },
            { "Gran Canaria",               "LPA" },
            { "Ibiza",                      "IBZ" },

            // Portugalia
            { "Portugal",                   "LIS" },
            { "Portugalia",                 "LIS" },
            { "Lisbon",                     "LIS" },
            { "Lisabona",                   "LIS" },
            { "Lisboa",                     "LIS" },
            { "Porto",                      "OPO" },
            { "Faro",                       "FAO" },
            { "Funchal",                    "FNC" },
            { "Madeira",                    "FNC" },
            { "Azores",                     "PDL" },
            { "Azore",                      "PDL" },

            // Olanda
            { "Netherlands",                "AMS" },
            { "Olanda",                     "AMS" },
            { "Holland",                    "AMS" },
            { "Amsterdam",                  "AMS" },
            { "Rotterdam",                  "RTM" },
            { "Eindhoven",                  "EIN" },

            // Belgia
            { "Belgium",                    "BRU" },
            { "Belgia",                     "BRU" },
            { "Brussels",                   "BRU" },
            { "Bruxelles",                  "BRU" },
            { "Bruxel",                     "BRU" },
            { "Antwerp",                    "ANR" },
            { "Anvers",                     "ANR" },
            { "Charleroi",                  "CRL" },

            // Elvetia
            { "Switzerland",                "ZRH" },
            { "Elvetia",                    "ZRH" },
            { "Elveția",                    "ZRH" },
            { "Zurich",                     "ZRH" },
            { "Zürich",                     "ZRH" },
            { "Geneva",                     "GVA" },
            { "Geneva",                     "GVA" },
            { "Genf",                       "GVA" },
            { "Basel",                      "BSL" },
            { "Bern",                       "BRN" },

            // Austria
            { "Austria",                    "VIE" },
            { "Vienna",                     "VIE" },
            { "Viena",                      "VIE" },
            { "Wien",                       "VIE" },
            { "Salzburg",                   "SZG" },
            { "Graz",                       "GRZ" },
            { "Innsbruck",                  "INN" },
            { "Linz",                       "LNZ" },

            // ================================================================
            //  EUROPA CENTRALA SI DE EST
            // ================================================================

            // Ungaria
            { "Hungary",                    "BUD" },
            { "Ungaria",                    "BUD" },
            { "Budapest",                   "BUD" },
            { "Budapesta",                  "BUD" },
            { "Debrecen",                   "DEB" },

            // Polonia
            { "Poland",                     "WAW" },
            { "Polonia",                    "WAW" },
            { "Warsaw",                     "WAW" },
            { "Varsovia",                   "WAW" },
            { "Varșovia",                   "WAW" },
            { "Krakow",                     "KRK" },
            { "Cracovia",                   "KRK" },
            { "Kraków",                     "KRK" },
            { "Gdansk",                     "GDN" },
            { "Gdańsk",                     "GDN" },
            { "Wroclaw",                    "WRO" },
            { "Wrocław",                    "WRO" },
            { "Katowice",                   "KTW" },
            { "Poznan",                     "POZ" },
            { "Poznań",                     "POZ" },
            { "Lodz",                       "LCJ" },
            { "Łódź",                       "LCJ" },

            // Cehia
            { "Czech Republic",             "PRG" },
            { "Czechia",                    "PRG" },
            { "Cehia",                      "PRG" },
            { "Praha",                      "PRG" },
            { "Prague",                     "PRG" },
            { "Praga",                      "PRG" },
            { "Brno",                       "BRQ" },
            { "Ostrava",                    "OSR" },

            // Slovacia
            { "Slovakia",                   "BTS" },
            { "Slovacia",                   "BTS" },
            { "Bratislava",                 "BTS" },
            { "Kosice",                     "KSC" },
            { "Košice",                     "KSC" },

            // Slovenia
            { "Slovenia",                   "LJU" },
            { "Ljubljana",                  "LJU" },

            // Croatia
            { "Croatia",                    "ZAG" },
            { "Croatia",                    "ZAG" },
            { "Croația",                    "ZAG" },
            { "Zagreb",                     "ZAG" },
            { "Split",                      "SPU" },
            { "Dubrovnik",                  "DBV" },
            { "Zadar",                      "ZAD" },
            { "Pula",                       "PUY" },

            // Serbia
            { "Serbia",                     "BEG" },
            { "Belgrade",                   "BEG" },
            { "Belgrad",                    "BEG" },
            { "Beograd",                    "BEG" },

            // Bulgaria
            { "Bulgaria",                   "SOF" },
            { "Sofia",                      "SOF" },
            { "Plovdiv",                    "PDV" },
            { "Varna",                      "VAR" },
            { "Burgas",                     "BOJ" },

            // Moldova
            { "Moldova",                    "KIV" },
            { "Republic of Moldova",        "KIV" },
            { "Chisinau",                   "KIV" },
            { "Chișinău",                   "KIV" },

            // Ucraina
            { "Ukraine",                    "KBP" },
            { "Ucraina",                    "KBP" },
            { "Kyiv",                       "KBP" },
            { "Kiev",                       "KBP" },
            { "Odessa",                     "ODS" },
            { "Lviv",                       "LWO" },
            { "Lvov",                       "LWO" },

            // Rusia
            { "Russia",                     "SVO" },
            { "Rusia",                      "SVO" },
            { "Moscow",                     "SVO" },
            { "Moscova",                    "SVO" },
            { "Moskva",                     "SVO" },
            { "Saint Petersburg",           "LED" },
            { "Sankt Petersburg",           "LED" },
            { "St. Petersburg",             "LED" },
            { "Novosibirsk",                "OVB" },
            { "Ekaterinburg",               "SVX" },
            { "Sochi",                      "AER" },
            { "Kazan",                      "KZN" },

            // ================================================================
            //  EUROPA DE NORD
            // ================================================================

            // Suedia
            { "Sweden",                     "ARN" },
            { "Suedia",                     "ARN" },
            { "Stockholm",                  "ARN" },
            { "Gothenburg",                 "GOT" },
            { "Goteborg",                   "GOT" },
            { "Göteborg",                   "GOT" },
            { "Malmo",                      "MMX" },
            { "Malmö",                      "MMX" },

            // Norvegia
            { "Norway",                     "OSL" },
            { "Norvegia",                   "OSL" },
            { "Oslo",                       "OSL" },
            { "Bergen",                     "BGO" },
            { "Stavanger",                  "SVG" },
            { "Trondheim",                  "TRD" },

            // Danemarca
            { "Denmark",                    "CPH" },
            { "Danemarca",                  "CPH" },
            { "Copenhagen",                 "CPH" },
            { "Copenhaga",                  "CPH" },
            { "København",                  "CPH" },
            { "Billund",                    "BLL" },
            { "Aarhus",                     "AAR" },

            // Finlanda
            { "Finland",                    "HEL" },
            { "Finlanda",                   "HEL" },
            { "Helsinki",                   "HEL" },
            { "Tampere",                    "TMP" },
            { "Turku",                      "TKU" },

            // Islanda
            { "Iceland",                    "KEF" },
            { "Islanda",                    "KEF" },
            { "Reykjavik",                  "KEF" },
            { "Reykjavík",                  "KEF" },

            // Irlanda
            { "Ireland",                    "DUB" },
            { "Irlanda",                    "DUB" },
            { "Dublin",                     "DUB" },
            { "Cork",                       "ORK" },

            // Tarile Baltice
            { "Estonia",                    "TLL" },
            { "Tallinn",                    "TLL" },
            { "Latvia",                     "RIX" },
            { "Letonia",                    "RIX" },
            { "Riga",                       "RIX" },
            { "Lithuania",                  "VNO" },
            { "Lituania",                   "VNO" },
            { "Vilnius",                    "VNO" },
            { "Kaunas",                     "KUN" },

            // ================================================================
            //  EUROPA DE SUD / MEDITERANA
            // ================================================================

            // Grecia
            { "Greece",                     "ATH" },
            { "Grecia",                     "ATH" },
            { "Athens",                     "ATH" },
            { "Atena",                      "ATH" },
            { "Athina",                     "ATH" },
            { "Thessaloniki",               "SKG" },
            { "Salonic",                    "SKG" },
            { "Heraklion",                  "HER" },
            { "Creta",                      "HER" },
            { "Crete",                      "HER" },
            { "Rhodes",                     "RHO" },
            { "Rodos",                      "RHO" },
            { "Corfu",                      "CFU" },
            { "Kos",                        "KGS" },
            { "Mykonos",                    "JMK" },
            { "Santorini",                  "JTR" },
            { "Zakynthos",                  "ZTH" },
            { "Zante",                      "ZTH" },

            // Turcia
            { "Turkey",                     "IST" },
            { "Turcia",                     "IST" },
            { "Istanbul",                   "IST" },
            { "Ankara",                     "ESB" },
            { "Antalya",                    "AYT" },
            { "Izmir",                      "ADB" },
            { "İzmir",                      "ADB" },
            { "Bodrum",                     "BJV" },
            { "Dalaman",                    "DLM" },
            { "Trabzon",                    "TZX" },
            { "Cappadocia",                 "NAV" },
            { "Capadocia",                  "NAV" },

            // Cipru
            { "Cyprus",                     "LCA" },
            { "Cipru",                      "LCA" },
            { "Larnaca",                    "LCA" },
            { "Paphos",                     "PFO" },

            // Malta
            { "Malta",                      "MLA" },
            { "Valletta",                   "MLA" },

            // Albania
            { "Albania",                    "TIA" },
            { "Tirana",                     "TIA" },
            { "Tirane",                     "TIA" },

            // Macedonia de Nord
            { "North Macedonia",            "SKP" },
            { "Macedonia",                  "SKP" },
            { "Macedonia de Nord",          "SKP" },
            { "Skopje",                     "SKP" },

            // Bosnia
            { "Bosnia",                     "SJJ" },
            { "Bosnia and Herzegovina",     "SJJ" },
            { "Sarajevo",                   "SJJ" },

            // Montenegro
            { "Montenegro",                 "TGD" },
            { "Podgorica",                  "TGD" },
            { "Tivat",                      "TIV" },

            // Kosovo
            { "Kosovo",                     "PRN" },
            { "Pristina",                   "PRN" },
            { "Priștina",                   "PRN" },

            // ================================================================
            //  ORIENTUL MIJLOCIU
            // ================================================================

            // Emiratele Arabe Unite
            { "UAE",                        "DXB" },
            { "EAU",                        "DXB" },
            { "Emiratele Arabe Unite",      "DXB" },
            { "United Arab Emirates",       "DXB" },
            { "Dubai",                      "DXB" },
            { "Abu Dhabi",                  "AUH" },
            { "Sharjah",                    "SHJ" },

            // Arabia Saudita
            { "Saudi Arabia",               "RUH" },
            { "Arabia Saudita",             "RUH" },
            { "Arabia Saudită",             "RUH" },
            { "Riyadh",                     "RUH" },
            { "Riad",                       "RUH" },
            { "Jeddah",                     "JED" },
            { "Djedda",                     "JED" },
            { "Mecca",                      "JED" },
            { "Medina",                     "MED" },
            { "Dammam",                     "DMM" },

            // Qatar
            { "Qatar",                      "DOH" },
            { "Katar",                      "DOH" },
            { "Doha",                       "DOH" },

            // Kuwait
            { "Kuwait",                     "KWI" },
            { "Kuwait City",                "KWI" },

            // Bahrain
            { "Bahrain",                    "BAH" },
            { "Bahrein",                    "BAH" },
            { "Manama",                     "BAH" },

            // Oman
            { "Oman",                       "MCT" },
            { "Muscat",                     "MCT" },

            // Israel
            { "Israel",                     "TLV" },
            { "Tel Aviv",                   "TLV" },
            { "Jerusalem",                  "TLV" },
            { "Ierusalim",                  "TLV" },

            // Iordania
            { "Jordan",                     "AMM" },
            { "Iordania",                   "AMM" },
            { "Amman",                      "AMM" },

            // Liban
            { "Lebanon",                    "BEY" },
            { "Liban",                      "BEY" },
            { "Beirut",                     "BEY" },

            // Irak
            { "Iraq",                       "BGW" },
            { "Irak",                       "BGW" },
            { "Baghdad",                    "BGW" },
            { "Bagdad",                     "BGW" },
            { "Erbil",                      "EBL" },

            // Iran
            { "Iran",                       "IKA" },
            { "Tehran",                     "IKA" },
            { "Teheran",                    "IKA" },

            // ================================================================
            //  AFRICA
            // ================================================================

            // Egipt
            { "Egypt",                      "CAI" },
            { "Egipt",                      "CAI" },
            { "Cairo",                      "CAI" },
            { "Cairo",                      "CAI" },
            { "Hurghada",                   "HRG" },
            { "Sharm el-Sheikh",            "SSH" },
            { "Sharm",                      "SSH" },
            { "Luxor",                      "LXR" },
            { "Alexandria",                 "HBE" },
            { "Alexandria",                 "HBE" },

            // Maroc
            { "Morocco",                    "CMN" },
            { "Maroc",                      "CMN" },
            { "Casablanca",                 "CMN" },
            { "Marrakesh",                  "RAK" },
            { "Marrakech",                  "RAK" },
            { "Rabat",                      "RBA" },
            { "Agadir",                     "AGA" },
            { "Fes",                        "FEZ" },
            { "Fez",                        "FEZ" },

            // Tunisia
            { "Tunisia",                    "TUN" },
            { "Tunisia",                    "TUN" },
            { "Tunis",                      "TUN" },
            { "Monastir",                   "MIR" },
            { "Djerba",                     "DJE" },
            { "Djerba",                     "DJE" },

            // Africa de Sud
            { "South Africa",               "JNB" },
            { "Africa de Sud",              "JNB" },
            { "Johannesburg",               "JNB" },
            { "Cape Town",                  "CPT" },
            { "Durban",                     "DUR" },

            // Kenya
            { "Kenya",                      "NBO" },
            { "Nairobi",                    "NBO" },
            { "Mombasa",                    "MBA" },

            // Tanzania
            { "Tanzania",                   "DAR" },
            { "Dar es Salaam",              "DAR" },
            { "Zanzibar",                   "ZNZ" },

            // Ethiopia
            { "Ethiopia",                   "ADD" },
            { "Etiopia",                    "ADD" },
            { "Addis Ababa",                "ADD" },
            { "Addis Abeba",                "ADD" },

            // Nigeria
            { "Nigeria",                    "LOS" },
            { "Lagos",                      "LOS" },
            { "Abuja",                      "ABV" },

            // Ghana
            { "Ghana",                      "ACC" },
            { "Accra",                      "ACC" },

            // ================================================================
            //  ASIA
            // ================================================================

            // Japonia
            { "Japan",                      "HND" },
            { "Japonia",                    "HND" },
            { "Tokyo",                      "HND" },
            { "Tokio",                      "HND" },
            { "Osaka",                      "KIX" },
            { "Nagoya",                     "NGO" },
            { "Fukuoka",                    "FUK" },
            { "Sapporo",                    "CTS" },
            { "Kyoto",                      "ITM" },

            // China
            { "China",                      "PEK" },
            { "Beijing",                    "PEK" },
            { "Peking",                     "PEK" },
            { "Beijing",                    "PEK" },
            { "Shanghai",                   "PVG" },
            { "Guangzhou",                  "CAN" },
            { "Shenzhen",                   "SZX" },
            { "Chengdu",                    "CTU" },
            { "Chongqing",                  "CKG" },
            { "Kunming",                    "KMG" },
            { "Xi'an",                      "XIY" },
            { "Hangzhou",                   "HGH" },
            { "Nanjing",                    "NKG" },
            { "Hong Kong",                  "HKG" },
            { "Macau",                      "MFM" },

            // Coreea de Sud
            { "South Korea",                "ICN" },
            { "Coreea de Sud",              "ICN" },
            { "Korea",                      "ICN" },
            { "Seoul",                      "ICN" },
            { "Seul",                       "ICN" },
            { "Busan",                      "PUS" },

            // India
            { "India",                      "DEL" },
            { "New Delhi",                  "DEL" },
            { "Delhi",                      "DEL" },
            { "Mumbai",                     "BOM" },
            { "Bombay",                     "BOM" },
            { "Bangalore",                  "BLR" },
            { "Bengaluru",                  "BLR" },
            { "Hyderabad",                  "HYD" },
            { "Chennai",                    "MAA" },
            { "Madras",                     "MAA" },
            { "Kolkata",                    "CCU" },
            { "Calcutta",                   "CCU" },
            { "Goa",                        "GOI" },
            { "Ahmedabad",                  "AMD" },
            { "Pune",                       "PNQ" },
            { "Jaipur",                     "JAI" },
            { "Kochi",                      "COK" },

            // Thailanda
            { "Thailand",                   "BKK" },
            { "Thailanda",                  "BKK" },
            { "Bangkok",                    "BKK" },
            { "Phuket",                     "HKT" },
            { "Chiang Mai",                 "CNX" },
            { "Ko Samui",                   "USM" },
            { "Krabi",                      "KBV" },

            // Vietnam
            { "Vietnam",                    "SGN" },
            { "Ho Chi Minh",                "SGN" },
            { "Ho Chi Minh City",           "SGN" },
            { "Saigon",                     "SGN" },
            { "Hanoi",                      "HAN" },
            { "Hanoi",                      "HAN" },
            { "Da Nang",                    "DAD" },
            { "Nha Trang",                  "CXR" },

            // Singapore
            { "Singapore",                  "SIN" },

            // Malaysia
            { "Malaysia",                   "KUL" },
            { "Kuala Lumpur",               "KUL" },
            { "Penang",                     "PEN" },
            { "Kota Kinabalu",              "BKI" },

            // Indonezia
            { "Indonesia",                  "CGK" },
            { "Indonezia",                  "CGK" },
            { "Jakarta",                    "CGK" },
            { "Bali",                       "DPS" },
            { "Denpasar",                   "DPS" },
            { "Surabaya",                   "SUB" },
            { "Lombok",                     "LOP" },
            { "Yogyakarta",                 "JOG" },

            // Filipine
            { "Philippines",                "MNL" },
            { "Filipine",                   "MNL" },
            { "Manila",                     "MNL" },
            { "Cebu",                       "CEB" },
            { "Davao",                      "DVO" },

            // Cambodgia
            { "Cambodia",                   "PNH" },
            { "Cambodgia",                  "PNH" },
            { "Phnom Penh",                 "PNH" },
            { "Siem Reap",                  "REP" },
            { "Angkor",                     "REP" },

            // Myanmar
            { "Myanmar",                    "RGN" },
            { "Burma",                      "RGN" },
            { "Yangon",                     "RGN" },
            { "Mandalay",                   "MDL" },

            // Nepal
            { "Nepal",                      "KTM" },
            { "Kathmandu",                  "KTM" },
            { "Katmandu",                   "KTM" },

            // Sri Lanka
            { "Sri Lanka",                  "CMB" },
            { "Colombo",                    "CMB" },

            // Pakistan
            { "Pakistan",                   "KHI" },
            { "Karachi",                    "KHI" },
            { "Lahore",                     "LHE" },
            { "Islamabad",                  "ISB" },

            // Bangladesh
            { "Bangladesh",                 "DAC" },
            { "Dhaka",                      "DAC" },

            // Kazahstan
            { "Kazakhstan",                 "ALA" },
            { "Kazahstan",                  "ALA" },
            { "Almaty",                     "ALA" },
            { "Nur-Sultan",                 "NQZ" },
            { "Astana",                     "NQZ" },

            // Uzbekistan
            { "Uzbekistan",                 "TAS" },
            { "Uzbekistan",                 "TAS" },
            { "Tashkent",                   "TAS" },
            { "Tașkent",                    "TAS" },
            { "Samarkand",                  "SKD" },
            { "Samarkanda",                 "SKD" },

            // Georgia
            { "Georgia",                    "TBS" },
            { "Georgia",                    "TBS" },
            { "Tbilisi",                    "TBS" },
            { "Batumi",                     "BUS" },

            // Armenia
            { "Armenia",                    "EVN" },
            { "Yerevan",                    "EVN" },
            { "Erevan",                     "EVN" },

            // Azerbaijan
            { "Azerbaijan",                 "GYD" },
            { "Azerbaidjan",                "GYD" },
            { "Azerbaïdjan",                "GYD" },
            { "Baku",                       "GYD" },
            { "Baku",                       "GYD" },

            // ================================================================
            //  AMERICA DE NORD
            // ================================================================

            // Statele Unite
            { "USA",                        "JFK" },
            { "US",                         "JFK" },
            { "America",                    "JFK" },
            { "Statele Unite",              "JFK" },
            { "Statele Unite ale Americii", "JFK" },
            { "United States",              "JFK" },
            { "New York",                   "JFK" },
            { "Los Angeles",                "LAX" },
            { "LA",                         "LAX" },
            { "Chicago",                    "ORD" },
            { "Miami",                      "MIA" },
            { "San Francisco",              "SFO" },
            { "Las Vegas",                  "LAS" },
            { "Seattle",                    "SEA" },
            { "Boston",                     "BOS" },
            { "Atlanta",                    "ATL" },
            { "Dallas",                     "DFW" },
            { "Houston",                    "IAH" },
            { "Denver",                     "DEN" },
            { "Washington",                 "IAD" },
            { "Washington DC",              "IAD" },
            { "Phoenix",                    "PHX" },
            { "Minneapolis",                "MSP" },
            { "Detroit",                    "DTW" },
            { "Orlando",                    "MCO" },
            { "Tampa",                      "TPA" },
            { "New Orleans",                "MSY" },
            { "Salt Lake City",             "SLC" },
            { "Portland",                   "PDX" },
            { "San Diego",                  "SAN" },
            { "Austin",                     "AUS" },
            { "Nashville",                  "BNA" },
            { "Charlotte",                  "CLT" },
            { "Philadelphia",               "PHL" },
            { "Baltimore",                  "BWI" },
            { "Honolulu",                   "HNL" },
            { "Hawaii",                     "HNL" },
            { "Anchorage",                  "ANC" },
            { "Alaska",                     "ANC" },

            // Canada
            { "Canada",                     "YYZ" },
            { "Toronto",                    "YYZ" },
            { "Vancouver",                  "YVR" },
            { "Montreal",                   "YUL" },
            { "Montréal",                   "YUL" },
            { "Calgary",                    "YYC" },
            { "Edmonton",                   "YEG" },
            { "Ottawa",                     "YOW" },
            { "Quebec City",                "YQB" },
            { "Quebec",                     "YQB" },
            { "Winnipeg",                   "YWG" },
            { "Halifax",                    "YHZ" },

            // Mexic
            { "Mexico",                     "MEX" },
            { "Mexic",                      "MEX" },
            { "Mexico City",                "MEX" },
            { "Cancun",                     "CUN" },
            { "Cancún",                     "CUN" },
            { "Guadalajara",                "GDL" },
            { "Monterrey",                  "MTY" },
            { "Los Cabos",                  "SJD" },
            { "Puerto Vallarta",            "PVR" },
            { "Playa del Carmen",           "CUN" },
            { "Riviera Maya",               "CUN" },

            // ================================================================
            //  AMERICA CENTRALA SI CARAIBE
            // ================================================================

            { "Cuba",                       "HAV" },
            { "Havana",                     "HAV" },
            { "Havanna",                    "HAV" },
            { "Habana",                     "HAV" },
            { "Dominican Republic",         "PUJ" },
            { "Republica Dominicana",       "PUJ" },
            { "Punta Cana",                 "PUJ" },
            { "Santo Domingo",              "SDQ" },
            { "Jamaica",                    "KIN" },
            { "Kingston",                   "KIN" },
            { "Puerto Rico",                "SJU" },
            { "San Juan",                   "SJU" },
            { "Bahamas",                    "NAS" },
            { "Nassau",                     "NAS" },
            { "Barbados",                   "BGI" },
            { "Aruba",                      "AUA" },
            { "Curacao",                    "CUR" },
            { "Curaçao",                    "CUR" },
            { "Costa Rica",                 "SJO" },
            { "San Jose",                   "SJO" },
            { "San José",                   "SJO" },
            { "Panama",                     "PTY" },
            { "Panama City",                "PTY" },
            { "Guatemala",                  "GUA" },
            { "Guatemala City",             "GUA" },

            // ================================================================
            //  AMERICA DE SUD
            // ================================================================

            // Brazilia
            { "Brazil",                     "GRU" },
            { "Brazilia",                   "GRU" },
            { "Brasil",                     "GRU" },
            { "Sao Paulo",                  "GRU" },
            { "São Paulo",                  "GRU" },
            { "Rio de Janeiro",             "GIG" },
            { "Rio",                        "GIG" },
            { "Brasilia",                   "BSB" },
            { "Brasília",                   "BSB" },
            { "Salvador",                   "SSA" },
            { "Fortaleza",                  "FOR" },
            { "Belo Horizonte",             "CNF" },
            { "Manaus",                     "MAO" },
            { "Recife",                     "REC" },

            // Argentina
            { "Argentina",                  "EZE" },
            { "Buenos Aires",               "EZE" },
            { "Cordoba",                    "COR" },
            { "Córdoba",                    "COR" },
            { "Mendoza",                    "MDZ" },
            { "Patagonia",                  "USH" },
            { "Ushuaia",                    "USH" },

            // Chile
            { "Chile",                      "SCL" },
            { "Santiago",                   "SCL" },
            { "Punta Arenas",               "PUQ" },

            // Columbia
            { "Colombia",                   "BOG" },
            { "Columbia",                   "BOG" },
            { "Bogota",                     "BOG" },
            { "Bogotá",                     "BOG" },
            { "Medellin",                   "MDE" },
            { "Medellín",                   "MDE" },
            { "Cartagena",                  "CTG" },

            // Peru
            { "Peru",                       "LIM" },
            { "Peru",                       "LIM" },
            { "Lima",                       "LIM" },
            { "Cusco",                      "CUZ" },
            { "Cuzco",                      "CUZ" },

            // Ecuador
            { "Ecuador",                    "UIO" },
            { "Quito",                      "UIO" },
            { "Guayaquil",                  "GYE" },
            { "Galapagos",                  "GPS" },

            // Bolivia
            { "Bolivia",                    "VVI" },
            { "Santa Cruz",                 "VVI" },
            { "La Paz",                     "LPB" },

            // Uruguay
            { "Uruguay",                    "MVD" },
            { "Montevideo",                 "MVD" },

            // Venezuela
            { "Venezuela",                  "CCS" },
            { "Caracas",                    "CCS" },

            // ================================================================
            //  OCEANIA
            // ================================================================

            // Australia
            { "Australia",                  "SYD" },
            { "Sydney",                     "SYD" },
            { "Melbourne",                  "MEL" },
            { "Brisbane",                   "BNE" },
            { "Perth",                      "PER" },
            { "Adelaide",                   "ADL" },
            { "Gold Coast",                 "OOL" },
            { "Cairns",                     "CNS" },
            { "Darwin",                     "DRW" },
            { "Hobart",                     "HBA" },
            { "Canberra",                   "CBR" },

            // Noua Zeelanda
            { "New Zealand",                "AKL" },
            { "Noua Zeelanda",              "AKL" },
            { "Noua Zeelandă",              "AKL" },
            { "Auckland",                   "AKL" },
            { "Wellington",                 "WLG" },
            { "Christchurch",               "CHC" },
            { "Queenstown",                 "ZQN" },

            // Pacific
            { "Fiji",                       "NAN" },
            { "Nadi",                       "NAN" },
            { "Bora Bora",                  "BOB" },
            { "Tahiti",                     "PPT" },
            { "French Polynesia",           "PPT" },
            { "Maldives",                   "MLE" },
            { "Maldive",                    "MLE" },
            { "Male",                       "MLE" },
            { "Malé",                       "MLE" },
            { "Seychelles",                 "SEZ" },
            { "Seișele",                    "SEZ" },
            { "Mauritius",                  "MRU" },
            { "Mauritius",                  "MRU" },
        };

        public static string GetSafeCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            if (input.Length == 2 || input.Length == 3) return input.ToUpper();

            if (NameToCode.TryGetValue(input.Trim(), out var code))
            {
                return code;
            }

            // if value doesn't exist return error
            return input;
        }
    }
}
