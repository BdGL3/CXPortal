using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace L3.Cargo.DetectorPlot
{
    class comments
    {
     public comments()
        {
        }
     public bool Save(string fname,string content)
        {
            bool bRet = false;
            try
            {
                FileInfo t = new FileInfo(fname);
                StreamWriter Tex = t.CreateText();
                Tex.Write(content);
                Tex.Close();
                bRet = true;
            }
            catch { }
            return bRet;
        }
     public string Read(string fname)
     {
       string  comment=null;
       if (File.Exists(fname))
       {
           StreamReader re = File.OpenText(fname);
           comment=re.ReadToEnd(); 
       }
 
       return comment;
     }
    }
}
