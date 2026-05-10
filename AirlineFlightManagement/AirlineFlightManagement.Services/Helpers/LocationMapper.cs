using System;
using System.Collections.Generic;

namespace AirlineFlightManagement.Services.Helpers
{
    public static class LocationMapper
    {
        //dictionary for common city and country names to their corresponding IATA airport codes, with case-insensitive keys
        public static readonly Dictionary<string, string> NameToCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bucuresti", "OTP" },
            { "București", "OTP" },
            { "Bucharest", "OTP" },
            { "Istanbul", "IST" },
            { "London", "LHR" },
            { "Londra", "LHR" },
            { "Paris", "CDG" },
            { "Rome", "FCO" },
            { "Roma", "FCO" },
            { "New York", "JFK" },
            { "Madrid", "MAD" },
            { "Barcelona", "BCN" },
            { "Berlin", "BER" },
            { "Vienna", "VIE" },
            { "Viena", "VIE" },
            { "Budapest", "BUD" },
            { "Budapesta", "BUD" },
            { "Sofia", "SOF" },
            { "Athens", "ATH" },
            { "Atena", "ATH" },
            { "Romania", "OTP" }, // București
            { "România", "OTP" },
            { "Suedia", "ARN" },  // Stockholm
            { "Sweden", "ARN" },
            { "Spania", "MAD" },  // Madrid
            { "Spain", "MAD" },
            { "Italia", "FCO" },  // Roma
            { "Italy", "FCO" },
            { "Franta", "CDG" },  // Paris
            { "Franța", "CDG" },
            { "France", "CDG" },
            { "Germania", "FRA" },// Frankfurt
            { "Germany", "FRA" },
            { "Marea Britanie", "LHR" }, // Londra
            { "UK", "LHR" },
            { "Anglia", "LHR" },
            { "United Kingdom", "LHR" },
            { "Turcia", "IST" },  // Istanbul
            { "Turkey", "IST" },
            { "Statele Unite", "JFK" }, // New York
            { "Statele Unite ale Americii", "JFK" },
            { "USA", "JFK" },
            { "America", "JFK" },
            { "Grecia", "ATH" },  // Atena
            { "Greece", "ATH" },
            { "Bulgaria", "SOF" }, // Sofia
            { "Ungaria", "BUD" },  // Budapesta
            { "Hungary", "BUD" },
            { "Olanda", "AMS" },   // Amsterdam
            { "Netherlands", "AMS" },
            { "Belgia", "BRU" },   // Bruxelles
            { "Belgium", "BRU" },
            { "Elvetia", "ZRH" },  // Zurich
            { "Elveția", "ZRH" },
            { "Switzerland", "ZRH" },
            { "Austria", "VIE" },  // Viena
            { "Polonia", "WAW" },  // Varșovia
            { "Poland", "WAW" },
            { "Norvegia", "OSL" }, // Oslo
            { "Norway", "OSL" },
            { "Danemarca", "CPH" },// Copenhaga
            { "Denmark", "CPH" },
            { "Finlanda", "HEL" }, // Helsinki
            { "Finland", "HEL" },
            { "Portugalia", "LIS" },// Lisabona
            { "Portugal", "LIS" },
            { "Cipru", "LCA" },    // Larnaca
            { "Cyprus", "LCA" },
            { "Egipt", "CAI" },    // Cairo
            { "Egypt", "CAI" },
            { "Emiratele Arabe Unite", "DXB" }, // Dubai
            { "EAU", "DXB" },
            { "UAE", "DXB" },
            { "Japonia", "HND" },  // Tokyo
            { "Japan", "HND" },
            { "China", "PEK" }     // Beijing
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
