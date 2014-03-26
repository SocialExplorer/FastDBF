///
/// Author: Ahmed Lacevic
/// Date: 12/1/2007
/// Desc: Reads a DBF file, outputs a CSV file!
/// 
/// Revision History:
/// -----------------------------------
///   Author:
///   Date:
///   Desc:


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SocialExplorer.IO.FastDBF;


namespace DBF2CSV
{ 
  class Program
  { 
    static void Main(string[] args)
    { 
      
      if(args.Length < 2)
      { 
        //print help
        Console.WriteLine("\n\n");
        Console.WriteLine("Welcome to Social Explorer DBF 2 CSV Utility");
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("\nParameters:");
        Console.WriteLine("1. input DBF file");
        Console.WriteLine("2. output CSV file");
        
        Console.WriteLine("\nOptional switches:");
        Console.WriteLine("/F  - format numbers so 5.5000 comes out as 5.5");
        Console.WriteLine("/P  - padded output, fixed width (/P trumps /F)");
        Console.WriteLine("/Q  - only output quotes when comma appears in data");
        
        Console.WriteLine("\n\nExample: dbf2csv \"in.dbf\" \"out.csv\" /P /Q");
        
      }
      else
      { 
        
        //check if input DBF file exists...
        if(!File.Exists(args[0]))
        {
          Console.WriteLine("Input file '" + args[0] + "' does not exist!");
          return;
        }
        
        
        //create output csv file overwrite if already exists.
        if(File.Exists(args[1]))
        { 
          //ask to overwrite:
          Console.WriteLine("Output CSV file '" + args[1] + "' already exists.");
          Console.WriteLine("Would you like to overwrite it? Press 'Y' for yes: ");
          if(Console.ReadKey().KeyChar.ToString().ToUpper() != "Y")
            return;
        }
        
        bool bSwitchF = false;
        bool bSwitchP = false;
        bool bSwitchQ = false;
        
        for(int i=0;i<args.Length;i++)
          if(args[i] == "/F")
            bSwitchF = true;
        
        for (int i = 0; i < args.Length; i++)
          if (args[i] == "/P")
            bSwitchP = true;
        
        for (int i = 0; i < args.Length; i++)
          if (args[i] == "/Q")
            bSwitchQ = true;
        
        
        //open DBF file and create CSV output file...
        StreamWriter swcsv = new StreamWriter(args[1], false, Encoding.Default);
        DbfFile dbf = new DbfFile(Encoding.UTF8);
        dbf.Open(args[0], FileMode.Open);
        
        
        //output column names
        for (int i = 0; i < dbf.Header.ColumnCount; i++)
        { 
          if(dbf.Header[i].ColumnType != DbfColumn.DbfColumnType.Binary && 
             dbf.Header[i].ColumnType != DbfColumn.DbfColumnType.Memo)
            swcsv.Write((i == 0 ? "": ",") + dbf.Header[i].Name);
          else
            Console.WriteLine("WARNING: Excluding Binary/Memo field '" + dbf.Header[i].Name + "'");
          
        }
        
        swcsv.WriteLine();
        
        //output values for all but binary and memo...
        DbfRecord orec = new DbfRecord(dbf.Header);
        while(dbf.ReadNext(orec))
        { 
          //output column values...
          if (!orec.IsDeleted)
          { 
            for (int i = 0; i < orec.ColumnCount; i++)
            { 
              if(orec.Column(i).ColumnType == DbfColumn.DbfColumnType.Character)
              { 
                //string values: trim, enclose in quotes and escape quotes with double quotes
                string sval = orec[i];
                
                if(!bSwitchP)
                  sval = orec[i].Trim();
                
                if(!bSwitchQ || sval.IndexOf('"') > -1)
                  sval = ("\"" + sval.Replace("\"", "\"\"") + "\"");
                
                swcsv.Write(sval);
                
              }
              else if(orec.Column(i).ColumnType == DbfColumn.DbfColumnType.Date)
                swcsv.Write(orec.GetDateValue(i).ToString("MM-dd-yyyy"));
              else
              { 
                if (bSwitchP)
                  swcsv.Write(orec[i]);
                else if(bSwitchF)
                  swcsv.Write(FormatNumber(orec[i].Trim()));
                else
                  swcsv.Write(orec[i].Trim());
              }
              
              //end record with a linefeed or end column with a comma.
              if(i < orec.ColumnCount-1) 
                swcsv.Write(",");
              
            }
            
            //write line...
            swcsv.WriteLine();
            
          }
          
        }
        
        
        //close files...
        swcsv.Flush();
        swcsv.Close();
        dbf.Close();
        
        
        
      }
      
    }
    
    
    
    /// <summary>
    /// Removes leading and trailing zeros from a number (double or int) value represented as a string.
    /// So for example if a '5.00' is passed this function would return '5'. If '00035.3420' is passed, '35.342' is returned.
    /// etc.
    /// </summary>
    /// <param name="sDouble"></param>
    /// <returns></returns>
    public static string FormatNumber(string sNumber)
    { 
      //an empty string is effectively a NULL number, not a zero or anything so just return it as empty.
      if (sNumber == null || sNumber == "")
        return "";
      
      //if this is not a decimal number, remove leading zeros...
      if (sNumber.IndexOf('.') == -1)
      { 
        //this is a bit tricky here. we could get a number "00000", which is really "0",
        sNumber = sNumber.TrimStart("0".ToCharArray());
        
        //if nothing is left, that means we had a zero to start with! since TrimStart("000") will return "".
        if (sNumber == "")
          sNumber = "0";
        
        return sNumber;
        
      }
      else
      { 
        string svalFormatted = sNumber;
        svalFormatted = svalFormatted.Trim().Trim("0".ToCharArray());
        
        //if only a period is left behind remove it
        if (svalFormatted != "" && svalFormatted[svalFormatted.Length - 1] == '.')
          svalFormatted = svalFormatted.Substring(0, svalFormatted.Length - 1);
        
        //if formatted number starts with a '.' then add a 0 in front!
        if (svalFormatted.Length > 0 && svalFormatted[0] == '.')
          svalFormatted = "0" + svalFormatted;
        
        //if nothing is left, that means we had a zero to start with! Trim("0") removed all 
        //trailing and leading zeros and a period was left over which was then removed as well, 
        //so the string is empty!
        if (svalFormatted == "")
          svalFormatted = "0";
        
        return svalFormatted;
        
      }
      
    }
    
  }
}
