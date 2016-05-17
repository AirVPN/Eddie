// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

/*
 * Official data from ISO 3166-1
 * */

namespace Eddie.Core
{	
    public class CountriesManager
    {
		public static Dictionary<string, string> Code2Name = new Dictionary<string, string>();
		public static Dictionary<string, string> Name2Code = new Dictionary<string, string>();
		
		public static void Init()
		{
			Add("AF","Afghanistan");
			Add("AX","Åland Islands");
			Add("AL","Albania");
			Add("DZ","Algeria");
			Add("AS","American Samoa");
			Add("AD","Andorra");
			Add("AO","Angola");
			Add("AI","Anguilla");
			Add("AQ","Antarctica");
			Add("AG","Antigua and Barbuda");
			Add("AR","Argentina");
			Add("AM","Armenia");
			Add("AW","Aruba");
			Add("AU","Australia");
			Add("AT","Austria");
			Add("AZ","Azerbaijan");
			Add("BS","Bahamas");
			Add("BH","Bahrain");
			Add("BD","Bangladesh");
			Add("BB","Barbados");
			Add("BY","Belarus");
			Add("BE","Belgium");
			Add("BZ","Belize");
			Add("BJ","Benin");
			Add("BM","Bermuda");
			Add("BT","Bhutan");
			Add("BO","Bolivia");
			Add("BQ","Bonaire, Sint Eustatius and Saba");
			Add("BA","Bosnia and Herzegovina");
			Add("BW","Botswana");
			Add("BV","Bouvet Island");
			Add("BR","Brazil");
			Add("IO","British Indian Ocean Territory");
			Add("BN","Brunei Darussalam");
			Add("BG","Bulgaria");
			Add("BF","Burkina Faso");
			Add("BI","Burundi");
			Add("CV","Cabo Verde");
			Add("KH","Cambodia");
			Add("CM","Cameroon");
			Add("CA","Canada");
			Add("KY","Cayman Islands");
			Add("CF","Central African Republic");
			Add("TD","Chad");
			Add("CL","Chile");
			Add("CN","China");
			Add("CX","Christmas Island");
			Add("CC","Cocos (Keeling) Islands");
			Add("CO","Colombia");
			Add("KM","Comoros");
			Add("CG","Congo");
			Add("CD","Congo (Democratic Republic of the)");
			Add("CK","Cook Islands");
			Add("CR","Costa Rica");
			Add("CI","Côte d'Ivoire");
			Add("HR","Croatia");
			Add("CU","Cuba");
			Add("CW","Curaçao");
			Add("CY","Cyprus");
			Add("CZ","Czech Republic");
			Add("DK","Denmark");
			Add("DJ","Djibouti");
			Add("DM","Dominica");
			Add("DO","Dominican Republic");
			Add("EC","Ecuador");
			Add("EG","Egypt");
			Add("SV","El Salvador");
			Add("GQ","Equatorial Guinea");
			Add("ER","Eritrea");
			Add("EE","Estonia");
			Add("ET","Ethiopia");
			Add("FK","Falkland Islands");
			Add("FO","Faroe Islands");
			Add("FJ","Fiji");
			Add("FI","Finland");
			Add("FR","France");
			Add("GF","French Guiana");
			Add("PF","French Polynesia");
			Add("TF","French Southern Territories");
			Add("GA","Gabon");
			Add("GM","Gambia");
			Add("GE","Georgia");
			Add("DE","Germany");
			Add("GH","Ghana");
			Add("GI","Gibraltar");
			Add("GR","Greece");
			Add("GL","Greenland");
			Add("GD","Grenada");
			Add("GP","Guadeloupe");
			Add("GU","Guam");
			Add("GT","Guatemala");
			Add("GG","Guernsey");
			Add("GN","Guinea");
			Add("GW","Guinea-Bissau");
			Add("GY","Guyana");
			Add("HT","Haiti");
			Add("HM","Heard Island and McDonald Islands");
			Add("VA","Holy See");
			Add("HN","Honduras");
			Add("HK","Hong Kong");
			Add("HU","Hungary");
			Add("IS","Iceland");
			Add("IN","India");
			Add("ID","Indonesia");
			Add("IR","Iran");
			Add("IQ","Iraq");
			Add("IE","Ireland");
			Add("IM","Isle of Man");
			Add("IL","Israel");
			Add("IT","Italy");
			Add("JM","Jamaica");
			Add("JP","Japan");
			Add("JE","Jersey");
			Add("JO","Jordan");
			Add("KZ","Kazakhstan");
			Add("KE","Kenya");
			Add("KI","Kiribati");
			Add("KP","Korea (Democratic People's Republic of)");
			Add("KR","Korea");
			Add("KW","Kuwait");
			Add("KG","Kyrgyzstan");
			Add("LA","Lao People's Democratic Republic");
			Add("LV","Latvia");
			Add("LB","Lebanon");
			Add("LS","Lesotho");
			Add("LR","Liberia");
			Add("LY","Libya");
			Add("LI","Liechtenstein");
			Add("LT","Lithuania");
			Add("LU","Luxembourg");
			Add("MO","Macao");
			Add("MK","Macedonia");
			Add("MG","Madagascar");
			Add("MW","Malawi");
			Add("MY","Malaysia");
			Add("MV","Maldives");
			Add("ML","Mali");
			Add("MT","Malta");
			Add("MH","Marshall Islands");
			Add("MQ","Martinique");
			Add("MR","Mauritania");
			Add("MU","Mauritius");
			Add("YT","Mayotte");
			Add("MX","Mexico");
			Add("FM","Micronesia");
			Add("MD","Moldova");
			Add("MC","Monaco");
			Add("MN","Mongolia");
			Add("ME","Montenegro");
			Add("MS","Montserrat");
			Add("MA","Morocco");
			Add("MZ","Mozambique");
			Add("MM","Myanmar");
			Add("NA","Namibia");
			Add("NR","Nauru");
			Add("NP","Nepal");
			Add("NL","Netherlands");
			Add("NC","New Caledonia");
			Add("NZ","New Zealand");
			Add("NI","Nicaragua");
			Add("NE","Niger");
			Add("NG","Nigeria");
			Add("NU","Niue");
			Add("NF","Norfolk Island");
			Add("MP","Northern Mariana Islands");
			Add("NO","Norway");
			Add("OM","Oman");
			Add("PK","Pakistan");
			Add("PW","Palau");
			Add("PS","Palestine, State of");
			Add("PA","Panama");
			Add("PG","Papua New Guinea");
			Add("PY","Paraguay");
			Add("PE","Peru");
			Add("PH","Philippines");
			Add("PN","Pitcairn");
			Add("PL","Poland");
			Add("PT","Portugal");
			Add("PR","Puerto Rico");
			Add("QA","Qatar");
			Add("RE","Réunion");
			Add("RO","Romania");
			Add("RU","Russian Federation");
			Add("RW","Rwanda");
			Add("BL","Saint Barthélemy");
			Add("SH","Saint Helena, Ascension and Tristan da Cunha");
			Add("KN","Saint Kitts and Nevis");
			Add("LC","Saint Lucia");
			Add("MF","Saint Martin (French part)");
			Add("PM","Saint Pierre and Miquelon");
			Add("VC","Saint Vincent and the Grenadines");
			Add("WS","Samoa");
			Add("SM","San Marino");
			Add("ST","Sao Tome and Principe");
			Add("SA","Saudi Arabia");
			Add("SN","Senegal");
			Add("RS","Serbia");
			Add("SC","Seychelles");
			Add("SL","Sierra Leone");
			Add("SG","Singapore");
			Add("SX","Sint Maarten (Dutch part)");
			Add("SK","Slovakia");
			Add("SI","Slovenia");
			Add("SB","Solomon Islands");
			Add("SO","Somalia");
			Add("ZA","South Africa");
			Add("GS","South Georgia and the South Sandwich Islands");
			Add("SS","South Sudan");
			Add("ES","Spain");
			Add("LK","Sri Lanka");
			Add("SD","Sudan");
			Add("SR","Suriname");
			Add("SJ","Svalbard and Jan Mayen");
			Add("SZ","Swaziland");
			Add("SE","Sweden");
			Add("CH","Switzerland");
			Add("SY","Syrian Arab Republic");
			Add("TW","Taiwan, Province of China");
			Add("TJ","Tajikistan");
			Add("TZ","Tanzania, United Republic of");
			Add("TH","Thailand");
			Add("TL","Timor-Leste");
			Add("TG","Togo");
			Add("TK","Tokelau");
			Add("TO","Tonga");
			Add("TT","Trinidad and Tobago");
			Add("TN","Tunisia");
			Add("TR","Turkey");
			Add("TM","Turkmenistan");
			Add("TC","Turks and Caicos Islands");
			Add("TV","Tuvalu");
			Add("UG","Uganda");
			Add("UA","Ukraine");
			Add("AE","United Arab Emirates");
			Add("GB","United Kingdom");
			Add("UM","United States Minor Outlying Islands");
			Add("US","United States of America");
			Add("UY","Uruguay");
			Add("UZ","Uzbekistan");
			Add("VU","Vanuatu");
			Add("VE","Venezuela");
			Add("VN","Viet Nam");
			Add("VG","Virgin Islands (British)");
			Add("VI","Virgin Islands (U.S.)");
			Add("WF","Wallis and Futuna");
			Add("EH","Western Sahara");
			Add("YE","Yemen");
			Add("ZM","Zambia");
			Add("ZW","Zimbabwe");
		}

		public static bool IsCountryCode(string code)
		{
			return Code2Name.ContainsKey(code);
		}

		public static string GetNameFromCode(string code)
		{
			if (Code2Name.ContainsKey(code))
				return Code2Name[code];
			else
				return "";
		}

		public static void Add(string code, string name)
		{
			Code2Name[code.ToLowerInvariant()] = name;
			Name2Code[name] = code.ToLowerInvariant();
		}

		
    }
}
